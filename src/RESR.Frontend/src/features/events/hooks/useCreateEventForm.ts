import { useEffect, useState } from "react";
import { useAuth } from "@/features/auth/hooks/useAuth";
import { eventsService } from "@/features/events/services/events.service";
import type { EventFormValues } from "@/features/events/types/event.types";
import type { Category } from "@/shared/types/article";
import type { Department } from "@/shared/types/user";
import type { FeedbackMessage } from "@/shared/ui/feedback/message.types";
import { flashMessageStorage } from "@/shared/lib/storage/flashMessageStorage";
import { navigateTo } from "@/shared/lib/navigation/navigateTo";
import {
  createErrorMessage,
  createSuccessMessage,
  createWarningMessage,
  showFormMessage
} from "@/shared/lib/feedback/showFormMessage";

const initialValues: EventFormValues = {
  title: "",
  description: "",
  subtitle: "",
  visibility: "PUBLIC",
  idCategory: "",
  startDate: "",
  endDate: "",
  address: "",
  idDepartment: "",
  defaultImageSelection: "",
  images: []
};

export function useCreateEventForm() {
  const { token } = useAuth();
  const [values, setValues] = useState<EventFormValues>(initialValues);
  const [categories, setCategories] = useState<Category[]>([]);
  const [departments, setDepartments] = useState<Department[]>([]);
  const [isLoadingCategories, setIsLoadingCategories] = useState(true);
  const [isLoadingDepartments, setIsLoadingDepartments] = useState(true);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [message, setMessage] = useState<FeedbackMessage | null>(null);

  useEffect(() => {
    let cancelled = false;

    async function loadOptions() {
      try {
        const [categoriesResponse, departmentsResponse] = await Promise.all([
          eventsService.getCategories(),
          eventsService.getDepartments()
        ]);

        if (!cancelled) {
          setCategories(categoriesResponse);
          setDepartments(departmentsResponse);
        }
      } catch (loadError) {
        if (!cancelled) {
          showFormMessage(setMessage, createErrorMessage(loadError, "Chargement impossible"));
        }
      } finally {
        if (!cancelled) {
          setIsLoadingCategories(false);
          setIsLoadingDepartments(false);
        }
      }
    }

    void loadOptions();

    return () => {
      cancelled = true;
    };
  }, []);

  function updateField<K extends keyof EventFormValues>(field: K, value: EventFormValues[K]) {
    setValues((current) => {
      if (field === "images") {
        const nextImages = value as EventFormValues["images"];
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
    const trimmedSubtitle = values.subtitle.trim();
    const trimmedAddress = values.address.trim();

    if (!token) {
      showFormMessage(setMessage, createErrorMessage(new Error("Vous devez etre connecte pour creer un evenement.")));
      return;
    }

    if (trimmedTitle.length < 3) {
      showFormMessage(setMessage, createWarningMessage("Le titre doit contenir au moins 3 caracteres."));
      return;
    }

    if (trimmedTitle.length > 50) {
      showFormMessage(setMessage, createWarningMessage("Le titre ne peut pas depasser 50 caracteres."));
      return;
    }

    if (trimmedDescription.length > 5000) {
      showFormMessage(setMessage, createWarningMessage("La description ne peut pas depasser 5000 caracteres."));
      return;
    }

    if (trimmedSubtitle.length > 255) {
      showFormMessage(setMessage, createWarningMessage("Le sous-titre ne peut pas depasser 255 caracteres."));
      return;
    }

    if (trimmedAddress.length > 255) {
      showFormMessage(setMessage, createWarningMessage("L'adresse ne peut pas depasser 255 caracteres."));
      return;
    }

    if (typeof values.idCategory !== "number" || values.idCategory <= 0) {
      showFormMessage(setMessage, createWarningMessage("Veuillez choisir une categorie."));
      return;
    }

    if (!values.startDate) {
      showFormMessage(setMessage, createWarningMessage("La date de debut est obligatoire."));
      return;
    }

    if (values.endDate && new Date(values.endDate) <= new Date(values.startDate)) {
      showFormMessage(setMessage, createWarningMessage("La date de fin doit etre strictement apres la date de debut."));
      return;
    }

    if (values.images.length > 6) {
      showFormMessage(setMessage, createWarningMessage("Vous ne pouvez pas envoyer plus de 6 images."));
      return;
    }

    for (const image of values.images) {
      if (!image.type.startsWith("image/")) {
        showFormMessage(setMessage, createWarningMessage("Seules les images sont autorisees."));
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
      await eventsService.createEvent(token, {
        title: trimmedTitle,
        description: trimmedDescription || null,
        visibility: values.visibility,
        idCategory: values.idCategory,
        subtitle: trimmedSubtitle || null,
        startDate: values.startDate,
        endDate: values.endDate || null,
        address: trimmedAddress || null,
        idDepartment: typeof values.idDepartment === "number" ? values.idDepartment : null,
        defaultImageIndex: values.defaultImageSelection.startsWith("new:") ? Number(values.defaultImageSelection.slice(4)) : undefined,
        images: values.images
      });

      flashMessageStorage.set(createSuccessMessage("Votre evenement a bien ete envoye. Il sera visible publiquement apres validation."));
      navigateTo("/events");
    } catch (submitError) {
      showFormMessage(setMessage, createErrorMessage(submitError, "Creation impossible"));
    } finally {
      setIsSubmitting(false);
    }
  }

  return {
    values,
    categories,
    departments,
    isLoadingCategories,
    isLoadingDepartments,
    isSubmitting,
    message,
    updateField,
    submit
  };
}
