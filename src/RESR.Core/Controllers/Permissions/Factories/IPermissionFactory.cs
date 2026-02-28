using RESR.Models.Permissions;

namespace RESR.Core.Controllers.Permissions.Factories;

public interface IPermissionFactory
{
    Permission Create(int idPermission, string name, string? description);
}
