using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using RESR.Core.Controllers.Follows;
using RESR.Core.Errors;
using RESR.Core.Controllers.Users.Ports;
using RESR.Core.Security.Token;
using RESR.Models.Departments;
using RESR.Models.Follows;
using RESR.Models.Users;
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
    public async Task GetOwnFollowing_ReturnsUnauthorized_WhenTokenMissing()
    {
        var controller = CreateController(out _);

        var result = await controller.GetOwnFollowing(page: 1, pageSize: 10, CancellationToken.None);

        Assert.IsType<UnauthorizedObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetOwnFollowing_ReturnsCurrentUserFollowing()
    {
        var controller = CreateAuthenticatedController(out var service);
        service.Setup(s => s.GetAllFollowingAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<FollowUser> { BuildFollowUser(20, "x") });

        var result = await controller.GetOwnFollowing(page: 1, pageSize: 10, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<PaginatedFollowUsersResponse>(ok.Value);
        Assert.Single(response.Items);
        service.Verify(s => s.GetAllFollowingAsync(1, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetOwnFollowingState_ReturnsFollowStateForCurrentUser()
    {
        var controller = CreateAuthenticatedController(out var service);
        service.Setup(s => s.ExistsAsync(1, 5, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var result = await controller.GetOwnFollowingState(5, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<FollowStateResponse>(ok.Value);
        Assert.Equal(1, response.IdFollower);
        Assert.Equal(5, response.IdFollowing);
        Assert.True(response.IsFollowing);
    }

    [Fact]
    public async Task Create_ReturnsNoContent_WhenSuccess()
    {
        var controller = CreateAuthenticatedController(out var service);
        service.Setup(s => s.CreateAsync(1, 2, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await controller.Create(2, CancellationToken.None);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task Create_ReturnsConflict_WhenServiceThrows()
    {
        var controller = CreateAuthenticatedController(out var service);
        service.Setup(s => s.CreateAsync(1, 2, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ConflictException("exists"));

        var result = await controller.Create(2, CancellationToken.None);

        Assert.IsType<ConflictObjectResult>(result);
    }

    [Fact]
    public async Task Create_ReturnsBadRequest_WhenValidationFails()
    {
        var controller = CreateAuthenticatedController(out var service);
        service.Setup(s => s.CreateAsync(1, 1, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ValidationException("bad"));

        var result = await controller.Create(1, CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Create_UsesAuthenticatedUserId()
    {
        var controller = CreateAuthenticatedController(out var service);
        service.Setup(s => s.CreateAsync(1, 7, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await controller.Create(7, CancellationToken.None);

        service.Verify(s => s.CreateAsync(1, 7, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Create_ReturnsUnauthorized_WhenTokenMissing()
    {
        var controller = CreateController(out _);

        var result = await controller.Create(2, CancellationToken.None);

        Assert.IsType<UnauthorizedObjectResult>(result);
    }

    [Fact]
    public async Task Delete_ReturnsNoContent_WhenSuccess()
    {
        var controller = CreateAuthenticatedController(out var service);
        service.Setup(s => s.DeleteAsync(1, 2, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await controller.Delete(2, CancellationToken.None);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task Delete_ReturnsNotFound_WhenMissing()
    {
        var controller = CreateAuthenticatedController(out var service);
        service.Setup(s => s.DeleteAsync(1, 2, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException("missing"));

        var result = await controller.Delete(2, CancellationToken.None);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task Delete_ReturnsUnauthorized_WhenTokenMissing()
    {
        var controller = CreateController(out _);

        var result = await controller.Delete(2, CancellationToken.None);

        Assert.IsType<UnauthorizedObjectResult>(result);
    }

    private static FollowsController CreateController(out Mock<IFollowsService> service)
    {
        service = new Mock<IFollowsService>();
        return new FollowsController(service.Object, Mock.Of<ITokenService>());
    }

    private static FollowsController CreateAuthenticatedController(out Mock<IFollowsService> service, IUserRepository? userRepository = null)
    {
        service = new Mock<IFollowsService>();
        var tokenService = new Mock<ITokenService>();
        tokenService.Setup(s => s.ValidateToken("jwt-token")).Returns(true);
        tokenService.Setup(s => s.GetArgumentFromToken("jwt-token", "sub")).Returns("1");

        var effectiveUserRepository = userRepository;
        if (effectiveUserRepository is null)
        {
            var repo = new Mock<IUserRepository>();
            repo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(BuildUser(idUser: 1));
            effectiveUserRepository = repo.Object;
        }

        var controller = new FollowsController(service.Object, tokenService.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };

        controller.HttpContext.RequestServices = new ServiceCollection()
            .AddSingleton(effectiveUserRepository)
            .BuildServiceProvider();
        controller.HttpContext.Request.Headers.Authorization = "Bearer jwt-token";

        return controller;
    }

    private static FollowUser BuildFollowUser(int idUser, string username) => new()
    {
        IdUser = idUser,
        Username = username,
        FirstName = "Name"
    };

    private static User BuildUser(int idUser = 1) => new()
    {
        IdUser = idUser,
        Username = $"user{idUser}",
        Email = $"user{idUser}@example.com",
        FirstName = $"User {idUser}",
        Department = new Department { IdDepartment = 1, Name = "Department 1", Code = 10 },
        IdRole = 1,
        HashedPassword = "hash"
    };
}
