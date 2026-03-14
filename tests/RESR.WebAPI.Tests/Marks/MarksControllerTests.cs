using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using RESR.Core.Controllers.Marks;
using RESR.Core.Errors;
using RESR.Models.Marks;
using RESR.WebAPI.Routes.Marks;

namespace RESR.WebAPI.Tests.Marks;

public sealed class MarksControllerTests
{
    [Fact]
    public async Task GetFavoriteRessources_ReturnsBadRequest_WhenPageInvalid()
    {
        var controller = CreateController(out _, userId: 2);

        var result = await controller.GetFavoriteRessources(page: 0, pageSize: 20, CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetFavoriteRessources_ReturnsUnauthorized_WhenUserMissing()
    {
        var controller = CreateController(out _);

        var result = await controller.GetFavoriteRessources(page: 1, pageSize: 20, CancellationToken.None);

        Assert.IsType<UnauthorizedObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetFavoriteRessources_ReturnsBadRequest_WhenValidationException()
    {
        var controller = CreateController(out var service, userId: 2);
        service.Setup(s => s.GetFavoriteRessourcesAsync(2, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ValidationException("bad"));

        var result = await controller.GetFavoriteRessources(page: 1, pageSize: 20, CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetFavoriteRessources_ReturnsZeroTotalPages_WhenEmpty()
    {
        var controller = CreateController(out var service, userId: 2);
        service.Setup(s => s.GetFavoriteRessourcesAsync(2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Mark>());

        var result = await controller.GetFavoriteRessources(page: 1, pageSize: 20, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<PaginatedMarksResponse>(ok.Value);
        Assert.Equal(0, response.TotalPages);
    }

    [Fact]
    public async Task GetFavoriteRessources_ReturnsOk_WithPagination()
    {
        var controller = CreateController(out var service, userId: 2);
        service.Setup(s => s.GetFavoriteRessourcesAsync(2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Mark>
            {
                BuildMark(10),
                BuildMark(11),
                BuildMark(12)
            });

        var result = await controller.GetFavoriteRessources(page: 2, pageSize: 2, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<PaginatedMarksResponse>(ok.Value);
        Assert.Equal(2, response.Page);
        Assert.Equal(2, response.PageSize);
        Assert.Equal(3, response.TotalCount);
        Assert.Equal(2, response.TotalPages);
        Assert.Single(response.Items);
        Assert.Equal(12, response.Items[0].IdMark);
    }

    [Fact]
    public async Task GetReadLaterRessources_ReturnsBadRequest_WhenPageSizeInvalid()
    {
        var controller = CreateController(out _, userId: 2);

        var result = await controller.GetReadLaterRessources(page: 1, pageSize: 0, CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetReadLaterRessources_ReturnsBadRequest_WhenPageInvalid()
    {
        var controller = CreateController(out _, userId: 2);

        var result = await controller.GetReadLaterRessources(page: 0, pageSize: 20, CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetReadLaterRessources_ReturnsUnauthorized_WhenUserMissing()
    {
        var controller = CreateController(out _);

        var result = await controller.GetReadLaterRessources(page: 1, pageSize: 20, CancellationToken.None);

        Assert.IsType<UnauthorizedObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetReadLaterRessources_ReturnsBadRequest_WhenValidationException()
    {
        var controller = CreateController(out var service, userId: 2);
        service.Setup(s => s.GetReadLaterRessourcesAsync(2, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ValidationException("bad"));

        var result = await controller.GetReadLaterRessources(page: 1, pageSize: 20, CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetFavoriteRessource_ReturnsNotFound_WhenMissing()
    {
        var controller = CreateController(out var service, userId: 2);
        service.Setup(s => s.GetFavoriteRessourceAsync(4, 2, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Mark?)null);

        var result = await controller.GetFavoriteRessource(4, CancellationToken.None);

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetFavoriteRessource_ReturnsUnauthorized_WhenUserMissing()
    {
        var controller = CreateController(out _);

        var result = await controller.GetFavoriteRessource(4, CancellationToken.None);

        Assert.IsType<UnauthorizedObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetFavoriteRessource_ReturnsOk_WhenFound()
    {
        var controller = CreateController(out var service, userId: 2);
        service.Setup(s => s.GetFavoriteRessourceAsync(4, 2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildMark());

        var result = await controller.GetFavoriteRessource(4, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.IsType<MarkResponse>(ok.Value);
    }

    [Fact]
    public async Task MarkAsFavorite_ReturnsOk_WhenSuccess()
    {
        var controller = CreateController(out var service, userId: 2);
        service.Setup(s => s.MarkAsFavoriteAsync(4, 2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildMark());

        var result = await controller.MarkAsFavorite(4, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.IsType<MarkResponse>(ok.Value);
    }

    [Fact]
    public async Task MarkAsFavorite_ReturnsUnauthorized_WhenUserMissing()
    {
        var controller = CreateController(out _);

        var result = await controller.MarkAsFavorite(4, CancellationToken.None);

        Assert.IsType<UnauthorizedObjectResult>(result.Result);
    }

    [Fact]
    public async Task MarkAsFavorite_ReturnsNotFound_WhenServiceThrows()
    {
        var controller = CreateController(out var service, userId: 2);
        service.Setup(s => s.MarkAsFavoriteAsync(4, 2, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException("missing"));

        var result = await controller.MarkAsFavorite(4, CancellationToken.None);

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task MarkAsFavorite_ReturnsBadRequest_WhenServiceThrows()
    {
        var controller = CreateController(out var service, userId: 2);
        service.Setup(s => s.MarkAsFavoriteAsync(0, 2, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ValidationException("bad"));

        var result = await controller.MarkAsFavorite(0, CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task UnmarkAsFavorite_ReturnsNotFound_WhenServiceThrows()
    {
        var controller = CreateController(out var service, userId: 2);
        service.Setup(s => s.UnmarkAsFavoriteAsync(4, 2, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException("missing"));

        var result = await controller.UnmarkAsFavorite(4, CancellationToken.None);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task UnmarkAsFavorite_ReturnsNoContent_WhenSuccess()
    {
        var controller = CreateController(out var service, userId: 2);
        service.Setup(s => s.UnmarkAsFavoriteAsync(4, 2, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await controller.UnmarkAsFavorite(4, CancellationToken.None);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task UnmarkAsFavorite_ReturnsUnauthorized_WhenUserMissing()
    {
        var controller = CreateController(out _);

        var result = await controller.UnmarkAsFavorite(4, CancellationToken.None);

        Assert.IsType<UnauthorizedObjectResult>(result);
    }

    [Fact]
    public async Task UnmarkAsFavorite_ReturnsBadRequest_WhenServiceThrows()
    {
        var controller = CreateController(out var service, userId: 2);
        service.Setup(s => s.UnmarkAsFavoriteAsync(0, 2, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ValidationException("bad"));

        var result = await controller.UnmarkAsFavorite(0, CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task MarkAsReadLater_ReturnsUnauthorized_WhenUserMissing()
    {
        var controller = CreateController(out _);

        var result = await controller.MarkAsReadLater(4, CancellationToken.None);

        Assert.IsType<UnauthorizedObjectResult>(result.Result);
    }

    [Fact]
    public async Task MarkAsReadLater_ReturnsOk_WhenSuccess()
    {
        var controller = CreateController(out var service, userId: 2);
        service.Setup(s => s.MarkAsReadLaterAsync(4, 2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildMark(isReadLater: true, isFavorite: false));

        var result = await controller.MarkAsReadLater(4, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.IsType<MarkResponse>(ok.Value);
    }

    [Fact]
    public async Task MarkAsReadLater_ReturnsNotFound_WhenServiceThrows()
    {
        var controller = CreateController(out var service, userId: 2);
        service.Setup(s => s.MarkAsReadLaterAsync(4, 2, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException("missing"));

        var result = await controller.MarkAsReadLater(4, CancellationToken.None);

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task MarkAsReadLater_ReturnsBadRequest_WhenServiceThrows()
    {
        var controller = CreateController(out var service, userId: 2);
        service.Setup(s => s.MarkAsReadLaterAsync(0, 2, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ValidationException("bad"));

        var result = await controller.MarkAsReadLater(0, CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task UnmarkAsReadLater_ReturnsNoContent_WhenSuccess()
    {
        var controller = CreateController(out var service, userId: 2);
        service.Setup(s => s.UnmarkAsReadLaterAsync(4, 2, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await controller.UnmarkAsReadLater(4, CancellationToken.None);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task UnmarkAsReadLater_ReturnsNotFound_WhenServiceThrows()
    {
        var controller = CreateController(out var service, userId: 2);
        service.Setup(s => s.UnmarkAsReadLaterAsync(4, 2, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException("missing"));

        var result = await controller.UnmarkAsReadLater(4, CancellationToken.None);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task UnmarkAsReadLater_ReturnsBadRequest_WhenServiceThrows()
    {
        var controller = CreateController(out var service, userId: 2);
        service.Setup(s => s.UnmarkAsReadLaterAsync(0, 2, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ValidationException("bad"));

        var result = await controller.UnmarkAsReadLater(0, CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task UnmarkAsReadLater_ReturnsUnauthorized_WhenUserMissing()
    {
        var controller = CreateController(out _);

        var result = await controller.UnmarkAsReadLater(4, CancellationToken.None);

        Assert.IsType<UnauthorizedObjectResult>(result);
    }

    [Fact]
    public async Task GetReadLaterRessources_ReturnsOk_WithPagination()
    {
        var controller = CreateController(out var service, userId: 2);
        service.Setup(s => s.GetReadLaterRessourcesAsync(2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Mark>
            {
                BuildMark(10, isReadLater: true, isFavorite: false),
                BuildMark(11, isReadLater: true, isFavorite: false),
                BuildMark(12, isReadLater: true, isFavorite: false)
            });

        var result = await controller.GetReadLaterRessources(page: 1, pageSize: 2, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<PaginatedMarksResponse>(ok.Value);
        Assert.Equal(2, response.PageSize);
        Assert.Equal(2, response.TotalPages);
        Assert.Equal(3, response.TotalCount);
        Assert.Equal(2, response.Items.Count);
    }

    [Fact]
    public async Task GetReadLaterRessources_ReturnsZeroTotalPages_WhenEmpty()
    {
        var controller = CreateController(out var service, userId: 2);
        service.Setup(s => s.GetReadLaterRessourcesAsync(2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Mark>());

        var result = await controller.GetReadLaterRessources(page: 1, pageSize: 20, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<PaginatedMarksResponse>(ok.Value);
        Assert.Equal(0, response.TotalPages);
    }

    [Fact]
    public async Task GetFavoriteRessource_ReturnsBadRequest_WhenValidationException()
    {
        var controller = CreateController(out var service, userId: 2);
        service.Setup(s => s.GetFavoriteRessourceAsync(0, 2, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ValidationException("bad"));

        var result = await controller.GetFavoriteRessource(0, CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetReadLaterRessource_ReturnsOk_WhenFound()
    {
        var controller = CreateController(out var service, userId: 2);
        service.Setup(s => s.GetReadLaterRessourceAsync(4, 2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildMark(isReadLater: true, isFavorite: false));

        var result = await controller.GetReadLaterRessource(4, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.IsType<MarkResponse>(ok.Value);
    }

    [Fact]
    public async Task GetReadLaterRessource_ReturnsUnauthorized_WhenUserMissing()
    {
        var controller = CreateController(out _);

        var result = await controller.GetReadLaterRessource(4, CancellationToken.None);

        Assert.IsType<UnauthorizedObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetReadLaterRessource_ReturnsNotFound_WhenMissing()
    {
        var controller = CreateController(out var service, userId: 2);
        service.Setup(s => s.GetReadLaterRessourceAsync(4, 2, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Mark?)null);

        var result = await controller.GetReadLaterRessource(4, CancellationToken.None);

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetReadLaterRessource_ReturnsBadRequest_WhenValidationException()
    {
        var controller = CreateController(out var service, userId: 2);
        service.Setup(s => s.GetReadLaterRessourceAsync(0, 2, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ValidationException("bad"));

        var result = await controller.GetReadLaterRessource(0, CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    private static MarksController CreateController(out Mock<IMarkService> service, int? userId = null)
    {
        service = new Mock<IMarkService>();
        var controller = new MarksController(service.Object);

        if (userId.HasValue)
        {
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(
                        new ClaimsIdentity(
                            new[] { new Claim(JwtRegisteredClaimNames.Sub, userId.Value.ToString()) },
                            "test"))
                }
            };
        }

        return controller;
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
