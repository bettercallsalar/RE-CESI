using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace RESR.WebAPI.Security;

public static class CurrentUserClaimsExtensions
{
    public static bool TryGetCurrentUserId(this ClaimsPrincipal? user, out int idUser)
    {
        var subject = user?.Claims?.FirstOrDefault(c =>
            c.Type == JwtRegisteredClaimNames.Sub ||
            c.Type == ClaimTypes.NameIdentifier
        )?.Value;

        return int.TryParse(subject, out idUser);
    }

    public static IReadOnlySet<string> GetCurrentPermissions(this ClaimsPrincipal? user) =>
        (user?.Claims ?? Enumerable.Empty<Claim>())
            .Where(c => string.Equals(c.Type, "permission", StringComparison.OrdinalIgnoreCase))
            .Select(c => c.Value)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
}
