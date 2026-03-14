ALTER TABLE `article`
    ADD COLUMN `default_image_id` int NULL AFTER `content`,
    ADD KEY `idx_article_default_image` (`default_image_id`),
    ADD CONSTRAINT `fk_article_default_image`
        FOREIGN KEY (`default_image_id`) REFERENCES `file` (`id_file`)
        ON DELETE SET NULL
        ON UPDATE CASCADE;

UPDATE `article` a
LEFT JOIN (
    SELECT `id_ressource`, MIN(`id_file`) AS `first_file_id`
    FROM `file`
    GROUP BY `id_ressource`
) f ON f.`id_ressource` = a.`id_ressource`
SET a.`default_image_id` = f.`first_file_id`
WHERE a.`default_image_id` IS NULL;
