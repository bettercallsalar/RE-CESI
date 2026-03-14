import { useEffect, useState } from "react";
import { useAuth } from "@/features/auth/hooks/useAuth";
import { hasMeaningfulArticleContent } from "@/features/articles/lib/articleContent";
import { articlesService } from "@/features/articles/services/articles.service";
import type { ArticleFormValues } from "@/features/articles/types/article.types";
import type { Category } from "@/shared/types/article";
import type { FeedbackMessage } from "@/shared/ui/feedback/message.types";
import { flashMessageStorage } from "@/shared/lib/storage/flashMessageStorage";
import { navigateTo } from "@/shared/lib/navigation/navigateTo";
import {
  createErrorMessage,
  createSuccessMessage,
  createWarningMessage,
  showFormMessage
} from "@/shared/lib/feedback/showFormMessage";

const initialValues: ArticleFormValues = {
  title: "",
  description: "",
  visibility: "PUBLIC",
  idCategory: "",
  content: "",
  defaultImageSelection: "",
  images: []
};

export function useCreateArticleForm() {
  const { token } = useAuth();
  const [values, setValues] = useState<ArticleFormValues>(initialValues);
  const [categories, setCategories] = useState<Category[]>([]);
  const [isLoadingCategories, setIsLoadingCategories] = useState(true);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [message, setMessage] = useState<FeedbackMessage | null>(null);

  useEffect(() => {
    let cancelled = false;

    async function loadCategories() {
      try {
        const response = await articlesService.getCategories();

        if (!cancelled) {
          setCategories(response);
        }
      } catch (loadError) {
        if (!cancelled) {
          showFormMessage(setMessage, createErrorMessage(loadError, "Chargement impossible"));
        }
      } finally {
        if (!cancelled) {
          setIsLoadingCategories(false);
        }
      }
    }

    void loadCategories();

    return () => {
      cancelled = true;
    };
  }, []);

  function updateField<K extends keyof ArticleFormValues>(field: K, value: ArticleFormValues[K]) {
    setValues((current) => {
      if (field === "images") {
        const nextImages = value as ArticleFormValues["images"];
        return {
          ...current,
          images: nextImages,
          defaultImageSelection: nextImages.length > 0 ? "new:0" : ""
        };
      }

      return {
        ...current,
        [field]: value
      };
    });
    showFormMessage(setMessage, null);
  }

  async function submit() {
    const trimmedTitle = values.title.trim();
    const trimmedDescription = values.description.trim();
    const trimmedContent = values.content.trim();

    if (!token) {
      showFormMessage(setMessage, createErrorMessage(new Error("Vous devez être connecté pour publier un article.")));
      return;
    }

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
      await articlesService.createArticle(token, {
        title: trimmedTitle,
        description: trimmedDescription || null,
        visibility: values.visibility,
        idCategory: values.idCategory,
        content: trimmedContent,
        defaultImageIndex: values.defaultImageSelection.startsWith("new:") ? Number(values.defaultImageSelection.slice(4)) : undefined,
        images: values.images
      });

      flashMessageStorage.set(
        createSuccessMessage("Votre article a bien été envoyé. Il sera visible publiquement après validation.")
      );
      navigateTo("/articles");
    } catch (submitError) {
      showFormMessage(setMessage, createErrorMessage(submitError, "Publication impossible"));
    } finally {
      setIsSubmitting(false);
    }
  }

  return {
    values,
    categories,
    isLoadingCategories,
    isSubmitting,
    message,
    updateField,
    submit
  };
}
