using RESR.Models.Resources;

namespace RESR.Core.Controllers.Articles;

public interface IArticleService
{
    Task<IReadOnlyList<Article>> GetAllAsync(CancellationToken ct);
    Task<Article?> GetByResourceIdAsync(int idResource, CancellationToken ct);
    Task<int> CreateAsync(CreateArticleCommand cmd, CancellationToken ct);
    Task<Article> UpdateAsync(UpdateArticleCommand cmd, CancellationToken ct);
    Task<Article> SetApprovalAsync(SetArticleApprovalCommand cmd, CancellationToken ct);
    Task<bool> SoftDeleteAsync(int idResource, CancellationToken ct);
}
