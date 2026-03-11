using Moq;
using RESR.Core.Controllers.Permissions;
using RESR.Core.Controllers.Permissions.Ports;
using RESR.Models.Permissions;

namespace RESR.Core.Tests.Permissions;

public sealed class PermissionServiceTests
{
    [Fact]
    public async Task GetAllAsync_DelegatesToRepository()
    {
        var repo = new Mock<IPermissionRepository>();
        var expected = new List<Permission> { new() { IdPermission = 1, Name = "P", Description = "D" } };
        repo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(expected);

        var service = new PermissionService(repo.Object);

        var permissions = await service.GetAllAsync(CancellationToken.None);

        Assert.Same(expected, permissions);
    }

    [Fact]
    public async Task GetByIdAsync_DelegatesToRepository()
    {
        var repo = new Mock<IPermissionRepository>();
        var expected = new Permission { IdPermission = 2, Name = "P2", Description = "D2" };
        repo.Setup(r => r.GetByIdAsync(2, It.IsAny<CancellationToken>())).ReturnsAsync(expected);

        var service = new PermissionService(repo.Object);

        var permission = await service.GetByIdAsync(2, CancellationToken.None);

        Assert.Same(expected, permission);
    }
}
