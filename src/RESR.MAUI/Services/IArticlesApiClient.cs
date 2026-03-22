using RESR.Models.Resources;

namespace RESR.MAUI.Services;

public interface IArticlesApiClient
{
    Task<ArticleResponse> GetByIdAsync(int idResource, CancellationToken ct);
    Task<ArticleResponse> GetOwnByIdAsync(int idResource, CancellationToken ct);
    Task CreateAsync(
        CreateArticleRequest request,
        IReadOnlyList<SelectedImageUpload> images,
        int? defaultImageIndex,
        CancellationToken ct);
    Task UpdateAsync(
        int idResource,
        UpdateArticleRequest request,
        IReadOnlyList<SelectedImageUpload> images,
        int? defaultImageIndex,
        CancellationToken ct);
}
