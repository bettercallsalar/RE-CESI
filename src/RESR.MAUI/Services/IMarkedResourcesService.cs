namespace RESR.MAUI.Services;

public interface IMarkedResourcesService
{
    Task<IReadOnlyList<MarkedResourceItem>> GetFavoritesAsync(CancellationToken ct);
    Task<IReadOnlyList<MarkedResourceItem>> GetReadLaterAsync(CancellationToken ct);
}
