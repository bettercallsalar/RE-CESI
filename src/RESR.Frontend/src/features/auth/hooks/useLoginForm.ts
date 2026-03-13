import { useState } from "react";
import { useAuth } from "@/features/auth/hooks/useAuth";
import type { LoginCredentials } from "@/features/auth/types/auth.types";
import { getErrorMessage } from "@/shared/lib/errors/getErrorMessage";

const initialValues: LoginCredentials = {
  email: "",
  password: ""
};

export function useLoginForm() {
  const { signIn } = useAuth();
  const [values, setValues] = useState<LoginCredentials>(initialValues);
  const [error, setError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  function updateField<K extends keyof LoginCredentials>(field: K, value: LoginCredentials[K]) {
    setValues((current) => ({
      ...current,
      [field]: value
    }));
  }

  async function submit() {
    setIsSubmitting(true);
    setError(null);

    try {
      await signIn(values);
    } catch (submitError) {
      setError(getErrorMessage(submitError));
    } finally {
      setIsSubmitting(false);
    }
  }

  return {
    values,
    error,
    isSubmitting,
    updateField,
    submit
  };
}
