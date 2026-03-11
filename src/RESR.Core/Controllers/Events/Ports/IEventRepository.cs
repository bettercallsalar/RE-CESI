using RESR.Models.Resources;

namespace RESR.Core.Controllers.Events.Ports;

public interface IEventRepository
{
    Task<IReadOnlyList<Event>> GetAllAsync(CancellationToken ct);
    Task<Event?> GetByResourceIdAsync(int idResource, CancellationToken ct);
    Task<int> CreateAsync(CreateEventCommand cmd, CancellationToken ct);
    Task<Event?> PatchAsync(UpdateEventCommand cmd, CancellationToken ct);
    Task<bool> SoftDeleteAsync(int idResource, CancellationToken ct);
}
