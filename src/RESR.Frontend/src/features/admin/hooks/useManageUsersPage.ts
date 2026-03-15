import { useEffect, useState } from "react";
import { useAuth } from "@/features/auth/hooks/useAuth";
import { adminService } from "@/features/admin/services/admin.service";
import type { RoleSummary } from "@/features/admin/types/admin.types";
import type { User } from "@/shared/types/user";
import type { FeedbackMessage } from "@/shared/ui/feedback/message.types";
import { createErrorMessage, createSuccessMessage, showFormMessage } from "@/shared/lib/feedback/showFormMessage";

const PAGE_SIZE = 12;

interface ManageUsersFilters {
  keyword: string;
  idRole: number | "";
}

const initialFilters: ManageUsersFilters = {
  keyword: "",
  idRole: ""
};

export function useManageUsersPage() {
  const { token } = useAuth();
  const [users, setUsers] = useState<User[]>([]);
  const [roles, setRoles] = useState<RoleSummary[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [message, setMessage] = useState<FeedbackMessage | null>(null);
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(0);
  const [totalCount, setTotalCount] = useState(0);
  const [filters, setFilters] = useState<ManageUsersFilters>(initialFilters);
  const [appliedFilters, setAppliedFilters] = useState<ManageUsersFilters>(initialFilters);

  function normalizeFilters(source: ManageUsersFilters) {
    return {
      keyword: source.keyword.trim(),
      idRole: source.idRole
    };
  }

  function applyUsersResponse(response: { items: User[]; page: number; totalPages: number; totalCount: number }) {
    setUsers(response.items);
    setPage(response.page);
    setTotalPages(response.totalPages);
    setTotalCount(response.totalCount);
  }

  async function loadUsers(nextPage: number, nextFilters: ManageUsersFilters, options?: { preserveMessage?: boolean }) {
    if (!token) {
      setUsers([]);
      setRoles([]);
      setIsLoading(false);
      return;
    }

    setIsLoading(true);

    try {
      const response = await adminService.getUsers(token, {
        page: nextPage,
        pageSize: PAGE_SIZE,
        keyword: nextFilters.keyword || undefined,
        roleIds: typeof nextFilters.idRole === "number" ? nextFilters.idRole : undefined
      });

      applyUsersResponse(response);

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
    if (!token) {
      setUsers([]);
      setRoles([]);
      setIsLoading(false);
      return;
    }

    const authenticatedToken = token;

    let cancelled = false;

    async function loadInitialState() {
      setIsLoading(true);

      try {
        const normalizedFilters = normalizeFilters(initialFilters);
        const [rolesResponse, usersResponse] = await Promise.all([
          adminService.getAssignableRoles(authenticatedToken),
          adminService.getUsers(authenticatedToken, {
            page: 1,
            pageSize: PAGE_SIZE,
            keyword: normalizedFilters.keyword || undefined
          })
        ]);

        if (cancelled) {
          return;
        }

        setRoles([...rolesResponse].sort((left, right) => left.idRole - right.idRole));
        applyUsersResponse(usersResponse);
        setAppliedFilters(normalizedFilters);
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

    void loadInitialState();

    return () => {
      cancelled = true;
    };
  }, [token]);

  async function goToPage(nextPage: number) {
    if (nextPage < 1 || (totalPages > 0 && nextPage > totalPages)) {
      return;
    }

    await loadUsers(nextPage, appliedFilters);
  }

  function updateFilter<K extends keyof ManageUsersFilters>(field: K, value: ManageUsersFilters[K]) {
    setFilters((current) => ({
      ...current,
      [field]: value
    }));
  }

  async function applyFilters() {
    const normalizedFilters = normalizeFilters(filters);
    setAppliedFilters(normalizedFilters);
    await loadUsers(1, normalizedFilters);
  }

  async function resetFilters() {
    setFilters(initialFilters);
    setAppliedFilters(initialFilters);
    await loadUsers(1, initialFilters);
  }

  async function setUserBanStatus(user: User, isBanned: boolean) {
    if (!token) {
      return;
    }

    setIsSubmitting(true);

    try {
      await adminService.setManageableUserBanStatus(token, user.idUser, isBanned);

      await loadUsers(page, appliedFilters, { preserveMessage: true });
      showFormMessage(setMessage, createSuccessMessage(`Le compte ${user.username} a ete ${isBanned ? "banni" : "debanni"}.`));
    } catch (submitError) {
      showFormMessage(setMessage, createErrorMessage(submitError, "Mise a jour impossible"));
    } finally {
      setIsSubmitting(false);
    }
  }

  async function setUserRole(user: User, idRole: number) {
    if (!token || user.idRole === idRole) {
      return;
    }

    setIsSubmitting(true);

    try {
      await adminService.updateUserRole(token, user.idUser, idRole);
      await loadUsers(page, appliedFilters, { preserveMessage: true });
      showFormMessage(setMessage, createSuccessMessage(`Le role de ${user.username} a ete mis a jour.`));
    } catch (submitError) {
      showFormMessage(setMessage, createErrorMessage(submitError, "Mise a jour impossible"));
    } finally {
      setIsSubmitting(false);
    }
  }

  return {
    users,
    roles,
    isLoading,
    isSubmitting,
    message,
    page,
    totalPages,
    totalCount,
    filters,
    updateFilter,
    applyFilters,
    resetFilters,
    goToPage,
    setUserBanStatus,
    setUserRole
  };
}
