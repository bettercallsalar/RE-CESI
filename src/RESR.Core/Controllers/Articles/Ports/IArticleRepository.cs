using RESR.Models.Resources;

namespace RESR.Core.Controllers.Articles.Ports;

public interface IArticleRepository
{
    Task<IReadOnlyList<Article>> GetPaginatedAsync(int page, int pageSize, ArticleListingFilters filters, CancellationToken ct);
    Task<int> CountAsync(ArticleListingFilters filters, CancellationToken ct);
    Task<Article?> GetByResourceIdAsync(int idResource, CancellationToken ct);
    Task<int> CreateAsync(CreateArticleCommand cmd, CancellationToken ct);
    Task<Article?> PatchAsync(UpdateArticleCommand cmd, CancellationToken ct);
    Task<Article?> SetApprovalAsync(SetArticleApprovalCommand cmd, CancellationToken ct);
    Task<bool> SoftDeleteAsync(int idResource, CancellationToken ct);
}
