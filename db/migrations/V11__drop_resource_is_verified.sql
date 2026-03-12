SET @resource_has_is_verified := (
    SELECT COUNT(*)
    FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA = DATABASE()
      AND TABLE_NAME = 'resource'
      AND COLUMN_NAME = 'is_verified'
);

SET @drop_resource_is_verified_sql := IF(
    @resource_has_is_verified = 1,
    'ALTER TABLE `resource` DROP COLUMN `is_verified`',
    'SELECT 1'
);

PREPARE drop_resource_is_verified_stmt FROM @drop_resource_is_verified_sql;
EXECUTE drop_resource_is_verified_stmt;
DEALLOCATE PREPARE drop_resource_is_verified_stmt;
