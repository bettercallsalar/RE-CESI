using RESR.Models.Resources;

namespace RESR.MAUI.Services;

public interface IArticlesApiClient
{
    Task CreateAsync(CreateArticleRequest request, CancellationToken ct);
}
