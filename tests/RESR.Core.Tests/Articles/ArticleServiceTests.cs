using Moq;
using RESR.Core.Controllers.Articles;
using RESR.Core.Controllers.Articles.Ports;
using RESR.Core.Errors;
using RESR.Models.Resources;

namespace RESR.Core.Tests.Articles;

public sealed class ArticleServiceTests
{
    [Fact]
    public async Task CreateAsync_Throws_WhenTitleMissing()
    {
        var repo = new Mock<IArticleRepository>();
        var service = new ArticleService(repo.Object);

        await Assert.ThrowsAsync<ValidationException>(() =>
            service.CreateAsync(new CreateArticleCommand("", null, ResourceVisibility.PUBLIC, 1, 1, "content"), CancellationToken.None));
    }

    [Fact]
    public async Task CreateAsync_NormalizesAndDelegates()
    {
        var repo = new Mock<IArticleRepository>();
        CreateArticleCommand? captured = null;
        repo.Setup(r => r.CreateAsync(It.IsAny<CreateArticleCommand>(), It.IsAny<CancellationToken>()))
            .Callback<CreateArticleCommand, CancellationToken>((cmd, _) => captured = cmd)
            .ReturnsAsync(12);
        var service = new ArticleService(repo.Object);

        var id = await service.CreateAsync(
            new CreateArticleCommand("  title ", "  desc ", ResourceVisibility.PRIVATE, 1, 2, "  body "),
            CancellationToken.None);

        Assert.Equal(12, id);
        Assert.NotNull(captured);
        Assert.Equal("title", captured!.Title);
        Assert.Equal("desc", captured.Description);
        Assert.Equal("body", captured.Content);
    }

    [Fact]
    public async Task UpdateAsync_Throws_WhenNotFound()
    {
        var repo = new Mock<IArticleRepository>();
        repo.Setup(r => r.GetByResourceIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Article?)null);
        var service = new ArticleService(repo.Object);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            service.UpdateAsync(new UpdateArticleCommand(999, Title: "x"), CancellationToken.None));
    }

    [Fact]
    public async Task UpdateAsync_Throws_WhenContentIsBlank()
    {
        var repo = new Mock<IArticleRepository>();
        repo.Setup(r => r.GetByResourceIdAsync(4, It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildArticle());
        var service = new ArticleService(repo.Object);

        await Assert.ThrowsAsync<ValidationException>(() =>
            service.UpdateAsync(new UpdateArticleCommand(4, Content: "   "), CancellationToken.None));
    }

    [Fact]
    public async Task SoftDeleteAsync_DelegatesToRepository()
    {
        var repo = new Mock<IArticleRepository>();
        repo.Setup(r => r.SoftDeleteAsync(5, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var service = new ArticleService(repo.Object);

        var deleted = await service.SoftDeleteAsync(5, CancellationToken.None);

        Assert.True(deleted);
    }

    [Fact]
    public async Task SetApprovalAsync_DelegatesToRepository()
    {
        var repo = new Mock<IArticleRepository>();
        var approved = BuildArticle();
        approved.IsApproved = true;
        repo.Setup(r => r.SetApprovalAsync(new SetArticleApprovalCommand(9, true), It.IsAny<CancellationToken>()))
            .ReturnsAsync(approved);
        var service = new ArticleService(repo.Object);

        var article = await service.SetApprovalAsync(new SetArticleApprovalCommand(9, true), CancellationToken.None);

        Assert.True(article.IsApproved);
    }

    private static Article BuildArticle() =>
        new()
        {
            IdResource = 4,
            IdArticle = 7,
            Title = "Title",
            Description = null,
            Visibility = ResourceVisibility.PUBLIC,
            CreatedAt = DateTime.UtcNow,
            ModifiedAt = null,
            DeletedAt = null,
            IdUser = 1,
            IdCategory = 2,
            Content = "Content",
            IsApproved = false
        };
}
