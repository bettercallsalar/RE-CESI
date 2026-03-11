using Microsoft.AspNetCore.Mvc;
using RESR.Core.Controllers.Events;
using RESR.Core.Errors;
using RESR.Models.Resources;
using RESR.WebAPI.Security;

namespace RESR.WebAPI.Routes.Events;

[ApiController]
[Route("api/events")]
public sealed class EventsController : ControllerBase
{
    private readonly IEventService _service;

    public EventsController(IEventService service) => _service = service;

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<EventResponse>>> GetAll(CancellationToken ct)
    {
        var events = await _service.GetAllAsync(ct);
        return Ok(events.Select(ToResponse).ToList());
    }

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

        try
        {
            var idResource = await _service.CreateAsync(
                new CreateEventCommand(
                    req.Title,
                    req.Description,
                    visibility,
                    req.IdUser,
                    req.IdCategory,
                    req.Subtitle,
                    req.StartDate,
                    req.EndDate,
                    req.Address,
                    req.IdDepartment),
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
        var deleted = await _service.SoftDeleteAsync(idResource, ct);
        return deleted ? NoContent() : NotFound();
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
            @event.IdDepartment
        );
    }
}
