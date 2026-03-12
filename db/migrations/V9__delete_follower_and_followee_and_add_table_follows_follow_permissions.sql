-- Drop old tables if they exist
DROP TABLE IF EXISTS follower;
DROP TABLE IF EXISTS followee;

-- Create the unified follows table
CREATE TABLE IF NOT EXISTS follows (
  id_follower  INT NOT NULL,
  id_following  INT NOT NULL,
  PRIMARY KEY (id_follower, id_following),
  FOREIGN KEY (id_follower) REFERENCES user(id_user) ON DELETE CASCADE,
  FOREIGN KEY (id_following) REFERENCES user(id_user) ON DELETE CASCADE
);

-- Add permisisions for the new follows table
INSERT IGNORE INTO `permission` (`id_permission`, `name`, `description`) VALUES
    (15, 'FollowUser', 'Permission to follow, unfollow other users.');

-- Allow admin-level roles to manage role-permission assignments.
INSERT IGNORE INTO `role_permission` (`id_role`, `id_permission`) VALUES
    (1, 15); -- User -> FollowUser