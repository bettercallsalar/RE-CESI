import { useEffect, useMemo, useState } from "react";
import { useAuth } from "@/features/auth/hooks/useAuth";
import { profileService } from "@/features/profile/services/profile.service";
import type { ProfileFormValues, UpdateOwnProfilePayload } from "@/features/profile/types/profile.types";
import type { Department } from "@/shared/types/user";
import { flashMessageStorage } from "@/shared/lib/storage/flashMessageStorage";
import type { FeedbackMessage } from "@/shared/ui/feedback/message.types";
import {
  createErrorMessage,
  createInfoMessage,
  createSuccessMessage,
  createWarningMessage,
  showFormMessage
} from "@/shared/lib/feedback/showFormMessage";

const emailPattern = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
const usernamePattern = /^[a-zA-Z0-9_]+$/;
const firstNamePattern = /^[a-zA-ZÀ-ÿ'\\ -]+$/;

function toFormValues(user: {
  username: string;
  email: string;
  firstName: string;
  birthDate: string | null;
  bio: string | null;
  department: Department;
}): ProfileFormValues {
  return {
    username: user.username,
    email: user.email,
    firstName: user.firstName,
    birthDate: user.birthDate ?? "",
    bio: user.bio ?? "",
    idDepartment: user.department.idDepartment
  };
}

export function useProfileForm() {
  const { token, user, setCurrentUser, signOut } = useAuth();
  const [departments, setDepartments] = useState<Department[]>([]);
  const [values, setValues] = useState<ProfileFormValues | null>(user ? toFormValues(user) : null);
  const [isLoadingDepartments, setIsLoadingDepartments] = useState(true);
  const [isSaving, setIsSaving] = useState(false);
  const [isDeleting, setIsDeleting] = useState(false);
  const [formMessage, setFormMessage] = useState<FeedbackMessage | null>(null);
  const [deleteMessage, setDeleteMessage] = useState<FeedbackMessage | null>(null);

  useEffect(() => {
    if (user) {
      setValues(toFormValues(user));
    }
  }, [user]);

  useEffect(() => {
    let cancelled = false;

    async function loadDepartments() {
      setIsLoadingDepartments(true);

      try {
        const items = await profileService.getDepartments();

        if (!cancelled) {
          setDepartments(items);
        }
      } catch (loadError) {
        if (!cancelled) {
          showFormMessage(setFormMessage, createErrorMessage(loadError, "Chargement impossible"));
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

  const hasChanges = useMemo(() => {
    if (!user || !values) {
      return false;
    }

    const initial = toFormValues(user);

    return (
      initial.username !== values.username ||
      initial.email !== values.email ||
      initial.firstName !== values.firstName ||
      initial.birthDate !== values.birthDate ||
      initial.bio !== values.bio ||
      initial.idDepartment !== values.idDepartment
    );
  }, [user, values]);

  function updateField<K extends keyof ProfileFormValues>(field: K, value: ProfileFormValues[K]) {
    showFormMessage(setFormMessage, null);
    setValues((current) =>
      current
        ? {
            ...current,
            [field]: value
          }
        : current
    );
  }

  async function save() {
    if (!token || !user || !values) {
      return;
    }

    if (!hasChanges) {
      showFormMessage(setFormMessage, createInfoMessage("Aucune modification à enregistrer."));
      return;
    }

    setIsSaving(true);
    showFormMessage(setFormMessage, null);

    const trimmedEmail = values.email.trim();
    const trimmedUsername = values.username.trim();
    const trimmedFirstName = values.firstName.trim();
    const trimmedBio = values.bio.trim();

    if (!trimmedFirstName || trimmedFirstName.length < 2 || !firstNamePattern.test(trimmedFirstName)) {
      setIsSaving(false);
      showFormMessage(setFormMessage, createWarningMessage("Le prénom doit contenir au moins 2 caractères valides."));
      return;
    }

    if (!trimmedUsername || trimmedUsername.length < 3 || !usernamePattern.test(trimmedUsername)) {
      setIsSaving(false);
      showFormMessage(
        setFormMessage,
        createWarningMessage("Le nom d'utilisateur doit contenir au moins 3 caractères et uniquement des lettres, chiffres ou underscore.")
      );
      return;
    }

    if (!trimmedEmail || !emailPattern.test(trimmedEmail)) {
      setIsSaving(false);
      showFormMessage(setFormMessage, createWarningMessage("Veuillez saisir une adresse e-mail valide."));
      return;
    }

    if (trimmedBio.length > 500) {
      setIsSaving(false);
      showFormMessage(setFormMessage, createWarningMessage("La biographie ne peut pas dépasser 500 caractères."));
      return;
    }

    const payload: UpdateOwnProfilePayload = {};

    if (values.username !== user.username) {
      payload.username = trimmedUsername;
    }

    if (values.email !== user.email) {
      payload.email = trimmedEmail;
    }

    if (values.firstName !== user.firstName) {
      payload.firstName = trimmedFirstName;
    }

    if ((values.birthDate || null) !== user.birthDate) {
      payload.birthDate = values.birthDate || null;
    }

    if (values.bio !== (user.bio ?? "")) {
      payload.bio = trimmedBio ? trimmedBio : "";
    }

    if (values.idDepartment !== user.department.idDepartment && values.idDepartment !== "") {
      payload.idDepartment = values.idDepartment;
    }

    try {
      const updatedUser = await profileService.updateOwnProfile(token, payload);
      setCurrentUser(updatedUser);
      setValues(toFormValues(updatedUser));
      showFormMessage(setFormMessage, createSuccessMessage("Vos informations ont bien été mises à jour."));
    } catch (saveError) {
      showFormMessage(setFormMessage, createErrorMessage(saveError));
    } finally {
      setIsSaving(false);
    }
  }

  async function deleteAccount() {
    if (!token) {
      return;
    }

    showFormMessage(setDeleteMessage, null);
    setIsDeleting(true);

    try {
      await profileService.deleteOwnProfile(token);
      flashMessageStorage.set(createSuccessMessage("Votre compte a été supprimé définitivement."));
      signOut();
      window.history.replaceState({}, "", "/");
      window.dispatchEvent(new PopStateEvent("popstate"));
    } catch (deleteAccountError) {
      showFormMessage(setDeleteMessage, createErrorMessage(deleteAccountError));
    } finally {
      setIsDeleting(false);
    }
  }

  return {
    user,
    values,
    departments,
    isLoadingDepartments,
    isSaving,
    isDeleting,
    hasChanges,
    formMessage,
    deleteMessage,
    updateField,
    save,
    deleteAccount
  };
}
