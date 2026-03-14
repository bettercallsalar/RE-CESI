import { useEffect, useState } from "react";
import { eventsService } from "@/features/events/services/events.service";
import type { EventListFilters } from "@/features/events/types/event.types";
import type { Category } from "@/shared/types/article";
import type { Event } from "@/shared/types/event";
import type { Department } from "@/shared/types/user";
import type { FeedbackMessage } from "@/shared/ui/feedback/message.types";
import { createErrorMessage, showFormMessage } from "@/shared/lib/feedback/showFormMessage";

const initialFilters: EventListFilters = {
  keyword: "",
  idCategory: "",
  idDepartment: "",
  startFrom: "",
  startTo: ""
};

export function useEventsPage() {
  const [filters, setFilters] = useState<EventListFilters>(initialFilters);
  const [appliedFilters, setAppliedFilters] = useState<EventListFilters>(initialFilters);
  const [categories, setCategories] = useState<Category[]>([]);
  const [departments, setDepartments] = useState<Department[]>([]);
  const [events, setEvents] = useState<Event[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [message, setMessage] = useState<FeedbackMessage | null>(null);
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(0);
  const [totalCount, setTotalCount] = useState(0);

  useEffect(() => {
    let cancelled = false;

    async function loadInitialData() {
      setIsLoading(true);

      try {
        const [categoriesResponse, departmentsResponse, eventsResponse] = await Promise.all([
          eventsService.getCategories(),
          eventsService.getDepartments(),
          eventsService.getPublicEvents({ page: 1, pageSize: 9 })
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
  }, []);

  async function loadEvents(nextPage: number, nextFilters: EventListFilters) {
    setIsLoading(true);

    try {
      const response = await eventsService.getPublicEvents({
        page: nextPage,
        pageSize: 9,
        keyword: nextFilters.keyword.trim() || undefined,
        idCategory: typeof nextFilters.idCategory === "number" ? nextFilters.idCategory : undefined,
        idDepartment: typeof nextFilters.idDepartment === "number" ? nextFilters.idDepartment : undefined,
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

  function updateFilter<K extends keyof EventListFilters>(field: K, value: EventListFilters[K]) {
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

  return {
    filters,
    categories,
    departments,
    events,
    isLoading,
    message,
    page,
    totalPages,
    totalCount,
    updateFilter,
    submitFilters,
    goToPage
  };
}
