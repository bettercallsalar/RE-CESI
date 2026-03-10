-- Imported from: /Users/salar/Downloads/Script setup db mysql.sql
-- Adapted for Flyway: removed CREATE DATABASE statement; migration runs in current database.


-- social_platform.category definition

CREATE TABLE `category` (
  `id_category` int NOT NULL AUTO_INCREMENT,
  `name` varchar(50) COLLATE utf8mb4_unicode_ci NOT NULL,
  PRIMARY KEY (`id_category`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;


-- social_platform.department definition

CREATE TABLE `department` (
  `id_department` int NOT NULL AUTO_INCREMENT,
  `name` varchar(50) COLLATE utf8mb4_unicode_ci NOT NULL,
  `code` int DEFAULT NULL,
  PRIMARY KEY (`id_department`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;


-- social_platform.permission definition

CREATE TABLE `permission` (
  `id_permission` int NOT NULL AUTO_INCREMENT,
  `name` varchar(50) COLLATE utf8mb4_unicode_ci NOT NULL,
  `description` varchar(255) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  PRIMARY KEY (`id_permission`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;


-- social_platform.`role` definition

CREATE TABLE `role` (
  `id_role` int NOT NULL AUTO_INCREMENT,
  `name` varchar(50) COLLATE utf8mb4_unicode_ci NOT NULL,
  `description` varchar(255) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  PRIMARY KEY (`id_role`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;


-- social_platform.role_permission definition

CREATE TABLE `role_permission` (
  `id_role` int NOT NULL,
  `id_permission` int NOT NULL,
  PRIMARY KEY (`id_role`,`id_permission`),
  KEY `fk_role_permission_permission` (`id_permission`),
  CONSTRAINT `fk_role_permission_permission` FOREIGN KEY (`id_permission`) REFERENCES `permission` (`id_permission`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `fk_role_permission_role` FOREIGN KEY (`id_role`) REFERENCES `role` (`id_role`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;


-- social_platform.`user` definition

CREATE TABLE `user` (
  `id_user` int NOT NULL AUTO_INCREMENT,
  `username` varchar(50) COLLATE utf8mb4_unicode_ci NOT NULL,
  `first_name` varchar(50) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `birth_date` date DEFAULT NULL,
  `bio` text COLLATE utf8mb4_unicode_ci,
  `email` varchar(255) COLLATE utf8mb4_unicode_ci NOT NULL,
  `hashed_password` varchar(255) COLLATE utf8mb4_unicode_ci NOT NULL,
  `is_verified` bit(1) NOT NULL DEFAULT b'0',
  `deleted_at` datetime DEFAULT NULL,
  `id_department` int DEFAULT NULL,
  `id_role` int DEFAULT NULL,
  PRIMARY KEY (`id_user`),
  UNIQUE KEY `uq_user_email` (`email`),
  UNIQUE KEY `uq_user_username` (`username`),
  KEY `idx_user_department` (`id_department`),
  KEY `idx_user_role` (`id_role`),
  CONSTRAINT `fk_user_department` FOREIGN KEY (`id_department`) REFERENCES `department` (`id_department`) ON DELETE SET NULL ON UPDATE CASCADE,
  CONSTRAINT `fk_user_role` FOREIGN KEY (`id_role`) REFERENCES `role` (`id_role`) ON DELETE SET NULL ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;


-- social_platform.user_category definition

CREATE TABLE `user_category` (
  `id_user` int NOT NULL,
  `id_category` int NOT NULL,
  PRIMARY KEY (`id_user`,`id_category`),
  KEY `idx_user_category_category` (`id_category`),
  CONSTRAINT `fk_user_category_category` FOREIGN KEY (`id_category`) REFERENCES `category` (`id_category`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `fk_user_category_user` FOREIGN KEY (`id_user`) REFERENCES `user` (`id_user`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;


-- social_platform.followee definition

CREATE TABLE `followee` (
  `id_user` int NOT NULL,
  `id_user_followee` int NOT NULL,
  PRIMARY KEY (`id_user`,`id_user_followee`),
  KEY `fk_followee_user_1` (`id_user_followee`),
  CONSTRAINT `fk_followee_user` FOREIGN KEY (`id_user`) REFERENCES `user` (`id_user`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `fk_followee_user_1` FOREIGN KEY (`id_user_followee`) REFERENCES `user` (`id_user`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;


-- social_platform.follower definition

CREATE TABLE `follower` (
  `id_user` int NOT NULL,
  `id_user_follow` int NOT NULL,
  PRIMARY KEY (`id_user`,`id_user_follow`),
  KEY `fk_follower_user_1` (`id_user_follow`),
  CONSTRAINT `fk_follower_user` FOREIGN KEY (`id_user`) REFERENCES `user` (`id_user`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `fk_follower_user_1` FOREIGN KEY (`id_user_follow`) REFERENCES `user` (`id_user`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;


-- social_platform.resource definition

CREATE TABLE `resource` (
  `id_ressource` int NOT NULL AUTO_INCREMENT,
  `title` varchar(50) COLLATE utf8mb4_unicode_ci NOT NULL,
  `description` text COLLATE utf8mb4_unicode_ci,
  `type` enum('article','event') COLLATE utf8mb4_unicode_ci NOT NULL,
  `is_verified` bit(1) NOT NULL DEFAULT b'0',
  `visibility` enum('public','private') COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT 'public',
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `modified_at` datetime DEFAULT NULL,
  `deleted_at` datetime DEFAULT NULL,
  `id_user` int NOT NULL,
  `id_category` int NOT NULL,
  PRIMARY KEY (`id_ressource`),
  KEY `idx_resource_user` (`id_user`),
  KEY `idx_resource_category` (`id_category`),
  CONSTRAINT `fk_resource_category` FOREIGN KEY (`id_category`) REFERENCES `category` (`id_category`) ON DELETE RESTRICT ON UPDATE CASCADE,
  CONSTRAINT `fk_resource_user` FOREIGN KEY (`id_user`) REFERENCES `user` (`id_user`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;


-- social_platform.article definition

CREATE TABLE `article` (
  `id_article` int NOT NULL AUTO_INCREMENT,
  `content` text COLLATE utf8mb4_unicode_ci NOT NULL,
  `is_approved` tinyint(1) NOT NULL DEFAULT '0',
  `id_ressource` int NOT NULL,
  PRIMARY KEY (`id_article`),
  UNIQUE KEY `uq_article_resource` (`id_ressource`),
  CONSTRAINT `fk_article_resource` FOREIGN KEY (`id_ressource`) REFERENCES `resource` (`id_ressource`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;


-- social_platform.comment definition

CREATE TABLE `comment` (
  `id_comment` int NOT NULL AUTO_INCREMENT,
  `content` text COLLATE utf8mb4_unicode_ci NOT NULL,
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `modified_at` datetime DEFAULT NULL,
  `deleted_at` datetime DEFAULT NULL,
  `id_ressource` int NOT NULL,
  `id_user` int NOT NULL,
  PRIMARY KEY (`id_comment`),
  KEY `idx_comment_resource` (`id_ressource`),
  KEY `idx_comment_user` (`id_user`),
  CONSTRAINT `fk_comment_resource` FOREIGN KEY (`id_ressource`) REFERENCES `resource` (`id_ressource`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `fk_comment_user` FOREIGN KEY (`id_user`) REFERENCES `user` (`id_user`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;


-- social_platform.event definition

CREATE TABLE `event` (
  `id_event` int NOT NULL AUTO_INCREMENT,
  `subtitle` varchar(255) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `start_date` datetime NOT NULL,
  `end_date` datetime DEFAULT NULL,
  `adress` varchar(255) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `id_department` int DEFAULT NULL,
  `id_ressource` int NOT NULL,
  PRIMARY KEY (`id_event`),
  UNIQUE KEY `uq_event_resource` (`id_ressource`),
  KEY `idx_event_department` (`id_department`),
  CONSTRAINT `fk_event_department` FOREIGN KEY (`id_department`) REFERENCES `department` (`id_department`) ON DELETE SET NULL ON UPDATE CASCADE,
  CONSTRAINT `fk_event_resource` FOREIGN KEY (`id_ressource`) REFERENCES `resource` (`id_ressource`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;


-- social_platform.file definition

CREATE TABLE `file` (
  `id_file` int NOT NULL AUTO_INCREMENT,
  `name` varchar(50) COLLATE utf8mb4_unicode_ci NOT NULL,
  `path` varchar(50) COLLATE utf8mb4_unicode_ci NOT NULL,
  `id_ressource` int NOT NULL,
  PRIMARY KEY (`id_file`),
  KEY `idx_file_resource` (`id_ressource`),
  CONSTRAINT `fk_file_resource` FOREIGN KEY (`id_ressource`) REFERENCES `resource` (`id_ressource`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;


-- social_platform.mark definition

CREATE TABLE `mark` (
  `id_mark` int NOT NULL AUTO_INCREMENT,
  `is_favorite` bit(1) NOT NULL DEFAULT b'0',
  `is_read_later` bit(1) NOT NULL DEFAULT b'0',
  `id_ressource` int NOT NULL,
  `id_user` int NOT NULL,
  PRIMARY KEY (`id_mark`),
  UNIQUE KEY `uq_mark_user_resource` (`id_user`,`id_ressource`),
  KEY `idx_mark_resource` (`id_ressource`),
  CONSTRAINT `fk_mark_resource` FOREIGN KEY (`id_ressource`) REFERENCES `resource` (`id_ressource`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `fk_mark_user` FOREIGN KEY (`id_user`) REFERENCES `user` (`id_user`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;


-- social_platform.reaction definition

CREATE TABLE `reaction` (
  `id_reaction` int NOT NULL AUTO_INCREMENT,
  `name` varchar(50) COLLATE utf8mb4_unicode_ci NOT NULL,
  `id_ressource` int NOT NULL,
  `id_user` int NOT NULL,
  PRIMARY KEY (`id_reaction`),
  KEY `idx_reaction_resource` (`id_ressource`),
  KEY `idx_reaction_user` (`id_user`),
  CONSTRAINT `fk_reaction_resource` FOREIGN KEY (`id_ressource`) REFERENCES `resource` (`id_ressource`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `fk_reaction_user` FOREIGN KEY (`id_user`) REFERENCES `user` (`id_user`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;


-- social_platform.reply definition

CREATE TABLE `reply` (
  `id_comment` int NOT NULL,
  `id_comment_post` int NOT NULL,
  PRIMARY KEY (`id_comment`,`id_comment_post`),
  KEY `fk_reply_comment_child` (`id_comment_post`),
  CONSTRAINT `fk_reply_comment_child` FOREIGN KEY (`id_comment_post`) REFERENCES `comment` (`id_comment`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `fk_reply_comment_parent` FOREIGN KEY (`id_comment`) REFERENCES `comment` (`id_comment`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;