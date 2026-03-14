import { useEffect, useState } from "react";
import { useAuth } from "@/features/auth/hooks/useAuth";
import { adminService } from "@/features/admin/services/admin.service";
import type { Role } from "@/features/admin/types/admin.types";
import type { FeedbackMessage } from "@/shared/ui/feedback/message.types";
import { createErrorMessage, showFormMessage } from "@/shared/lib/feedback/showFormMessage";

export function useRolesManagementPage() {
  const { token } = useAuth();
  const [roles, setRoles] = useState<Role[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [message, setMessage] = useState<FeedbackMessage | null>(null);

  useEffect(() => {
    let cancelled = false;

    async function loadRoles() {
      if (!token) {
        setRoles([]);
        setIsLoading(false);
        return;
      }

      setIsLoading(true);

      try {
        const response = await adminService.getRoles(token);

        if (cancelled) {
          return;
        }

        setRoles(response);
        showFormMessage(setMessage, null);
      } catch (loadError) {
        if (!cancelled) {
          showFormMessage(setMessage, createErrorMessage(loadError, "Chargement impossible"));
        }
      } finally {
        if (!cancelled) {
          setIsLoading(false);
        }
      }
    }

    void loadRoles();

    return () => {
      cancelled = true;
    };
  }, [token]);

  return {
    roles,
    isLoading,
    message
  };
}
