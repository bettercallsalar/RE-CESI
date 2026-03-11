using RESR.Core.Controllers.Follows.Factories;

namespace RESR.Core.Tests.Follows;

public sealed class FollowsFactoryTests
{
    [Fact]
    public void Create_MapsAllFields()
    {
        var factory = new FollowsFactory();

        var follow = factory.Create(3, 7);

        Assert.Equal(3, follow.IdFollower);
        Assert.Equal(7, follow.IdFollowing);
    }
}
