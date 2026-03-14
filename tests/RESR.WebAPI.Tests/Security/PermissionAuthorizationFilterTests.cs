using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Moq;
using RESR.Core.Security.Token;
using RESR.WebAPI.Security;

namespace RESR.WebAPI.Tests.Security;

public sealed class PermissionAuthorizationFilterTests
{
    [Fact]
    public void OnAuthorization_ReturnsUnauthorized_WhenAuthorizationHeaderMissing()
    {
        var tokenService = new Mock<ITokenService>();
        var filter = new PermissionAuthorizationFilter(tokenService.Object, new[] { "ManageUsers" });
        var context = CreateContext();

        filter.OnAuthorization(context);

        Assert.IsType<UnauthorizedObjectResult>(context.Result);
    }

    [Fact]
    public void OnAuthorization_AllowsRequest_WhenPermissionClaimExists()
    {
        var tokenService = new Mock<ITokenService>();
        tokenService.Setup(s => s.ValidateToken("jwt-token")).Returns(true);
        var filter = new PermissionAuthorizationFilter(tokenService.Object, new[] { "ManageUsers" });
        var context = CreateContext(permissions: new[] { "ManageUsers" });

        filter.OnAuthorization(context);

        Assert.Null(context.Result);
    }

    [Fact]
    public void OnAuthorization_ReturnsForbid_WhenPermissionClaimMissing()
    {
        var tokenService = new Mock<ITokenService>();
        tokenService.Setup(s => s.ValidateToken("jwt-token")).Returns(true);
        var filter = new PermissionAuthorizationFilter(tokenService.Object, new[] { "ManageUsers" });
        var context = CreateContext();

        filter.OnAuthorization(context);

        Assert.IsType<ForbidResult>(context.Result);
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
