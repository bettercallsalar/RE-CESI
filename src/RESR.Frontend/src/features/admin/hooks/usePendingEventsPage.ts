import { useEffect, useState } from "react";
import { eventsService } from "@/features/events/services/events.service";
import { useAuth } from "@/features/auth/hooks/useAuth";
import { createErrorMessage, showFormMessage } from "@/shared/lib/feedback/showFormMessage";
import type { Event, EventPaginatedResponse } from "@/shared/types/event";
import type { FeedbackMessage } from "@/shared/ui/feedback/message.types";
import { loadAllPaginatedItems, sortByCreatedAtDesc } from "@/features/admin/lib/pendingApproval";

const pageSize = 100;

export function usePendingEventsPage() {
  const { token } = useAuth();
  const [events, setEvents] = useState<Event[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [message, setMessage] = useState<FeedbackMessage | null>(null);

  useEffect(() => {
    let cancelled = false;

    async function loadEvents() {
      if (!token) {
        if (!cancelled) {
          setEvents([]);
          setIsLoading(false);
        }
        return;
      }

      setIsLoading(true);

      try {
        const pendingEvents = await loadAllPaginatedItems<Event>((page) =>
          eventsService.getPendingEvents(token, {
            page,
            pageSize,
            isApproved: false
          }) as Promise<EventPaginatedResponse>
        );

        if (cancelled) {
          return;
        }

        setEvents(sortByCreatedAtDesc(pendingEvents));
        showFormMessage(setMessage, null);
      } catch (loadError) {
        if (!cancelled) {
          setEvents([]);
          showFormMessage(setMessage, createErrorMessage(loadError, "Chargement impossible"));
        }
      } finally {
        if (!cancelled) {
          setIsLoading(false);
        }
      }
    }

    void loadEvents();

    return () => {
      cancelled = true;
    };
  }, [token]);

  return {
    events,
    isLoading,
    message
  };
}
