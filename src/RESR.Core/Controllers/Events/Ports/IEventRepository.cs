using RESR.Models.Resources;

namespace RESR.Core.Controllers.Events.Ports;

public interface IEventRepository
{
    Task<IReadOnlyList<Event>> GetPaginatedAsync(int page, int pageSize, EventListingFilters filters, CancellationToken ct);
    Task<int> CountAsync(EventListingFilters filters, CancellationToken ct);
    Task<Event?> GetByResourceIdAsync(int idResource, CancellationToken ct);
    Task<int> CreateAsync(CreateEventCommand cmd, CancellationToken ct);
    Task<Event?> PatchAsync(UpdateEventCommand cmd, CancellationToken ct);
    Task SetDefaultImageAsync(int idResource, int? defaultImageId, CancellationToken ct);
    Task<Event?> SetApprovalAsync(SetEventApprovalCommand cmd, CancellationToken ct);
    Task<bool> SoftDeleteAsync(int idResource, CancellationToken ct);
}
