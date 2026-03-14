import { useEffect, useState } from "react";
import { eventsService } from "@/features/events/services/events.service";
import type { Category } from "@/shared/types/article";
import type { Event } from "@/shared/types/event";
import type { FeedbackMessage } from "@/shared/ui/feedback/message.types";
import { createErrorMessage, showFormMessage } from "@/shared/lib/feedback/showFormMessage";

export function useLatestEvents(limit = 3) {
  const [events, setEvents] = useState<Event[]>([]);
  const [categories, setCategories] = useState<Category[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [message, setMessage] = useState<FeedbackMessage | null>(null);

  useEffect(() => {
    let cancelled = false;

    async function load() {
      setIsLoading(true);

      try {
        const [categoriesResponse, eventsResponse] = await Promise.all([
          eventsService.getCategories(),
          eventsService.getPublicEvents({ page: 1, pageSize: limit })
        ]);

        if (cancelled) {
          return;
        }

        setCategories(categoriesResponse);
        setEvents(eventsResponse.items);
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

    void load();

    return () => {
      cancelled = true;
    };
  }, [limit]);

  return {
    events,
    categories,
    isLoading,
    message
  };
}
