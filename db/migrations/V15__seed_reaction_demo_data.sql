-- Seed demo data for reactions.
-- This migration inserts a small, self-contained dataset:
-- - demo users dedicated to reactions
-- - demo resources that can receive reactions
-- - demo reactions using the supported enum values

-- Demo users
-- Passwords use the legacy SHA256 format supported by the API:
-- - reaction.user.one@example.com => Password123!
-- - reaction.user.two@example.com => Comment123!
-- - reaction.user.three@example.com => Reply123!
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
        9501,
        'reaction_user_one',
        'ReactionOne',
        '1998-01-15',
        'Demo user used for reaction test data.',
        'reaction.user.one@example.com',
        'A109E36947AD56DE1DCA1CC49F0EF8AC9AD9A7B1AA0DF41FB3C4CB73C1FF01EA',
        1,
        NULL,
        1,
        1
    ),
    (
        9502,
        'reaction_user_two',
        'ReactionTwo',
        '1999-04-22',
        'Second demo user used for reaction test data.',
        'reaction.user.two@example.com',
        '853E87E38F9FB816C44BF266434CF077B3BE7A0AC3639C0712D422F1282DAE39',
        1,
        NULL,
        1,
        1
    ),
    (
        9503,
        'reaction_user_three',
        'ReactionThree',
        '2000-09-09',
        'Third demo user used for reaction test data.',
        'reaction.user.three@example.com',
        '99E9C3414F2C6CBA7940D5AB5553E6D15202B0BD4F9B5470C46C53DD137AB3FA',
        1,
        NULL,
        1,
        1
    );

-- Demo resources
INSERT IGNORE INTO `resource`
(
    `id_ressource`,
    `title`,
    `description`,
    `type`,
    `is_verified`,
    `visibility`,
    `created_at`,
    `modified_at`,
    `deleted_at`,
    `id_user`,
    `id_category`
)
VALUES
    (
        9601,
        'Resource demo reactions article',
        'Article resource dedicated to reaction test data.',
        'article',
        1,
        'public',
        '2026-03-12 09:00:00',
        NULL,
        NULL,
        9501,
        COALESCE(
            (SELECT `id_category` FROM `category` WHERE `name` = 'Atelier' ORDER BY `id_category` LIMIT 1),
            (SELECT `id_category` FROM `category` ORDER BY `id_category` LIMIT 1)
        )
    ),
    (
        9602,
        'Resource demo reactions event',
        'Event resource dedicated to reaction test data.',
        'event',
        1,
        'public',
        '2026-03-12 10:00:00',
        NULL,
        NULL,
        9501,
        COALESCE(
            (SELECT `id_category` FROM `category` WHERE `name` = 'Forum' ORDER BY `id_category` LIMIT 1),
            (SELECT `id_category` FROM `category` ORDER BY `id_category` LIMIT 1)
        )
    );

-- Demo reactions
-- Unique pair per (id_ressource, id_user), compatible with V12.
INSERT IGNORE INTO `reaction`
(
    `id_reaction`,
    `name`,
    `id_ressource`,
    `id_user`
)
VALUES
    (9701, 'like', 9601, 9501),
    (9702, 'love', 9601, 9502),
    (9703, 'dislike', 9601, 9503),
    (9704, 'love', 9602, 9501),
    (9705, 'like', 9602, 9502),
    (9706, 'dislike', 9602, 9503);
