using RESR.Models.Marks;

namespace RESR.MAUI.Services;

public interface IMarksApiClient
{
    Task<PaginatedMarksResponse> GetFavoritesAsync(int page, int pageSize, CancellationToken ct);
    Task<PaginatedMarksResponse> GetReadLaterAsync(int page, int pageSize, CancellationToken ct);
    Task<MarkResponse?> GetFavoriteAsync(int idResource, CancellationToken ct);
    Task<MarkResponse?> GetReadLaterAsync(int idResource, CancellationToken ct);
    Task<MarkResponse> MarkAsFavoriteAsync(int idResource, CancellationToken ct);
    Task UnmarkAsFavoriteAsync(int idResource, CancellationToken ct);
    Task<MarkResponse> MarkAsReadLaterAsync(int idResource, CancellationToken ct);
    Task UnmarkAsReadLaterAsync(int idResource, CancellationToken ct);
}
