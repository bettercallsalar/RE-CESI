using Moq;
using RESR.Core.Controllers.Departments.Ports;
using RESR.Core.Controllers.Roles.Ports;
using RESR.Core.Controllers.Users;
using RESR.Core.Controllers.Users.Factories;
using RESR.Core.Controllers.Users.Ports;
using RESR.Models.Departments;
using RESR.Core.Errors;
using RESR.Core.Security.Token;
using RESR.Core.Security.Tools;
using RESR.Models.Permissions;
using RESR.Models.Roles;
using RESR.Models.Users;

namespace RESR.Core.Tests.Users;

public sealed class UserServiceTests
{
    [Fact]
    public async Task GetUsersPaginatedAsync_NormalizesFilters()
    {
        var repo = new Mock<IUserRepository>();
        var roleRepo = new Mock<IRoleRepository>();
        var factory = new Mock<IUserFactory>();
        var passwordHasher = new Mock<IPasswordHasher>();
        var tokenService = new Mock<ITokenService>();

        UserListingFilters? captured = null;
        repo.Setup(r => r.GetUsersPaginatedAsync(1, 5, It.IsAny<UserListingFilters>(), It.IsAny<CancellationToken>()))
            .Callback<int, int, UserListingFilters, CancellationToken>((_, _, f, _) => captured = f)
            .ReturnsAsync(new List<User>());
        repo.Setup(r => r.CountUsersAsync(It.IsAny<UserListingFilters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        var departmentRepo = new Mock<IDepartmentRepository>();
        var service = new UserService(repo.Object, roleRepo.Object, departmentRepo.Object, factory.Object, passwordHasher.Object, tokenService.Object);
        var filters = new UserListingFilters(
            Keyword: "  key ",
            DepartmentIds: new List<int> { 1, 0, 1 },
            RoleIds: new List<int> { 2, -1, 2 },
            BirthDate: null,
            IsVerified: null,
            IncludeDeleted: false
        );

        await service.GetUsersPaginatedAsync(1, 5, filters, CancellationToken.None);

        Assert.NotNull(captured);
        Assert.Equal("key", captured!.Keyword);
        Assert.Equal(new[] { 1 }, captured.DepartmentIds);
        Assert.Equal(new[] { 2 }, captured.RoleIds);
    }

    [Fact]
    public async Task GetUsersPaginatedAsync_DropsEmptyFilterLists()
    {
        var repo = new Mock<IUserRepository>();
        var roleRepo = new Mock<IRoleRepository>();
        var factory = new Mock<IUserFactory>();
        var passwordHasher = new Mock<IPasswordHasher>();
        var tokenService = new Mock<ITokenService>();

        UserListingFilters? captured = null;
        repo.Setup(r => r.GetUsersPaginatedAsync(1, 5, It.IsAny<UserListingFilters>(), It.IsAny<CancellationToken>()))
            .Callback<int, int, UserListingFilters, CancellationToken>((_, _, f, _) => captured = f)
            .ReturnsAsync(new List<User>());
        repo.Setup(r => r.CountUsersAsync(It.IsAny<UserListingFilters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        var departmentRepo = new Mock<IDepartmentRepository>();
        var service = new UserService(repo.Object, roleRepo.Object, departmentRepo.Object, factory.Object, passwordHasher.Object, tokenService.Object);
        var filters = new UserListingFilters(
            Keyword: "   ",
            DepartmentIds: new List<int> { 0, -2 },
            RoleIds: new List<int>(),
            BirthDate: null,
            IsVerified: null,
            IncludeDeleted: false
        );

        await service.GetUsersPaginatedAsync(1, 5, filters, CancellationToken.None);

        Assert.NotNull(captured);
        Assert.Null(captured!.Keyword);
        Assert.Null(captured.DepartmentIds);
        Assert.Null(captured.RoleIds);
    }

    [Fact]
    public async Task GetManageableUsersPaginatedAsync_ForcesUserRoleFilter()
    {
        var repo = new Mock<IUserRepository>();
        var roleRepo = new Mock<IRoleRepository>();
        var factory = new Mock<IUserFactory>();
        var passwordHasher = new Mock<IPasswordHasher>();
        var tokenService = new Mock<ITokenService>();

        UserListingFilters? captured = null;
        repo.Setup(r => r.GetUsersPaginatedAsync(1, 10, It.IsAny<UserListingFilters>(), It.IsAny<CancellationToken>()))
            .Callback<int, int, UserListingFilters, CancellationToken>((_, _, f, _) => captured = f)
            .ReturnsAsync(new List<User>());
        repo.Setup(r => r.CountUsersAsync(It.IsAny<UserListingFilters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        var departmentRepo = new Mock<IDepartmentRepository>();
        var service = new UserService(repo.Object, roleRepo.Object, departmentRepo.Object, factory.Object, passwordHasher.Object, tokenService.Object);

        await service.GetManageableUsersPaginatedAsync(
            1,
            10,
            new UserListingFilters("  user ", new List<int> { 1, 0, 1 }, new List<int> { 2, 3 }, null, null, true),
            CancellationToken.None);

        Assert.NotNull(captured);
        Assert.Equal("user", captured!.Keyword);
        Assert.Equal(new[] { 1 }, captured.DepartmentIds);
        Assert.Equal(new[] { 1 }, captured.RoleIds);
        Assert.False(captured.IncludeDeleted);
    }

    [Fact]
    public async Task GetByIdAsync_DelegatesToRepository()
    {
        var repo = new Mock<IUserRepository>();
        var roleRepo = new Mock<IRoleRepository>();
        var factory = new Mock<IUserFactory>();
        var passwordHasher = new Mock<IPasswordHasher>();
        var tokenService = new Mock<ITokenService>();

        var expected = BuildUser(idUser: 7);
        repo.Setup(r => r.GetByIdAsync(7, It.IsAny<CancellationToken>())).ReturnsAsync(expected);

        var departmentRepo = new Mock<IDepartmentRepository>();
        var service = new UserService(repo.Object, roleRepo.Object, departmentRepo.Object, factory.Object, passwordHasher.Object, tokenService.Object);

        var user = await service.GetByIdAsync(7, CancellationToken.None);

        Assert.Same(expected, user);
    }

    [Fact]
    public async Task RegisterAsync_Throws_WhenFirstNameMissing()
    {
        var service = CreateService(out _, out _, out _, out _, out _);

        await Assert.ThrowsAsync<ValidationException>(() =>
            service.RegisterAsync(NewRegisterCommand(firstName: "  "), CancellationToken.None));
    }

    [Fact]
    public async Task RegisterAsync_Throws_WhenDepartmentInvalid()
    {
        var service = CreateService(out _, out _, out _, out _, out _);

        await Assert.ThrowsAsync<ValidationException>(() =>
            service.RegisterAsync(NewRegisterCommand(idDepartment: 0), CancellationToken.None));
    }

    [Fact]
    public async Task RegisterAsync_Throws_WhenRoleInvalid()
    {
        var service = CreateService(out _, out _, out _, out _, out _);

        await Assert.ThrowsAsync<ValidationException>(() =>
            service.RegisterAsync(NewRegisterCommand(idRole: 0), CancellationToken.None));
    }

    [Fact]
    public async Task RegisterAsync_Throws_WhenEmailExists()
    {
        var service = CreateService(out var repo, out _, out _, out _, out _);
        repo.Setup(r => r.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildUser());

        await Assert.ThrowsAsync<ConflictException>(() =>
            service.RegisterAsync(NewRegisterCommand(), CancellationToken.None));
    }

    [Fact]
    public async Task RegisterAsync_Throws_WhenUsernameExists()
    {
        var service = CreateService(out var repo, out _, out _, out _, out _);
        repo.Setup(r => r.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);
        repo.Setup(r => r.GetByUsernameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildUser());

        await Assert.ThrowsAsync<ConflictException>(() =>
            service.RegisterAsync(NewRegisterCommand(), CancellationToken.None));
    }

    [Fact]
    public async Task RegisterAsync_Throws_WhenRoleMissing()
    {
        var service = CreateService(out var repo, out var roles, out _, out _, out _);
        repo.Setup(r => r.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);
        repo.Setup(r => r.GetByUsernameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);
        roles.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Role?)null);

        await Assert.ThrowsAsync<ValidationException>(() =>
            service.RegisterAsync(NewRegisterCommand(), CancellationToken.None));
    }

    [Fact]
    public async Task RegisterAsync_CreatesUser_WithNormalizedInputs()
    {
        var service = CreateService(out var repo, out var roles, out var departments, out var factory, out var passwordHasher, out _);
        repo.Setup(r => r.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);
        repo.Setup(r => r.GetByUsernameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);
        roles.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Role { IdRole = 2, Name = "Role" });
        departments.Setup(d => d.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Department { IdDepartment = 1, Name = "IT", Code = 10 });

        passwordHasher.Setup(p => p.HashPassword("pass")).Returns("hash");

        factory.Setup(f => f.CreateForRegistration(
                "user",
                "user@example.com",
                "hash",
                "User",
                null,
                null,
                It.Is<Department>(d => d.IdDepartment == 1 && d.Name == "IT" && d.Code == 10),
                2))
            .Returns(BuildUser());

        repo.Setup(r => r.CreateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(99);

        var id = await service.RegisterAsync(NewRegisterCommand(
            username: "  user  ",
            email: "  user@example.com ",
            password: "pass",
            firstName: "  User  ",
            bio: "   "), CancellationToken.None);

        Assert.Equal(99, id);
        factory.VerifyAll();
    }

    [Fact]
    public async Task UpdateAsync_Throws_WhenUserNotFound()
    {
        var service = CreateService(out var repo, out _, out _, out _, out _);
        repo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            service.UpdateAsync(new UpdateUserCommand(IdUser: 1), CancellationToken.None));
    }

    [Fact]
    public async Task UpdateAsync_Throws_WhenUserDeleted()
    {
        var service = CreateService(out var repo, out _, out _, out _, out _);
        var user = BuildUser(deletedAt: DateTime.UtcNow);
        repo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(user);

        await Assert.ThrowsAsync<ValidationException>(() =>
            service.UpdateAsync(new UpdateUserCommand(IdUser: 1), CancellationToken.None));
    }

    [Fact]
    public async Task UpdateAsync_Throws_WhenEmailExists()
    {
        var service = CreateService(out var repo, out _, out _, out _, out _);
        var user = BuildUser(email: "old@example.com");
        repo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        repo.Setup(r => r.GetByEmailAsync("new@example.com", It.IsAny<CancellationToken>())).ReturnsAsync(BuildUser());

        await Assert.ThrowsAsync<ConflictException>(() =>
            service.UpdateAsync(new UpdateUserCommand(IdUser: 1, Email: "new@example.com"), CancellationToken.None));
    }

    [Fact]
    public async Task UpdateAsync_Throws_WhenUsernameExists()
    {
        var service = CreateService(out var repo, out _, out _, out _, out _);
        var user = BuildUser(username: "old");
        repo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        repo.Setup(r => r.GetByUsernameAsync("new", It.IsAny<CancellationToken>())).ReturnsAsync(BuildUser());

        await Assert.ThrowsAsync<ConflictException>(() =>
            service.UpdateAsync(new UpdateUserCommand(IdUser: 1, Username: "new"), CancellationToken.None));
    }

    [Fact]
    public async Task UpdateAsync_Throws_WhenRoleMissing()
    {
        var service = CreateService(out var repo, out var roles, out _, out _, out _);
        var user = BuildUser(idRole: 1);
        repo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        roles.Setup(r => r.GetByIdAsync(2, It.IsAny<CancellationToken>())).ReturnsAsync((Role?)null);

        await Assert.ThrowsAsync<ValidationException>(() =>
            service.UpdateAsync(new UpdateUserCommand(IdUser: 1, IdRole: 2), CancellationToken.None));
    }

    [Fact]
    public async Task UpdateAsync_NormalizesAndPatches()
    {
        var service = CreateService(out var repo, out var roles, out _, out _, out _);
        var user = BuildUser(email: "old@example.com", username: "old", idRole: 1);
        repo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        repo.Setup(r => r.GetByEmailAsync("new@example.com", It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);
        repo.Setup(r => r.GetByUsernameAsync("new", It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);
        roles.Setup(r => r.GetByIdAsync(2, It.IsAny<CancellationToken>())).ReturnsAsync(new Role { IdRole = 2, Name = "Role" });

        UpdateUserCommand? captured = null;
        repo.Setup(r => r.PatchAsync(It.IsAny<UpdateUserCommand>(), It.IsAny<CancellationToken>()))
            .Callback<UpdateUserCommand, CancellationToken>((cmd, _) => captured = cmd)
            .ReturnsAsync(BuildUser(username: "new", email: "new@example.com"));

        var result = await service.UpdateAsync(new UpdateUserCommand(
            IdUser: 1,
            Username: "  new ",
            Email: "  new@example.com ",
            FirstName: "  New  ",
            Bio: "   ",
            IdRole: 2
        ), CancellationToken.None);

        Assert.NotNull(captured);
        Assert.Equal("new", captured!.Username);
        Assert.Equal("new@example.com", captured.Email);
        Assert.Equal("New", captured.FirstName);
        Assert.Null(captured.Bio);
        Assert.Equal(2, captured.IdRole);
        Assert.Equal("new", result.Username);
    }

    [Fact]
    public async Task SetVerificationAsync_Throws_WhenUserMissing()
    {
        var service = CreateService(out var repo, out _, out _, out _, out _);
        repo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            service.SetVerificationAsync(new SetUserVerificationCommand(1, true), CancellationToken.None));
    }

    [Fact]
    public async Task SetVerificationAsync_Throws_WhenUserDeleted()
    {
        var service = CreateService(out var repo, out _, out _, out _, out _);
        repo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(BuildUser(deletedAt: DateTime.UtcNow));

        await Assert.ThrowsAsync<ValidationException>(() =>
            service.SetVerificationAsync(new SetUserVerificationCommand(1, true), CancellationToken.None));
    }

    [Fact]
    public async Task SetVerificationAsync_Throws_WhenUserBanned()
    {
        var service = CreateService(out var repo, out _, out _, out _, out _);
        repo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(BuildUser(isBanned: true));

        await Assert.ThrowsAsync<ValidationException>(() =>
            service.SetVerificationAsync(new SetUserVerificationCommand(1, true), CancellationToken.None));
    }

    [Fact]
    public async Task SetVerificationAsync_ReturnsExisting_WhenAlreadyVerified()
    {
        var service = CreateService(out var repo, out _, out _, out _, out _);
        var user = BuildUser(isVerified: true);
        repo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(user);

        var result = await service.SetVerificationAsync(new SetUserVerificationCommand(1, true), CancellationToken.None);

        Assert.Same(user, result);
        repo.Verify(r => r.SetVerificationAsync(It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SetVerificationAsync_Updates_WhenDifferent()
    {
        var service = CreateService(out var repo, out _, out _, out _, out _);
        repo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(BuildUser(isVerified: false));
        repo.Setup(r => r.SetVerificationAsync(1, true, It.IsAny<CancellationToken>())).ReturnsAsync(BuildUser(isVerified: true));

        var result = await service.SetVerificationAsync(new SetUserVerificationCommand(1, true), CancellationToken.None);

        Assert.True(result.IsVerified);
    }

    [Fact]
    public async Task SoftDeleteAsync_DelegatesToRepository()
    {
        var service = CreateService(out var repo, out _, out _, out _, out _);
        repo.Setup(r => r.SoftDeleteAsync(5, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var ok = await service.SoftDeleteAsync(5, CancellationToken.None);

        Assert.True(ok);
    }

    [Fact]
    public async Task BanManageableUserAsync_Throws_WhenUserMissing()
    {
        var service = CreateService(out var repo, out _, out _, out _, out _);
        repo.Setup(r => r.GetByIdAsync(5, It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => service.BanManageableUserAsync(5, CancellationToken.None));
    }

    [Fact]
    public async Task BanManageableUserAsync_Throws_WhenUserIsAdmin()
    {
        var service = CreateService(out var repo, out _, out _, out _, out _);
        repo.Setup(r => r.GetByIdAsync(5, It.IsAny<CancellationToken>())).ReturnsAsync(BuildUser(idUser: 5, idRole: 2));

        await Assert.ThrowsAsync<NotFoundException>(() => service.BanManageableUserAsync(5, CancellationToken.None));
    }

    [Fact]
    public async Task BanManageableUserAsync_BansStandardUser()
    {
        var service = CreateService(out var repo, out _, out _, out _, out _);
        repo.Setup(r => r.GetByIdAsync(5, It.IsAny<CancellationToken>())).ReturnsAsync(BuildUser(idUser: 5, idRole: 1));
        repo.Setup(r => r.SetBannedAsync(5, true, It.IsAny<CancellationToken>())).ReturnsAsync(BuildUser(idUser: 5, idRole: 1, isBanned: true));

        await service.BanManageableUserAsync(5, CancellationToken.None);

        repo.Verify(r => r.SetBannedAsync(5, true, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task BanManageableUserAsync_DoesNothing_WhenAlreadyBanned()
    {
        var service = CreateService(out var repo, out _, out _, out _, out _);
        repo.Setup(r => r.GetByIdAsync(5, It.IsAny<CancellationToken>())).ReturnsAsync(BuildUser(idUser: 5, idRole: 1, isBanned: true));

        await service.BanManageableUserAsync(5, CancellationToken.None);

        repo.Verify(r => r.SetBannedAsync(It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SetManageableUserBanStatusAsync_UnbansStandardUser()
    {
        var service = CreateService(out var repo, out _, out _, out _, out _);
        repo.Setup(r => r.GetByIdAsync(5, It.IsAny<CancellationToken>())).ReturnsAsync(BuildUser(idUser: 5, idRole: 1, isBanned: true));
        repo.Setup(r => r.SetBannedAsync(5, false, It.IsAny<CancellationToken>())).ReturnsAsync(BuildUser(idUser: 5, idRole: 1, isBanned: false));

        await service.SetManageableUserBanStatusAsync(5, false, CancellationToken.None);

        repo.Verify(r => r.SetBannedAsync(5, false, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SetManageableUserBanStatusAsync_DoesNothing_WhenStatusIsAlreadyApplied()
    {
        var service = CreateService(out var repo, out _, out _, out _, out _);
        repo.Setup(r => r.GetByIdAsync(5, It.IsAny<CancellationToken>())).ReturnsAsync(BuildUser(idUser: 5, idRole: 1, isBanned: false));

        await service.SetManageableUserBanStatusAsync(5, false, CancellationToken.None);

        repo.Verify(r => r.SetBannedAsync(It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task LoginUserAsync_Throws_WhenEmailMissing()
    {
        var service = CreateService(out var repo, out _, out _, out _, out _);
        repo.Setup(r => r.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.LoginUserAsync(new Login("user@example.com", "pass"), CancellationToken.None));
    }

    [Fact]
    public async Task LoginUserAsync_Throws_WhenPasswordInvalid()
    {
        var service = CreateService(out var repo, out _, out _, out var hasher, out _);
        repo.Setup(r => r.GetByEmailAsync("user@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildUser());
        hasher.Setup(h => h.VerifyPassword("hash", "pass")).Returns(false);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.LoginUserAsync(new Login("user@example.com", "pass"), CancellationToken.None));
    }

    [Fact]
    public async Task LoginUserAsync_Throws_WhenNotVerified()
    {
        var service = CreateService(out var repo, out _, out _, out var hasher, out _);
        repo.Setup(r => r.GetByEmailAsync("user@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildUser(isVerified: false));
        hasher.Setup(h => h.VerifyPassword("hash", "pass")).Returns(true);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.LoginUserAsync(new Login("user@example.com", "pass"), CancellationToken.None));
    }

    [Fact]
    public async Task LoginUserAsync_Throws_WhenDeleted()
    {
        var service = CreateService(out var repo, out _, out _, out var hasher, out _);
        repo.Setup(r => r.GetByEmailAsync("user@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildUser(isVerified: true, deletedAt: DateTime.UtcNow));
        hasher.Setup(h => h.VerifyPassword("hash", "pass")).Returns(true);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.LoginUserAsync(new Login("user@example.com", "pass"), CancellationToken.None));
    }

    [Fact]
    public async Task LoginUserAsync_Throws_WhenBanned()
    {
        var service = CreateService(out var repo, out _, out _, out var hasher, out _);
        repo.Setup(r => r.GetByEmailAsync("user@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(BuildUser(isVerified: true, isBanned: true));
        hasher.Setup(h => h.VerifyPassword("hash", "pass")).Returns(true);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.LoginUserAsync(new Login("user@example.com", "pass"), CancellationToken.None));
    }

    [Fact]
    public async Task LoginUserAsync_ReturnsToken_WhenValid()
    {
        var service = CreateService(out var repo, out var roles, out _, out var hasher, out var tokens);
        var user = BuildUser(idRole: 2, isVerified: true);
        repo.Setup(r => r.GetByEmailAsync("user@example.com", It.IsAny<CancellationToken>())).ReturnsAsync(user);
        hasher.Setup(h => h.VerifyPassword("hash", "pass")).Returns(true);
        roles.Setup(r => r.GetPermissionsByRoleIdAsync(2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Permission> { new() { IdPermission = 1, Name = "P" } });
        tokens.Setup(t => t.GenerateUserToken(user, It.IsAny<IReadOnlyList<Permission>>())).Returns("token");

        var token = await service.LoginUserAsync(new Login("user@example.com", "pass"), CancellationToken.None);

        Assert.Equal("token", token);
    }

    private static UserService CreateService(
        out Mock<IUserRepository> repo,
        out Mock<IRoleRepository> roleRepo,
        out Mock<IDepartmentRepository> departmentRepo,
        out Mock<IUserFactory> factory,
        out Mock<IPasswordHasher> passwordHasher,
        out Mock<ITokenService> tokenService)
    {
        repo = new Mock<IUserRepository>();
        roleRepo = new Mock<IRoleRepository>();
        departmentRepo = new Mock<IDepartmentRepository>();
        factory = new Mock<IUserFactory>();
        passwordHasher = new Mock<IPasswordHasher>();
        tokenService = new Mock<ITokenService>();

        return new UserService(repo.Object, roleRepo.Object, departmentRepo.Object, factory.Object, passwordHasher.Object, tokenService.Object);
    }

    private static UserService CreateService(
        out Mock<IUserRepository> repo,
        out Mock<IRoleRepository> roleRepo,
        out Mock<IUserFactory> factory,
        out Mock<IPasswordHasher> passwordHasher,
        out Mock<ITokenService> tokenService)
    {
        return CreateService(out repo, out roleRepo, out _, out factory, out passwordHasher, out tokenService);
    }

    private static RegisterUserCommand NewRegisterCommand(
        string username = "user",
        string email = "user@example.com",
        string password = "pass",
        string firstName = "User",
        DateOnly? birthDate = null,
        string? bio = null,
        int idDepartment = 1,
        int idRole = 2
    ) => new(username, email, password, firstName, birthDate, bio, idDepartment, idRole);

    private static User BuildUser(
        int idUser = 1,
        string username = "user",
        string email = "user@example.com",
        string firstName = "User",
        string hashedPassword = "hash",
        int idDepartment = 1,
        int idRole = 1,
        bool isVerified = true,
        bool isBanned = false,
        DateTime? deletedAt = null
    ) => new()
    {
        IdUser = idUser,
        Username = username,
        Email = email,
        FirstName = firstName,
        HashedPassword = hashedPassword,
        Department = new Department { IdDepartment = idDepartment, Name = $"Department {idDepartment}", Code = idDepartment * 10 },
        IdRole = idRole,
        IsVerified = isVerified,
        IsBanned = isBanned,
        DeletedAt = deletedAt
    };
}
