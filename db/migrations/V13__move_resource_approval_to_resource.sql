SET @resource_has_is_approved := (
    SELECT COUNT(*)
    FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA = DATABASE()
      AND TABLE_NAME = 'resource'
      AND COLUMN_NAME = 'is_approved'
);

SET @add_resource_is_approved_sql := IF(
    @resource_has_is_approved = 0,
    'ALTER TABLE `resource` ADD COLUMN `is_approved` tinyint(1) NOT NULL DEFAULT ''0'' AFTER `type`',
    'SELECT 1'
);

PREPARE add_resource_is_approved_stmt FROM @add_resource_is_approved_sql;
EXECUTE add_resource_is_approved_stmt;
DEALLOCATE PREPARE add_resource_is_approved_stmt;

SET @article_has_is_approved := (
    SELECT COUNT(*)
    FROM information_schema.COLUMNS
    WHERE TABLE_SCHEMA = DATABASE()
      AND TABLE_NAME = 'article'
      AND COLUMN_NAME = 'is_approved'
);

SET @backfill_resource_is_approved_sql := IF(
    @article_has_is_approved = 1,
    'UPDATE `resource` r INNER JOIN `article` a ON a.`id_ressource` = r.`id_ressource` SET r.`is_approved` = a.`is_approved` WHERE r.`type` = ''article''',
    'SELECT 1'
);

PREPARE backfill_resource_is_approved_stmt FROM @backfill_resource_is_approved_sql;
EXECUTE backfill_resource_is_approved_stmt;
DEALLOCATE PREPARE backfill_resource_is_approved_stmt;

SET @drop_article_is_approved_sql := IF(
    @article_has_is_approved = 1,
    'ALTER TABLE `article` DROP COLUMN `is_approved`',
    'SELECT 1'
);

PREPARE drop_article_is_approved_stmt FROM @drop_article_is_approved_sql;
EXECUTE drop_article_is_approved_stmt;
DEALLOCATE PREPARE drop_article_is_approved_stmt;
