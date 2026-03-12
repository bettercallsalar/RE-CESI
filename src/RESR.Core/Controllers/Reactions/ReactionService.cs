using RESR.Core.Controllers.Reactions.Factories;
using RESR.Core.Controllers.Reactions.Ports;
using RESR.Core.Errors;
using RESR.Models.Reactions;

namespace RESR.Core.Controllers.Reactions;

public sealed class ReactionService : IReactionService
{
    private readonly IReactionRepository _repo;
    private readonly IReactionFactory _factory;

    public ReactionService(IReactionRepository repo, IReactionFactory factory)
    {
        _repo = repo;
        _factory = factory;
    }

    public async Task<IReadOnlyList<Reaction>> GetByResourceIdAsync(int idResource, CancellationToken ct)
    {
        if (idResource <= 0)
            throw new ValidationException("IdResource must be greater than 0");

        if (!await _repo.ResourceExistsAsync(idResource, ct))
            throw new NotFoundException($"Resource {idResource} not found");

        return await _repo.GetByResourceIdAsync(idResource, ct);
    }

    public async Task<IReadOnlyList<Reaction>> GetByUserIdAsync(int idUser, CancellationToken ct)
    {
        if (idUser <= 0)
            throw new ValidationException("IdUser must be greater than 0");

        if (!await _repo.UserExistsAsync(idUser, ct))
            throw new NotFoundException($"User {idUser} not found");

        return await _repo.GetByUserIdAsync(idUser, ct);
    }

    public async Task<Reaction?> GetByIdAsync(int idReaction, CancellationToken ct)
    {
        if (idReaction <= 0)
            throw new ValidationException("IdReaction must be greater than 0");

        return await _repo.GetByIdAsync(idReaction, ct);
    }

    public async Task<Reaction> CreateAsync(CreateReactionCommand cmd, CancellationToken ct)
    {
        if (cmd.IdResource <= 0)
            throw new ValidationException("IdResource must be greater than 0");

        if (cmd.IdUser <= 0)
            throw new ValidationException("IdUser must be greater than 0");

        if (!await _repo.ResourceExistsAsync(cmd.IdResource, ct))
            throw new NotFoundException($"Resource {cmd.IdResource} not found");

        if (await _repo.GetByResourceAndUserAsync(cmd.IdResource, cmd.IdUser, ct) is not null)
            throw new ConflictException("User already reacted to this resource");

        var reaction = _factory.CreateForCreation(
            NormalizeReactionName(cmd.Name),
            cmd.IdResource,
            cmd.IdUser
        );

        return await _repo.CreateAsync(reaction, ct);
    }

    public async Task<Reaction> UpdateAsync(UpdateReactionCommand cmd, CancellationToken ct)
    {
        if (cmd.IdReaction <= 0)
            throw new ValidationException("IdReaction must be greater than 0");

        if (cmd.ActorUserId <= 0)
            throw new ValidationException("ActorUserId must be greater than 0");

        var reaction = await _repo.GetByIdAsync(cmd.IdReaction, ct)
            ?? throw new NotFoundException($"Reaction {cmd.IdReaction} not found");

        if (reaction.IdUser != cmd.ActorUserId)
            throw new UnauthorizedAccessException("Only the reaction author can update this reaction");

        return await _repo.UpdateNameAsync(cmd.IdReaction, NormalizeReactionName(cmd.Name), ct);
    }

    public async Task DeleteAsync(int idReaction, int actorUserId, CancellationToken ct)
    {
        if (idReaction <= 0)
            throw new ValidationException("IdReaction must be greater than 0");

        if (actorUserId <= 0)
            throw new ValidationException("ActorUserId must be greater than 0");

        var reaction = await _repo.GetByIdAsync(idReaction, ct)
            ?? throw new NotFoundException($"Reaction {idReaction} not found");

        if (reaction.IdUser != actorUserId)
            throw new UnauthorizedAccessException("Only the reaction author can delete this reaction");

        if (!await _repo.DeleteAsync(idReaction, ct))
            throw new NotFoundException($"Reaction {idReaction} not found");
    }

    private static string NormalizeReactionName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ValidationException("Name is required");

        var normalized = name.Trim().ToLowerInvariant();
        if (!ReactionNames.All.Contains(normalized))
            throw new ValidationException("Name must be one of: like, dislike, love");

        return normalized;
    }
}
