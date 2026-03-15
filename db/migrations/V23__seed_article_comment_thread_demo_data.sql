-- Keep this demo seed at the end of the migration chain to avoid version conflicts.
-- Seed data for article detail comment threads.
-- This migration creates:
-- - demo users dedicated to article discussions
-- - one public approved article resource
-- - a threaded comment tree with nested replies and a deleted parent comment

INSERT INTO `category` (`id_category`, `name`)
SELECT 9800, 'Actualites'
WHERE NOT EXISTS (SELECT 1 FROM `category`);

INSERT INTO `department` (`id_department`, `name`, `code`)
SELECT 9900, 'General', 0
WHERE NOT EXISTS (SELECT 1 FROM `department`);

INSERT IGNORE INTO `role` (`id_role`, `name`, `description`)
VALUES (1, 'User', 'Standard user with basic permissions.');

SET @demo_category_id := COALESCE(
    (SELECT `id_category` FROM `category` WHERE `name` IN ('Atelier', 'Actualites') ORDER BY `id_category` LIMIT 1),
    (SELECT `id_category` FROM `category` ORDER BY `id_category` LIMIT 1)
);

SET @demo_department_id := COALESCE(
    (SELECT `id_department` FROM `department` ORDER BY `id_department` LIMIT 1),
    9900
);

INSERT IGNORE INTO `user`
(
    `id_user`,
    `username`,
    `first_name`,
    `birth_date`,
    `bio`,
    `email`,
    `hashed_password`,
    `is_verified`,
    `deleted_at`,
    `id_department`,
    `id_role`
)
VALUES
    (
        9801,
        'article_author_demo',
        'Auteur',
        '1997-04-14',
        'Auteur de demonstration pour la lecture d''article.',
        'article.author.demo@example.com',
        'A109E36947AD56DE1DCA1CC49F0EF8AC9AD9A7B1AA0DF41FB3C4CB73C1FF01EA',
        1,
        NULL,
        @demo_department_id,
        1
    ),
    (
        9802,
        'comment_reader_one',
        'Camille',
        '1998-07-03',
        'Utilisateur de demonstration pour les commentaires.',
        'comment.reader.one@example.com',
        '853E87E38F9FB816C44BF266434CF077B3BE7A0AC3639C0712D422F1282DAE39',
        1,
        NULL,
        @demo_department_id,
        1
    ),
    (
        9803,
        'comment_reader_two',
        'Yanis',
        '2000-02-19',
        'Utilisateur de demonstration pour les reponses.',
        'comment.reader.two@example.com',
        '99E9C3414F2C6CBA7940D5AB5553E6D15202B0BD4F9B5470C46C53DD137AB3FA',
        1,
        NULL,
        @demo_department_id,
        1
    ),
    (
        9804,
        'comment_reader_three',
        'Salome',
        '1999-11-28',
        'Profil de demonstration pour les discussions imbriquees.',
        'comment.reader.three@example.com',
        'A109E36947AD56DE1DCA1CC49F0EF8AC9AD9A7B1AA0DF41FB3C4CB73C1FF01EA',
        1,
        NULL,
        @demo_department_id,
        1
    ),
    (
        9805,
        'comment_reader_four',
        'Noa',
        '1996-05-06',
        'Utilisateur de demonstration pour les longues branches de reponses.',
        'comment.reader.four@example.com',
        '853E87E38F9FB816C44BF266434CF077B3BE7A0AC3639C0712D422F1282DAE39',
        1,
        NULL,
        @demo_department_id,
        1
    );

INSERT IGNORE INTO `resource`
(
    `id_ressource`,
    `title`,
    `description`,
    `type`,
    `is_approved`,
    `visibility`,
    `created_at`,
    `modified_at`,
    `deleted_at`,
    `id_user`,
    `id_category`
)
VALUES
    (
        9810,
        'Article demo commentaires',
        'Article public de demonstration pour valider l''affichage d''un thread de commentaires imbriques.',
        'article',
        1,
        'public',
        '2026-03-14 09:00:00',
        NULL,
        NULL,
        9801,
        @demo_category_id
    );

INSERT IGNORE INTO `article`
(
    `id_article`,
    `content`,
    `id_ressource`
)
VALUES
    (
        9810,
        'Cet article de demonstration sert a tester la page detail MAUI avec un vrai thread de commentaires, des reponses imbriquees et un parent supprime qui conserve sa branche visible.',
        9810
    );

INSERT IGNORE INTO `comment`
(
    `id_comment`,
    `content`,
    `created_at`,
    `modified_at`,
    `deleted_at`,
    `id_ressource`,
    `id_user`
)
VALUES
    (
        9821,
        'Le sujet est super clair, merci pour le partage.',
        '2026-03-14 09:10:00',
        NULL,
        NULL,
        9810,
        9802
    ),
    (
        9822,
        'Je suis d''accord, la structure est facile a suivre.',
        '2026-03-14 09:12:00',
        NULL,
        NULL,
        9810,
        9803
    ),
    (
        9823,
        'Tu as teste les exemples proposes a la fin de l''article ?',
        '2026-03-14 09:15:00',
        NULL,
        NULL,
        9810,
        9804
    ),
    (
        9824,
        'Oui, surtout la partie sur l''organisation des reponses. C''est celle que j''attendais.',
        '2026-03-14 09:18:00',
        '2026-03-14 09:19:30',
        NULL,
        9810,
        9802
    ),
    (
        9825,
        'Je verrais bien un exemple supplementaire avec moderation plus tard.',
        '2026-03-14 09:22:00',
        NULL,
        NULL,
        9810,
        9805
    ),
    (
        9826,
        'Bonne idee, surtout si on ajoute aussi les reactions dans le detail.',
        '2026-03-14 09:25:00',
        NULL,
        NULL,
        9810,
        9801
    ),
    (
        9827,
        'Ancien commentaire retire par moderation.',
        '2026-03-14 09:28:00',
        NULL,
        '2026-03-14 09:35:00',
        9810,
        9804
    ),
    (
        9828,
        'Je laisse ma reponse dessous pour que la discussion reste lisible.',
        '2026-03-14 09:31:00',
        NULL,
        NULL,
        9810,
        9803
    ),
    (
        9829,
        'On pourrait aussi ajouter un bouton pour tout replier.',
        '2026-03-14 09:33:00',
        NULL,
        NULL,
        9810,
        9805
    ),
    (
        9830,
        'Clairement, surtout sur mobile quand la branche devient profonde.',
        '2026-03-14 09:36:00',
        NULL,
        NULL,
        9810,
        9802
    );

INSERT IGNORE INTO `reply`
(
    `id_comment`,
    `id_comment_post`
)
VALUES
    (9821, 9822),
    (9822, 9823),
    (9823, 9824),
    (9821, 9829),
    (9829, 9830),
    (9825, 9826),
    (9827, 9828);
