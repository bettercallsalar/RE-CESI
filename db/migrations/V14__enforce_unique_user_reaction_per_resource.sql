-- Keep a single reaction per user and per resource.
-- When duplicates exist, keep the most recent row (highest id_reaction).

DELETE r1
FROM `reaction` r1
INNER JOIN `reaction` r2
    ON r1.id_ressource = r2.id_ressource
   AND r1.id_user = r2.id_user
   AND r1.id_reaction < r2.id_reaction;

ALTER TABLE `reaction`
    ADD CONSTRAINT `uq_reaction_resource_user` UNIQUE (`id_ressource`, `id_user`);
