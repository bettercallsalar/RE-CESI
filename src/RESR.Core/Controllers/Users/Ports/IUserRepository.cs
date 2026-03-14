using RESR.Models.Users;

namespace RESR.Core.Controllers.Users.Ports;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(int idUser, CancellationToken ct);
    Task<User?> GetByEmailAsync(string email, CancellationToken ct);
    Task<User?> GetByUsernameAsync(string username, CancellationToken ct);
    Task<User?> GetByEmailAndPasswordHashAsync(string email, string passwordHash, CancellationToken ct);
    Task<IReadOnlyList<User>> GetUsersPaginatedAsync(int page, int pageSize, UserListingFilters filters, CancellationToken ct);
    Task<int> CountUsersAsync(UserListingFilters filters, CancellationToken ct);
    Task<int> CreateAsync(User user, CancellationToken ct);
    Task<User> PatchAsync(UpdateUserCommand cmd, CancellationToken ct);
    Task<User> SetVerificationAsync(int idUser, bool isVerified, CancellationToken ct);
    Task<User> SetBannedAsync(int idUser, bool isBanned, CancellationToken ct);
    Task<bool> SoftDeleteAsync(int idUser, CancellationToken ct);
}
