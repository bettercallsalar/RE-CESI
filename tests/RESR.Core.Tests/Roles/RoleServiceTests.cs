using Moq;
using RESR.Core.Controllers.Permissions.Ports;
using RESR.Core.Controllers.Roles;
using RESR.Core.Controllers.Roles.Ports;
using RESR.Core.Errors;
using RESR.Models.Permissions;
using RESR.Models.Roles;

namespace RESR.Core.Tests.Roles;

public sealed class RoleServiceTests
{
    [Fact]
    public async Task GetAllAsync_DelegatesToRepository()
    {
        var repo = new Mock<IRoleRepository>();
        var permRepo = new Mock<IPermissionRepository>();
        var expected = new List<Role> { new() { IdRole = 1, Name = "User" } };
        repo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(expected);

        var service = new RoleService(repo.Object, permRepo.Object);

        var roles = await service.GetAllAsync(CancellationToken.None);

        Assert.Same(expected, roles);
    }

    [Fact]
    public async Task GetByIdAsync_DelegatesToRepository()
    {
        var repo = new Mock<IRoleRepository>();
        var permRepo = new Mock<IPermissionRepository>();
        var expected = new Role { IdRole = 2, Name = "Admin" };
        repo.Setup(r => r.GetByIdAsync(2, It.IsAny<CancellationToken>())).ReturnsAsync(expected);

        var service = new RoleService(repo.Object, permRepo.Object);

        var role = await service.GetByIdAsync(2, CancellationToken.None);

        Assert.Same(expected, role);
    }

    [Fact]
    public async Task GetPermissionsByRoleIdAsync_DelegatesToRepository()
    {
        var repo = new Mock<IRoleRepository>();
        var permRepo = new Mock<IPermissionRepository>();
        var expected = new List<Permission> { new() { IdPermission = 1, Name = "P" } };
        repo.Setup(r => r.GetPermissionsByRoleIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(expected);

        var service = new RoleService(repo.Object, permRepo.Object);

        var permissions = await service.GetPermissionsByRoleIdAsync(1, CancellationToken.None);

        Assert.Same(expected, permissions);
    }

    [Fact]
    public async Task AddPermissionToRoleAsync_Throws_WhenRoleMissing()
    {
        var repo = new Mock<IRoleRepository>();
        var permRepo = new Mock<IPermissionRepository>();
        repo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync((Role?)null);

        var service = new RoleService(repo.Object, permRepo.Object);

        await Assert.ThrowsAsync<NotFoundException>(() => service.AddPermissionToRoleAsync(1, 2, CancellationToken.None));
    }

    [Fact]
    public async Task AddPermissionToRoleAsync_Throws_WhenPermissionMissing()
    {
        var repo = new Mock<IRoleRepository>();
        var permRepo = new Mock<IPermissionRepository>();
        repo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(new Role { IdRole = 1, Name = "Role" });
        permRepo.Setup(r => r.GetByIdAsync(2, It.IsAny<CancellationToken>())).ReturnsAsync((Permission?)null);

        var service = new RoleService(repo.Object, permRepo.Object);

        await Assert.ThrowsAsync<NotFoundException>(() => service.AddPermissionToRoleAsync(1, 2, CancellationToken.None));
    }

    [Fact]
    public async Task AddPermissionToRoleAsync_Throws_WhenAlreadyAssigned()
    {
        var repo = new Mock<IRoleRepository>();
        var permRepo = new Mock<IPermissionRepository>();
        repo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(new Role { IdRole = 1, Name = "Role" });
        permRepo.Setup(r => r.GetByIdAsync(2, It.IsAny<CancellationToken>())).ReturnsAsync(new Permission { IdPermission = 2, Name = "Perm" });
        repo.Setup(r => r.AddPermissionToRoleAsync(1, 2, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var service = new RoleService(repo.Object, permRepo.Object);

        await Assert.ThrowsAsync<ConflictException>(() => service.AddPermissionToRoleAsync(1, 2, CancellationToken.None));
    }

    [Fact]
    public async Task AddPermissionToRoleAsync_Adds_WhenValid()
    {
        var repo = new Mock<IRoleRepository>();
        var permRepo = new Mock<IPermissionRepository>();
        repo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(new Role { IdRole = 1, Name = "Role" });
        permRepo.Setup(r => r.GetByIdAsync(2, It.IsAny<CancellationToken>())).ReturnsAsync(new Permission { IdPermission = 2, Name = "Perm" });
        repo.Setup(r => r.AddPermissionToRoleAsync(1, 2, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var service = new RoleService(repo.Object, permRepo.Object);

        await service.AddPermissionToRoleAsync(1, 2, CancellationToken.None);
    }

    [Fact]
    public async Task RemovePermissionFromRoleAsync_Throws_WhenRoleMissing()
    {
        var repo = new Mock<IRoleRepository>();
        var permRepo = new Mock<IPermissionRepository>();
        repo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync((Role?)null);

        var service = new RoleService(repo.Object, permRepo.Object);

        await Assert.ThrowsAsync<NotFoundException>(() => service.RemovePermissionFromRoleAsync(1, 2, CancellationToken.None));
    }

    [Fact]
    public async Task RemovePermissionFromRoleAsync_Throws_WhenPermissionMissing()
    {
        var repo = new Mock<IRoleRepository>();
        var permRepo = new Mock<IPermissionRepository>();
        repo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(new Role { IdRole = 1, Name = "Role" });
        permRepo.Setup(r => r.GetByIdAsync(2, It.IsAny<CancellationToken>())).ReturnsAsync((Permission?)null);

        var service = new RoleService(repo.Object, permRepo.Object);

        await Assert.ThrowsAsync<NotFoundException>(() => service.RemovePermissionFromRoleAsync(1, 2, CancellationToken.None));
    }

    [Fact]
    public async Task RemovePermissionFromRoleAsync_Throws_WhenNotAssigned()
    {
        var repo = new Mock<IRoleRepository>();
        var permRepo = new Mock<IPermissionRepository>();
        repo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(new Role { IdRole = 1, Name = "Role" });
        permRepo.Setup(r => r.GetByIdAsync(2, It.IsAny<CancellationToken>())).ReturnsAsync(new Permission { IdPermission = 2, Name = "Perm" });
        repo.Setup(r => r.RemovePermissionFromRoleAsync(1, 2, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var service = new RoleService(repo.Object, permRepo.Object);

        await Assert.ThrowsAsync<NotFoundException>(() => service.RemovePermissionFromRoleAsync(1, 2, CancellationToken.None));
    }

    [Fact]
    public async Task RemovePermissionFromRoleAsync_Removes_WhenValid()
    {
        var repo = new Mock<IRoleRepository>();
        var permRepo = new Mock<IPermissionRepository>();
        repo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(new Role { IdRole = 1, Name = "Role" });
        permRepo.Setup(r => r.GetByIdAsync(2, It.IsAny<CancellationToken>())).ReturnsAsync(new Permission { IdPermission = 2, Name = "Perm" });
        repo.Setup(r => r.RemovePermissionFromRoleAsync(1, 2, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var service = new RoleService(repo.Object, permRepo.Object);

        await service.RemovePermissionFromRoleAsync(1, 2, CancellationToken.None);
    }
}
