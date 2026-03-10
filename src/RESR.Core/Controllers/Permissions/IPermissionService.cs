using RESR.Models.Permissions;

namespace RESR.Core.Controllers.Permissions;

public interface IPermissionService
{
    Task<IReadOnlyList<Permission>> GetAllAsync(CancellationToken ct);
    Task<Permission?> GetByIdAsync(int idPermission, CancellationToken ct);
}
