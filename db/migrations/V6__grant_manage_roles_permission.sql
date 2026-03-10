-- Allow admin-level roles to manage role-permission assignments.
INSERT IGNORE INTO `role_permission` (`id_role`, `id_permission`) VALUES
    (2, 10), -- Admin -> ManageRoles
    (3, 10); -- SuperAdmin -> ManageRoles
