using RESR.Core.Controllers.Reactions.Factories;
using RESR.Models.Reactions;

namespace RESR.Core.Tests.Reactions.Factories;

public sealed class ReactionFactoryTests
{
    [Fact]
    public void CreateForCreation_AssignsExpectedFields()
    {
        var factory = new ReactionFactory();

        var reaction = factory.CreateForCreation(ReactionNames.Like, 4, 2);

        Assert.Equal(ReactionNames.Like, reaction.Name);
        Assert.Equal(4, reaction.IdResource);
        Assert.Equal(2, reaction.IdUser);
    }

    [Fact]
    public void CreateFromPersistence_AssignsExpectedFields()
    {
        var factory = new ReactionFactory();

        var reaction = factory.CreateFromPersistence(9, ReactionNames.Love, 4, 2, "alice", "Alice");

        Assert.Equal(9, reaction.IdReaction);
        Assert.Equal(ReactionNames.Love, reaction.Name);
        Assert.Equal(4, reaction.IdResource);
        Assert.Equal(2, reaction.IdUser);
        Assert.Equal("alice", reaction.Username);
        Assert.Equal("Alice", reaction.FirstName);
    }
}
