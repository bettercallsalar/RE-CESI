using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using RESR.Core.Security.Token;

namespace RESR.WebAPI.Security;

public sealed class GenericAuthorizationFilter : IAuthorizationFilter
{
    private readonly ITokenService _tokenService;
    private readonly TokenRole[] _expectedRoles;

    public GenericAuthorizationFilter(ITokenService tokenService, TokenRole[] expectedRoles)
    {
        _tokenService = tokenService;
        _expectedRoles = expectedRoles ?? Array.Empty<TokenRole>();
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var authHeader = context.HttpContext.Request.Headers.Authorization.ToString();
        if (!authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            context.Result = new UnauthorizedObjectResult("Missing or invalid Authorization header.");
            return;
        }

        var jwtToken = authHeader["Bearer ".Length..].Trim();

        if (string.IsNullOrEmpty(jwtToken) || !_tokenService.ValidateToken(jwtToken))
        {
            context.Result = new UnauthorizedObjectResult("Invalid token or unauthorized access.");
            return;
        }

        if (_expectedRoles.Length == 0)
            return;

        var roleClaim = _tokenService.GetArgumentFromToken(jwtToken, "id_role")
            ?? _tokenService.GetArgumentFromToken(jwtToken, ClaimTypes.Role);

        if (!int.TryParse(roleClaim, out var roleId))
        {
            context.Result = new ForbidResult();
            return;
        }

        var actualRole = MapRole(roleId);
        var isAuthorized = _expectedRoles.Any(expected => IsRoleAllowed(expected, actualRole));

        if (!isAuthorized)
            context.Result = new ForbidResult();
    }

    private static TokenRole MapRole(int roleId) =>
        roleId switch
        {
            1 => TokenRole.User,
            2 => TokenRole.Admin,
            3 => TokenRole.Admin,
            _ => TokenRole.None
        };

    private static bool IsRoleAllowed(TokenRole required, TokenRole actual)
    {
        if (required == TokenRole.Admin)
            return actual == TokenRole.Admin;

        if (required == TokenRole.User)
            return actual is TokenRole.User or TokenRole.Admin;

        if (required == TokenRole.Customer)
            return actual is TokenRole.Customer or TokenRole.Admin;

        return false;
    }
}
