using RESR.Models.Reactions;

namespace RESR.MAUI.Services;

public interface IReactionsApiClient
{
    Task<IReadOnlyList<ReactionResponse>> GetByResourceIdAsync(int idResource, CancellationToken ct);
    Task<ReactionResponse> CreateAsync(int idResource, CreateReactionRequest request, CancellationToken ct);
    Task<ReactionResponse> UpdateAsync(int idReaction, UpdateReactionRequest request, CancellationToken ct);
    Task DeleteAsync(int idReaction, CancellationToken ct);
}
