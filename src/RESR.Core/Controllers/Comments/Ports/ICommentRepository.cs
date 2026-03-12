using RESR.Models.Comments;

namespace RESR.Core.Controllers.Comments.Ports;

public interface ICommentRepository
{
    Task<bool> ResourceExistsAsync(int idResource, CancellationToken ct);
    Task<IReadOnlyList<Comment>> GetByResourceIdAsync(int idResource, CancellationToken ct);
    Task<Comment?> GetByIdAsync(int idComment, CancellationToken ct);
    Task<Comment> CreateAsync(Comment comment, CancellationToken ct);
    Task<Comment> UpdateContentAsync(int idComment, string content, CancellationToken ct);
    Task<bool> SoftDeleteAsync(int idComment, CancellationToken ct);
}
