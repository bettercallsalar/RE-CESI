using Moq;
using RESR.Core.Controllers.Marks;
using RESR.Core.Controllers.Marks.Ports;
using RESR.Core.Errors;
using RESR.Models.Marks;

namespace RESR.Core.Tests.Marks;

public sealed class MarkServiceTests
{
    [Fact]
    public async Task MarkAsFavoriteAsync_Throws_WhenResourceMissing()
    {
        var service = CreateService(out var repo);
        repo.Setup(r => r.ResourceExistsAsync(4, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            service.MarkAsFavoriteAsync(4, 2, CancellationToken.None));
    }

    [Fact]
    public async Task MarkAsFavoriteAsync_ReturnsMark_WhenValid()
    {
        var service = CreateService(out var repo);
        var mark = BuildMark();
        repo.Setup(r => r.ResourceExistsAsync(4, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        repo.Setup(r => r.MarkAsFavoriteAsync(4, 2, It.IsAny<CancellationToken>())).ReturnsAsync(mark);

        var result = await service.MarkAsFavoriteAsync(4, 2, CancellationToken.None);

        Assert.Same(mark, result);
    }

    [Fact]
    public async Task MarkAsFavoriteAsync_Throws_WhenIdResourceInvalid()
    {
        var service = CreateService(out _);

        await Assert.ThrowsAsync<ValidationException>(() =>
            service.MarkAsFavoriteAsync(0, 2, CancellationToken.None));
    }

    [Fact]
    public async Task MarkAsFavoriteAsync_Throws_WhenIdUserInvalid()
    {
        var service = CreateService(out _);

        await Assert.ThrowsAsync<ValidationException>(() =>
            service.MarkAsFavoriteAsync(4, 0, CancellationToken.None));
    }

    [Fact]
    public async Task UnmarkAsFavoriteAsync_Throws_WhenMissing()
    {
        var service = CreateService(out var repo);
        repo.Setup(r => r.UnmarkAsFavoriteAsync(4, 2, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            service.UnmarkAsFavoriteAsync(4, 2, CancellationToken.None));
    }

    [Fact]
    public async Task UnmarkAsFavoriteAsync_Throws_WhenIdsInvalid()
    {
        var service = CreateService(out _);

        await Assert.ThrowsAsync<ValidationException>(() =>
            service.UnmarkAsFavoriteAsync(0, 0, CancellationToken.None));
    }

    [Fact]
    public async Task MarkAsReadLaterAsync_Throws_WhenResourceMissing()
    {
        var service = CreateService(out var repo);
        repo.Setup(r => r.ResourceExistsAsync(4, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            service.MarkAsReadLaterAsync(4, 2, CancellationToken.None));
    }

    [Fact]
    public async Task MarkAsReadLaterAsync_ReturnsMark_WhenValid()
    {
        var service = CreateService(out var repo);
        var mark = BuildMark(isReadLater: true);
        repo.Setup(r => r.ResourceExistsAsync(4, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        repo.Setup(r => r.MarkAsReadLaterAsync(4, 2, It.IsAny<CancellationToken>())).ReturnsAsync(mark);

        var result = await service.MarkAsReadLaterAsync(4, 2, CancellationToken.None);

        Assert.Same(mark, result);
    }

    [Fact]
    public async Task UnmarkAsReadLaterAsync_Throws_WhenMissing()
    {
        var service = CreateService(out var repo);
        repo.Setup(r => r.UnmarkAsReadLaterAsync(4, 2, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            service.UnmarkAsReadLaterAsync(4, 2, CancellationToken.None));
    }

    [Fact]
    public async Task GetFavoriteRessourcesAsync_Throws_WhenUserInvalid()
    {
        var service = CreateService(out _);

        await Assert.ThrowsAsync<ValidationException>(() =>
            service.GetFavoriteRessourcesAsync(0, CancellationToken.None));
    }

    [Fact]
    public async Task GetFavoriteRessourcesAsync_ReturnsList_WhenValid()
    {
        var service = CreateService(out var repo);
        var list = new List<Mark> { BuildMark(1), BuildMark(2) };
        repo.Setup(r => r.GetFavoriteRessourcesAsync(2, It.IsAny<CancellationToken>())).ReturnsAsync(list);

        var result = await service.GetFavoriteRessourcesAsync(2, CancellationToken.None);

        Assert.Same(list, result);
    }

    [Fact]
    public async Task GetReadLaterRessourcesAsync_ReturnsList_WhenValid()
    {
        var service = CreateService(out var repo);
        var list = new List<Mark> { BuildMark(isReadLater: true) };
        repo.Setup(r => r.GetReadLaterRessourcesAsync(2, It.IsAny<CancellationToken>())).ReturnsAsync(list);

        var result = await service.GetReadLaterRessourcesAsync(2, CancellationToken.None);

        Assert.Same(list, result);
    }

    [Fact]
    public async Task GetFavoriteRessourceAsync_Throws_WhenIdsInvalid()
    {
        var service = CreateService(out _);

        await Assert.ThrowsAsync<ValidationException>(() =>
            service.GetFavoriteRessourceAsync(0, 2, CancellationToken.None));
    }

    [Fact]
    public async Task GetReadLaterRessourceAsync_Throws_WhenIdsInvalid()
    {
        var service = CreateService(out _);

        await Assert.ThrowsAsync<ValidationException>(() =>
            service.GetReadLaterRessourceAsync(4, 0, CancellationToken.None));
    }

    [Fact]
    public async Task GetReadLaterRessourceAsync_ReturnsMark_WhenExists()
    {
        var service = CreateService(out var repo);
        var mark = BuildMark(isReadLater: true);
        repo.Setup(r => r.GetReadLaterRessourceAsync(4, 2, It.IsAny<CancellationToken>())).ReturnsAsync(mark);

        var result = await service.GetReadLaterRessourceAsync(4, 2, CancellationToken.None);

        Assert.Same(mark, result);
    }

    private static MarkService CreateService(out Mock<IMarksRepository> repo)
    {
        repo = new Mock<IMarksRepository>();
        return new MarkService(repo.Object);
    }

    private static Mark BuildMark(
        int idMark = 5,
        bool isFavorite = true,
        bool isReadLater = false,
        int idResource = 4,
        int idUser = 2)
    {
        return new Mark
        {
            IdMark = idMark,
            IsFavorite = isFavorite,
            IsReadLater = isReadLater,
            IdRessource = idResource,
            IdUser = idUser
        };
    }
}
