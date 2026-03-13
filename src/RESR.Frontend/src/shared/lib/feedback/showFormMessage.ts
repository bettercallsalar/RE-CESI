import { getErrorMessage } from "@/shared/lib/errors/getErrorMessage";
import { getApiMessageTone } from "@/shared/lib/feedback/getApiMessageTone";
import type { FeedbackMessage, MessageTone } from "@/shared/ui/feedback/message.types";

type MessageSetter = (message: FeedbackMessage | null) => void;

export function showFormMessage(setter: MessageSetter, message: FeedbackMessage | null) {
  setter(message);
}

export function createFeedbackMessage(
  tone: MessageTone,
  message: string,
  title?: string
): FeedbackMessage {
  return {
    tone,
    title,
    message
  };
}

export function createSuccessMessage(message: string, title = "Succès"): FeedbackMessage {
  return createFeedbackMessage("success", message, title);
}

export function createWarningMessage(message: string, title = "Attention"): FeedbackMessage {
  return createFeedbackMessage("warning", message, title);
}

export function createInfoMessage(message: string, title = "Information"): FeedbackMessage {
  return createFeedbackMessage("info", message, title);
}

export function createErrorMessage(error: unknown, title = "Erreur"): FeedbackMessage {
  return createFeedbackMessage(getApiMessageTone(error), getErrorMessage(error), title);
}
