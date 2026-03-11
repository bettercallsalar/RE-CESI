using RESR.Models.Resources;

namespace RESR.Core.Controllers.Events;

public interface IEventService
{
    Task<IReadOnlyList<Event>> GetAllAsync(CancellationToken ct);
    Task<Event?> GetByResourceIdAsync(int idResource, CancellationToken ct);
    Task<int> CreateAsync(CreateEventCommand cmd, CancellationToken ct);
    Task<Event> UpdateAsync(UpdateEventCommand cmd, CancellationToken ct);
    Task<bool> SoftDeleteAsync(int idResource, CancellationToken ct);
}
