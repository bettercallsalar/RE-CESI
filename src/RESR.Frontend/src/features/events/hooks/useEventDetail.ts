import { useEffect, useMemo, useState } from "react";
import { useAuth } from "@/features/auth/hooks/useAuth";
import { eventsService } from "@/features/events/services/events.service";
import { ApiError } from "@/shared/api/httpClient";
import type { Category } from "@/shared/types/article";
import type { Event } from "@/shared/types/event";
import type { FeedbackMessage } from "@/shared/ui/feedback/message.types";
import { createErrorMessage, createWarningMessage, showFormMessage } from "@/shared/lib/feedback/showFormMessage";

export function useEventDetail(idResource: number) {
  const { status, user, token } = useAuth();
  const [event, setEvent] = useState<Event | null>(null);
  const [categories, setCategories] = useState<Category[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [message, setMessage] = useState<FeedbackMessage | null>(null);

  useEffect(() => {
    let cancelled = false;

    async function load() {
      setIsLoading(true);
      setEvent(null);

      try {
        const loadEvent = async () => {
          try {
            return await eventsService.getEventById(idResource);
          } catch (error) {
            if (!(error instanceof ApiError) || error.status !== 404 || !token) {
              throw error;
            }

            return eventsService.getOwnEventById(token, idResource);
          }
        };

        const [eventResponse, categoriesResponse] = await Promise.all([
          loadEvent(),
          eventsService.getCategories()
        ]);

        if (cancelled) {
          return;
        }

        setEvent(eventResponse);
        setCategories(categoriesResponse);

        if (eventResponse.deletedAt) {
          showFormMessage(setMessage, createWarningMessage("Cet evenement a ete supprime. Il reste consultable mais ne peut plus etre modifie."));
          return;
        }

        showFormMessage(setMessage, null);
      } catch (loadError) {
        if (!cancelled) {
          setEvent(null);
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
  }, [idResource, token]);

  const categoryName = useMemo(
    () => categories.find((category) => category.idCategory === event?.idCategory)?.name,
    [categories, event?.idCategory]
  );

  return {
    event,
    categoryName,
    isLoading,
    message,
    canEdit: status === "authenticated" && user?.idUser === event?.idUser && !event?.deletedAt
  };
}
