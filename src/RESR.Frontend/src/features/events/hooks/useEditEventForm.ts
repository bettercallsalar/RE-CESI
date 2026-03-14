import { useEffect, useState } from "react";
import { useAuth } from "@/features/auth/hooks/useAuth";
import { formatEventDateTimeInput } from "@/features/events/lib/eventDates";
import { eventsService } from "@/features/events/services/events.service";
import type { EventFormValues, UpdateEventPayload } from "@/features/events/types/event.types";
import type { Category, ResourceFile } from "@/shared/types/article";
import type { Event } from "@/shared/types/event";
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

function toFormValues(event: Event): EventFormValues {
  return {
    title: event.title,
    description: event.description ?? "",
    subtitle: event.subtitle ?? "",
    visibility: event.visibility,
    idCategory: event.idCategory,
    startDate: formatEventDateTimeInput(event.startDate),
    endDate: formatEventDateTimeInput(event.endDate),
    address: event.address ?? "",
    idDepartment: event.department?.idDepartment ?? "",
    defaultImageSelection: event.defaultImageId ? `existing:${event.defaultImageId}` : event.files[0] ? `existing:${event.files[0].idFile}` : "",
    images: []
  };
}

export function useEditEventForm(idResource: number) {
  const { token, user } = useAuth();
  const [event, setEvent] = useState<Event | null>(null);
  const [values, setValues] = useState<EventFormValues | null>(null);
  const [categories, setCategories] = useState<Category[]>([]);
  const [departments, setDepartments] = useState<Department[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [isLoadingCategories, setIsLoadingCategories] = useState(true);
  const [isLoadingDepartments, setIsLoadingDepartments] = useState(true);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [message, setMessage] = useState<FeedbackMessage | null>(null);

  useEffect(() => {
    let cancelled = false;

    async function load() {
      setIsLoading(true);
      setIsLoadingCategories(true);
      setIsLoadingDepartments(true);

      try {
        if (!token) {
          throw new Error("Vous devez etre connecte pour modifier un evenement.");
        }

        const [eventResponse, categoriesResponse, departmentsResponse] = await Promise.all([
          eventsService.getOwnEventById(token, idResource),
          eventsService.getCategories(),
          eventsService.getDepartments()
        ]);

        if (cancelled) {
          return;
        }

        setEvent(eventResponse);
        setValues(toFormValues(eventResponse));
        setCategories(categoriesResponse);
        setDepartments(departmentsResponse);

        if (eventResponse.deletedAt) {
          showFormMessage(setMessage, createWarningMessage("Un evenement supprime ne peut plus etre modifie."));
          return;
        }

        showFormMessage(setMessage, null);
      } catch (loadError) {
        if (!cancelled) {
          showFormMessage(setMessage, createErrorMessage(loadError, "Chargement impossible"));
        }
      } finally {
        if (!cancelled) {
          setIsLoading(false);
          setIsLoadingCategories(false);
          setIsLoadingDepartments(false);
        }
      }
    }

    void load();

    return () => {
      cancelled = true;
    };
  }, [idResource, token]);

  function updateField<K extends keyof EventFormValues>(field: K, value: EventFormValues[K]) {
    setValues((current) => {
      if (!current) {
        return current;
      }

      if (field === "images") {
        const nextImages = value as EventFormValues["images"];
        return {
          ...current,
          images: nextImages,
          defaultImageSelection: nextImages.length > 0 ? "new:0" : event?.defaultImageId ? `existing:${event.defaultImageId}` : event?.files[0] ? `existing:${event.files[0].idFile}` : ""
        };
      }

      return { ...current, [field]: value };
    });
    showFormMessage(setMessage, null);
  }

  async function submit() {
    if (!token || !user || !event || !values) {
      showFormMessage(setMessage, createErrorMessage(new Error("Vous devez etre connecte pour modifier un evenement.")));
      return;
    }

    if (user.idUser !== event.idUser) {
      showFormMessage(setMessage, createWarningMessage("Seul l'auteur de l'evenement peut le modifier."));
      return;
    }

    if (event.deletedAt) {
      showFormMessage(setMessage, createWarningMessage("Un evenement supprime ne peut plus etre modifie."));
      return;
    }

    const trimmedTitle = values.title.trim();
    const trimmedDescription = values.description.trim();
    const trimmedSubtitle = values.subtitle.trim();
    const trimmedAddress = values.address.trim();

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
      const payload: UpdateEventPayload = {
        title: trimmedTitle,
        description: trimmedDescription || null,
        visibility: values.visibility,
        idCategory: values.idCategory,
        subtitle: trimmedSubtitle || null,
        startDate: values.startDate,
        endDate: values.endDate || null,
        address: trimmedAddress || null,
        idDepartment: typeof values.idDepartment === "number" ? values.idDepartment : null,
        replaceImages: values.images.length > 0,
        defaultImageId: values.images.length === 0 && values.defaultImageSelection.startsWith("existing:")
          ? Number(values.defaultImageSelection.slice(9))
          : undefined,
        defaultImageIndex: values.images.length > 0 && values.defaultImageSelection.startsWith("new:")
          ? Number(values.defaultImageSelection.slice(4))
          : undefined,
        images: values.images
      };

      await eventsService.updateEvent(token, idResource, payload);
      flashMessageStorage.set(createSuccessMessage("Votre evenement a bien ete mis a jour."));
      navigateTo(`/events/${idResource}`);
    } catch (submitError) {
      showFormMessage(setMessage, createErrorMessage(submitError, "Mise a jour impossible"));
    } finally {
      setIsSubmitting(false);
    }
  }

  return {
    values,
    categories,
    departments,
    existingFiles: event?.files ?? ([] as ResourceFile[]),
    canEdit: !!event && !!user && event.idUser === user.idUser && !event.deletedAt,
    isLoading,
    isLoadingCategories,
    isLoadingDepartments,
    isSubmitting,
    message,
    updateField,
    submit
  };
}
