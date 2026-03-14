ALTER TABLE `user`
ADD COLUMN `is_banned` tinyint(1) NOT NULL DEFAULT 0
AFTER `is_verified`;
