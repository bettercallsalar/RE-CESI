using Moq;
using RESR.Core.Controllers.Reactions;
using RESR.Core.Controllers.Reactions.Factories;
using RESR.Core.Controllers.Reactions.Ports;
using RESR.Core.Errors;
using RESR.Models.Reactions;

namespace RESR.Core.Tests.Reactions;

public sealed class ReactionServiceTests
{
    [Fact]
    public async Task GetByResourceIdAsync_Throws_WhenResourceMissing()
    {
        var service = CreateService(out var repo, out _);
        repo.Setup(r => r.ResourceExistsAsync(4, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        await Assert.ThrowsAsync<NotFoundException>(() => service.GetByResourceIdAsync(4, CancellationToken.None));
    }

    [Fact]
    public async Task GetByResourceIdAsync_ReturnsReactions_WhenResourceExists()
    {
        var service = CreateService(out var repo, out _);
        var expected = new List<Reaction> { BuildReaction() };
        repo.Setup(r => r.ResourceExistsAsync(4, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        repo.Setup(r => r.GetByResourceIdAsync(4, It.IsAny<CancellationToken>())).ReturnsAsync(expected);

        var result = await service.GetByResourceIdAsync(4, CancellationToken.None);

        Assert.Same(expected, result);
    }

    [Fact]
    public async Task GetByUserIdAsync_Throws_WhenUserMissing()
    {
        var service = CreateService(out var repo, out _);
        repo.Setup(r => r.UserExistsAsync(2, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        await Assert.ThrowsAsync<NotFoundException>(() => service.GetByUserIdAsync(2, CancellationToken.None));
    }

    [Fact]
    public async Task GetByUserIdAsync_ReturnsReactions_WhenUserExists()
    {
        var service = CreateService(out var repo, out _);
        var expected = new List<Reaction> { BuildReaction(idReaction: 9, idUser: 2) };
        repo.Setup(r => r.UserExistsAsync(2, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        repo.Setup(r => r.GetByUserIdAsync(2, It.IsAny<CancellationToken>())).ReturnsAsync(expected);

        var result = await service.GetByUserIdAsync(2, CancellationToken.None);

        Assert.Same(expected, result);
    }

    [Fact]
    public async Task CreateAsync_Throws_WhenAlreadyReacted()
    {
        var service = CreateService(out var repo, out _);
        repo.Setup(r => r.ResourceExistsAsync(4, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        repo.Setup(r => r.GetByResourceAndUserAsync(4, 2, It.IsAny<CancellationToken>())).ReturnsAsync(BuildReaction());

        await Assert.ThrowsAsync<ConflictException>(() =>
            service.CreateAsync(new CreateReactionCommand(4, ReactionNames.Love, 2), CancellationToken.None));
    }

    [Fact]
    public async Task CreateAsync_NormalizesName_AndDelegatesToRepository()
    {
        var service = CreateService(out var repo, out var factory);
        repo.Setup(r => r.ResourceExistsAsync(4, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        repo.Setup(r => r.GetByResourceAndUserAsync(4, 2, It.IsAny<CancellationToken>())).ReturnsAsync((Reaction?)null);
        factory.Setup(f => f.CreateForCreation(ReactionNames.Like, 4, 2)).Returns(BuildReaction(name: ReactionNames.Like));
        repo.Setup(r => r.CreateAsync(It.IsAny<Reaction>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildReaction(idReaction: 12, name: ReactionNames.Like));

        var result = await service.CreateAsync(new CreateReactionCommand(4, " Like ", 2), CancellationToken.None);

        Assert.Equal(12, result.IdReaction);
        factory.VerifyAll();
    }

    [Fact]
    public async Task UpdateAsync_Throws_WhenActorIsNotAuthor()
    {
        var service = CreateService(out var repo, out _);
        repo.Setup(r => r.GetByIdAsync(5, It.IsAny<CancellationToken>())).ReturnsAsync(BuildReaction(idReaction: 5, idUser: 4));

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            service.UpdateAsync(new UpdateReactionCommand(5, ReactionNames.Dislike, 2), CancellationToken.None));
    }

    [Fact]
    public async Task UpdateAsync_UpdatesReaction_WhenActorIsAuthor()
    {
        var service = CreateService(out var repo, out _);
        repo.Setup(r => r.GetByIdAsync(5, It.IsAny<CancellationToken>())).ReturnsAsync(BuildReaction(idReaction: 5, idUser: 2));
        repo.Setup(r => r.UpdateNameAsync(5, ReactionNames.Dislike, It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildReaction(idReaction: 5, name: ReactionNames.Dislike, idUser: 2));

        var result = await service.UpdateAsync(new UpdateReactionCommand(5, ReactionNames.Dislike, 2), CancellationToken.None);

        Assert.Equal(ReactionNames.Dislike, result.Name);
    }

    [Fact]
    public async Task DeleteAsync_Throws_WhenActorIsNotAuthor()
    {
        var service = CreateService(out var repo, out _);
        repo.Setup(r => r.GetByIdAsync(5, It.IsAny<CancellationToken>())).ReturnsAsync(BuildReaction(idReaction: 5, idUser: 4));

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            service.DeleteAsync(5, 2, CancellationToken.None));
    }

    [Fact]
    public async Task DeleteAsync_Deletes_WhenActorIsAuthor()
    {
        var service = CreateService(out var repo, out _);
        repo.Setup(r => r.GetByIdAsync(5, It.IsAny<CancellationToken>())).ReturnsAsync(BuildReaction(idReaction: 5, idUser: 2));
        repo.Setup(r => r.DeleteAsync(5, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        await service.DeleteAsync(5, 2, CancellationToken.None);

        repo.Verify(r => r.DeleteAsync(5, It.IsAny<CancellationToken>()), Times.Once);
    }

    private static ReactionService CreateService(out Mock<IReactionRepository> repo, out Mock<IReactionFactory> factory)
    {
        repo = new Mock<IReactionRepository>();
        factory = new Mock<IReactionFactory>();
        return new ReactionService(repo.Object, factory.Object);
    }

    private static Reaction BuildReaction(
        int idReaction = 5,
        string name = ReactionNames.Like,
        int idResource = 4,
        int idUser = 2)
    {
        return new Reaction
        {
            IdReaction = idReaction,
            Name = name,
            IdResource = idResource,
            IdUser = idUser
        };
    }
}
