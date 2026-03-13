INSERT IGNORE INTO `permission` (`id_permission`, `name`, `description`) VALUES
    (1, 'CreateResource', 'Permission to create new resources.'),
    (2, 'EditResource', 'Permission to edit existing resources.'),
    (3, 'DeleteResource', 'Permission to delete resources.'),
    (4, 'ManageUsers', 'Permission to manage user accounts and roles.'),
    (5, 'ModerateContent', 'Permission to moderate user-generated content.'),
    (6, 'ApproveArticle', 'Permission to approve articles for publication.'),
    (7, 'ManageCategories', 'Permission to create, edit, and delete categories.'),
    (8, 'ManageDepartments', 'Permission to create, edit, and delete departments.'),
    (9, 'ViewAnalytics', 'Permission to view platform analytics and reports.'),
    (10, 'ManageRoles', 'Permission to create, edit, and delete user roles and permissions.'),
    (11, 'AccessAdminPanel', 'Permission to access the administrative panel of the platform.'),
    (12, 'BanUser', 'Permission to ban or suspend user accounts for violations of platform rules.'),
    (13, 'DeleteComment', 'Permission to delete inappropriate comments from resources.'),
    (14, 'DeleteOtherUserContent', 'Permission to delete content created by other users, such as articles or events.'),
    (15, 'ApproveEvent', 'Permission to approve events for publication.');

INSERT IGNORE INTO `role` (`id_role`, `name`, `description`) VALUES
    (1, 'User', 'Standard user with basic permissions.'),
    (2, 'Admin', 'User with administrative permissions.'),
    (3, 'SuperAdmin', 'User with all permissions, including management of other admins.');

INSERT IGNORE INTO `role_permission` (`id_role`, `id_permission`) VALUES
    (1, 1),
    (1, 2),
    (1, 3),
    (2, 1),
    (2, 2),
    (2, 3),
    (2, 4),
    (2, 5),
    (2, 15),
    (3, 1),
    (3, 2),
    (3, 3),
    (3, 4),
    (3, 5),
    (3, 15);
    
