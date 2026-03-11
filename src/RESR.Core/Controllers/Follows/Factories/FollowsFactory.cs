using RESR.Models.Follows;

namespace RESR.Core.Controllers.Follows.Factories;

public sealed class FollowsFactory : IFollowsFactory
{
    public Follow Create(int idFollower, int idFollowing) =>
        new()
        {
            IdFollower = idFollower,
            IdFollowing = idFollowing
        };
}
