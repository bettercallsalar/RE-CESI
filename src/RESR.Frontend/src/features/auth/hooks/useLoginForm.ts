import { useState } from "react";
import { useAuth } from "@/features/auth/hooks/useAuth";
import type { LoginCredentials } from "@/features/auth/types/auth.types";
import { flashMessageStorage } from "@/shared/lib/storage/flashMessageStorage";
import type { FeedbackMessage } from "@/shared/ui/feedback/message.types";
import {
  createErrorMessage,
  createSuccessMessage,
  showFormMessage
} from "@/shared/lib/feedback/showFormMessage";

const initialValues: LoginCredentials = {
  email: "",
  password: ""
};

export function useLoginForm() {
  const { signIn } = useAuth();
  const [values, setValues] = useState<LoginCredentials>(initialValues);
  const [message, setMessage] = useState<FeedbackMessage | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  function updateField<K extends keyof LoginCredentials>(field: K, value: LoginCredentials[K]) {
    showFormMessage(setMessage, null);
    setValues((current) => ({
      ...current,
      [field]: value
    }));
  }

  async function submit() {
    setIsSubmitting(true);
    showFormMessage(setMessage, null);

    try {
      await signIn(values);
      flashMessageStorage.set(createSuccessMessage("Connexion réussie."));
    } catch (submitError) {
      showFormMessage(setMessage, createErrorMessage(submitError, "Erreur de connexion"));
    } finally {
      setIsSubmitting(false);
    }
  }

  return {
    values,
    message,
    isSubmitting,
    updateField,
    submit
  };
}
