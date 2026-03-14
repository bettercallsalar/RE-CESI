using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using RESR.Core.Controllers.Users.Ports;
using RESR.Core.Security.Token;

namespace RESR.WebAPI.Security;

public abstract class AuthenticatedResourceControllerBase : ControllerBase
{
    private readonly ITokenService _tokenService;

    protected AuthenticatedResourceControllerBase(ITokenService tokenService)
    {
        _tokenService = tokenService;
    }

    protected async Task<(ActionResult? Error, int IdUser)> RequireAuthenticatedUserAsync(CancellationToken ct)
    {
        if (HttpContext?.Request is null)
            return (Unauthorized(new { message = "Missing or invalid Authorization header." }), default);

        var authHeader = HttpContext.Request.Headers.Authorization.ToString();
        if (!authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            return (Unauthorized(new { message = "Missing or invalid Authorization header." }), default);

        var token = authHeader["Bearer ".Length..].Trim();
        if (string.IsNullOrWhiteSpace(token) || !_tokenService.ValidateToken(token))
            return (Unauthorized(new { message = "Invalid token or unauthorized access." }), default);

        var subject = _tokenService.GetArgumentFromToken(token, JwtRegisteredClaimNames.Sub);
        if (!int.TryParse(subject, out var idUser))
            return (Unauthorized(new { message = "Invalid token or missing subject claim." }), default);

        var serviceProvider = HttpContext.Features.Get<IServiceProvidersFeature>()?.RequestServices;
        var userRepository = serviceProvider?.GetService(typeof(IUserRepository)) as IUserRepository;
        if (userRepository is not null)
        {
            var user = await userRepository.GetByIdAsync(idUser, ct);
            if (user is null)
                return (Unauthorized(new { message = "Invalid token or unauthorized access." }), default);

            if (user.IsBanned)
                return (Unauthorized(new { message = "User account is banned." }), default);
        }

        return (null, idUser);
    }
}
