namespace RESR.Models.Roles;

public sealed record RegisterRoleRequest(
    string Name
);

public sealed record RoleResponse(
    int Idrole,
    string Name
);