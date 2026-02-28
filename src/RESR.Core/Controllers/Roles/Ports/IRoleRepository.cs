using RESR.Models.Permissions;
using RESR.Models.Roles;

namespace RESR.Core.Controllers.Roles.Ports;

public interface IRoleRepository
{
    Task<IReadOnlyList<Role>> GetAllAsync(CancellationToken ct);
    Task<Role?> GetByIdAsync(int idRole, CancellationToken ct);
    Task<IReadOnlyList<Permission>> GetPermissionsByRoleIdAsync(int idRole, CancellationToken ct);
}
