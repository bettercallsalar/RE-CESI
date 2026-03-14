import { useEffect, useState } from "react";
import { useAuth } from "@/features/auth/hooks/useAuth";
import { hasMeaningfulArticleContent } from "@/features/articles/lib/articleContent";
import { articlesService } from "@/features/articles/services/articles.service";
import type { ArticleFormValues, UpdateArticlePayload } from "@/features/articles/types/article.types";
import type { Article, Category } from "@/shared/types/article";
import type { FeedbackMessage } from "@/shared/ui/feedback/message.types";
import { flashMessageStorage } from "@/shared/lib/storage/flashMessageStorage";
import { navigateTo } from "@/shared/lib/navigation/navigateTo";
import {
  createErrorMessage,
  createSuccessMessage,
  createWarningMessage,
  showFormMessage
} from "@/shared/lib/feedback/showFormMessage";

function toFormValues(article: Article): ArticleFormValues {
  return {
    title: article.title,
    description: article.description ?? "",
    visibility: article.visibility,
    idCategory: article.idCategory,
    content: article.content,
    defaultImageSelection: article.defaultImageId ? `existing:${article.defaultImageId}` : article.files[0] ? `existing:${article.files[0].idFile}` : "",
    images: []
  };
}

export function useEditArticleForm(idResource: number) {
  const { token, user } = useAuth();
  const [article, setArticle] = useState<Article | null>(null);
  const [values, setValues] = useState<ArticleFormValues | null>(null);
  const [categories, setCategories] = useState<Category[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [isLoadingCategories, setIsLoadingCategories] = useState(true);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [message, setMessage] = useState<FeedbackMessage | null>(null);

  useEffect(() => {
    let cancelled = false;

    async function load() {
      setIsLoading(true);
      setIsLoadingCategories(true);

      try {
        if (!token) {
          throw new Error("Vous devez être connecté pour modifier un article.");
        }

        const [articleResponse, categoriesResponse] = await Promise.all([
          articlesService.getOwnArticleById(token, idResource),
          articlesService.getCategories()
        ]);

        if (cancelled) {
          return;
        }

        if (articleResponse.deletedAt) {
          setArticle(articleResponse);
          setValues(null);
          setCategories(categoriesResponse);
          showFormMessage(setMessage, createWarningMessage("Un article supprimé ne peut plus être modifié."));
          return;
        }

        setArticle(articleResponse);
        setValues(toFormValues(articleResponse));
        setCategories(categoriesResponse);
        showFormMessage(setMessage, null);
      } catch (loadError) {
        if (!cancelled) {
          showFormMessage(setMessage, createErrorMessage(loadError, "Chargement impossible"));
        }
      } finally {
        if (!cancelled) {
          setIsLoading(false);
          setIsLoadingCategories(false);
        }
      }
    }

    void load();

    return () => {
      cancelled = true;
    };
  }, [idResource, token]);

  function updateField<K extends keyof ArticleFormValues>(field: K, value: ArticleFormValues[K]) {
    setValues((current) => {
      if (!current) {
        return current;
      }

      if (field === "images") {
        const nextImages = value as ArticleFormValues["images"];
        return {
          ...current,
          images: nextImages,
          defaultImageSelection: nextImages.length > 0 ? "new:0" : article?.defaultImageId ? `existing:${article.defaultImageId}` : article?.files[0] ? `existing:${article.files[0].idFile}` : ""
        };
      }

      return { ...current, [field]: value };
    });
    showFormMessage(setMessage, null);
  }

  async function submit() {
    if (!token || !user || !article || !values) {
      showFormMessage(setMessage, createErrorMessage(new Error("Vous devez être connecté pour modifier un article.")));
      return;
    }

    if (user.idUser !== article.idUser) {
      showFormMessage(setMessage, createWarningMessage("Seul l'auteur de l'article peut le modifier."));
      return;
    }

    const trimmedTitle = values.title.trim();
    const trimmedDescription = values.description.trim();
    const trimmedContent = values.content.trim();

    if (trimmedTitle.length < 3) {
      showFormMessage(setMessage, createWarningMessage("Le titre doit contenir au moins 3 caractères."));
      return;
    }

    if (trimmedTitle.length > 50) {
      showFormMessage(setMessage, createWarningMessage("Le titre ne peut pas dépasser 50 caractères."));
      return;
    }

    if (trimmedDescription.length > 5000) {
      showFormMessage(setMessage, createWarningMessage("La description ne peut pas dépasser 5000 caractères."));
      return;
    }

    if (typeof values.idCategory !== "number" || values.idCategory <= 0) {
      showFormMessage(setMessage, createWarningMessage("Veuillez choisir une catégorie."));
      return;
    }

    if (!hasMeaningfulArticleContent(trimmedContent)) {
      showFormMessage(setMessage, createWarningMessage("Le contenu de l'article est obligatoire."));
      return;
    }

    if (values.images.length > 6) {
      showFormMessage(setMessage, createWarningMessage("Vous ne pouvez pas envoyer plus de 6 images."));
      return;
    }

    for (const image of values.images) {
      if (!image.type.startsWith("image/")) {
        showFormMessage(setMessage, createWarningMessage("Seules les images sont autorisées."));
        return;
      }

      if (image.size > 5 * 1024 * 1024) {
        showFormMessage(setMessage, createWarningMessage("Chaque image doit faire moins de 5 Mo."));
        return;
      }
    }

    setIsSubmitting(true);
    showFormMessage(setMessage, null);

    try {
      const payload: UpdateArticlePayload = {
        title: trimmedTitle,
        description: trimmedDescription || null,
        visibility: values.visibility,
        idCategory: values.idCategory,
        content: trimmedContent,
        replaceImages: values.images.length > 0,
        defaultImageId: values.images.length === 0 && values.defaultImageSelection.startsWith("existing:")
          ? Number(values.defaultImageSelection.slice(9))
          : undefined,
        defaultImageIndex: values.images.length > 0 && values.defaultImageSelection.startsWith("new:")
          ? Number(values.defaultImageSelection.slice(4))
          : undefined,
        images: values.images
      };

      await articlesService.updateArticle(token, idResource, payload);
      flashMessageStorage.set(createSuccessMessage("Votre article a bien été mis à jour."));
      navigateTo(`/articles/${idResource}`);
    } catch (submitError) {
      showFormMessage(setMessage, createErrorMessage(submitError, "Mise à jour impossible"));
    } finally {
      setIsSubmitting(false);
    }
  }

  return {
    values,
    categories,
    existingFiles: article?.files ?? [],
    canEdit: !!article && !!user && article.idUser === user.idUser && !article.deletedAt,
    isLoading,
    isLoadingCategories,
    isSubmitting,
    message,
    updateField,
    submit
  };
}
