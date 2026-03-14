using Microsoft.AspNetCore.Mvc;
using RESR.Core.Controllers.Events;
using RESR.Core.Controllers.Resources;
using RESR.Core.Controllers.Users.Ports;
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
    private readonly IUserRepository _users;
    private const int MaxPageSize = 100;

    public EventsController(IEventService service, IUserRepository users, ITokenService tokenService)
        : base(tokenService)
    {
        _service = service;
        _users = users;
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
        var items = await ToResponsesAsync(events, ct);
        var totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling((double)totalCount / pageSize);

        return Ok(new PaginatedEventsResponse(items, page, pageSize, totalCount, totalPages));
    }

    [AuthorizePermissionOrSelf("idUser")]
    [HttpGet("{idUser:int}/my-events")]
    public async Task<ActionResult<PaginatedEventsResponse>> GetMyEvents(
        [FromRoute] int idUser,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? keyword = null,
        [FromQuery] ResourceVisibility? visibility = null,
        [FromQuery] int? idCategory = null,
        [FromQuery] int? idDepartment = null,
        [FromQuery] bool? isApproved = null,
        [FromQuery] DateTime? startFrom = null,
        [FromQuery] DateTime? startTo = null,
        CancellationToken ct = default)
    {
        if (page <= 0 || pageSize <= 0 || idCategory is <= 0 || idDepartment is <= 0)
            return BadRequest(new { message = "Page, PageSize, IdCategory and IdDepartment must be greater than 0." });
        if (pageSize > MaxPageSize) return BadRequest(new { message = $"PageSize cannot be greater than {MaxPageSize}." });

        var filters = new EventListingFilters(
            Keyword: keyword,
            Visibility: visibility,
            IdUser: idUser,
            IdCategory: idCategory,
            IdDepartment: idDepartment,
            IsApproved: isApproved,
            StartFrom: startFrom,
            StartTo: startTo,
            IncludeDeleted: true
        );

        var (events, totalCount) = await _service.GetPaginatedAsync(page, pageSize, filters, ct);
        var items = await ToResponsesAsync(events, ct);
        var totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling((double)totalCount / pageSize);

        return Ok(new PaginatedEventsResponse(items, page, pageSize, totalCount, totalPages));
    }

    [HttpGet("{idResource:int}")]
    public async Task<ActionResult<EventResponse>> GetByResourceId([FromRoute] int idResource, CancellationToken ct)
    {
        var @event = await _service.GetByResourceIdAsync(idResource, ct);

        if (@event is null || @event.DeletedAt is not null || @event.Visibility != ResourceVisibility.PUBLIC || !@event.IsApproved)
            return NotFound();

        return Ok(await ToResponseAsync(@event, ct));
    }

    [HttpGet("me/{idResource:int}")]
    public async Task<ActionResult<EventResponse>> GetOwnByResourceId([FromRoute] int idResource, CancellationToken ct)
    {
        var (authResult, idUser) = await RequireAuthenticatedUserAsync(ct);
        if (authResult is not null)
            return authResult;

        var @event = await _service.GetByResourceIdAsync(idResource, ct);
        if (@event is null)
            return NotFound();
        if (@event.IdUser != idUser)
            return Forbid();

        return Ok(await ToResponseAsync(@event, ct));
    }

    [AuthorizePermission(PermissionNames.ApproveEvent)]
    [HttpGet("approval/pending")]
    public async Task<ActionResult<PaginatedEventsResponse>> GetPendingApprovalEvents(
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
            Visibility: null,
            IdUser: idUser,
            IdCategory: idCategory,
            IdDepartment: idDepartment,
            IsApproved: false,
            StartFrom: startFrom,
            StartTo: startTo,
            IncludeDeleted: false
        );

        var (events, totalCount) = await _service.GetPaginatedAsync(page, pageSize, filters, ct);
        var items = await ToResponsesAsync(events, ct);
        var totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling((double)totalCount / pageSize);

        return Ok(new PaginatedEventsResponse(items, page, pageSize, totalCount, totalPages));
    }

    [AuthorizePermission(PermissionNames.ApproveEvent)]
    [HttpGet("approval/{idResource:int}")]
    public async Task<ActionResult<EventResponse>> GetByResourceIdForApproval([FromRoute] int idResource, CancellationToken ct)
    {
        var @event = await _service.GetByResourceIdAsync(idResource, ct);

        if (@event is null || @event.DeletedAt is not null || @event.IsApproved)
            return NotFound();

        return Ok(await ToResponseAsync(@event, ct));
    }

    [AuthorizePermission(PermissionNames.ModerateContent)]
    [HttpGet("moderation/{idResource:int}")]
    public async Task<ActionResult<EventResponse>> GetByResourceIdForModeration([FromRoute] int idResource, CancellationToken ct)
    {
        var @event = await _service.GetByResourceIdAsync(idResource, ct);
        return @event is null ? NotFound() : Ok(await ToResponseAsync(@event, ct));
    }

    [AuthorizePermission(PermissionNames.CreateResource)]
    [HttpPost]
    public async Task<ActionResult> Create([FromForm] CreateEventFormRequest req, CancellationToken ct)
    {
        var visibility = Enum.Parse<ResourceVisibility>(req.Visibility, ignoreCase: true);
        var (authResult, idUser) = await RequireAuthenticatedUserAsync(ct);
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
                    req.IdDepartment,
                    await ToUploadsAsync(req.Images, ct),
                    req.DefaultImageIndex
                    ),
                ct);

            return CreatedAtAction(nameof(GetOwnByResourceId), new { idResource }, new { idResource });
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
        [FromForm] UpdateEventFormRequest req,
        CancellationToken ct)
    {
        var (authResult, idUser) = await RequireAuthenticatedUserAsync(ct);
        if (authResult is not null)
            return authResult;

        ResourceVisibility? visibility = req.Visibility is null
            ? null
            : Enum.Parse<ResourceVisibility>(req.Visibility, ignoreCase: true);

        try
        {
            var @event = await _service.UpdateAsync(
                new UpdateEventCommand(
                    idResource,
                    idUser,
                    req.Title,
                    req.Description,
                    visibility,
                    req.IdCategory,
                    req.Subtitle,
                    req.StartDate,
                    req.EndDate,
                    req.Address,
                    req.IdDepartment,
                    await ToUploadsAsync(req.Images, ct),
                    req.ReplaceImages,
                    req.DefaultImageId,
                    req.DefaultImageIndex),
                ct);

            return Ok(await ToResponseAsync(@event, ct));
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (ForbiddenException)
        {
            return Forbid();
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
        var (authResult, idUser) = await RequireAuthenticatedUserAsync(ct);
        if (authResult is not null)
            return authResult;

        try
        {
            var deleted = await _service.SoftDeleteAsync(idResource, idUser, ct);
            return deleted ? NoContent() : NotFound();
        }
        catch (ForbiddenException)
        {
            return Forbid();
        }
    }

    [AuthorizePermission(PermissionNames.ApproveEvent)]
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

            return Ok(await ToResponseAsync(@event, ct));
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

    private async Task<List<EventResponse>> ToResponsesAsync(IEnumerable<Event> events, CancellationToken ct)
    {
        var eventList = events.ToList();
        var authorMap = await BuildAuthorMapAsync(eventList.Select(@event => @event.IdUser), ct);

        return eventList.Select(@event => ToResponse(@event, authorMap)).ToList();
    }

    private async Task<EventResponse> ToResponseAsync(Event @event, CancellationToken ct)
    {
        var authorMap = await BuildAuthorMapAsync([@event.IdUser], ct);
        return ToResponse(@event, authorMap);
    }

    private async Task<Dictionary<int, ResourceAuthorResponse>> BuildAuthorMapAsync(IEnumerable<int> userIds, CancellationToken ct)
    {
        var ids = userIds.Distinct().ToList();
        var users = await Task.WhenAll(ids.Select(id => _users.GetByIdAsync(id, ct)));

        return users
            .Where(user => user is not null)
            .ToDictionary(
                user => user!.IdUser,
                user => new ResourceAuthorResponse(user!.IdUser, user.Username, user.FirstName));
    }

    private static EventResponse ToResponse(Event @event, IReadOnlyDictionary<int, ResourceAuthorResponse> authorMap)
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
            authorMap.TryGetValue(@event.IdUser, out var author)
                ? author
                : new ResourceAuthorResponse(@event.IdUser, string.Empty, string.Empty),
            @event.IdCategory,
            @event.Subtitle,
            @event.StartDate,
            @event.EndDate,
            @event.Address,
            @event.Department,
            @event.IsApproved,
            @event.Files.Select(ToFileResponse).ToList(),
            @event.DeletedAt,
            @event.DefaultImageId
        );
    }

    private static ResourceFileResponse ToFileResponse(ResourceFile file) =>
        new(file.IdFile, file.FileName, file.OriginalName, file.MimeType, file.Size, file.Path, file.CreatedAt);

    private static async Task<IReadOnlyList<ResourceFileUpload>> ToUploadsAsync(IReadOnlyList<IFormFile>? files, CancellationToken ct)
    {
        if (files is null || files.Count == 0)
            return Array.Empty<ResourceFileUpload>();

        var uploads = new List<ResourceFileUpload>(files.Count);

        foreach (var file in files.Where(file => file.Length > 0))
        {
            await using var stream = file.OpenReadStream();
            using var memory = new MemoryStream();
            await stream.CopyToAsync(memory, ct);

            uploads.Add(new ResourceFileUpload(
                file.FileName,
                file.ContentType,
                Convert.ToInt32(file.Length),
                memory.ToArray()));
        }

        return uploads;
    }
}
