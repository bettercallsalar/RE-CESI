-- Restrict reaction names to the supported set used by the API.
-- Existing values are normalized before altering the column type.

UPDATE `reaction`
SET `name` = LOWER(TRIM(`name`));

UPDATE `reaction`
SET `name` = 'like'
WHERE `name` NOT IN ('like', 'dislike', 'love');

ALTER TABLE `reaction`
    MODIFY `name` ENUM('like', 'dislike', 'love')
    COLLATE utf8mb4_unicode_ci
    NOT NULL;
