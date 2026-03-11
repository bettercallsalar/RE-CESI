using RESR.Models.Users;

namespace RESR.MAUI.Services;

public interface IUsersApiClient
{
    Task<IReadOnlyList<UserResponse>> GetUsersAsync(CancellationToken ct);
}
