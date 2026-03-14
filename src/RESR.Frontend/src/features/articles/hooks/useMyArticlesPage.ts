import { useEffect, useState } from "react";
import { useAuth } from "@/features/auth/hooks/useAuth";
import { articlesService } from "@/features/articles/services/articles.service";
import type { MyArticlesFilters } from "@/features/articles/types/article.types";
import type { Article, Category } from "@/shared/types/article";
import type { FeedbackMessage } from "@/shared/ui/feedback/message.types";
import { createErrorMessage, createSuccessMessage, showFormMessage } from "@/shared/lib/feedback/showFormMessage";

const initialFilters: MyArticlesFilters = {
  keyword: "",
  idCategory: "",
  visibility: "",
  approval: "",
  createdFrom: "",
  createdTo: ""
};

export function useMyArticlesPage() {
  const { token, user } = useAuth();
  const [filters, setFilters] = useState<MyArticlesFilters>(initialFilters);
  const [appliedFilters, setAppliedFilters] = useState<MyArticlesFilters>(initialFilters);
  const [categories, setCategories] = useState<Category[]>([]);
  const [articles, setArticles] = useState<Article[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [message, setMessage] = useState<FeedbackMessage | null>(null);
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(0);
  const [totalCount, setTotalCount] = useState(0);
  const [isDeleting, setIsDeleting] = useState(false);

  useEffect(() => {
    let cancelled = false;

    async function loadInitialData() {
      if (!token || !user) {
        return;
      }

      setIsLoading(true);

      try {
        const [categoriesResponse, articlesResponse] = await Promise.all([
          articlesService.getCategories(),
          articlesService.getOwnArticles(token, user.idUser, { page: 1, pageSize: 9 })
        ]);

        if (cancelled) {
          return;
        }

        setCategories(categoriesResponse);
        setArticles(articlesResponse.items);
        setPage(articlesResponse.page);
        setTotalPages(articlesResponse.totalPages);
        setTotalCount(articlesResponse.totalCount);
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
  }, [token, user]);

  async function loadArticles(nextPage: number, nextFilters: MyArticlesFilters) {
    if (!token || !user) {
      return;
    }

    setIsLoading(true);

    try {
      const response = await articlesService.getOwnArticles(token, user.idUser, {
        page: nextPage,
        pageSize: 9,
        keyword: nextFilters.keyword.trim() || undefined,
        idCategory: typeof nextFilters.idCategory === "number" ? nextFilters.idCategory : undefined,
        visibility: nextFilters.visibility || undefined,
        isApproved:
          nextFilters.approval === "approved"
            ? true
            : nextFilters.approval === "pending"
              ? false
              : undefined,
        createdFrom: nextFilters.createdFrom || undefined,
        createdTo: nextFilters.createdTo || undefined
      });

      setArticles(response.items);
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

  function updateFilter<K extends keyof MyArticlesFilters>(field: K, value: MyArticlesFilters[K]) {
    setFilters((current) => ({
      ...current,
      [field]: value
    }));
  }

  async function submitFilters() {
    await loadArticles(1, filters);
  }

  async function goToPage(nextPage: number) {
    if (nextPage < 1 || (totalPages > 0 && nextPage > totalPages)) {
      return;
    }

    await loadArticles(nextPage, appliedFilters);
  }

  async function deleteArticle(idResource: number) {
    if (!token) {
      return;
    }

    setIsDeleting(true);

    try {
      await articlesService.deleteArticle(token, idResource);
      await loadArticles(page, appliedFilters);
      showFormMessage(setMessage, createSuccessMessage("L'article a bien été supprimé. Cette suppression ne peut pas être annulée."));
    } catch (deleteError) {
      showFormMessage(setMessage, createErrorMessage(deleteError, "Suppression impossible"));
    } finally {
      setIsDeleting(false);
    }
  }

  return {
    filters,
    categories,
    articles,
    isLoading,
    isDeleting,
    message,
    page,
    totalPages,
    totalCount,
    updateFilter,
    submitFilters,
    goToPage,
    deleteArticle
  };
}
