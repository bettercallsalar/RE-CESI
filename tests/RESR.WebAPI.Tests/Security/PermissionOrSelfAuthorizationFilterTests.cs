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
    public async Task OnAuthorization_ReturnsUnauthorized_WhenAuthorizationHeaderMissing()
    {
        var tokenService = new Mock<ITokenService>();
        var filter = new PermissionOrSelfAuthorizationFilter(tokenService.Object, "idUser", Array.Empty<string>());
        var context = CreateContext(routeUserId: 7);

        context.HttpContext.Request.Headers.Authorization = string.Empty;

        await filter.OnAuthorizationAsync(context);

        Assert.IsType<UnauthorizedObjectResult>(context.Result);
    }

    [Fact]
    public async Task OnAuthorization_AllowsRequest_WhenRouteUserMatchesTokenSubject()
    {
        var tokenService = new Mock<ITokenService>();
        tokenService.Setup(s => s.ValidateToken("jwt-token")).Returns(true);
        tokenService.Setup(s => s.GetArgumentFromToken("jwt-token", "sub")).Returns("7");
        var filter = new PermissionOrSelfAuthorizationFilter(tokenService.Object, "idUser", Array.Empty<string>());
        var context = CreateContext(routeUserId: 7);

        await filter.OnAuthorizationAsync(context);

        Assert.Null(context.Result);
    }

    [Fact]
    public async Task OnAuthorization_AllowsRequest_WhenPermissionClaimExists()
    {
        var tokenService = new Mock<ITokenService>();
        tokenService.Setup(s => s.ValidateToken("jwt-token")).Returns(true);
        tokenService.Setup(s => s.GetArgumentFromToken("jwt-token", "sub")).Returns("7");
        var filter = new PermissionOrSelfAuthorizationFilter(tokenService.Object, "idUser", new[] { "ManageUsers" });
        var context = CreateContext(routeUserId: 9, permissions: new[] { "ManageUsers" });

        await filter.OnAuthorizationAsync(context);

        Assert.Null(context.Result);
    }

    [Fact]
    public async Task OnAuthorization_ReturnsForbid_WhenUserIsNotSelf_AndPermissionMissing()
    {
        var tokenService = new Mock<ITokenService>();
        tokenService.Setup(s => s.ValidateToken("jwt-token")).Returns(true);
        tokenService.Setup(s => s.GetArgumentFromToken("jwt-token", "sub")).Returns("7");
        var filter = new PermissionOrSelfAuthorizationFilter(tokenService.Object, "idUser", new[] { "ManageUsers" });
        var context = CreateContext(routeUserId: 9);

        await filter.OnAuthorizationAsync(context);

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
