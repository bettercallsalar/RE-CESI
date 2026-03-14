import { useEffect, useState } from "react";
import { useAuth } from "@/features/auth/hooks/useAuth";
import { adminService } from "@/features/admin/services/admin.service";
import type { User } from "@/shared/types/user";
import type { FeedbackMessage } from "@/shared/ui/feedback/message.types";
import { createErrorMessage, createSuccessMessage, showFormMessage } from "@/shared/lib/feedback/showFormMessage";

export function useManageUsersPage() {
  const { token } = useAuth();
  const [users, setUsers] = useState<User[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [message, setMessage] = useState<FeedbackMessage | null>(null);
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(0);
  const [totalCount, setTotalCount] = useState(0);

  async function loadUsers(nextPage: number, options?: { preserveMessage?: boolean }) {
    if (!token) {
      setUsers([]);
      setIsLoading(false);
      return;
    }

    setIsLoading(true);

    try {
      const response = await adminService.getManageableUsers(token, {
        page: nextPage,
        pageSize: 12
      });

      setUsers(response.items);
      setPage(response.page);
      setTotalPages(response.totalPages);
      setTotalCount(response.totalCount);

      if (!options?.preserveMessage) {
        showFormMessage(setMessage, null);
      }
    } catch (loadError) {
      showFormMessage(setMessage, createErrorMessage(loadError, "Chargement impossible"));
    } finally {
      setIsLoading(false);
    }
  }

  useEffect(() => {
    void loadUsers(1);
  }, [token]);

  async function goToPage(nextPage: number) {
    if (nextPage < 1 || (totalPages > 0 && nextPage > totalPages)) {
      return;
    }

    await loadUsers(nextPage);
  }

  async function setUserBanStatus(user: User, isBanned: boolean) {
    if (!token) {
      return;
    }

    setIsSubmitting(true);

    try {
      await adminService.setManageableUserBanStatus(token, user.idUser, isBanned);

      await loadUsers(page, { preserveMessage: true });
      showFormMessage(setMessage, createSuccessMessage(`Le compte ${user.username} a ete ${isBanned ? "banni" : "debanni"}.`));
    } catch (submitError) {
      showFormMessage(setMessage, createErrorMessage(submitError, "Mise a jour impossible"));
    } finally {
      setIsSubmitting(false);
    }
  }

  return {
    users,
    isLoading,
    isSubmitting,
    message,
    page,
    totalPages,
    totalCount,
    goToPage,
    setUserBanStatus
  };
}
