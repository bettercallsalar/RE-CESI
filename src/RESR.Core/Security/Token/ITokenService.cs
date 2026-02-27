using RESR.Models.Users;

namespace RESR.Core.Security.Token;

public interface ITokenService
{
    string GenerateUserToken(User user);
}
