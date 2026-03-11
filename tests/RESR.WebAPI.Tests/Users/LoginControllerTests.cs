using Microsoft.AspNetCore.Mvc;
using Moq;
using RESR.Core.Controllers.Users;
using RESR.Models.Users;
using RESR.WebAPI.Routes.Users;

namespace RESR.WebAPI.Tests.Users;

public sealed class LoginControllerTests
{
    [Fact]
    public async Task LoginUserAsync_ReturnsOk_WithToken()
    {
        var service = new Mock<IUserService>();
        service.Setup(s => s.LoginUserAsync(It.IsAny<Login>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("token");
        var controller = new LoginController(service.Object);

        var result = await controller.LoginUserAsync(new Login("user@example.com", "pass"), CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(ok.Value);
    }

    [Fact]
    public async Task LoginUserAsync_ReturnsUnauthorized_WhenInvalid()
    {
        var service = new Mock<IUserService>();
        service.Setup(s => s.LoginUserAsync(It.IsAny<Login>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Invalid"));
        var controller = new LoginController(service.Object);

        var result = await controller.LoginUserAsync(new Login("user@example.com", "pass"), CancellationToken.None);

        Assert.IsType<UnauthorizedObjectResult>(result);
    }
}
