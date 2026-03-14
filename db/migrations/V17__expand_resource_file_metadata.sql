ALTER TABLE `file`
    CHANGE COLUMN `name` `file_name` varchar(255) COLLATE utf8mb4_unicode_ci NOT NULL,
    MODIFY COLUMN `path` varchar(255) COLLATE utf8mb4_unicode_ci NOT NULL,
    ADD COLUMN `original_name` varchar(255) COLLATE utf8mb4_unicode_ci NULL AFTER `file_name`,
    ADD COLUMN `mime_type` varchar(100) COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT 'application/octet-stream' AFTER `original_name`,
    ADD COLUMN `size` int NOT NULL DEFAULT 0 AFTER `mime_type`,
    ADD COLUMN `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP AFTER `path`,
    ADD COLUMN `created_by` varchar(255) COLLATE utf8mb4_unicode_ci NULL AFTER `created_at`,
    ADD COLUMN `updated_at` datetime NULL AFTER `created_by`,
    ADD COLUMN `updated_by` varchar(255) COLLATE utf8mb4_unicode_ci NULL AFTER `updated_at`;

UPDATE `file`
SET `original_name` = `file_name`
WHERE `original_name` IS NULL;
