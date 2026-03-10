using RESR.Models.Permissions;

namespace RESR.Core.Controllers.Permissions.Factories;

public sealed class PermissionFactory : IPermissionFactory
{
    public Permission Create(int idPermission, string name, string? description) =>
        new()
        {
            IdPermission = idPermission,
            Name = name,
            Description = description ?? string.Empty
        };
}
