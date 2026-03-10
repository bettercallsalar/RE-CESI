using RESR.Models.Permissions;
using RESR.Models.Roles;

namespace RESR.Core.Controllers.Roles;

public interface IRoleService
{
    Task<IReadOnlyList<Role>> GetAllAsync(CancellationToken ct);
    Task<Role?> GetByIdAsync(int idRole, CancellationToken ct);
    Task<IReadOnlyList<Permission>> GetPermissionsByRoleIdAsync(int idRole, CancellationToken ct);
    Task AddPermissionToRoleAsync(int idRole, int idPermission, CancellationToken ct);
    Task RemovePermissionFromRoleAsync(int idRole, int idPermission, CancellationToken ct);
}
