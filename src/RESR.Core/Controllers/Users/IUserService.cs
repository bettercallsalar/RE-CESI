using RESR.Models.Users;

namespace RESR.Core.Controllers.Users;

public interface IUserService
{
    Task<string?> LoginUserAsync(Login loginDto, CancellationToken ct);
    Task<(IReadOnlyList<User> Users, int TotalCount)> GetUsersPaginatedAsync(int page, int pageSize, UserListingFilters filters, CancellationToken ct);
    Task<(IReadOnlyList<User> Users, int TotalCount)> GetManageableUsersPaginatedAsync(int page, int pageSize, UserListingFilters filters, CancellationToken ct);
    Task<User?> GetByIdAsync(int idUser, CancellationToken ct);
    Task<int?> RegisterAsync(RegisterUserCommand cmd, CancellationToken ct);
    Task<User> UpdateAsync(UpdateUserCommand cmd, CancellationToken ct);
    Task<User> SetVerificationAsync(SetUserVerificationCommand cmd, CancellationToken ct);
    Task<bool> SoftDeleteAsync(int idUser, CancellationToken ct);
    Task SetManageableUserBanStatusAsync(int idUser, bool isBanned, CancellationToken ct);
    Task BanManageableUserAsync(int idUser, CancellationToken ct);
}
