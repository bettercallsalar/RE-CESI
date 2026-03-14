export const PermissionNames = {
  accessAdminPanel: "AccessAdminPanel",
  approveArticle: "ApproveArticle",
  approveEvent: "ApproveEvent",
  banUser: "BanUser",
  deleteComment: "DeleteComment",
  manageRoles: "ManageRoles",
  manageUsers: "ManageUsers"
} as const;

export const adminDashboardPermissions = [
  PermissionNames.accessAdminPanel,
  PermissionNames.approveArticle,
  PermissionNames.approveEvent,
  PermissionNames.manageRoles,
  PermissionNames.manageUsers,
  PermissionNames.banUser
];
