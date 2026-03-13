using RESR.Models.Resources;

namespace RESR.MAUI.Services;

public interface IResourcesApiClient
{
    Task<PaginatedArticlesResponse> GetArticlesAsync(int page, int pageSize, CancellationToken ct);
    Task<PaginatedEventsResponse> GetEventsAsync(int page, int pageSize, CancellationToken ct);
}
