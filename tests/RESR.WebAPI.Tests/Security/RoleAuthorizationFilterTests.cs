using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Moq;
using RESR.Core.Security.Token;
using RESR.WebAPI.Security;

namespace RESR.WebAPI.Tests.Security;

public sealed class RoleAuthorizationFilterTests
{
    [Fact]
    public void OnAuthorization_ReturnsUnauthorized_WhenAuthorizationHeaderMissing()
    {
        var tokenService = new Mock<ITokenService>();
        var filter = new RoleAuthorizationFilter(tokenService.Object, new[] { RoleIds.SuperAdmin });
        var context = CreateContext();

        context.HttpContext.Request.Headers.Authorization = string.Empty;
        filter.OnAuthorization(context);

        Assert.IsType<UnauthorizedObjectResult>(context.Result);
    }

    [Fact]
    public void OnAuthorization_AllowsRequest_WhenRoleClaimExists()
    {
        var tokenService = new Mock<ITokenService>();
        tokenService.Setup(service => service.ValidateToken("jwt-token")).Returns(true);
        var filter = new RoleAuthorizationFilter(tokenService.Object, new[] { RoleIds.SuperAdmin });
        var context = CreateContext(new Claim("id_role", RoleIds.SuperAdmin.ToString()));

        filter.OnAuthorization(context);

        Assert.Null(context.Result);
    }

    [Fact]
    public void OnAuthorization_ReturnsForbid_WhenRoleClaimMissing()
    {
        var tokenService = new Mock<ITokenService>();
        tokenService.Setup(service => service.ValidateToken("jwt-token")).Returns(true);
        var filter = new RoleAuthorizationFilter(tokenService.Object, new[] { RoleIds.SuperAdmin });
        var context = CreateContext(new Claim("id_role", "2"));

        filter.OnAuthorization(context);

        Assert.IsType<ForbidResult>(context.Result);
    }

    private static AuthorizationFilterContext CreateContext(params Claim[] claims)
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers.Authorization = "Bearer jwt-token";

        if (claims.Length > 0)
        {
            httpContext.User = new ClaimsPrincipal(
                new ClaimsIdentity(claims, "test"));
        }

        return new AuthorizationFilterContext(
            new ActionContext(httpContext, new RouteData(), new ActionDescriptor()),
            new List<IFilterMetadata>());
    }
}
