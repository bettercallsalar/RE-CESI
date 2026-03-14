using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using RESR.Core.Security.Token;

namespace RESR.WebAPI.Security;

public sealed class RoleAuthorizationFilter : IAuthorizationFilter
{
    private readonly ITokenService _tokenService;
    private readonly int[] _allowedRoleIds;

    public RoleAuthorizationFilter(ITokenService tokenService, int[] allowedRoleIds)
    {
        _tokenService = tokenService;
        _allowedRoleIds = allowedRoleIds ?? Array.Empty<int>();
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

        if (_allowedRoleIds.Length == 0)
            return;

        var userRoleIds = context.HttpContext.User.Claims
            .Where(claim =>
                string.Equals(claim.Type, "id_role", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(claim.Type, ClaimTypes.Role, StringComparison.OrdinalIgnoreCase))
            .Select(claim => claim.Value)
            .Select(value => int.TryParse(value, out var parsedRoleId) ? parsedRoleId : (int?)null)
            .Where(value => value.HasValue)
            .Select(value => value!.Value)
            .ToHashSet();

        var isAllowedRole = _allowedRoleIds.Any(userRoleIds.Contains);
        if (!isAllowedRole)
            context.Result = new ForbidResult();
    }
}
