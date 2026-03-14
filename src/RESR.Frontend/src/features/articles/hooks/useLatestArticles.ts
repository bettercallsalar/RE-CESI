import { useEffect, useState } from "react";
import { articlesService } from "@/features/articles/services/articles.service";
import type { Article, Category } from "@/shared/types/article";
import type { FeedbackMessage } from "@/shared/ui/feedback/message.types";
import { createErrorMessage, showFormMessage } from "@/shared/lib/feedback/showFormMessage";

export function useLatestArticles(limit = 3) {
  const [articles, setArticles] = useState<Article[]>([]);
  const [categories, setCategories] = useState<Category[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [message, setMessage] = useState<FeedbackMessage | null>(null);

  useEffect(() => {
    let cancelled = false;

    async function load() {
      setIsLoading(true);

      try {
        const [categoriesResponse, articlesResponse] = await Promise.all([
          articlesService.getCategories(),
          articlesService.getPublicArticles({ page: 1, pageSize: limit })
        ]);

        if (cancelled) {
          return;
        }

        setCategories(categoriesResponse);
        setArticles(articlesResponse.items);
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
    articles,
    categories,
    isLoading,
    message
  };
}
