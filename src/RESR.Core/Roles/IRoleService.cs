using RESR.Models.Roles;

namespace RESR.Core.Roles;

public interface IRoleService
{
    Task<IReadOnlyList<Role>> GetAllAsync(CancellationToken ct);
}