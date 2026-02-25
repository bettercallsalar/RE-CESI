-- Allow reusing email/username after soft delete while keeping active users unique.
-- MySQL 8.4 supports functional indexes, so we can enforce uniqueness only for rows
-- where deleted_at IS NULL without adding a helper column.

ALTER TABLE `user`
  DROP INDEX `uq_user_email`,
  DROP INDEX `uq_user_username`;

CREATE UNIQUE INDEX `uq_user_email`
  ON `user` ((CASE WHEN `deleted_at` IS NULL THEN `email` ELSE NULL END));

CREATE UNIQUE INDEX `uq_user_username`
  ON `user` ((CASE WHEN `deleted_at` IS NULL THEN `username` ELSE NULL END));
