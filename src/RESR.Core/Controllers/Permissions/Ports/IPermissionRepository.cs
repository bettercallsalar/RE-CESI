using RESR.Models.Permissions;

namespace RESR.Core.Controllers.Permissions.Ports;

public interface IPermissionRepository
{
    Task<IReadOnlyList<Permission>> GetAllAsync(CancellationToken ct);
    Task<Permission?> GetByIdAsync(int idPermission, CancellationToken ct);
}
