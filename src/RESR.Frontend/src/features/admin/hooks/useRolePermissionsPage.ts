import { useEffect, useState } from "react";
import { useAuth } from "@/features/auth/hooks/useAuth";
import { adminService } from "@/features/admin/services/admin.service";
import type { Permission, Role } from "@/features/admin/types/admin.types";
import type { FeedbackMessage } from "@/shared/ui/feedback/message.types";
import { createErrorMessage, createSuccessMessage, showFormMessage } from "@/shared/lib/feedback/showFormMessage";

function sortPermissions(permissions: Permission[]) {
  return [...permissions].sort((left, right) => left.name.localeCompare(right.name, "fr"));
}

export function useRolePermissionsPage(idRole: number) {
  const { token } = useAuth();
  const [role, setRole] = useState<Role | null>(null);
  const [allPermissions, setAllPermissions] = useState<Permission[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [message, setMessage] = useState<FeedbackMessage | null>(null);

  useEffect(() => {
    let cancelled = false;

    async function loadRole() {
      if (!token) {
        setRole(null);
        setAllPermissions([]);
        setIsLoading(false);
        return;
      }

      setIsLoading(true);

      try {
        const [roleResponse, permissionsResponse] = await Promise.all([
          adminService.getRole(token, idRole),
          adminService.getPermissions(token)
        ]);

        if (cancelled) {
          return;
        }

        const sortedRole = {
          ...roleResponse,
          permissions: sortPermissions(roleResponse.permissions)
        };

        setRole(sortedRole);
        setAllPermissions(sortPermissions(permissionsResponse));
        showFormMessage(setMessage, null);
      } catch (loadError) {
        if (!cancelled) {
          setRole(null);
          setAllPermissions([]);
          showFormMessage(setMessage, createErrorMessage(loadError, "Chargement impossible"));
        }
      } finally {
        if (!cancelled) {
          setIsLoading(false);
        }
      }
    }

    void loadRole();

    return () => {
      cancelled = true;
    };
  }, [idRole, token]);

  async function activatePermission(idPermission: number) {
    if (!token || !role) {
      return;
    }

    const permissionToAdd = allPermissions.find((permission) => permission.idPermission === idPermission);

    if (!permissionToAdd) {
      return;
    }

    if (role.permissions.some((permission) => permission.idPermission === idPermission)) {
      return;
    }

    setIsSubmitting(true);

    try {
      await adminService.addPermissionToRole(token, idRole, idPermission);

      const nextRole = {
        ...role,
        permissions: sortPermissions([...role.permissions, permissionToAdd])
      };

      setRole(nextRole);
      showFormMessage(setMessage, createSuccessMessage(`La permission ${permissionToAdd.name} a ete ajoutee au role ${role.name}.`));
    } catch (submitError) {
      showFormMessage(setMessage, createErrorMessage(submitError, "Mise a jour impossible"));
    } finally {
      setIsSubmitting(false);
    }
  }

  async function deactivatePermission(idPermission: number) {
    if (!token || !role) {
      return;
    }

    const permissionToRemove = role.permissions.find((permission) => permission.idPermission === idPermission);

    if (!permissionToRemove) {
      return;
    }

    setIsSubmitting(true);

    try {
      await adminService.removePermissionFromRole(token, idRole, idPermission);

      const nextRole = {
        ...role,
        permissions: sortPermissions(role.permissions.filter((permission) => permission.idPermission !== idPermission))
      };

      setRole(nextRole);
      showFormMessage(setMessage, createSuccessMessage(`La permission ${permissionToRemove.name} a ete retiree du role ${role.name}.`));
    } catch (submitError) {
      showFormMessage(setMessage, createErrorMessage(submitError, "Mise a jour impossible"));
    } finally {
      setIsSubmitting(false);
    }
  }

  return {
    role,
    allPermissions,
    isLoading,
    isSubmitting,
    message,
    activatePermission,
    deactivatePermission
  };
}
