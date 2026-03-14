import { useEffect, useState } from "react";
import { useAuth } from "@/features/auth/hooks/useAuth";
import { eventsService } from "@/features/events/services/events.service";
import type { MyEventsFilters } from "@/features/events/types/event.types";
import type { Category } from "@/shared/types/article";
import type { Event } from "@/shared/types/event";
import type { Department } from "@/shared/types/user";
import type { FeedbackMessage } from "@/shared/ui/feedback/message.types";
import { createErrorMessage, createSuccessMessage, showFormMessage } from "@/shared/lib/feedback/showFormMessage";

const initialFilters: MyEventsFilters = {
  keyword: "",
  idCategory: "",
  idDepartment: "",
  startFrom: "",
  startTo: "",
  visibility: "",
  approval: ""
};

export function useMyEventsPage() {
  const { token, user } = useAuth();
  const [filters, setFilters] = useState<MyEventsFilters>(initialFilters);
  const [appliedFilters, setAppliedFilters] = useState<MyEventsFilters>(initialFilters);
  const [categories, setCategories] = useState<Category[]>([]);
  const [departments, setDepartments] = useState<Department[]>([]);
  const [events, setEvents] = useState<Event[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [message, setMessage] = useState<FeedbackMessage | null>(null);
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(0);
  const [totalCount, setTotalCount] = useState(0);
  const [isDeleting, setIsDeleting] = useState(false);

  useEffect(() => {
    let cancelled = false;

    async function loadInitialData() {
      if (!token || !user) {
        return;
      }

      setIsLoading(true);

      try {
        const [categoriesResponse, departmentsResponse, eventsResponse] = await Promise.all([
          eventsService.getCategories(),
          eventsService.getDepartments(),
          eventsService.getOwnEvents(token, user.idUser, { page: 1, pageSize: 9 })
        ]);

        if (cancelled) {
          return;
        }

        setCategories(categoriesResponse);
        setDepartments(departmentsResponse);
        setEvents(eventsResponse.items);
        setPage(eventsResponse.page);
        setTotalPages(eventsResponse.totalPages);
        setTotalCount(eventsResponse.totalCount);
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

    void loadInitialData();

    return () => {
      cancelled = true;
    };
  }, [token, user]);

  async function loadEvents(nextPage: number, nextFilters: MyEventsFilters) {
    if (!token || !user) {
      return;
    }

    setIsLoading(true);

    try {
      const response = await eventsService.getOwnEvents(token, user.idUser, {
        page: nextPage,
        pageSize: 9,
        keyword: nextFilters.keyword.trim() || undefined,
        idCategory: typeof nextFilters.idCategory === "number" ? nextFilters.idCategory : undefined,
        idDepartment: typeof nextFilters.idDepartment === "number" ? nextFilters.idDepartment : undefined,
        visibility: nextFilters.visibility || undefined,
        isApproved:
          nextFilters.approval === "approved"
            ? true
            : nextFilters.approval === "pending"
              ? false
              : undefined,
        startFrom: nextFilters.startFrom || undefined,
        startTo: nextFilters.startTo || undefined
      });

      setEvents(response.items);
      setPage(response.page);
      setTotalPages(response.totalPages);
      setTotalCount(response.totalCount);
      setAppliedFilters(nextFilters);
      showFormMessage(setMessage, null);
    } catch (loadError) {
      showFormMessage(setMessage, createErrorMessage(loadError, "Chargement impossible"));
    } finally {
      setIsLoading(false);
    }
  }

  function updateFilter<K extends keyof MyEventsFilters>(field: K, value: MyEventsFilters[K]) {
    setFilters((current) => ({
      ...current,
      [field]: value
    }));
  }

  async function submitFilters() {
    await loadEvents(1, filters);
  }

  async function goToPage(nextPage: number) {
    if (nextPage < 1 || (totalPages > 0 && nextPage > totalPages)) {
      return;
    }

    await loadEvents(nextPage, appliedFilters);
  }

  async function deleteEvent(idResource: number) {
    if (!token) {
      return;
    }

    setIsDeleting(true);

    try {
      await eventsService.deleteEvent(token, idResource);
      await loadEvents(page, appliedFilters);
      showFormMessage(setMessage, createSuccessMessage("L'evenement a bien ete supprime. Cette suppression ne peut pas etre annulee."));
    } catch (deleteError) {
      showFormMessage(setMessage, createErrorMessage(deleteError, "Suppression impossible"));
    } finally {
      setIsDeleting(false);
    }
  }

  return {
    filters,
    categories,
    departments,
    events,
    isLoading,
    isDeleting,
    message,
    page,
    totalPages,
    totalCount,
    updateFilter,
    submitFilters,
    goToPage,
    deleteEvent
  };
}
