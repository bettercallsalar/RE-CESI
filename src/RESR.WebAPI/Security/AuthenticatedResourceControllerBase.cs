using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Mvc;
using RESR.Core.Security.Token;

namespace RESR.WebAPI.Security;

public abstract class AuthenticatedResourceControllerBase : ControllerBase
{
    private readonly ITokenService _tokenService;

    protected AuthenticatedResourceControllerBase(ITokenService tokenService)
    {
        _tokenService = tokenService;
    }

    protected ActionResult? RequireAuthenticatedUser(out int idUser)
    {
        idUser = default;

        if (HttpContext?.Request is null)
            return Unauthorized(new { message = "Missing or invalid Authorization header." });

        var authHeader = HttpContext.Request.Headers.Authorization.ToString();
        if (!authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            return Unauthorized(new { message = "Missing or invalid Authorization header." });

        var token = authHeader["Bearer ".Length..].Trim();
        if (string.IsNullOrWhiteSpace(token) || !_tokenService.ValidateToken(token))
            return Unauthorized(new { message = "Invalid token or unauthorized access." });

        var subject = _tokenService.GetArgumentFromToken(token, JwtRegisteredClaimNames.Sub);
        if (!int.TryParse(subject, out idUser))
            return Unauthorized(new { message = "Invalid token or missing subject claim." });

        return null;
    }
}
