using RESR.Models.Roles;

namespace RESR.Core.Roles.Ports;

public interface IRoleRepository
{
    Task<IReadOnlyList<Role>> GetAllAsync(CancellationToken ct);
}