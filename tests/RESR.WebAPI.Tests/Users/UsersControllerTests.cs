using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using RESR.Core.Controllers.Users;
using RESR.Core.Controllers.Users.Ports;
using RESR.Core.Errors;
using RESR.Core.Security.Token;
using RESR.Models.Departments;
using RESR.Models.Users;
using RESR.WebAPI.Routes.Users;

namespace RESR.WebAPI.Tests.Users;

public sealed class UsersControllerTests
{
    [Fact]
    public async Task GetUsersPaginated_ReturnsBadRequest_WhenPageInvalid()
    {
        var controller = CreateController(out _);

        var result = await controller.GetUsersPaginated(page: 0, pageSize: 20, keyword: null, departmentIds: null, roleIds: null, birthDate: null, isVerified: null, includeDeleted: false, ct: CancellationToken.None);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal(400, badRequest.StatusCode);
    }

    [Fact]
    public async Task GetUsersPaginated_ReturnsBadRequest_WhenDepartmentIdsInvalid()
    {
        var controller = CreateController(out _);

        var result = await controller.GetUsersPaginated(page: 1, pageSize: 20, keyword: null, departmentIds: new List<int> { 0 }, roleIds: null, birthDate: null, isVerified: null, includeDeleted: false, ct: CancellationToken.None);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal(400, badRequest.StatusCode);
    }

    [Fact]
    public async Task GetUsersPaginated_ReturnsOk_WithPagination()
    {
        var controller = CreateController(out var service);
        service.Setup(s => s.GetUsersPaginatedAsync(1, 10, It.IsAny<UserListingFilters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<User> { BuildUser() }, 25));

        var result = await controller.GetUsersPaginated(page: 1, pageSize: 10, keyword: null, departmentIds: null, roleIds: null, birthDate: null, isVerified: null, includeDeleted: false, ct: CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<PaginatedUsersResponse>(ok.Value);
        Assert.Equal(1, response.Page);
        Assert.Equal(10, response.PageSize);
        Assert.Equal(25, response.TotalCount);
        Assert.Equal(3, response.TotalPages);
        Assert.Single(response.Items);
    }

    [Fact]
    public async Task GetUsersPaginated_ReturnsZeroTotalPages_WhenEmpty()
    {
        var controller = CreateController(out var service);
        service.Setup(s => s.GetUsersPaginatedAsync(1, 10, It.IsAny<UserListingFilters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<User>(), 0));

        var result = await controller.GetUsersPaginated(page: 1, pageSize: 10, keyword: null, departmentIds: null, roleIds: null, birthDate: null, isVerified: null, includeDeleted: false, ct: CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<PaginatedUsersResponse>(ok.Value);
        Assert.Equal(0, response.TotalPages);
    }

    [Fact]
    public async Task GetManageableUsersPaginated_ReturnsBadRequest_WhenDepartmentIdsInvalid()
    {
        var controller = CreateController(out _);

        var result = await controller.GetManageableUsersPaginated(page: 1, pageSize: 20, keyword: null, departmentIds: new List<int> { 0 }, birthDate: null, isVerified: null, ct: CancellationToken.None);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal(400, badRequest.StatusCode);
    }

    [Fact]
    public async Task GetManageableUsersPaginated_ReturnsOk_WithPagination()
    {
        var controller = CreateController(out var service);
        service.Setup(s => s.GetManageableUsersPaginatedAsync(1, 10, It.IsAny<UserListingFilters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<User> { BuildUser(idRole: 1) }, 12));

        var result = await controller.GetManageableUsersPaginated(page: 1, pageSize: 10, keyword: null, departmentIds: null, birthDate: null, isVerified: null, ct: CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<PaginatedUsersResponse>(ok.Value);
        Assert.Single(response.Items);
        Assert.Equal(12, response.TotalCount);
        Assert.Equal(2, response.TotalPages);
        Assert.All(response.Items, item => Assert.Equal(1, item.IdRole));
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenMissing()
    {
        var controller = CreateController(out var service);
        service.Setup(s => s.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);

        var result = await controller.GetById(1, CancellationToken.None);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task GetById_ReturnsOk_WhenFound()
    {
        var controller = CreateController(out var service);
        service.Setup(s => s.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(BuildUser());

        var result = await controller.GetById(1, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<UserResponse>(ok.Value);
        Assert.Equal(1, response.IdUser);
        Assert.Equal(1, response.Department.IdDepartment);
        Assert.False(response.IsBanned);
    }

    [Fact]
    public async Task GetOwnProfile_ReturnsOk_WhenTokenUserExists()
    {
        var controller = CreateAuthenticatedController(out var service);
        service.Setup(s => s.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(BuildUser());

        var result = await controller.GetOwnProfile(CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<UserResponse>(ok.Value);
        Assert.Equal(1, response.IdUser);
        Assert.False(response.IsBanned);
    }

    [Fact]
    public async Task GetOwnProfile_ReturnsUnauthorized_WhenTokenUserIsBanned()
    {
        var userRepository = new Mock<IUserRepository>();
        userRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildUser(isBanned: true));

        var controller = CreateAuthenticatedController(out var service, userRepository.Object);

        var result = await controller.GetOwnProfile(CancellationToken.None);

        Assert.IsType<UnauthorizedObjectResult>(result.Result);
        service.Verify(s => s.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Register_ReturnsCreated_WhenSuccess()
    {
        var controller = CreateController(out var service);
        service.Setup(s => s.RegisterAsync(It.IsAny<RegisterUserCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(42);

        var result = await controller.Register(new RegisterUserRequest(
            "user",
            "user@example.com",
            "pass",
            "User",
            null,
            null,
            1
        ), CancellationToken.None);

        var created = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(nameof(UsersController.GetById), created.ActionName);
    }

    [Fact]
    public async Task Register_ReturnsConflict_WhenServiceThrows()
    {
        var controller = CreateController(out var service);
        service.Setup(s => s.RegisterAsync(It.IsAny<RegisterUserCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ConflictException("Email already exists"));

        var result = await controller.Register(new RegisterUserRequest(
            "user",
            "user@example.com",
            "pass",
            "User",
            null,
            null,
            1
        ), CancellationToken.None);

        Assert.IsType<ConflictObjectResult>(result);
    }

    [Fact]
    public async Task Register_ReturnsBadRequest_WhenServiceThrowsValidation()
    {
        var controller = CreateController(out var service);
        service.Setup(s => s.RegisterAsync(It.IsAny<RegisterUserCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ValidationException("Bad"));

        var result = await controller.Register(new RegisterUserRequest(
            "user",
            "user@example.com",
            "pass",
            "User",
            null,
            null,
            1
        ), CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task SetVerification_ReturnsOk_WhenSuccess()
    {
        var controller = CreateController(out var service);
        service.Setup(s => s.SetVerificationAsync(It.IsAny<SetUserVerificationCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildUser(isVerified: true));

        var result = await controller.SetVerification(1, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<UserResponse>(ok.Value);
        Assert.True(response.IsVerified);
    }

    [Fact]
    public async Task SetVerification_ReturnsNotFound_WhenMissing()
    {
        var controller = CreateController(out var service);
        service.Setup(s => s.SetVerificationAsync(It.IsAny<SetUserVerificationCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException("Missing"));

        var result = await controller.SetVerification(1, CancellationToken.None);

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task SetVerification_ReturnsBadRequest_WhenValidationFails()
    {
        var controller = CreateController(out var service);
        service.Setup(s => s.SetVerificationAsync(It.IsAny<SetUserVerificationCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ValidationException("Bad"));

        var result = await controller.SetVerification(1, CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task UpdateRoleOfUser_ReturnsBadRequest_WhenRoleMissing()
    {
        var controller = CreateController(out _);

        var result = await controller.UpdateRoleOfUser(1, new UpdateUserRequest(null, null, null, null, null, null, null), CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task UpdateRoleOfUser_ReturnsOk_WhenSuccess()
    {
        var controller = CreateController(out var service);
        service.Setup(s => s.UpdateAsync(It.IsAny<UpdateUserCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildUser(idRole: 2));

        var result = await controller.UpdateRoleOfUser(1, new UpdateUserRequest(null, null, null, null, null, null, 2), CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<UserResponse>(ok.Value);
        Assert.Equal(2, response.IdRole);
    }

    [Fact]
    public async Task UpdateRoleOfUser_ReturnsNotFound_WhenMissing()
    {
        var controller = CreateController(out var service);
        service.Setup(s => s.UpdateAsync(It.IsAny<UpdateUserCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException("Missing"));

        var result = await controller.UpdateRoleOfUser(1, new UpdateUserRequest(null, null, null, null, null, null, 2), CancellationToken.None);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task UpdateOwnProfile_ReturnsBadRequest_WhenNoFields()
    {
        var controller = CreateAuthenticatedController(out _);

        var result = await controller.UpdateOwnProfile(new UpdateOwnProfileRequest(null, null, null, null, null, null), CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task UpdateOwnProfile_ReturnsOk_WhenSuccess()
    {
        var controller = CreateAuthenticatedController(out var service);
        service.Setup(s => s.UpdateAsync(It.IsAny<UpdateUserCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildUser(username: "new"));

        var result = await controller.UpdateOwnProfile(new UpdateOwnProfileRequest("new", null, null, null, null, null), CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<UserResponse>(ok.Value);
        Assert.Equal("new", response.Username);
    }

    [Fact]
    public async Task UpdateOwnProfile_UsesTokenUserId()
    {
        var controller = CreateAuthenticatedController(out var service);
        service.Setup(s => s.UpdateAsync(It.IsAny<UpdateUserCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildUser(username: "new"));

        await controller.UpdateOwnProfile(new UpdateOwnProfileRequest("new", null, null, null, null, null), CancellationToken.None);

        service.Verify(s => s.UpdateAsync(
            It.Is<UpdateUserCommand>(cmd => cmd.IdUser == 1 && cmd.Username == "new"),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateOwnProfile_SetsClearBio_WhenBioIsBlank()
    {
        var controller = CreateAuthenticatedController(out var service);
        service.Setup(s => s.UpdateAsync(It.IsAny<UpdateUserCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildUser());

        await controller.UpdateOwnProfile(new UpdateOwnProfileRequest(null, null, null, null, "   ", null), CancellationToken.None);

        service.Verify(s => s.UpdateAsync(
            It.Is<UpdateUserCommand>(cmd => cmd.IdUser == 1 && cmd.Bio == "   " && cmd.ClearBio),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateOwnProfile_ReturnsUnauthorized_WhenTokenMissing()
    {
        var controller = CreateController(out _);

        var result = await controller.UpdateOwnProfile(new UpdateOwnProfileRequest("new", null, null, null, null, null), CancellationToken.None);

        Assert.IsType<UnauthorizedObjectResult>(result);
    }

    [Fact]
    public async Task UpdateOwnProfile_ReturnsNotFound_WhenMissing()
    {
        var controller = CreateAuthenticatedController(out var service);
        service.Setup(s => s.UpdateAsync(It.IsAny<UpdateUserCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException("Missing"));

        var result = await controller.UpdateOwnProfile(new UpdateOwnProfileRequest("new", null, null, null, null, null), CancellationToken.None);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task UpdateOwnProfile_ReturnsBadRequest_WhenValidationFails()
    {
        var controller = CreateAuthenticatedController(out var service);
        service.Setup(s => s.UpdateAsync(It.IsAny<UpdateUserCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ValidationException("Bad"));

        var result = await controller.UpdateOwnProfile(new UpdateOwnProfileRequest("new", null, null, null, null, null), CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task UpdateOwnProfile_ReturnsConflict_WhenConflict()
    {
        var controller = CreateAuthenticatedController(out var service);
        service.Setup(s => s.UpdateAsync(It.IsAny<UpdateUserCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ConflictException("Conflict"));

        var result = await controller.UpdateOwnProfile(new UpdateOwnProfileRequest("new", null, null, null, null, null), CancellationToken.None);

        Assert.IsType<ConflictObjectResult>(result);
    }

    [Fact]
    public async Task SoftDelete_ReturnsNoContent_WhenDeleted()
    {
        var controller = CreateAuthenticatedController(out var service);
        service.Setup(s => s.SoftDeleteAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var result = await controller.SoftDelete(CancellationToken.None);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task SoftDelete_ReturnsNotFound_WhenMissing()
    {
        var controller = CreateAuthenticatedController(out var service);
        service.Setup(s => s.SoftDeleteAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var result = await controller.SoftDelete(CancellationToken.None);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task BanManageableUser_ReturnsNoContent_WhenDeleted()
    {
        var controller = CreateController(out var service);
        service.Setup(s => s.BanManageableUserAsync(5, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await controller.BanManageableUser(5, CancellationToken.None);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task BanManageableUser_ReturnsNotFound_WhenMissing()
    {
        var controller = CreateController(out var service);
        service.Setup(s => s.BanManageableUserAsync(5, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException("User 5 not found."));

        var result = await controller.BanManageableUser(5, CancellationToken.None);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task SetManageableUserBanStatus_ReturnsNoContent_WhenUpdated()
    {
        var controller = CreateController(out var service);
        service.Setup(s => s.SetManageableUserBanStatusAsync(5, false, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await controller.SetManageableUserBanStatus(5, new SetUserBanRequest(false), CancellationToken.None);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task SetManageableUserBanStatus_ReturnsNotFound_WhenMissing()
    {
        var controller = CreateController(out var service);
        service.Setup(s => s.SetManageableUserBanStatusAsync(5, false, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException("User 5 not found."));

        var result = await controller.SetManageableUserBanStatus(5, new SetUserBanRequest(false), CancellationToken.None);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    private static UsersController CreateController(out Mock<IUserService> service)
    {
        service = new Mock<IUserService>();
        return new UsersController(service.Object, Mock.Of<ITokenService>());
    }

    private static UsersController CreateAuthenticatedController(out Mock<IUserService> service, IUserRepository? userRepository = null)
    {
        service = new Mock<IUserService>();
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

        var controller = new UsersController(service.Object, tokenService.Object)
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

    private static User BuildUser(
        int idUser = 1,
        string username = "user",
        string email = "user@example.com",
        string firstName = "User",
        bool isVerified = false,
        bool isBanned = false,
        int idDepartment = 1,
        int idRole = 1,
        DateOnly? birthDate = null,
        string? bio = null
    ) => new()
    {
        IdUser = idUser,
        Username = username,
        Email = email,
        FirstName = firstName,
        IsVerified = isVerified,
        IsBanned = isBanned,
        Department = new Department { IdDepartment = idDepartment, Name = $"Department {idDepartment}", Code = idDepartment * 10 },
        IdRole = idRole,
        BirthDate = birthDate,
        Bio = bio,
        HashedPassword = "hash"
    };
}
