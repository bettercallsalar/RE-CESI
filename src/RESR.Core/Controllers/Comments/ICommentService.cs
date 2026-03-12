using RESR.Models.Comments;

namespace RESR.Core.Controllers.Comments;

public interface ICommentService
{
    Task<IReadOnlyList<Comment>> GetByResourceIdAsync(int idResource, CancellationToken ct);
    Task<Comment?> GetByIdAsync(int idComment, CancellationToken ct);
    Task<Comment> CreateAsync(CreateCommentCommand cmd, CancellationToken ct);
    Task<Comment> UpdateAsync(UpdateCommentCommand cmd, CancellationToken ct);
    Task DeleteAsync(int idComment, int actorUserId, IReadOnlySet<string> actorPermissions, CancellationToken ct);
}
