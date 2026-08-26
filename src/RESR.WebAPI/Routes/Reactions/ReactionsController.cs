using Microsoft.AspNetCore.Mvc;
using RESR.Core.Controllers.Reactions;
using RESR.Core.Errors;
using RESR.Models.Reactions;
using RESR.WebAPI.Security;

namespace RESR.WebAPI.Routes.Reactions;

[ApiController]
[Route("api/reactions")]
public sealed class ReactionsController : ControllerBase
{
    private readonly IReactionService _service;

    public ReactionsController(IReactionService service) => _service = service;

    [HttpGet("resources/{idResource:int}")]
    public async Task<ActionResult<IReadOnlyList<ReactionResponse>>> GetByResourceId([FromRoute] int idResource, CancellationToken ct)
    {
        try
        {
            var reactions = await _service.GetByResourceIdAsync(idResource, ct);
            return Ok(reactions.Select(ToResponse).ToList());
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

    [AuthorizePermissionOrQuerySelf("idUser", PermissionNames.ViewOtherUserReactions)]
    [HttpGet("user")]
    public async Task<ActionResult<UserReactionsResponse>> GetByUser([FromQuery] int? idUser, CancellationToken ct)
    {
        var targetUserId = idUser;
        if (!targetUserId.HasValue)
        {
            if (!User.TryGetCurrentUserId(out var currentUserId))
                return Unauthorized(new { message = "Invalid token or unauthorized access." });

            targetUserId = currentUserId;
        }

        try
        {
            var reactions = await _service.GetByUserIdAsync(targetUserId.Value, ct);
            var items = reactions.Select(ToResponse).ToList();
            return Ok(new UserReactionsResponse(targetUserId.Value, items.Count, items));
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

    [HttpGet("{idReaction:int}")]
    public async Task<ActionResult<ReactionResponse>> GetById([FromRoute] int idReaction, CancellationToken ct)
    {
        try
        {
            var reaction = await _service.GetByIdAsync(idReaction, ct);
            return reaction is null ? NotFound() : Ok(ToResponse(reaction));
        }
        catch (ValidationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [AuthorizePermission]
    [HttpPost("resources/{idResource:int}")]
    public async Task<ActionResult<ReactionResponse>> Create([FromRoute] int idResource, [FromBody] CreateReactionRequest req, CancellationToken ct)
    {
        if (!User.TryGetCurrentUserId(out var currentUserId))
            return Unauthorized(new { message = "Invalid token or unauthorized access." });

        try
        {
            var reaction = await _service.CreateAsync(
                new CreateReactionCommand(idResource, req.Name, currentUserId),
                ct
            );

            return CreatedAtAction(nameof(GetById), new { idReaction = reaction.IdReaction }, ToResponse(reaction));
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (ValidationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (ConflictException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [AuthorizePermission]
    [HttpPatch("{idReaction:int}")]
    public async Task<ActionResult<ReactionResponse>> Update([FromRoute] int idReaction, [FromBody] UpdateReactionRequest req, CancellationToken ct)
    {
        if (!User.TryGetCurrentUserId(out var currentUserId))
            return Unauthorized(new { message = "Invalid token or unauthorized access." });

        try
        {
            var reaction = await _service.UpdateAsync(
                new UpdateReactionCommand(idReaction, req.Name, currentUserId),
                ct
            );

            return Ok(ToResponse(reaction));
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (ValidationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    [AuthorizePermission]
    [HttpDelete("{idReaction:int}")]
    public async Task<ActionResult> Delete([FromRoute] int idReaction, CancellationToken ct)
    {
        if (!User.TryGetCurrentUserId(out var currentUserId))
            return Unauthorized(new { message = "Invalid token or unauthorized access." });

        try
        {
            await _service.DeleteAsync(idReaction, currentUserId, ct);
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
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    private static ReactionResponse ToResponse(Reaction reaction) =>
        new(
            reaction.IdReaction,
            reaction.Name,
            reaction.IdResource,
            reaction.IdUser,
            new ReactionUserResponse(reaction.IdUser, reaction.Username, reaction.FirstName)
        );
}
