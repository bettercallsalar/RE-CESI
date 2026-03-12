using System.IdentityModel.Tokens.Jwt;
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

public sealed class PermissionOrQuerySelfAuthorizationFilterTests
{
    [Fact]
    public void OnAuthorization_AllowsSelf_WhenQueryMatchesTokenSubject()
    {
        var filter = CreateFilter(validateToken: true, requiredPermissions: new[] { PermissionNames.ViewOtherUserReactions });
        var context = CreateContext(BuildToken("2"), "2");

        filter.OnAuthorization(context);

        Assert.Null(context.Result);
    }

    [Fact]
    public void OnAuthorization_AllowsCurrentUser_WhenQueryIsMissing()
    {
        var filter = CreateFilter(validateToken: true, requiredPermissions: new[] { PermissionNames.ViewOtherUserReactions });
        var context = CreateContext(BuildToken("2"), null);

        filter.OnAuthorization(context);

        Assert.Null(context.Result);
    }

    [Fact]
    public void OnAuthorization_ReturnsForbid_WhenQueryTargetsOtherUserWithoutPermission()
    {
        var filter = CreateFilter(validateToken: true, requiredPermissions: new[] { PermissionNames.ViewOtherUserReactions });
        var context = CreateContext(BuildToken("2"), "4");

        filter.OnAuthorization(context);

        Assert.IsType<ForbidResult>(context.Result);
    }

    [Fact]
    public void OnAuthorization_AllowsOtherUser_WhenPermissionIsPresent()
    {
        var filter = CreateFilter(validateToken: true, requiredPermissions: new[] { PermissionNames.ViewOtherUserReactions });
        var context = CreateContext(BuildToken("2", PermissionNames.ViewOtherUserReactions), "4");

        filter.OnAuthorization(context);

        Assert.Null(context.Result);
    }

    [Fact]
    public void OnAuthorization_ReturnsUnauthorized_WhenAuthorizationHeaderIsMissing()
    {
        var filter = CreateFilter(validateToken: true);
        var context = CreateContext(null, "2");

        filter.OnAuthorization(context);

        Assert.IsType<UnauthorizedObjectResult>(context.Result);
    }

    private static PermissionOrQuerySelfAuthorizationFilter CreateFilter(bool validateToken, string[]? requiredPermissions = null)
    {
        var tokenService = new Mock<ITokenService>();
        tokenService.Setup(service => service.ValidateToken(It.IsAny<string>())).Returns(validateToken);
        return new PermissionOrQuerySelfAuthorizationFilter(
            tokenService.Object,
            "idUser",
            requiredPermissions ?? Array.Empty<string>());
    }

    private static AuthorizationFilterContext CreateContext(string? token, string? queryUserId)
    {
        var httpContext = new DefaultHttpContext();
        if (!string.IsNullOrWhiteSpace(token))
            httpContext.Request.Headers.Authorization = $"Bearer {token}";

        if (!string.IsNullOrWhiteSpace(queryUserId))
            httpContext.Request.QueryString = new QueryString($"?idUser={queryUserId}");

        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
        return new AuthorizationFilterContext(actionContext, Array.Empty<IFilterMetadata>());
    }

    private static string BuildToken(string subject, params string[] permissions)
    {
        var claims = new List<Claim> { new(JwtRegisteredClaimNames.Sub, subject) };
        claims.AddRange(permissions.Select(permission => new Claim("permission", permission)));

        var token = new JwtSecurityToken(claims: claims);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
