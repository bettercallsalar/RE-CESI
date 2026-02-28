using RESR.Core.Controllers.Roles.Ports;
using RESR.Models.Permissions;
using RESR.Models.Roles;

namespace RESR.Core.Controllers.Roles;

public sealed class RoleService : IRoleService
{
    private readonly IRoleRepository _repo;

    public RoleService(IRoleRepository repo) => _repo = repo;

    public Task<IReadOnlyList<Role>> GetAllAsync(CancellationToken ct) => _repo.GetAllAsync(ct);
    public Task<Role?> GetByIdAsync(int idRole, CancellationToken ct) => _repo.GetByIdAsync(idRole, ct);
    public Task<IReadOnlyList<Permission>> GetPermissionsByRoleIdAsync(int idRole, CancellationToken ct) =>
        _repo.GetPermissionsByRoleIdAsync(idRole, ct);
}
