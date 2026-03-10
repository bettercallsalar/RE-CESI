using RESR.Models.Permissions;
namespace RESR.Models.Roles;

// public sealed record RegisterRoleRequest(
//     string Name,
//     string? Description
// );

public sealed record RoleResponse(
    int IdRole,
    string Name,
    string? Description,
    IReadOnlyList<PermissionResponse> Permissions
);
