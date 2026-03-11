using Microsoft.AspNetCore.Mvc;
using Moq;
using RESR.Core.Controllers.Permissions;
using RESR.Models.Permissions;
using RESR.WebAPI.Routes.Permissions;

namespace RESR.WebAPI.Tests.Permissions;

public sealed class PermissionsControllerTests
{
    [Fact]
    public async Task GetAll_ReturnsOk_WithList()
    {
        var service = new Mock<IPermissionService>();
        service.Setup(s => s.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Permission> { new() { IdPermission = 1, Name = "P", Description = "D" } });
        var controller = new PermissionsController(service.Object);

        var result = await controller.GetAll(CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var list = Assert.IsType<List<PermissionResponse>>(ok.Value);
        Assert.Single(list);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenMissing()
    {
        var service = new Mock<IPermissionService>();
        service.Setup(s => s.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync((Permission?)null);
        var controller = new PermissionsController(service.Object);

        var result = await controller.GetById(1, CancellationToken.None);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task GetById_ReturnsOk_WhenFound()
    {
        var service = new Mock<IPermissionService>();
        service.Setup(s => s.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Permission { IdPermission = 1, Name = "P", Description = string.Empty });
        var controller = new PermissionsController(service.Object);

        var result = await controller.GetById(1, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<PermissionResponse>(ok.Value);
        Assert.Equal(1, response.IdPermission);
    }
}
