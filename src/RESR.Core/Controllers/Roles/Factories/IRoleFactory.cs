using RESR.Models.Roles;

namespace RESR.Core.Controllers.Roles.Factories;

public interface IRoleFactory
{
    Role Create(int idRole, string name, string? description);
}
