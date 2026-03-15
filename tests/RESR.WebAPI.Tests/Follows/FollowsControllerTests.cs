using Microsoft.AspNetCore.Mvc;
using Moq;
using RESR.Core.Controllers.Follows;
using RESR.Core.Errors;
using RESR.Models.Follows;
using RESR.WebAPI.Routes.Follows;

namespace RESR.WebAPI.Tests.Follows;

public sealed class FollowsControllerTests
{
    [Fact]
    public async Task GetAllFollowers_ReturnsBadRequest_WhenPageInvalid()
    {
        var controller = CreateController(out _);

        var result = await controller.GetAllFollowers(1, page: 0, pageSize: 20, CancellationToken.None);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal(400, badRequest.StatusCode);
    }

    [Fact]
    public async Task GetAllFollowers_ReturnsBadRequest_WhenPageSizeInvalid()
    {
        var controller = CreateController(out _);

        var result = await controller.GetAllFollowers(1, page: 1, pageSize: 0, CancellationToken.None);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal(400, badRequest.StatusCode);
    }

    [Fact]
    public async Task GetAllFollowers_ReturnsOk_WithPagination()
    {
        var controller = CreateController(out var service);
        service.Setup(s => s.GetAllFollowersAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<FollowUser>
            {
                BuildFollowUser(10, "a"),
                BuildFollowUser(11, "b"),
                BuildFollowUser(12, "c")
            });

        var result = await controller.GetAllFollowers(1, page: 2, pageSize: 2, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<PaginatedFollowUsersResponse>(ok.Value);
        Assert.Equal(2, response.Page);
        Assert.Equal(2, response.PageSize);
        Assert.Equal(3, response.TotalCount);
        Assert.Equal(2, response.TotalPages);
        Assert.Single(response.Items);
        Assert.Equal(12, response.Items[0].IdUser);
    }

    [Fact]
    public async Task GetAllFollowers_ReturnsZeroTotalPages_WhenEmpty()
    {
        var controller = CreateController(out var service);
        service.Setup(s => s.GetAllFollowersAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<FollowUser>());

        var result = await controller.GetAllFollowers(1, page: 1, pageSize: 10, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<PaginatedFollowUsersResponse>(ok.Value);
        Assert.Equal(0, response.TotalPages);
    }

    [Fact]
    public async Task GetAllFollowing_ReturnsBadRequest_WhenPageInvalid()
    {
        var controller = CreateController(out _);

        var result = await controller.GetAllFollowing(1, page: 0, pageSize: 20, CancellationToken.None);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal(400, badRequest.StatusCode);
    }

    [Fact]
    public async Task GetAllFollowing_ReturnsBadRequest_WhenPageSizeInvalid()
    {
        var controller = CreateController(out _);

        var result = await controller.GetAllFollowing(1, page: 1, pageSize: 0, CancellationToken.None);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal(400, badRequest.StatusCode);
    }

    [Fact]
    public async Task GetAllFollowing_ReturnsOk_WithPagination()
    {
        var controller = CreateController(out var service);
        service.Setup(s => s.GetAllFollowingAsync(2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<FollowUser>
            {
                BuildFollowUser(20, "x"),
                BuildFollowUser(21, "y")
            });

        var result = await controller.GetAllFollowing(2, page: 1, pageSize: 1, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<PaginatedFollowUsersResponse>(ok.Value);
        Assert.Equal(1, response.Page);
        Assert.Equal(1, response.PageSize);
        Assert.Equal(2, response.TotalCount);
        Assert.Equal(2, response.TotalPages);
        Assert.Single(response.Items);
    }

    [Fact]
    public async Task GetAllFollowing_ReturnsZeroTotalPages_WhenEmpty()
    {
        var controller = CreateController(out var service);
        service.Setup(s => s.GetAllFollowingAsync(2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<FollowUser>());

        var result = await controller.GetAllFollowing(2, page: 1, pageSize: 10, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<PaginatedFollowUsersResponse>(ok.Value);
        Assert.Equal(0, response.TotalPages);
    }

    [Fact]
    public async Task Exists_ReturnsNoContent_WhenFollowExists()
    {
        var controller = CreateController(out var service);
        service.Setup(s => s.ExistsAsync(1, 2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await controller.Exists(1, 2, CancellationToken.None);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task Exists_ReturnsNotFound_WhenFollowMissing()
    {
        var controller = CreateController(out var service);
        service.Setup(s => s.ExistsAsync(1, 2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await controller.Exists(1, 2, CancellationToken.None);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Create_ReturnsNoContent_WhenSuccess()
    {
        var controller = CreateController(out var service);
        service.Setup(s => s.CreateAsync(1, 2, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await controller.Create(new FollowRequest(1, 2), CancellationToken.None);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task Create_ReturnsConflict_WhenServiceThrows()
    {
        var controller = CreateController(out var service);
        service.Setup(s => s.CreateAsync(1, 2, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ConflictException("exists"));

        var result = await controller.Create(new FollowRequest(1, 2), CancellationToken.None);

        Assert.IsType<ConflictObjectResult>(result);
    }

    [Fact]
    public async Task Create_ReturnsBadRequest_WhenValidationFails()
    {
        var controller = CreateController(out var service);
        service.Setup(s => s.CreateAsync(1, 1, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ValidationException("bad"));

        var result = await controller.Create(new FollowRequest(1, 1), CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Delete_ReturnsNoContent_WhenSuccess()
    {
        var controller = CreateController(out var service);
        service.Setup(s => s.DeleteAsync(1, 2, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await controller.Delete(1, 2, CancellationToken.None);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task Delete_ReturnsNotFound_WhenMissing()
    {
        var controller = CreateController(out var service);
        service.Setup(s => s.DeleteAsync(1, 2, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException("missing"));

        var result = await controller.Delete(1, 2, CancellationToken.None);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    private static FollowsController CreateController(out Mock<IFollowsService> service)
    {
        service = new Mock<IFollowsService>();
        return new FollowsController(service.Object);
    }

    private static FollowUser BuildFollowUser(int idUser, string username) => new()
    {
        IdUser = idUser,
        Username = username,
        FirstName = "Name"
    };
}
