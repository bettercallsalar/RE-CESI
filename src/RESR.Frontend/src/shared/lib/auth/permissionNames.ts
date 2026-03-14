export const PermissionNames = {
  accessAdminPanel: "AccessAdminPanel",
  banUser: "BanUser",
  manageRoles: "ManageRoles",
  manageUsers: "ManageUsers"
} as const;

export const adminDashboardPermissions = [
  PermissionNames.accessAdminPanel,
  PermissionNames.manageRoles,
  PermissionNames.manageUsers,
  PermissionNames.banUser
];
