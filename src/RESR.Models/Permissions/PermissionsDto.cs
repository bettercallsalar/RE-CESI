namespace RESR.Models.Permissions;

// public sealed record RegisterPermissionRequest(
//     string Name,
//     string Description
// );

public sealed record PermissionResponse(
    int IdPermission,
    string Name,
    string? Description
);
