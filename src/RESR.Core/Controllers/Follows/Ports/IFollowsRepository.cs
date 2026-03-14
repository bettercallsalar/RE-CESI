using RESR.Models.Follows;

namespace RESR.Core.Controllers.Follows.Ports;

public interface IFollowsRepository
{
    Task<IReadOnlyList<FollowUser>> GetAllFollowersAsync(int idUser, CancellationToken ct);
    Task<IReadOnlyList<FollowUser>> GetAllFollowingAsync(int idUser, CancellationToken ct);
    Task<bool> CreateAsync(int idFollower, int idFollowing, CancellationToken ct);
    Task<bool> DeleteAsync(int idFollower, int idFollowing, CancellationToken ct);
}
