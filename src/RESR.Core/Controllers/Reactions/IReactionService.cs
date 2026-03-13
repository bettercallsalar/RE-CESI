using RESR.Models.Reactions;

namespace RESR.Core.Controllers.Reactions;

public interface IReactionService
{
    Task<IReadOnlyList<Reaction>> GetByResourceIdAsync(int idResource, CancellationToken ct);
    Task<IReadOnlyList<Reaction>> GetByUserIdAsync(int idUser, CancellationToken ct);
    Task<Reaction?> GetByIdAsync(int idReaction, CancellationToken ct);
    Task<Reaction> CreateAsync(CreateReactionCommand cmd, CancellationToken ct);
    Task<Reaction> UpdateAsync(UpdateReactionCommand cmd, CancellationToken ct);
    Task DeleteAsync(int idReaction, int actorUserId, CancellationToken ct);
}
