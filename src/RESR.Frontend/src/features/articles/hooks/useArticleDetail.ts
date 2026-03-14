import { useEffect, useMemo, useState } from "react";
import { useAuth } from "@/features/auth/hooks/useAuth";
import { articlesService } from "@/features/articles/services/articles.service";
import { ApiError } from "@/shared/api/httpClient";
import { PermissionNames } from "@/shared/lib/auth/permissionNames";
import type { Article, Category } from "@/shared/types/article";
import type { FeedbackMessage } from "@/shared/ui/feedback/message.types";
import { createErrorMessage, createSuccessMessage, createWarningMessage, showFormMessage } from "@/shared/lib/feedback/showFormMessage";

export function useArticleDetail(idResource: number) {
  const { status, user, token, hasPermission } = useAuth();
  const [article, setArticle] = useState<Article | null>(null);
  const [categories, setCategories] = useState<Category[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [isApproving, setIsApproving] = useState(false);
  const [message, setMessage] = useState<FeedbackMessage | null>(null);
  const canApproveArticle = hasPermission(PermissionNames.approveArticle);

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

            if (canApproveArticle) {
              try {
                return await articlesService.getApprovalArticleById(token, idResource);
              } catch (approvalError) {
                if (!(approvalError instanceof ApiError) || approvalError.status !== 404) {
                  throw approvalError;
                }
              }
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
          setArticle(articleResponse);
          setCategories(categoriesResponse);
          showFormMessage(setMessage, createWarningMessage("Cet article a été supprimé. Il reste consultable mais ne peut plus être modifié."));
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
  }, [canApproveArticle, idResource, token]);

  async function approveArticle() {
    if (!token || !article || article.deletedAt || article.isApproved || !canApproveArticle) {
      return;
    }

    setIsApproving(true);

    try {
      const updatedArticle = await articlesService.setArticleApproval(token, article.idResource, true);
      setArticle(updatedArticle);
      showFormMessage(setMessage, createSuccessMessage("L'article a ete approuve et devient maintenant visible publiquement."));
    } catch (approvalError) {
      showFormMessage(setMessage, createErrorMessage(approvalError, "Approbation impossible"));
    } finally {
      setIsApproving(false);
    }
  }

  const categoryName = useMemo(
    () => categories.find((category) => category.idCategory === article?.idCategory)?.name,
    [article?.idCategory, categories]
  );

  return {
    article,
    categoryName,
    isLoading,
    isApproving,
    message,
    canEdit: status === "authenticated" && user?.idUser === article?.idUser && !article?.deletedAt,
    canApprove: status === "authenticated" && canApproveArticle && !article?.deletedAt,
    approveArticle
  };
}
