using RESR.Models.Follows;

namespace RESR.Core.Controllers.Follows.Factories;

public interface IFollowsFactory
{
    Follow Create(int idFollower, int idFollowing);
}