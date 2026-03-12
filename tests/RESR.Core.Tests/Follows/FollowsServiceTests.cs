using Moq;
using RESR.Core.Controllers.Follows;
using RESR.Core.Controllers.Follows.Ports;
using RESR.Core.Errors;
using RESR.Models.Follows;

namespace RESR.Core.Tests.Follows;

public sealed class FollowsServiceTests
{
    [Fact]
    public async Task GetAllFollowersAsync_DelegatesToRepository()
    {
        var repo = new Mock<IFollowsRepository>();
        var expected = new List<FollowUser> { BuildFollowUser(1) };
        repo.Setup(r => r.GetAllFollowersAsync(5, It.IsAny<CancellationToken>())).ReturnsAsync(expected);
        var service = new FollowsService(repo.Object);

        var result = await service.GetAllFollowersAsync(5, CancellationToken.None);

        Assert.Same(expected, result);
    }

    [Fact]
    public async Task GetAllFollowingAsync_DelegatesToRepository()
    {
        var repo = new Mock<IFollowsRepository>();
        var expected = new List<FollowUser> { BuildFollowUser(2) };
        repo.Setup(r => r.GetAllFollowingAsync(6, It.IsAny<CancellationToken>())).ReturnsAsync(expected);
        var service = new FollowsService(repo.Object);

        var result = await service.GetAllFollowingAsync(6, CancellationToken.None);

        Assert.Same(expected, result);
    }

    [Fact]
    public async Task CreateAsync_Throws_WhenSameFollowerAndFollowing()
    {
        var service = new FollowsService(new Mock<IFollowsRepository>().Object);

        await Assert.ThrowsAsync<ValidationException>(() => service.CreateAsync(1, 1, CancellationToken.None));
    }

    [Fact]
    public async Task CreateAsync_ThrowsConflict_WhenRepositoryReturnsFalse()
    {
        var repo = new Mock<IFollowsRepository>();
        repo.Setup(r => r.CreateAsync(1, 2, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        var service = new FollowsService(repo.Object);

        await Assert.ThrowsAsync<ConflictException>(() => service.CreateAsync(1, 2, CancellationToken.None));
    }

    [Fact]
    public async Task CreateAsync_DoesNotThrow_WhenRepositoryReturnsTrue()
    {
        var repo = new Mock<IFollowsRepository>();
        repo.Setup(r => r.CreateAsync(1, 2, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var service = new FollowsService(repo.Object);

        await service.CreateAsync(1, 2, CancellationToken.None);

        repo.Verify(r => r.CreateAsync(1, 2, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ThrowsNotFound_WhenRepositoryReturnsFalse()
    {
        var repo = new Mock<IFollowsRepository>();
        repo.Setup(r => r.DeleteAsync(1, 2, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        var service = new FollowsService(repo.Object);

        await Assert.ThrowsAsync<NotFoundException>(() => service.DeleteAsync(1, 2, CancellationToken.None));
    }

    [Fact]
    public async Task DeleteAsync_DoesNotThrow_WhenRepositoryReturnsTrue()
    {
        var repo = new Mock<IFollowsRepository>();
        repo.Setup(r => r.DeleteAsync(1, 2, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var service = new FollowsService(repo.Object);

        await service.DeleteAsync(1, 2, CancellationToken.None);

        repo.Verify(r => r.DeleteAsync(1, 2, It.IsAny<CancellationToken>()), Times.Once);
    }

    private static FollowUser BuildFollowUser(int idUser) => new()
    {
        IdUser = idUser,
        Username = "user",
        FirstName = "User"
    };
}
