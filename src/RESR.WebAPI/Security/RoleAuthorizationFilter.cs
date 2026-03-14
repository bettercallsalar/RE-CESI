using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using RESR.Core.Controllers.Users.Ports;
using RESR.Core.Security.Token;

namespace RESR.WebAPI.Security;

public sealed class RoleAuthorizationFilter : IAsyncAuthorizationFilter
{
    private readonly ITokenService _tokenService;
    private readonly int[] _allowedRoleIds;

    public RoleAuthorizationFilter(ITokenService tokenService, int[] allowedRoleIds)
    {
        _tokenService = tokenService;
        _allowedRoleIds = allowedRoleIds ?? Array.Empty<int>();
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
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

        var subject = _tokenService.GetArgumentFromToken(jwtToken, JwtRegisteredClaimNames.Sub);
        if (!int.TryParse(subject, out var idUser))
        {
            context.Result = new UnauthorizedObjectResult("Invalid token or missing subject claim.");
            return;
        }

        var serviceProvider = context.HttpContext.Features.Get<IServiceProvidersFeature>()?.RequestServices;
        var userRepository = serviceProvider?.GetService(typeof(IUserRepository)) as IUserRepository;
        if (userRepository is not null)
        {
            var user = await userRepository.GetByIdAsync(idUser, context.HttpContext.RequestAborted);
            if (user is null)
            {
                context.Result = new UnauthorizedObjectResult("Invalid token or unauthorized access.");
                return;
            }

            if (user.IsBanned)
            {
                context.Result = new UnauthorizedObjectResult("User account is banned.");
                return;
            }
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
