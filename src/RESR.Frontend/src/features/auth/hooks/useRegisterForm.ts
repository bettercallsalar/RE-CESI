import { useEffect, useState } from "react";
import { authService } from "@/features/auth/services/auth.service";
import type { RegisterAccountFormValues } from "@/features/auth/types/auth.types";
import { navigateTo } from "@/shared/lib/navigation/navigateTo";
import { flashMessageStorage } from "@/shared/lib/storage/flashMessageStorage";
import type { Department } from "@/shared/types/user";
import type { FeedbackMessage } from "@/shared/ui/feedback/message.types";
import {
  createErrorMessage,
  createSuccessMessage,
  createWarningMessage,
  showFormMessage
} from "@/shared/lib/feedback/showFormMessage";

const initialValues: RegisterAccountFormValues = {
  username: "",
  email: "",
  password: "",
  confirmPassword: "",
  firstName: "",
  birthDate: "",
  bio: "",
  idDepartment: ""
};

export function useRegisterForm() {
  const [values, setValues] = useState<RegisterAccountFormValues>(initialValues);
  const [departments, setDepartments] = useState<Department[]>([]);
  const [message, setMessage] = useState<FeedbackMessage | null>(null);
  const [isLoadingDepartments, setIsLoadingDepartments] = useState(true);
  const [isSubmitting, setIsSubmitting] = useState(false);

  useEffect(() => {
    let cancelled = false;

    async function loadDepartments() {
      try {
        const items = await authService.getDepartments();

        if (!cancelled) {
          setDepartments(items);
        }
      } catch (loadError) {
        if (!cancelled) {
          showFormMessage(setMessage, createErrorMessage(loadError, "Chargement impossible"));
        }
      } finally {
        if (!cancelled) {
          setIsLoadingDepartments(false);
        }
      }
    }

    void loadDepartments();

    return () => {
      cancelled = true;
    };
  }, []);

  function updateField<K extends keyof RegisterAccountFormValues>(field: K, value: RegisterAccountFormValues[K]) {
    showFormMessage(setMessage, null);
    setValues((current) => ({
      ...current,
      [field]: value
    }));
  }

  async function submit() {
    const trimmedUsername = values.username.trim();
    const trimmedEmail = values.email.trim();
    const trimmedFirstName = values.firstName.trim();
    const trimmedBio = values.bio.trim();

    if (!trimmedUsername || !trimmedFirstName || !trimmedEmail || !values.password) {
      showFormMessage(setMessage, createWarningMessage("Le pseudo, le prenom, l'adresse e-mail et le mot de passe sont obligatoires."));
      return;
    }

    if (typeof values.idDepartment !== "number" || values.idDepartment <= 0) {
      showFormMessage(setMessage, createWarningMessage("Veuillez choisir un departement."));
      return;
    }

    if (values.password !== values.confirmPassword) {
      showFormMessage(setMessage, createWarningMessage("La confirmation du mot de passe ne correspond pas."));
      return;
    }

    setIsSubmitting(true);
    showFormMessage(setMessage, null);

    try {
      await authService.register({
        username: trimmedUsername,
        email: trimmedEmail,
        password: values.password,
        firstName: trimmedFirstName,
        birthDate: values.birthDate || null,
        bio: trimmedBio || null,
        idDepartment: values.idDepartment
      });

      flashMessageStorage.set(
        createSuccessMessage("Compte cree. Vous pouvez maintenant vous connecter.")
      );
      navigateTo("/login", { replace: true });
    } catch (submitError) {
      showFormMessage(setMessage, createErrorMessage(submitError, "Inscription impossible"));
    } finally {
      setIsSubmitting(false);
    }
  }

  return {
    values,
    departments,
    message,
    isLoadingDepartments,
    isSubmitting,
    updateField,
    submit
  };
}
