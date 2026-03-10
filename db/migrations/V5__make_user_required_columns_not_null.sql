-- Enforce non-null user identity fields requested by domain rules:
-- - first_name
-- - id_department
-- - id_role
--
-- Before applying NOT NULL, normalize existing rows to valid values.

SET @default_department_id := (SELECT id_department FROM `department` ORDER BY id_department LIMIT 1);

INSERT INTO `department` (`name`, `code`)
SELECT 'General', 0
WHERE @default_department_id IS NULL;

SET @default_department_id := (SELECT id_department FROM `department` ORDER BY id_department LIMIT 1);

INSERT INTO `role` (`name`, `description`)
SELECT 'User', 'Default user role'
WHERE NOT EXISTS (SELECT 1 FROM `role` WHERE `name` = 'User');

SET @default_role_id := (SELECT id_role FROM `role` WHERE `name` = 'User' ORDER BY id_role LIMIT 1);
SET @default_role_id := COALESCE(@default_role_id, (SELECT id_role FROM `role` ORDER BY id_role LIMIT 1));

UPDATE `user`
SET `first_name` = 'Unknown'
WHERE `first_name` IS NULL OR TRIM(`first_name`) = '';

UPDATE `user`
SET `id_department` = @default_department_id
WHERE `id_department` IS NULL;

UPDATE `user`
SET `id_role` = @default_role_id
WHERE `id_role` IS NULL;

ALTER TABLE `user`
  DROP FOREIGN KEY `fk_user_department`,
  DROP FOREIGN KEY `fk_user_role`;

ALTER TABLE `user`
  MODIFY `first_name` varchar(50) COLLATE utf8mb4_unicode_ci NOT NULL,
  MODIFY `id_department` int NOT NULL,
  MODIFY `id_role` int NOT NULL;

ALTER TABLE `user`
  ADD CONSTRAINT `fk_user_department` FOREIGN KEY (`id_department`) REFERENCES `department` (`id_department`) ON DELETE RESTRICT ON UPDATE CASCADE,
  ADD CONSTRAINT `fk_user_role` FOREIGN KEY (`id_role`) REFERENCES `role` (`id_role`) ON DELETE RESTRICT ON UPDATE CASCADE;
