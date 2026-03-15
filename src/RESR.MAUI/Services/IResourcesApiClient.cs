using RESR.Models.Resources;

namespace RESR.MAUI.Services;

public interface IResourcesApiClient
{
    Task<PaginatedArticlesResponse> GetArticlesAsync(int page, int pageSize, CancellationToken ct);
    Task<PaginatedArticlesResponse> GetArticlesAsync(int page, int pageSize, string? keyword, CancellationToken ct);
    Task<PaginatedArticlesResponse> GetArticlesByUserAsync(int idUser, int page, int pageSize, string? keyword, CancellationToken ct);
    Task<PaginatedArticlesResponse> GetMyArticlesAsync(int idUser, int page, int pageSize, string? keyword, CancellationToken ct);
    Task<ArticleResponse?> GetArticleByIdAsync(int idResource, CancellationToken ct);
    Task<ArticleResponse?> GetOwnArticleByIdAsync(int idResource, CancellationToken ct);
    Task<PaginatedEventsResponse> GetEventsAsync(int page, int pageSize, CancellationToken ct);
    Task<PaginatedEventsResponse> GetEventsAsync(int page, int pageSize, string? keyword, CancellationToken ct);
    Task<EventResponse?> GetEventByIdAsync(int idResource, CancellationToken ct);
}
