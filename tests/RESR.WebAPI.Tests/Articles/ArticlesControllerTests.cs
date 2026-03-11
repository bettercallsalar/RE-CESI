using Microsoft.AspNetCore.Mvc;
using Moq;
using RESR.Core.Controllers.Articles;
using RESR.Models.Resources;
using RESR.WebAPI.Routes.Articles;

namespace RESR.WebAPI.Tests.Articles;

public sealed class ArticlesControllerTests
{
    [Fact]
    public async Task GetByResourceId_ReturnsNotFound_WhenMissing()
    {
        var service = new Mock<IArticleService>();
        service.Setup(s => s.GetByResourceIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Article?)null);
        var controller = new ArticlesController(service.Object);

        var result = await controller.GetByResourceId(1, CancellationToken.None);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task Create_ReturnsCreatedAtAction_WhenValid()
    {
        var service = new Mock<IArticleService>();
        service.Setup(s => s.CreateAsync(It.IsAny<CreateArticleCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(11);
        var controller = new ArticlesController(service.Object);

        var result = await controller.Create(
            new CreateArticleRequest("Title", null, "public", 1, 2, "Body"),
            CancellationToken.None);

        var created = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(nameof(controller.GetByResourceId), created.ActionName);
    }

    [Fact]
    public async Task Delete_ReturnsNoContent_WhenDeleted()
    {
        var service = new Mock<IArticleService>();
        service.Setup(s => s.SoftDeleteAsync(6, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        var controller = new ArticlesController(service.Object);

        var result = await controller.Delete(6, CancellationToken.None);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task SetApproval_ReturnsOk_WhenValid()
    {
        var service = new Mock<IArticleService>();
        service.Setup(s => s.SetApprovalAsync(It.IsAny<SetArticleApprovalCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Article
            {
                IdResource = 6,
                IdArticle = 3,
                Title = "T",
                Description = null,
                Visibility = ResourceVisibility.PUBLIC,
                CreatedAt = DateTime.UtcNow,
                ModifiedAt = null,
                DeletedAt = null,
                IdUser = 1,
                IdCategory = 2,
                Content = "Body",
                IsApproved = true
            });

        var controller = new ArticlesController(service.Object);
        var result = await controller.SetApproval(6, new SetArticleApprovalRequest(true), CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ArticleResponse>(ok.Value);
        Assert.True(response.IsApproved);
    }
}
