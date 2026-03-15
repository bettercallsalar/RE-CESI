using RESR.Models.Comments;

namespace RESR.MAUI.Services;

public interface ICommentsApiClient
{
    Task<IReadOnlyList<CommentResponse>> GetByResourceIdAsync(int idResource, CancellationToken ct);
    Task<CommentResponse> CreateAsync(int idResource, CreateCommentRequest request, CancellationToken ct);
}
