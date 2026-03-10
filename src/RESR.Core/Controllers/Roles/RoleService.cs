using RESR.Core.Controllers.Permissions.Ports;
using RESR.Core.Errors;
using RESR.Core.Controllers.Roles.Ports;
using RESR.Models.Permissions;
using RESR.Models.Roles;

namespace RESR.Core.Controllers.Roles;

public sealed class RoleService : IRoleService
{
    private readonly IRoleRepository _repo;
    private readonly IPermissionRepository _permissionRepository;

    public RoleService(IRoleRepository repo, IPermissionRepository permissionRepository)
    {
        _repo = repo;
        _permissionRepository = permissionRepository;
    }

    public Task<IReadOnlyList<Role>> GetAllAsync(CancellationToken ct) => _repo.GetAllAsync(ct);
    public Task<Role?> GetByIdAsync(int idRole, CancellationToken ct) => _repo.GetByIdAsync(idRole, ct);
    public Task<IReadOnlyList<Permission>> GetPermissionsByRoleIdAsync(int idRole, CancellationToken ct) =>
        _repo.GetPermissionsByRoleIdAsync(idRole, ct);

    public async Task AddPermissionToRoleAsync(int idRole, int idPermission, CancellationToken ct)
    {
        if (await _repo.GetByIdAsync(idRole, ct) is null)
            throw new NotFoundException($"Role {idRole} not found");

        if (await _permissionRepository.GetByIdAsync(idPermission, ct) is null)
            throw new NotFoundException($"Permission {idPermission} not found");

        var added = await _repo.AddPermissionToRoleAsync(idRole, idPermission, ct);
        if (!added)
            throw new ConflictException($"Permission {idPermission} is already assigned to role {idRole}");
    }

    public async Task RemovePermissionFromRoleAsync(int idRole, int idPermission, CancellationToken ct)
    {
        if (await _repo.GetByIdAsync(idRole, ct) is null)
            throw new NotFoundException($"Role {idRole} not found");

        if (await _permissionRepository.GetByIdAsync(idPermission, ct) is null)
            throw new NotFoundException($"Permission {idPermission} not found");

        var removed = await _repo.RemovePermissionFromRoleAsync(idRole, idPermission, ct);
        if (!removed)
            throw new NotFoundException($"Permission {idPermission} is not assigned to role {idRole}");
    }
}
