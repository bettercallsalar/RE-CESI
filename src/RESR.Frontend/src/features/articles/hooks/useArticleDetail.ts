import { useEffect, useMemo, useState } from "react";
import { useAuth } from "@/features/auth/hooks/useAuth";
import { articlesService } from "@/features/articles/services/articles.service";
import { ApiError } from "@/shared/api/httpClient";
import type { Article, Category } from "@/shared/types/article";
import type { FeedbackMessage } from "@/shared/ui/feedback/message.types";
import { createErrorMessage, createWarningMessage, showFormMessage } from "@/shared/lib/feedback/showFormMessage";

export function useArticleDetail(idResource: number) {
  const { status, user, token } = useAuth();
  const [article, setArticle] = useState<Article | null>(null);
  const [categories, setCategories] = useState<Category[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [message, setMessage] = useState<FeedbackMessage | null>(null);

  useEffect(() => {
    let cancelled = false;

    async function load() {
      setIsLoading(true);
      setArticle(null);

      try {
        const loadArticle = async () => {
          try {
            return await articlesService.getArticleById(idResource);
          } catch (error) {
            if (!(error instanceof ApiError) || error.status !== 404 || !token) {
              throw error;
            }

            return articlesService.getOwnArticleById(token, idResource);
          }
        };

        const [articleResponse, categoriesResponse] = await Promise.all([
          loadArticle(),
          articlesService.getCategories()
        ]);

        if (cancelled) {
          return;
        }

        if (articleResponse.deletedAt) {
          setArticle(null);
          setCategories(categoriesResponse);
          showFormMessage(setMessage, createWarningMessage("Cet article a été supprimé et n'est plus accessible."));
          return;
        }

        setArticle(articleResponse);
        setCategories(categoriesResponse);
        showFormMessage(setMessage, null);
      } catch (loadError) {
        if (!cancelled) {
          setArticle(null);
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
    () => categories.find((category) => category.idCategory === article?.idCategory)?.name,
    [article?.idCategory, categories]
  );

  return {
    article,
    categoryName,
    isLoading,
    message,
    canEdit: status === "authenticated" && user?.idUser === article?.idUser
  };
}
