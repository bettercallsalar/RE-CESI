using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using RESR.Core.Controllers.Articles;
using RESR.Core.Errors;
using RESR.Core.Security.Token;
using RESR.Models.Resources;
using RESR.WebAPI.Routes.Articles;

namespace RESR.WebAPI.Tests.Articles;

public sealed class ArticlesControllerTests
{
    [Fact]
    public async Task GetAll_ReturnsPaginatedResponse()
    {
        var service = new Mock<IArticleService>();
        var tokenService = new Mock<ITokenService>();
        service.Setup(s => s.GetPaginatedAsync(1, 20, It.IsAny<ArticleListingFilters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<Article>
            {
                new()
                {
                    IdResource = 1,
                    IdArticle = 2,
                    Title = "T",
                    Description = null,
                    Visibility = ResourceVisibility.PUBLIC,
                    CreatedAt = DateTime.UtcNow,
                    IdUser = 1,
                    IdCategory = 2,
                    Content = "Body",
                    IsApproved = false
                }
            }, 1));
        var controller = new ArticlesController(service.Object, tokenService.Object);

        var result = await controller.GetAll(ct: CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<PaginatedArticlesResponse>(ok.Value);
        Assert.Single(response.Items);
        Assert.Equal(1, response.TotalCount);
    }

    [Fact]
    public async Task GetByResourceId_ReturnsNotFound_WhenMissing()
    {
        var service = new Mock<IArticleService>();
        var tokenService = new Mock<ITokenService>();
        service.Setup(s => s.GetByResourceIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Article?)null);
        var controller = new ArticlesController(service.Object, tokenService.Object);

        var result = await controller.GetByResourceId(1, CancellationToken.None);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task Create_ReturnsCreatedAtAction_WhenValid()
    {
        var service = new Mock<IArticleService>();
        var tokenService = new Mock<ITokenService>();
        service.Setup(s => s.CreateAsync(It.IsAny<CreateArticleCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(11);
        tokenService.Setup(s => s.ValidateToken("jwt-token"))
            .Returns(true);
        tokenService.Setup(s => s.GetArgumentFromToken("jwt-token", "sub"))
            .Returns("7");
        var controller = new ArticlesController(service.Object, tokenService.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
        controller.HttpContext.Request.Headers.Authorization = "Bearer jwt-token";

        var result = await controller.Create(
            new CreateArticleRequest("Title", null, "public", 2, "Body"),
            CancellationToken.None);

        var created = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(nameof(controller.GetByResourceId), created.ActionName);
        service.Verify(s => s.CreateAsync(
            It.Is<CreateArticleCommand>(cmd => cmd.IdUser == 7 && cmd.IdCategory == 2),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Delete_ReturnsNoContent_WhenDeleted()
    {
        var service = new Mock<IArticleService>();
        var tokenService = new Mock<ITokenService>();
        service.Setup(s => s.SoftDeleteAsync(6, 7, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        tokenService.Setup(s => s.ValidateToken("jwt-token"))
            .Returns(true);
        tokenService.Setup(s => s.GetArgumentFromToken("jwt-token", "sub"))
            .Returns("7");
        var controller = new ArticlesController(service.Object, tokenService.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
        controller.HttpContext.Request.Headers.Authorization = "Bearer jwt-token";

        var result = await controller.Delete(6, CancellationToken.None);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task Update_ReturnsForbid_WhenTokenUserDoesNotOwnArticle()
    {
        var service = new Mock<IArticleService>();
        var tokenService = new Mock<ITokenService>();
        service.Setup(s => s.UpdateAsync(It.IsAny<UpdateArticleCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ForbiddenException("Forbidden"));
        tokenService.Setup(s => s.ValidateToken("jwt-token"))
            .Returns(true);
        tokenService.Setup(s => s.GetArgumentFromToken("jwt-token", "sub"))
            .Returns("7");

        var controller = new ArticlesController(service.Object, tokenService.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
        controller.HttpContext.Request.Headers.Authorization = "Bearer jwt-token";

        var result = await controller.Update(6, new UpdateArticleRequest(Title: "Updated"), CancellationToken.None);

        Assert.IsType<ForbidResult>(result.Result);
        service.Verify(s => s.UpdateAsync(
            It.Is<UpdateArticleCommand>(cmd => cmd.IdResource == 6 && cmd.IdUser == 7),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task SetApproval_ReturnsOk_WhenValid()
    {
        var service = new Mock<IArticleService>();
        var tokenService = new Mock<ITokenService>();
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

        var controller = new ArticlesController(service.Object, tokenService.Object);
        var result = await controller.SetApproval(6, new SetResourceApprovalRequest(true), CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ArticleResponse>(ok.Value);
        Assert.True(response.IsApproved);
    }
}
