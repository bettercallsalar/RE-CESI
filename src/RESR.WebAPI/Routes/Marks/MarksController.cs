using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using RESR.Core.Controllers.Marks;
using RESR.Core.Errors;
using RESR.Models.Marks;
using RESR.WebAPI.Security;

namespace RESR.WebAPI.Routes.Marks;

[ApiController]
[Route("api/marks")]
public sealed class MarksController : ControllerBase
{
    private readonly IMarkService _service;
    private const string UnauthorizedMessage = "Invalid token or unauthorized access.";

    public MarksController(IMarkService service) => _service = service;

    /** mark ressource as favorite */
    [AuthorizePermission]
    [HttpPost("favorite/{idResource:int}")]
    public async Task<ActionResult<MarkResponse>> MarkAsFavorite([FromRoute] int idResource, CancellationToken ct)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId is null)
            return Unauthorized(new { message = UnauthorizedMessage });

        try
        {
            var mark = await _service.MarkAsFavoriteAsync(idResource, currentUserId.Value, ct);
            return Ok(ToResponse(mark));
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

    // remove resource from favorite
    [AuthorizePermission]
    [HttpDelete("favorite/{idResource:int}")]
    public async Task<IActionResult> UnmarkAsFavorite([FromRoute] int idResource, CancellationToken ct)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId is null)
            return Unauthorized(new { message = UnauthorizedMessage });

        try
        {
            await _service.UnmarkAsFavoriteAsync(idResource, currentUserId.Value, ct);
            return NoContent();
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

    // mark ressource as read later
    [AuthorizePermission]
    [HttpPost("readLater/{idResource:int}")]
    public async Task<ActionResult<MarkResponse>> MarkAsReadLater([FromRoute] int idResource, CancellationToken ct)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId is null)
            return Unauthorized(new { message = UnauthorizedMessage });

        try
        {
            var mark = await _service.MarkAsReadLaterAsync(idResource, currentUserId.Value, ct);
            return Ok(ToResponse(mark));
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

    // remove ressource from read later
    [AuthorizePermission]
    [HttpDelete("readLater/{idResource:int}")]
    public async Task<IActionResult> UnmarkAsReadLater([FromRoute] int idResource, CancellationToken ct)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId is null)
            return Unauthorized(new { message = UnauthorizedMessage });

        try
        {
            await _service.UnmarkAsReadLaterAsync(idResource, currentUserId.Value, ct);
            return NoContent();
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

    // get all favorite ressources from user
    [AuthorizePermission]
    [HttpGet("favorite")]
    public async Task<ActionResult<PaginatedMarksResponse>> GetFavoriteRessources(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId is null)
            return Unauthorized(new { message = UnauthorizedMessage });

        if (page <= 0 || pageSize <= 0)
            return BadRequest(new { message = "Page number and page size must be greater than 0" });

        try
        {
            var marks = await _service.GetFavoriteRessourcesAsync(currentUserId.Value, ct);
            var totalCount = marks.Count;
            var items = marks.Select(ToResponse).Skip((page - 1) * pageSize).Take(pageSize).ToList();
            var totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling((double)totalCount / pageSize);

            return Ok(new PaginatedMarksResponse(items, page, pageSize, totalCount, totalPages));
        }
        catch (ValidationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // get all read later ressources from user
    [AuthorizePermission]
    [HttpGet("readLater")]
    public async Task<ActionResult<PaginatedMarksResponse>> GetReadLaterRessources(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId is null)
            return Unauthorized(new { message = UnauthorizedMessage });

        if (page <= 0 || pageSize <= 0)
            return BadRequest(new { message = "Page number and page size must be greater than 0" });

        try
        {
            var marks = await _service.GetReadLaterRessourcesAsync(currentUserId.Value, ct);
            var totalCount = marks.Count;
            var items = marks.Select(ToResponse).Skip((page - 1) * pageSize).Take(pageSize).ToList();
            var totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling((double)totalCount / pageSize);

            return Ok(new PaginatedMarksResponse(items, page, pageSize, totalCount, totalPages));
        }
        catch (ValidationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // get favorites by ressource
    [AuthorizePermission]
    [HttpGet("favorite/{idResource:int}")]
    public async Task<ActionResult<MarkResponse>> GetFavoriteRessource([FromRoute] int idResource, CancellationToken ct)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId is null)
            return Unauthorized(new { message = UnauthorizedMessage });

        try
        {
            var mark = await _service.GetFavoriteRessourceAsync(idResource, currentUserId.Value, ct);
            return mark is null
                ? NotFound(new { message = $"Favorite mark for resource {idResource} not found" })
                : Ok(ToResponse(mark));
        }
        catch (ValidationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // get read later by ressource
    [AuthorizePermission]
    [HttpGet("readLater/{idResource:int}")]
    public async Task<ActionResult<MarkResponse>> GetReadLaterRessource([FromRoute] int idResource, CancellationToken ct)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId is null)
            return Unauthorized(new { message = UnauthorizedMessage });

        try
        {
            var mark = await _service.GetReadLaterRessourceAsync(idResource, currentUserId.Value, ct);
            return mark is null
                ? NotFound(new { message = $"Read later mark for resource {idResource} not found" })
                : Ok(ToResponse(mark));
        }
        catch (ValidationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    private int? GetCurrentUserId()
    {
        var subject = User?.Claims?.FirstOrDefault(c =>
            c.Type == JwtRegisteredClaimNames.Sub ||
            c.Type == ClaimTypes.NameIdentifier
        )?.Value;

        return int.TryParse(subject, out var idUser) ? idUser : null;
    }

    public static MarkResponse ToResponse(Mark mark) =>
    new(
        mark.IdMark,
        mark.IsFavorite,
        mark.IsReadLater,
        mark.IdRessource,
        mark.IdUser
    );
}
