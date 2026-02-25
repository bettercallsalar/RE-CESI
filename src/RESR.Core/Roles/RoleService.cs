using RESR.Core.Roles.Ports;
using RESR.Models.Roles;

namespace RESR.Core.Roles;

public sealed class RoleService : IRoleService
{
    private readonly IRoleRepository _repo;

    public RoleService(IRoleRepository repo) => _repo = repo;

    public Task<IReadOnlyList<Role>> GetAllAsync(CancellationToken ct) => _repo.GetAllAsync(ct);
}