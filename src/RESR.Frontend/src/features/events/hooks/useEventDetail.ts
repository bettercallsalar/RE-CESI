import { useEffect, useMemo, useState } from "react";
import { useAuth } from "@/features/auth/hooks/useAuth";
import { eventsService } from "@/features/events/services/events.service";
import { ApiError } from "@/shared/api/httpClient";
import { PermissionNames } from "@/shared/lib/auth/permissionNames";
import type { Category } from "@/shared/types/article";
import type { Event } from "@/shared/types/event";
import type { FeedbackMessage } from "@/shared/ui/feedback/message.types";
import { createErrorMessage, createSuccessMessage, createWarningMessage, showFormMessage } from "@/shared/lib/feedback/showFormMessage";

export function useEventDetail(idResource: number) {
  const { status, user, token, hasPermission } = useAuth();
  const [event, setEvent] = useState<Event | null>(null);
  const [categories, setCategories] = useState<Category[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [isUpdatingApproval, setIsUpdatingApproval] = useState(false);
  const [message, setMessage] = useState<FeedbackMessage | null>(null);
  const canApproveEvent = hasPermission(PermissionNames.approveEvent);

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

            if (canApproveEvent) {
              try {
                return await eventsService.getApprovalEventById(token, idResource);
              } catch (approvalError) {
                if (!(approvalError instanceof ApiError) || approvalError.status !== 404) {
                  throw approvalError;
                }
              }
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
  }, [canApproveEvent, idResource, token]);

  async function setEventApproval(nextIsApproved: boolean) {
    if (!token || !event || event.deletedAt || event.isApproved === nextIsApproved || !canApproveEvent) {
      return false;
    }

    setIsUpdatingApproval(true);

    try {
      const updatedEvent = await eventsService.setEventApproval(token, event.idResource, nextIsApproved);
      setEvent(updatedEvent);

      showFormMessage(
        setMessage,
        createSuccessMessage(
          nextIsApproved
            ? "L'evenement a ete approuve et devient maintenant visible publiquement."
            : "L'evenement a ete desapprouve et n'apparait plus parmi les events publics."
        )
      );

      return true;
    } catch (approvalError) {
      showFormMessage(setMessage, createErrorMessage(approvalError, "Approbation impossible"));
      return false;
    } finally {
      setIsUpdatingApproval(false);
    }
  }

  const categoryName = useMemo(
    () => categories.find((category) => category.idCategory === event?.idCategory)?.name,
    [categories, event?.idCategory]
  );

  return {
    event,
    categoryName,
    isLoading,
    isUpdatingApproval,
    message,
    canEdit: status === "authenticated" && user?.idUser === event?.idUser && !event?.deletedAt,
    canApprove: status === "authenticated" && canApproveEvent && !event?.deletedAt,
    approveEvent: () => setEventApproval(true),
    unapproveEvent: () => setEventApproval(false)
  };
}
