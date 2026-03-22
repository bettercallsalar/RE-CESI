namespace RESR.MAUI.Services;

public interface IFollowsApiClient
{
    Task<bool> ExistsAsync(int idFollower, int idFollowing, CancellationToken ct);
    Task FollowAsync(int idFollower, int idFollowing, CancellationToken ct);
    Task UnfollowAsync(int idFollower, int idFollowing, CancellationToken ct);
}
