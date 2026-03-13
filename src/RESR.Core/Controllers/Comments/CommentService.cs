using RESR.Core.Controllers.Comments.Factories;
using RESR.Core.Controllers.Comments.Ports;
using RESR.Core.Errors;
using RESR.Models.Comments;

namespace RESR.Core.Controllers.Comments;

public sealed class CommentService : ICommentService
{
    private readonly ICommentRepository _repo;
    private readonly ICommentFactory _factory;

    public CommentService(ICommentRepository repo, ICommentFactory factory)
    {
        _repo = repo;
        _factory = factory;
    }

    public async Task<IReadOnlyList<Comment>> GetByResourceIdAsync(int idResource, CancellationToken ct)
    {
        if (idResource <= 0)
            throw new ValidationException("IdResource must be greater than 0");

        if (!await _repo.ResourceExistsAsync(idResource, ct))
            throw new NotFoundException($"Resource {idResource} not found");

        return await _repo.GetByResourceIdAsync(idResource, ct);
    }

    public async Task<Comment?> GetByIdAsync(int idComment, CancellationToken ct)
    {
        if (idComment <= 0)
            throw new ValidationException("IdComment must be greater than 0");

        return await _repo.GetByIdAsync(idComment, ct);
    }

    public async Task<Comment> CreateAsync(CreateCommentCommand cmd, CancellationToken ct)
    {
        if (cmd.IdResource <= 0)
            throw new ValidationException("IdResource must be greater than 0");

        if (cmd.IdUser <= 0)
            throw new ValidationException("IdUser must be greater than 0");

        if (!await _repo.ResourceExistsAsync(cmd.IdResource, ct))
            throw new NotFoundException($"Resource {cmd.IdResource} not found");

        var parentCommentId = NormalizeParentCommentId(cmd.IdParentComment);
        if (parentCommentId.HasValue)
        {
            var parentComment = await _repo.GetByIdAsync(parentCommentId.Value, ct)
                ?? throw new NotFoundException($"Comment {parentCommentId.Value} not found");

            if (parentComment.DeletedAt is not null)
                throw new ValidationException("Cannot reply to a deleted comment");

            if (parentComment.IdResource != cmd.IdResource)
                throw new ValidationException("Parent comment must belong to the same resource");
        }

        var comment = _factory.CreateForCreation(
            NormalizeContent(cmd.Content),
            cmd.IdResource,
            cmd.IdUser,
            parentCommentId
        );

        return await _repo.CreateAsync(comment, ct);
    }

    public async Task<Comment> UpdateAsync(UpdateCommentCommand cmd, CancellationToken ct)
    {
        if (cmd.IdComment <= 0)
            throw new ValidationException("IdComment must be greater than 0");

        if (cmd.ActorUserId <= 0)
            throw new ValidationException("ActorUserId must be greater than 0");

        var comment = await _repo.GetByIdAsync(cmd.IdComment, ct)
            ?? throw new NotFoundException($"Comment {cmd.IdComment} not found");

        if (comment.DeletedAt is not null)
            throw new ValidationException("Comment is deleted");

        if (comment.IdUser != cmd.ActorUserId)
            throw new UnauthorizedAccessException("Only the comment author can update this comment");

        return await _repo.UpdateContentAsync(cmd.IdComment, NormalizeContent(cmd.Content), ct);
    }

    public async Task DeleteAsync(int idComment, int actorUserId, bool canDeleteOtherUsersComments, CancellationToken ct)
    {
        if (idComment <= 0)
            throw new ValidationException("IdComment must be greater than 0");

        if (actorUserId <= 0)
            throw new ValidationException("ActorUserId must be greater than 0");

        var comment = await _repo.GetByIdAsync(idComment, ct)
            ?? throw new NotFoundException($"Comment {idComment} not found");

        if (comment.DeletedAt is not null)
            throw new ValidationException("Comment is already deleted");

        var canDelete =
            comment.IdUser == actorUserId ||
            canDeleteOtherUsersComments;

        if (!canDelete)
            throw new UnauthorizedAccessException("You are not allowed to delete this comment");

        if (!await _repo.SoftDeleteAsync(idComment, ct))
            throw new NotFoundException($"Comment {idComment} not found");
    }

    private static string NormalizeContent(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            throw new ValidationException("Content is required");

        return content.Trim();
    }

    private static int? NormalizeParentCommentId(int? idParentComment)
    {
        if (!idParentComment.HasValue)
            return null;

        if (idParentComment.Value <= 0)
            throw new ValidationException("IdParentComment must be greater than 0");

        return idParentComment.Value;
    }
}
