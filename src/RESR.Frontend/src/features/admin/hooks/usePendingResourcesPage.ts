import { useEffect, useState } from "react";
import { articlesService } from "@/features/articles/services/articles.service";
import { eventsService } from "@/features/events/services/events.service";
import { useAuth } from "@/features/auth/hooks/useAuth";
import { PermissionNames } from "@/shared/lib/auth/permissionNames";
import { createErrorMessage, showFormMessage } from "@/shared/lib/feedback/showFormMessage";
import type { Article, PaginatedResponse } from "@/shared/types/article";
import type { Event, EventPaginatedResponse } from "@/shared/types/event";
import type { FeedbackMessage } from "@/shared/ui/feedback/message.types";

const pageSize = 100;

interface PaginatedItems<T> {
  items: T[];
  totalPages: number;
}

async function loadAllPages<T>(fetchPage: (page: number) => Promise<PaginatedItems<T>>) {
  const firstPage = await fetchPage(1);
  const items = [...firstPage.items];

  if (firstPage.totalPages <= 1) {
    return items;
  }

  const remainingPages = await Promise.all(
    Array.from({ length: firstPage.totalPages - 1 }, (_, index) => fetchPage(index + 2))
  );

  for (const page of remainingPages) {
    items.push(...page.items);
  }

  return items;
}

export function usePendingResourcesPage() {
  const { token, hasPermission } = useAuth();
  const [articles, setArticles] = useState<Article[]>([]);
  const [events, setEvents] = useState<Event[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [message, setMessage] = useState<FeedbackMessage | null>(null);

  const canApproveArticles = hasPermission(PermissionNames.approveArticle);
  const canApproveEvents = hasPermission(PermissionNames.approveEvent);

  useEffect(() => {
    let cancelled = false;

    async function loadPendingResources() {
      if (!token) {
        if (!cancelled) {
          setArticles([]);
          setEvents([]);
          setIsLoading(false);
        }
        return;
      }

      setIsLoading(true);

      try {
        const [pendingArticles, pendingEvents] = await Promise.all([
          canApproveArticles
            ? loadAllPages<Article>((page) =>
                articlesService.getPendingArticles(token, {
                  page,
                  pageSize,
                  isApproved: false
                }) as Promise<PaginatedResponse<Article>>
              )
            : Promise.resolve([]),
          canApproveEvents
            ? loadAllPages<Event>((page) =>
                eventsService.getPendingEvents(token, {
                  page,
                  pageSize,
                  isApproved: false
                }) as Promise<EventPaginatedResponse>
              )
            : Promise.resolve([])
        ]);

        if (cancelled) {
          return;
        }

        setArticles([...pendingArticles].sort((left, right) => Date.parse(right.createdAt) - Date.parse(left.createdAt)));
        setEvents([...pendingEvents].sort((left, right) => Date.parse(right.createdAt) - Date.parse(left.createdAt)));
        showFormMessage(setMessage, null);
      } catch (loadError) {
        if (!cancelled) {
          setArticles([]);
          setEvents([]);
          showFormMessage(setMessage, createErrorMessage(loadError, "Chargement impossible"));
        }
      } finally {
        if (!cancelled) {
          setIsLoading(false);
        }
      }
    }

    void loadPendingResources();

    return () => {
      cancelled = true;
    };
  }, [canApproveArticles, canApproveEvents, token]);

  return {
    articles,
    events,
    isLoading,
    message,
    canApproveArticles,
    canApproveEvents
  };
}
