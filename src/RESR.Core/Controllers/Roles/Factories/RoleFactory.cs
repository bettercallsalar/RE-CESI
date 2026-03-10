using RESR.Models.Roles;

namespace RESR.Core.Controllers.Roles.Factories;

public sealed class RoleFactory : IRoleFactory
{
    public Role Create(int idRole, string name, string? description) =>
        new()
        {
            IdRole = idRole,
            Name = name,
            Description = description
        };
}
