ALTER TABLE `event`
    ADD COLUMN `default_image_id` int NULL AFTER `id_ressource`,
    ADD KEY `idx_event_default_image` (`default_image_id`),
    ADD CONSTRAINT `fk_event_default_image`
        FOREIGN KEY (`default_image_id`) REFERENCES `file` (`id_file`)
        ON DELETE SET NULL
        ON UPDATE CASCADE;

UPDATE `event` e
LEFT JOIN (
    SELECT f.`id_ressource`, MIN(f.`id_file`) AS `first_file_id`
    FROM `file` f
    GROUP BY f.`id_ressource`
) f ON f.`id_ressource` = e.`id_ressource`
SET e.`default_image_id` = f.`first_file_id`
WHERE e.`default_image_id` IS NULL;
