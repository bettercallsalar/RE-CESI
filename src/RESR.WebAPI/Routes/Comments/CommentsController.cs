using Microsoft.AspNetCore.Mvc;
using RESR.Core.Controllers.Comments;
using RESR.Core.Errors;
using RESR.Core.Security.Token;
using RESR.Models.Comments;
using RESR.WebAPI.Security;

namespace RESR.WebAPI.Routes.Comments;

[ApiController]
[Route("api/comments")]
public sealed class CommentsController : AuthenticatedResourceControllerBase
{
    private readonly ICommentService _service;

    public CommentsController(ICommentService service, ITokenService tokenService)
        : base(tokenService)
    {
        _service = service;
    }

    [HttpGet("resources/{idResource:int}")]
    public async Task<ActionResult<IReadOnlyList<CommentResponse>>> GetByResourceId([FromRoute] int idResource, CancellationToken ct)
    {
        try
        {
            var comments = await _service.GetByResourceIdAsync(idResource, ct);
            return Ok(comments.Select(ToResponse).ToList());
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

    [HttpGet("{idComment:int}")]
    public async Task<ActionResult<CommentResponse>> GetById([FromRoute] int idComment, CancellationToken ct)
    {
        try
        {
            var comment = await _service.GetByIdAsync(idComment, ct);
            return comment is null ? NotFound() : Ok(ToResponse(comment));
        }
        catch (ValidationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [AuthorizePermission]
    [HttpPost("resources/{idResource:int}")]
    public async Task<ActionResult<CommentResponse>> Create([FromRoute] int idResource, [FromBody] CreateCommentRequest req, CancellationToken ct)
    {
        var authResult = RequireAuthenticatedUser(out var currentUserId);
        if (authResult is not null)
            return authResult;

        try
        {
            var comment = await _service.CreateAsync(
                new CreateCommentCommand(idResource, req.Content, currentUserId, req.IdParentComment),
                ct
            );

            return CreatedAtAction(nameof(GetById), new { idComment = comment.IdComment }, ToResponse(comment));
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

    [AuthorizePermission]
    [HttpPatch("{idComment:int}")]
    public async Task<ActionResult<CommentResponse>> Update([FromRoute] int idComment, [FromBody] UpdateCommentRequest req, CancellationToken ct)
    {
        var authResult = RequireAuthenticatedUser(out var currentUserId);
        if (authResult is not null)
            return authResult;

        try
        {
            var comment = await _service.UpdateAsync(
                new UpdateCommentCommand(idComment, req.Content, currentUserId),
                ct
            );

            return Ok(ToResponse(comment));
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

    [AuthorizeCommentOwnerOrPermission("idComment", PermissionNames.DeleteComment)]
    [HttpDelete("{idComment:int}")]
    public async Task<ActionResult> Delete([FromRoute] int idComment, CancellationToken ct)
    {
        var authResult = RequireAuthenticatedUser(out var currentUserId);
        if (authResult is not null)
            return authResult;

        try
        {
            var canDeleteOtherUsersComments =
                HttpContext.Items.TryGetValue(CommentOwnerOrPermissionAuthorizationFilter.CanDeleteOtherUsersCommentsItemKey, out var value) &&
                value is true;

            await _service.DeleteAsync(idComment, currentUserId, canDeleteOtherUsersComments, ct);
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

    private static CommentResponse ToResponse(Comment comment) =>
        new(
            comment.IdComment,
            comment.Content,
            comment.CreatedAt,
            comment.ModifiedAt,
            comment.DeletedAt,
            comment.IdResource,
            comment.IdUser,
            comment.IdParentComment
        );
}
