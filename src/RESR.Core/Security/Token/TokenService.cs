using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using RESR.Models.Permissions;
using RESR.Models.Users;

namespace RESR.Core.Security.Token;

public sealed class TokenService : ITokenService
{
    private readonly JwtSettings _settings;

    public TokenService(IOptions<JwtSettings> options)
    {
        _settings = options.Value;

        if (string.IsNullOrWhiteSpace(_settings.SecretKey))
            throw new InvalidOperationException("JwtSettings:SecretKey is missing.");
        if (string.IsNullOrWhiteSpace(_settings.Issuer))
            throw new InvalidOperationException("JwtSettings:Issuer is missing.");
        if (string.IsNullOrWhiteSpace(_settings.Audience))
            throw new InvalidOperationException("JwtSettings:Audience is missing.");
    }

    public string GenerateUserToken(User user, IReadOnlyList<Permission> permissions)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.IdUser.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.UniqueName, user.Username),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        if (user.IdRole is int roleId)
        {
            claims.Add(new Claim(ClaimTypes.Role, roleId.ToString()));
            claims.Add(new Claim("id_role", roleId.ToString()));

            foreach (var permission in permissions)
            {
                claims.Add(new Claim("permission", permission.Name));
            }
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SecretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            // DEV NOTE: For testing purposes, tokens don't expire
            //expires: DateTime.UtcNow.AddMinutes(_settings.ExpirationMinutes),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public bool ValidateToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return false;

        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SecretKey));

            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateIssuerSigningKey = true,
                ValidateLifetime = true,
                ValidIssuer = _settings.Issuer,
                ValidAudience = _settings.Audience,
                IssuerSigningKey = key,
                ClockSkew = TimeSpan.Zero
            }, out _);

            return true;
        }
        catch
        {
            return false;
        }
    }

    public string? GetArgumentFromToken(string token, string argumentName)
    {
        if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(argumentName))
            return null;

        try
        {
            var jwtToken = new JwtSecurityTokenHandler().ReadJwtToken(token);
            return jwtToken.Claims.FirstOrDefault(c => c.Type == argumentName)?.Value;
        }
        catch
        {
            return null;
        }
    }
}
