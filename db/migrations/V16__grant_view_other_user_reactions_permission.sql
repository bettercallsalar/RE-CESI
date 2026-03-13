INSERT IGNORE INTO `permission` (`id_permission`, `name`, `description`) VALUES
    (16, 'ViewOtherUserReactions', 'Permission to view reactions created by another user.');

INSERT IGNORE INTO `role_permission` (`id_role`, `id_permission`) VALUES
    (2, 16),
    (3, 16);
