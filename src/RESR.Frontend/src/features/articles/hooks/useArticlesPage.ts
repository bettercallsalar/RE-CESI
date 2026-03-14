import { useEffect, useState } from "react";
import { articlesService } from "@/features/articles/services/articles.service";
import type { ArticleListFilters } from "@/features/articles/types/article.types";
import type { Article, Category } from "@/shared/types/article";
import type { FeedbackMessage } from "@/shared/ui/feedback/message.types";
import { createErrorMessage, showFormMessage } from "@/shared/lib/feedback/showFormMessage";

const initialFilters: ArticleListFilters = {
  keyword: "",
  idCategory: ""
};

export function useArticlesPage() {
  const [filters, setFilters] = useState<ArticleListFilters>(initialFilters);
  const [appliedFilters, setAppliedFilters] = useState<ArticleListFilters>(initialFilters);
  const [categories, setCategories] = useState<Category[]>([]);
  const [articles, setArticles] = useState<Article[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [message, setMessage] = useState<FeedbackMessage | null>(null);
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(0);
  const [totalCount, setTotalCount] = useState(0);

  useEffect(() => {
    let cancelled = false;

    async function loadInitialData() {
      setIsLoading(true);

      try {
        const [categoriesResponse, articlesResponse] = await Promise.all([
          articlesService.getCategories(),
          articlesService.getPublicArticles({ page: 1, pageSize: 9 })
        ]);

        if (cancelled) {
          return;
        }

        setCategories(categoriesResponse);
        setArticles(articlesResponse.items);
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
  }, []);

  async function loadArticles(nextPage: number, nextFilters: ArticleListFilters) {
    setIsLoading(true);

    try {
      const response = await articlesService.getPublicArticles({
        page: nextPage,
        pageSize: 9,
        keyword: nextFilters.keyword.trim() || undefined,
        idCategory: typeof nextFilters.idCategory === "number" ? nextFilters.idCategory : undefined
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

  function updateFilter<K extends keyof ArticleListFilters>(field: K, value: ArticleListFilters[K]) {
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

  return {
    filters,
    categories,
    articles,
    isLoading,
    message,
    page,
    totalPages,
    totalCount,
    updateFilter,
    submitFilters,
    goToPage
  };
}
