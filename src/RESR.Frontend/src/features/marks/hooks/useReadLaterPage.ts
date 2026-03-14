import { useEffect, useState } from "react";
import { articlesService } from "@/features/articles/services/articles.service";
import { useAuth } from "@/features/auth/hooks/useAuth";
import { eventsService } from "@/features/events/services/events.service";
import { marksService } from "@/features/marks/services/marks.service";
import type { Mark, PaginatedMarksResponse } from "@/features/marks/types/marks.types";
import { ApiError } from "@/shared/api/httpClient";
import type { Article, Category } from "@/shared/types/article";
import type { Event } from "@/shared/types/event";
import { createErrorMessage, showFormMessage } from "@/shared/lib/feedback/showFormMessage";
import type { FeedbackMessage } from "@/shared/ui/feedback/message.types";

const API_PAGE_SIZE = 100;

type MarkedResourceResult =
  | { kind: "article"; item: Article }
  | { kind: "event"; item: Event }
  | null;

async function collectAllPages(loadPage: (page: number) => Promise<PaginatedMarksResponse>) {
  const firstPage = await loadPage(1);
  const items = [...firstPage.items];

  for (let page = 2; page <= firstPage.totalPages; page += 1) {
    const nextPage = await loadPage(page);
    items.push(...nextPage.items);
  }

  return items;
}

async function loadMarkedResource(idResource: number): Promise<MarkedResourceResult> {
  try {
    return {
      kind: "article",
      item: await articlesService.getArticleById(idResource)
    };
  } catch (articleError) {
    if (!(articleError instanceof ApiError) || articleError.status !== 404) {
      throw articleError;
    }
  }

  try {
    return {
      kind: "event",
      item: await eventsService.getEventById(idResource)
    };
  } catch (eventError) {
    if (eventError instanceof ApiError && eventError.status === 404) {
      return null;
    }

    throw eventError;
  }
}

function mapMarkedResources(marks: Mark[], results: MarkedResourceResult[]) {
  const articles: Article[] = [];
  const events: Event[] = [];
  const seenArticles = new Set<number>();
  const seenEvents = new Set<number>();

  marks.forEach((mark, index) => {
    const result = results[index];

    if (!result) {
      return;
    }

    if (result.kind === "article") {
      if (seenArticles.has(result.item.idResource)) {
        return;
      }

      seenArticles.add(result.item.idResource);
      articles.push(result.item);
      return;
    }

    if (seenEvents.has(result.item.idResource)) {
      return;
    }

    seenEvents.add(result.item.idResource);
    events.push(result.item);
  });

  return { articles, events };
}

export function useReadLaterPage() {
  const { token, user } = useAuth();
  const [categories, setCategories] = useState<Category[]>([]);
  const [articles, setArticles] = useState<Article[]>([]);
  const [events, setEvents] = useState<Event[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [message, setMessage] = useState<FeedbackMessage | null>(null);

  useEffect(() => {
    let cancelled = false;

    async function load() {
      if (!token || !user) {
        setCategories([]);
        setArticles([]);
        setEvents([]);
        setIsLoading(false);
        return;
      }

      setIsLoading(true);

      try {
        const [categoriesResponse, marksResponse] = await Promise.all([
          articlesService.getCategories().catch(() => []),
          collectAllPages((page) => marksService.getReadLaterMarks(token, { page, pageSize: API_PAGE_SIZE }))
        ]);

        if (cancelled) {
          return;
        }

        setCategories(categoriesResponse);

        if (marksResponse.length === 0) {
          setArticles([]);
          setEvents([]);
          showFormMessage(setMessage, null);
          return;
        }

        const resourceResults = await Promise.all(
          marksResponse.map((mark) => loadMarkedResource(mark.idRessource))
        );

        if (cancelled) {
          return;
        }

        const mappedResources = mapMarkedResources(marksResponse, resourceResults);
        setArticles(mappedResources.articles);
        setEvents(mappedResources.events);
        showFormMessage(setMessage, null);
      } catch (loadError) {
        if (!cancelled) {
          setCategories([]);
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

    void load();

    return () => {
      cancelled = true;
    };
  }, [token, user]);

  return {
    categories,
    articles,
    events,
    isLoading,
    message
  };
}
