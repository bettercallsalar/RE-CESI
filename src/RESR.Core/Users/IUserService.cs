using RESR.Models.Users;

namespace RESR.Core.Users;

public interface IUserService
{
    Task<string?> LoginUserAsync(LoginDto loginDto);
    Task<IReadOnlyList<User>> GetAllAsync(CancellationToken ct);
    Task<User?> GetByIdAsync(int idUser, CancellationToken ct);
    Task<int?> RegisterAsync(RegisterUserCommand cmd, CancellationToken ct);
    Task<bool> SoftDeleteAsync(int idUser, CancellationToken ct);
}