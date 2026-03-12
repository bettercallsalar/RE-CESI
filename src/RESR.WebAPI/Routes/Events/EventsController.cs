using Microsoft.AspNetCore.Mvc;
using RESR.Core.Controllers.Events;
using RESR.Core.Errors;
using RESR.Core.Security.Token;
using RESR.Models.Resources;
using RESR.WebAPI.Security;

namespace RESR.WebAPI.Routes.Events;

[ApiController]
[Route("api/events")]
public sealed class EventsController : AuthenticatedResourceControllerBase
{
    private readonly IEventService _service;
    private const int MaxPageSize = 100;

    public EventsController(IEventService service, ITokenService tokenService)
        : base(tokenService)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<PaginatedEventsResponse>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? keyword = null,
        [FromQuery] int? idUser = null,
        [FromQuery] int? idCategory = null,
        [FromQuery] int? idDepartment = null,
        [FromQuery] DateTime? startFrom = null,
        [FromQuery] DateTime? startTo = null,
        CancellationToken ct = default)
    {
        if (page <= 0 || pageSize <= 0 || idUser is <= 0 || idCategory is <= 0 || idDepartment is <= 0)
            return BadRequest(new { message = "Page, PageSize, IdUser, IdCategory and IdDepartment must be greater than 0." });
        if (pageSize > MaxPageSize) return BadRequest(new { message = $"PageSize cannot be greater than {MaxPageSize}." });

        var filters = new EventListingFilters(
            Keyword: keyword,
            Visibility: ResourceVisibility.PUBLIC,
            IdUser: idUser,
            IdCategory: idCategory,
            IdDepartment: idDepartment,
            IsApproved: true,
            StartFrom: startFrom,
            StartTo: startTo
        );

        var (events, totalCount) = await _service.GetPaginatedAsync(page, pageSize, filters, ct);
        var items = events.Select(ToResponse).ToList();
        var totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling((double)totalCount / pageSize);

        return Ok(new PaginatedEventsResponse(items, page, pageSize, totalCount, totalPages));
    }

    [AuthorizePermissionOrSelf("idUser", PermissionNames.ModerateContent)]
    [HttpGet("{idResource:int}")]
    public async Task<ActionResult<EventResponse>> GetByResourceId([FromRoute] int idResource, CancellationToken ct)
    {
        var @event = await _service.GetByResourceIdAsync(idResource, ct);
        return @event is null ? NotFound() : Ok(ToResponse(@event));
    }

    [AuthorizePermission(PermissionNames.CreateResource)]
    [HttpPost]
    public async Task<ActionResult> Create([FromBody] CreateEventRequest req, CancellationToken ct)
    {
        var visibility = Enum.Parse<ResourceVisibility>(req.Visibility, ignoreCase: true);
        var authResult = RequireAuthenticatedUser(out var idUser);
        if (authResult is not null)
            return authResult;

        try
        {
            var idResource = await _service.CreateAsync(
                new CreateEventCommand(
                    req.Title,
                    req.Description,
                    visibility,
                    idUser,
                    req.IdCategory,
                    req.Subtitle,
                    req.StartDate,
                    req.EndDate,
                    req.Address,
                    req.IdDepartment
                    ),
                ct);

            return CreatedAtAction(nameof(GetByResourceId), new { idResource }, new { idResource });
        }
        catch (ValidationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [AuthorizePermission(PermissionNames.EditResource)]
    [HttpPatch("{idResource:int}")]
    public async Task<ActionResult<EventResponse>> Update(
        [FromRoute] int idResource,
        [FromBody] UpdateEventRequest req,
        CancellationToken ct)
    {
        var ownershipResult = await RequireResourceOwnerAsync(idResource, _service.GetByResourceIdAsync, ct);
        if (ownershipResult is not null)
            return ownershipResult;

        ResourceVisibility? visibility = req.Visibility is null
            ? null
            : Enum.Parse<ResourceVisibility>(req.Visibility, ignoreCase: true);

        try
        {
            var @event = await _service.UpdateAsync(
                new UpdateEventCommand(
                    idResource,
                    req.Title,
                    req.Description,
                    visibility,
                    req.IdCategory,
                    req.Subtitle,
                    req.StartDate,
                    req.EndDate,
                    req.Address,
                    req.IdDepartment),
                ct);

            return Ok(ToResponse(@event));
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (ValidationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [AuthorizePermission(PermissionNames.DeleteResource)]
    [HttpDelete("{idResource:int}")]
    public async Task<ActionResult> Delete([FromRoute] int idResource, CancellationToken ct)
    {
        var ownershipResult = await RequireResourceOwnerAsync(idResource, _service.GetByResourceIdAsync, ct);
        if (ownershipResult is not null)
            return ownershipResult;

        var deleted = await _service.SoftDeleteAsync(idResource, ct);
        return deleted ? NoContent() : NotFound();
    }

    [AuthorizePermission(PermissionNames.ApproveArticle)]
    [HttpPatch("{idResource:int}/approval")]
    public async Task<ActionResult<EventResponse>> SetApproval(
        [FromRoute] int idResource,
        [FromBody] SetResourceApprovalRequest req,
        CancellationToken ct)
    {
        try
        {
            var @event = await _service.SetApprovalAsync(
                new SetEventApprovalCommand(idResource, req.IsApproved),
                ct);

            return Ok(ToResponse(@event));
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (ValidationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    private static EventResponse ToResponse(Event @event)
    {
        return new EventResponse(
            @event.IdResource,
            @event.IdEvent,
            @event.Title,
            @event.Description,
            @event.Type.ToString().ToLowerInvariant(),
            @event.Visibility.ToString(),
            @event.CreatedAt,
            @event.ModifiedAt,
            @event.IdUser,
            @event.IdCategory,
            @event.Subtitle,
            @event.StartDate,
            @event.EndDate,
            @event.Address,
            @event.IdDepartment,
            @event.IsApproved
        );
    }
}
