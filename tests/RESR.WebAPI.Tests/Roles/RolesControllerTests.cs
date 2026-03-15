using Microsoft.AspNetCore.Mvc;
using Moq;
using RESR.Core.Controllers.Roles;
using RESR.Core.Errors;
using RESR.Models.Permissions;
using RESR.Models.Roles;
using RESR.WebAPI.Routes.Roles;

namespace RESR.WebAPI.Tests.Roles;

public sealed class RolesControllerTests
{
    [Fact]
    public async Task GetAssignableRoles_ReturnsOk_WithSummaries()
    {
        var service = new Mock<IRoleService>();
        service.Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Role>
            {
                new() { IdRole = 1, Name = "User", Description = "Standard" },
                new() { IdRole = 2, Name = "Admin", Description = "Administration" }
            });

        var controller = new RolesController(service.Object);

        var result = await controller.GetAssignableRoles(CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var list = Assert.IsType<List<RoleSummaryResponse>>(ok.Value);
        Assert.Collection(list,
            first =>
            {
                Assert.Equal(1, first.IdRole);
                Assert.Equal("User", first.Name);
                Assert.Equal("Standard", first.Description);
            },
            second =>
            {
                Assert.Equal(2, second.IdRole);
                Assert.Equal("Admin", second.Name);
                Assert.Equal("Administration", second.Description);
            });
    }

    [Fact]
    public async Task GetAll_ReturnsOk_WithResponses()
    {
        var service = new Mock<IRoleService>();
        service.Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Role>
            {
                new() { IdRole = 1, Name = "User" },
                new() { IdRole = 2, Name = "Admin" }
            });
        service.Setup(s => s.GetPermissionsByRoleIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Permission> { new() { IdPermission = 1, Name = "P" } });
        service.Setup(s => s.GetPermissionsByRoleIdAsync(2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Permission>());

        var controller = new RolesController(service.Object);

        var result = await controller.GetAll(CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var list = Assert.IsType<List<RoleResponse>>(ok.Value);
        Assert.Collection(list,
            first => Assert.Equal(1, first.IdRole),
            second => Assert.Equal(2, second.IdRole));
        Assert.Single(list[0].Permissions);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenMissing()
    {
        var service = new Mock<IRoleService>();
        service.Setup(s => s.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync((Role?)null);
        var controller = new RolesController(service.Object);

        var result = await controller.GetById(1, CancellationToken.None);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task GetById_ReturnsOk_WhenFound()
    {
        var service = new Mock<IRoleService>();
        service.Setup(s => s.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Role { IdRole = 1, Name = "User" });
        service.Setup(s => s.GetPermissionsByRoleIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Permission>());
        var controller = new RolesController(service.Object);

        var result = await controller.GetById(1, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<RoleResponse>(ok.Value);
        Assert.Equal(1, response.IdRole);
    }

    [Fact]
    public async Task GetRolePermissions_ReturnsNotFound_WhenRoleMissing()
    {
        var service = new Mock<IRoleService>();
        service.Setup(s => s.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync((Role?)null);
        var controller = new RolesController(service.Object);

        var result = await controller.GetRolePermissions(1, CancellationToken.None);

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetRolePermissions_ReturnsOk_WhenFound()
    {
        var service = new Mock<IRoleService>();
        service.Setup(s => s.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Role { IdRole = 1, Name = "User" });
        service.Setup(s => s.GetPermissionsByRoleIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Permission> { new() { IdPermission = 1, Name = "P", Description = "D" } });
        var controller = new RolesController(service.Object);

        var result = await controller.GetRolePermissions(1, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var list = Assert.IsType<List<PermissionResponse>>(ok.Value);
        Assert.Single(list);
    }

    [Fact]
    public async Task AddPermissionToRole_ReturnsNoContent_WhenSuccess()
    {
        var service = new Mock<IRoleService>();
        service.Setup(s => s.AddPermissionToRoleAsync(1, 2, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        var controller = new RolesController(service.Object);

        var result = await controller.AddPermissionToRole(1, 2, CancellationToken.None);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task AddPermissionToRole_ReturnsNotFound_WhenMissing()
    {
        var service = new Mock<IRoleService>();
        service.Setup(s => s.AddPermissionToRoleAsync(1, 2, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException("Missing"));
        var controller = new RolesController(service.Object);

        var result = await controller.AddPermissionToRole(1, 2, CancellationToken.None);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task AddPermissionToRole_ReturnsConflict_WhenAlreadyAssigned()
    {
        var service = new Mock<IRoleService>();
        service.Setup(s => s.AddPermissionToRoleAsync(1, 2, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ConflictException("Conflict"));
        var controller = new RolesController(service.Object);

        var result = await controller.AddPermissionToRole(1, 2, CancellationToken.None);

        Assert.IsType<ConflictObjectResult>(result);
    }

    [Fact]
    public async Task RemovePermissionFromRole_ReturnsNoContent_WhenSuccess()
    {
        var service = new Mock<IRoleService>();
        service.Setup(s => s.RemovePermissionFromRoleAsync(1, 2, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        var controller = new RolesController(service.Object);

        var result = await controller.RemovePermissionFromRole(1, 2, CancellationToken.None);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task RemovePermissionFromRole_ReturnsNotFound_WhenMissing()
    {
        var service = new Mock<IRoleService>();
        service.Setup(s => s.RemovePermissionFromRoleAsync(1, 2, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException("Missing"));
        var controller = new RolesController(service.Object);

        var result = await controller.RemovePermissionFromRole(1, 2, CancellationToken.None);

        Assert.IsType<NotFoundObjectResult>(result);
    }
}
