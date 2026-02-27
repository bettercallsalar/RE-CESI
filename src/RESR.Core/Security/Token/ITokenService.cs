using RESR.Models.Users;

namespace RESR.Core.Security.Token;

public interface ITokenService
{
    string GenerateUserToken(User user);
    bool ValidateToken(string token);
    string? GetArgumentFromToken(string token, string argumentName);
}
