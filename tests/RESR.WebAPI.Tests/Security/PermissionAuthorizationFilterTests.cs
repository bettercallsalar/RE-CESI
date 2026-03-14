using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using RESR.Core.Controllers.Users.Ports;
using RESR.Core.Security.Token;
using RESR.Models.Departments;
using RESR.Models.Users;
using RESR.WebAPI.Security;

namespace RESR.WebAPI.Tests.Security;

public sealed class PermissionAuthorizationFilterTests
{
    [Fact]
    public async Task OnAuthorization_ReturnsUnauthorized_WhenAuthorizationHeaderMissing()
    {
        var tokenService = new Mock<ITokenService>();
        var filter = new PermissionAuthorizationFilter(tokenService.Object, new[] { "ManageUsers" });
        var context = CreateContext();

        await filter.OnAuthorizationAsync(context);

        Assert.IsType<UnauthorizedObjectResult>(context.Result);
    }

    [Fact]
    public async Task OnAuthorization_AllowsRequest_WhenPermissionClaimExists()
    {
        var tokenService = new Mock<ITokenService>();
        tokenService.Setup(s => s.ValidateToken("jwt-token")).Returns(true);
        tokenService.Setup(s => s.GetArgumentFromToken("jwt-token", "sub")).Returns("1");
        var filter = new PermissionAuthorizationFilter(tokenService.Object, new[] { "ManageUsers" });
        var context = CreateContext(permissions: new[] { "ManageUsers" });

        await filter.OnAuthorizationAsync(context);

        Assert.Null(context.Result);
    }

    [Fact]
    public async Task OnAuthorization_ReturnsForbid_WhenPermissionClaimMissing()
    {
        var tokenService = new Mock<ITokenService>();
        tokenService.Setup(s => s.ValidateToken("jwt-token")).Returns(true);
        tokenService.Setup(s => s.GetArgumentFromToken("jwt-token", "sub")).Returns("1");
        var filter = new PermissionAuthorizationFilter(tokenService.Object, new[] { "ManageUsers" });
        var context = CreateContext();

        await filter.OnAuthorizationAsync(context);

        Assert.IsType<ForbidResult>(context.Result);
    }

    [Fact]
    public async Task OnAuthorization_ReturnsUnauthorized_WhenCurrentUserIsBanned()
    {
        var tokenService = new Mock<ITokenService>();
        tokenService.Setup(s => s.ValidateToken("jwt-token")).Returns(true);
        tokenService.Setup(s => s.GetArgumentFromToken("jwt-token", "sub")).Returns("1");

        var userRepository = new Mock<IUserRepository>();
        userRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User
            {
                IdUser = 1,
                Username = "user",
                Email = "user@example.com",
                FirstName = "User",
                HashedPassword = "hash",
                IsVerified = true,
                IsBanned = true,
                Department = new Department { IdDepartment = 1, Name = "IT", Code = 10 },
                IdRole = 1
            });

        var filter = new PermissionAuthorizationFilter(tokenService.Object, Array.Empty<string>());
        var context = CreateContext();
        context.HttpContext.RequestServices = new ServiceCollection()
            .AddSingleton(userRepository.Object)
            .BuildServiceProvider();

        await filter.OnAuthorizationAsync(context);

        var result = Assert.IsType<UnauthorizedObjectResult>(context.Result);
        Assert.Equal("User account is banned.", result.Value);
    }

    private static AuthorizationFilterContext CreateContext(IEnumerable<string>? permissions = null)
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers.Authorization = "Bearer jwt-token";

        if (permissions is not null)
        {
            httpContext.User = new System.Security.Claims.ClaimsPrincipal(
                new System.Security.Claims.ClaimsIdentity(
                    permissions.Select(permission => new System.Security.Claims.Claim("permission", permission)),
                    "test"));
        }

        return new AuthorizationFilterContext(
            new ActionContext(httpContext, new RouteData(), new ActionDescriptor()),
            new List<IFilterMetadata>());
    }
}
