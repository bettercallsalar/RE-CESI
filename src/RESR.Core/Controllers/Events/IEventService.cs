using RESR.Models.Resources;

namespace RESR.Core.Controllers.Events;

public interface IEventService
{
    Task<(IReadOnlyList<Event> Events, int TotalCount)> GetPaginatedAsync(int page, int pageSize, EventListingFilters filters, CancellationToken ct);
    Task<Event?> GetByResourceIdAsync(int idResource, CancellationToken ct);
    Task<int> CreateAsync(CreateEventCommand cmd, CancellationToken ct);
    Task<Event> UpdateAsync(UpdateEventCommand cmd, CancellationToken ct);
    Task<Event> SetApprovalAsync(SetEventApprovalCommand cmd, CancellationToken ct);
    Task<bool> SoftDeleteAsync(int idResource, int idUser, CancellationToken ct);
}
