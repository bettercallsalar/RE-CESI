using RESR.Models.Reactions;

namespace RESR.Core.Controllers.Reactions.Ports;

public interface IReactionRepository
{
    Task<bool> ResourceExistsAsync(int idResource, CancellationToken ct);
    Task<bool> UserExistsAsync(int idUser, CancellationToken ct);
    Task<IReadOnlyList<Reaction>> GetByResourceIdAsync(int idResource, CancellationToken ct);
    Task<IReadOnlyList<Reaction>> GetByUserIdAsync(int idUser, CancellationToken ct);
    Task<Reaction?> GetByIdAsync(int idReaction, CancellationToken ct);
    Task<Reaction?> GetByResourceAndUserAsync(int idResource, int idUser, CancellationToken ct);
    Task<Reaction> CreateAsync(Reaction reaction, CancellationToken ct);
    Task<Reaction> UpdateNameAsync(int idReaction, string name, CancellationToken ct);
    Task<bool> DeleteAsync(int idReaction, CancellationToken ct);
}
