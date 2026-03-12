using RESR.Core.Controllers.Follows.Ports;
using RESR.Core.Errors;
using RESR.Models.Follows;

namespace RESR.Core.Controllers.Follows;

public sealed class FollowsService : IFollowsService
{
    private readonly IFollowsRepository _repo;

    public FollowsService(IFollowsRepository repo) => _repo = repo;

    public Task<IReadOnlyList<FollowUser>> GetAllFollowersAsync(int idUser, CancellationToken ct) =>
        _repo.GetAllFollowersAsync(idUser, ct);

    public Task<IReadOnlyList<FollowUser>> GetAllFollowingAsync(int idUser, CancellationToken ct) =>
        _repo.GetAllFollowingAsync(idUser, ct);

    public async Task CreateAsync(int idFollower, int idFollowing, CancellationToken ct)
    {
        if (idFollower == idFollowing)
            throw new ValidationException("A user cannot follow themselves");

        var created = await _repo.CreateAsync(idFollower, idFollowing, ct);
        if (!created)
            throw new ConflictException($"Follow {idFollower}->{idFollowing} already exists");
    }

    public async Task DeleteAsync(int idFollower, int idFollowing, CancellationToken ct)
    {
        var deleted = await _repo.DeleteAsync(idFollower, idFollowing, ct);
        if (!deleted)
            throw new NotFoundException($"Follow {idFollower}->{idFollowing} not found");
    }
}
