using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using RESR.Core.Controllers.Users.Ports;
using RESR.Core.Security.Token;

namespace RESR.WebAPI.Security;

public sealed class PermissionOrSelfAuthorizationFilter : IAsyncAuthorizationFilter
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

        var tokenSubject = _tokenService.GetArgumentFromToken(jwtToken, JwtRegisteredClaimNames.Sub);
        if (!int.TryParse(tokenSubject, out var subjectUserId))
        {
            context.Result = new UnauthorizedObjectResult("Invalid token or missing subject claim.");
            return;
        }

        var serviceProvider = context.HttpContext.Features.Get<IServiceProvidersFeature>()?.RequestServices;
        var userRepository = serviceProvider?.GetService(typeof(IUserRepository)) as IUserRepository;
        if (userRepository is not null)
        {
            var user = await userRepository.GetByIdAsync(subjectUserId, context.HttpContext.RequestAborted);
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

        var routeUserIdValue = context.RouteData.Values[_routeIdParamName]?.ToString();
        var isSelf =
            int.TryParse(routeUserIdValue, out var routeUserId) &&
            routeUserId == subjectUserId;

        if (isSelf)
            return;

        if (_requiredPermissions.Length == 0)
        {
            context.Result = new ForbidResult();
            return;
        }

        var userPermissions = context.HttpContext.User.Claims
            .Where(c => string.Equals(c.Type, "permission", StringComparison.OrdinalIgnoreCase))
            .Select(c => c.Value)
            .Where(v => !string.IsNullOrWhiteSpace(v))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var hasRequiredPermissions = _requiredPermissions.All(userPermissions.Contains);
        if (!hasRequiredPermissions)
            context.Result = new ForbidResult();
    }
}
