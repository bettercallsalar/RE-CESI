using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RESR.Core.Controllers.Follows;
using RESR.Core.Errors;
using RESR.Models.Follows;
using RESR.WebAPI.Security;

namespace RESR.WebAPI.Routes.Follows;

[ApiController]
[Route("api/follows")]
public sealed class FollowsController : ControllerBase
{
    private readonly IFollowsService _service;

    public FollowsController(IFollowsService service) => _service = service;

    [HttpGet("{idUser:int}/followers")]
    public async Task<ActionResult<PaginatedFollowUsersResponse>> GetAllFollowers(
        [FromRoute] int idUser,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        if (page <= 0 || pageSize <= 0)
            return BadRequest(new { message = "Page number and page size must be greater than 0" });

        var follows = await _service.GetAllFollowersAsync(idUser, ct);
        var totalCount = follows.Count;
        var items = follows.Select(ToUserResponse).Skip((page - 1) * pageSize).Take(pageSize).ToList();
        var totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling((double)totalCount / pageSize);

        return Ok(new PaginatedFollowUsersResponse(items, page, pageSize, totalCount, totalPages));
    }

    [HttpGet("{idFollower:int}/following")]
    public async Task<ActionResult<PaginatedFollowUsersResponse>> GetAllFollowing(
        [FromRoute] int idFollower,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        if (page <= 0 || pageSize <= 0)
            return BadRequest(new { message = "Page number and page size must be greater than 0" });

        var follows = await _service.GetAllFollowingAsync(idFollower, ct);
        var totalCount = follows.Count;
        var items = follows.Select(ToUserResponse).Skip((page - 1) * pageSize).Take(pageSize).ToList();
        var totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling((double)totalCount / pageSize);

        return Ok(new PaginatedFollowUsersResponse(items, page, pageSize, totalCount, totalPages));
    }

    [HttpPost]
    [AuthorizePermission(PermissionNames.FollowUser)]
    public async Task<ActionResult> Create([FromBody] FollowRequest request, CancellationToken ct)
    {
        try
        {
            await _service.CreateAsync(request.IdFollower, request.IdFollowing, ct);
            return NoContent();
        }
        catch (ConflictException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (ValidationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{idFollower:int}/{idFollowing:int}")]
    [AuthorizePermission(PermissionNames.FollowUser)]
    public async Task<ActionResult> Delete([FromRoute] int idFollower, [FromRoute] int idFollowing, CancellationToken ct)
    {
        try
        {
            await _service.DeleteAsync(idFollower, idFollowing, ct);
            return NoContent();
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    private static FollowUserResponse ToUserResponse(FollowUser user) =>
        new(user.IdUser, user.Username, user.FirstName);
}
