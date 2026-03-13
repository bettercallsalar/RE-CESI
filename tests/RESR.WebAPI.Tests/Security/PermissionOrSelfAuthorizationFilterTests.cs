using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Moq;
using RESR.Core.Security.Token;
using RESR.WebAPI.Security;

namespace RESR.WebAPI.Tests.Security;

public sealed class PermissionOrSelfAuthorizationFilterTests
{
    [Fact]
    public void OnAuthorization_ReturnsUnauthorized_WhenAuthorizationHeaderMissing()
    {
        var tokenService = new Mock<ITokenService>();
        var filter = new PermissionOrSelfAuthorizationFilter(tokenService.Object, "idUser", Array.Empty<string>());
        var context = CreateContext(routeUserId: 7);

        context.HttpContext.Request.Headers.Authorization = string.Empty;

        filter.OnAuthorization(context);

        Assert.IsType<UnauthorizedObjectResult>(context.Result);
    }

    [Fact]
    public void OnAuthorization_AllowsRequest_WhenRouteUserMatchesTokenSubject()
    {
        var tokenService = new Mock<ITokenService>();
        tokenService.Setup(s => s.ValidateToken("jwt-token")).Returns(true);
        tokenService.Setup(s => s.GetArgumentFromToken("jwt-token", "sub")).Returns("7");
        var filter = new PermissionOrSelfAuthorizationFilter(tokenService.Object, "idUser", Array.Empty<string>());
        var context = CreateContext(routeUserId: 7);

        filter.OnAuthorization(context);

        Assert.Null(context.Result);
    }

    [Fact]
    public void OnAuthorization_AllowsRequest_WhenPermissionClaimExists()
    {
        var tokenService = new Mock<ITokenService>();
        tokenService.Setup(s => s.ValidateToken("jwt-token")).Returns(true);
        tokenService.Setup(s => s.GetArgumentFromToken("jwt-token", "sub")).Returns("7");
        var filter = new PermissionOrSelfAuthorizationFilter(tokenService.Object, "idUser", new[] { "ManageUsers" });
        var context = CreateContext(routeUserId: 9, permissions: new[] { "ManageUsers" });

        filter.OnAuthorization(context);

        Assert.Null(context.Result);
    }

    [Fact]
    public void OnAuthorization_ReturnsForbid_WhenUserIsNotSelf_AndPermissionMissing()
    {
        var tokenService = new Mock<ITokenService>();
        tokenService.Setup(s => s.ValidateToken("jwt-token")).Returns(true);
        tokenService.Setup(s => s.GetArgumentFromToken("jwt-token", "sub")).Returns("7");
        var filter = new PermissionOrSelfAuthorizationFilter(tokenService.Object, "idUser", new[] { "ManageUsers" });
        var context = CreateContext(routeUserId: 9);

        filter.OnAuthorization(context);

        Assert.IsType<ForbidResult>(context.Result);
    }

    private static AuthorizationFilterContext CreateContext(int routeUserId, IEnumerable<string>? permissions = null)
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

        var routeData = new RouteData();
        routeData.Values["idUser"] = routeUserId.ToString();

        return new AuthorizationFilterContext(
            new ActionContext(httpContext, routeData, new ActionDescriptor()),
            new List<IFilterMetadata>());
    }
}
