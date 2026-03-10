using RESR.Core.Controllers.Permissions.Ports;
using RESR.Models.Permissions;

namespace RESR.Core.Controllers.Permissions;

public sealed class PermissionService : IPermissionService
{
    private readonly IPermissionRepository _repo;

    public PermissionService(IPermissionRepository repo) => _repo = repo;

    public Task<IReadOnlyList<Permission>> GetAllAsync(CancellationToken ct) => _repo.GetAllAsync(ct);

    public Task<Permission?> GetByIdAsync(int idPermission, CancellationToken ct) => _repo.GetByIdAsync(idPermission, ct);
}
