import { useEffect, useState } from "react";
import { articlesService } from "@/features/articles/services/articles.service";
import { useAuth } from "@/features/auth/hooks/useAuth";
import { createErrorMessage, showFormMessage } from "@/shared/lib/feedback/showFormMessage";
import type { Article, PaginatedResponse } from "@/shared/types/article";
import type { FeedbackMessage } from "@/shared/ui/feedback/message.types";
import { loadAllPaginatedItems, sortByCreatedAtDesc } from "@/features/admin/lib/pendingApproval";

const pageSize = 100;

export function usePendingArticlesPage() {
  const { token } = useAuth();
  const [articles, setArticles] = useState<Article[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [message, setMessage] = useState<FeedbackMessage | null>(null);

  useEffect(() => {
    let cancelled = false;

    async function loadArticles() {
      if (!token) {
        if (!cancelled) {
          setArticles([]);
          setIsLoading(false);
        }
        return;
      }

      setIsLoading(true);

      try {
        const pendingArticles = await loadAllPaginatedItems<Article>((page) =>
          articlesService.getPendingArticles(token, {
            page,
            pageSize,
            isApproved: false
          }) as Promise<PaginatedResponse<Article>>
        );

        if (cancelled) {
          return;
        }

        setArticles(sortByCreatedAtDesc(pendingArticles));
        showFormMessage(setMessage, null);
      } catch (loadError) {
        if (!cancelled) {
          setArticles([]);
          showFormMessage(setMessage, createErrorMessage(loadError, "Chargement impossible"));
        }
      } finally {
        if (!cancelled) {
          setIsLoading(false);
        }
      }
    }

    void loadArticles();

    return () => {
      cancelled = true;
    };
  }, [token]);

  return {
    articles,
    isLoading,
    message
  };
}
