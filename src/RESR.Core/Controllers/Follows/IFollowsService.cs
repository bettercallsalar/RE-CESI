using RESR.Models.Follows;

namespace RESR.Core.Controllers.Follows;

public interface IFollowsService
{
    Task<IReadOnlyList<FollowUser>> GetAllFollowersAsync(int idUser, CancellationToken ct);
    Task<IReadOnlyList<FollowUser>> GetAllFollowingAsync(int idUser, CancellationToken ct);
    Task CreateAsync(int idFollower, int idFollowing, CancellationToken ct);
    Task DeleteAsync(int idFollower, int idFollowing, CancellationToken ct);
}
