-- Basic authenticated actions no longer rely on explicit role permissions.
DELETE rp
FROM `role_permission` rp
INNER JOIN `permission` p ON p.id_permission = rp.id_permission
WHERE p.name IN ('CreateResource', 'EditResource', 'DeleteResource', 'FollowUser');

-- V9 assigned permission id 15 to the User role while id 15 is ApproveEvent in V4.
DELETE FROM `role_permission`
WHERE id_role = 1
  AND id_permission = 15
  AND EXISTS (
    SELECT 1
    FROM `permission`
    WHERE id_permission = 15
      AND name = 'ApproveEvent'
  );

DELETE FROM `permission`
WHERE name IN ('CreateResource', 'EditResource', 'DeleteResource', 'FollowUser');
