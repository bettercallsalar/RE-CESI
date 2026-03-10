using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using RESR.Core.Security.Token;

namespace RESR.WebAPI.Security;

public sealed class PermissionOrSelfAuthorizationFilter : IAuthorizationFilter
{
    private readonly ITokenService _tokenService;
    private readonly string _routeIdParamName;
    private readonly string[] _requiredPermissions;

    public PermissionOrSelfAuthorizationFilter(
        ITokenService tokenService,
        string routeIdParamName,
        string[] requiredPermissions
    )
    {
        _tokenService = tokenService;
        _routeIdParamName = routeIdParamName;
        _requiredPermissions = requiredPermissions ?? Array.Empty<string>();
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
        if (string.IsNullOrWhiteSpace(jwtToken) || !_tokenService.ValidateToken(jwtToken))
        {
            context.Result = new UnauthorizedObjectResult("Invalid token or unauthorized access.");
            return;
        }

        JwtSecurityToken parsedToken;
        try
        {
            parsedToken = new JwtSecurityTokenHandler().ReadJwtToken(jwtToken);
        }
        catch
        {
            context.Result = new UnauthorizedObjectResult("Invalid token or unauthorized access.");
            return;
        }

        var routeUserIdValue = context.RouteData.Values[_routeIdParamName]?.ToString();
        var tokenSubject = parsedToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value;

        var isSelf =
            int.TryParse(routeUserIdValue, out var routeUserId) &&
            int.TryParse(tokenSubject, out var subjectUserId) &&
            routeUserId == subjectUserId;

        if (isSelf)
            return;

        if (_requiredPermissions.Length == 0)
        {
            context.Result = new ForbidResult();
            return;
        }

        var userPermissions = parsedToken.Claims
            .Where(c => string.Equals(c.Type, "permission", StringComparison.OrdinalIgnoreCase))
            .Select(c => c.Value)
            .Where(v => !string.IsNullOrWhiteSpace(v))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var hasRequiredPermissions = _requiredPermissions.All(userPermissions.Contains);
        if (!hasRequiredPermissions)
            context.Result = new ForbidResult();
    }
}
