import { ApiError } from "@/shared/api/httpClient";
import type { MessageTone } from "@/shared/ui/feedback/message.types";

export function getApiMessageTone(error: unknown): MessageTone {
  if (error instanceof ApiError) {
    if (error.status === 400 || error.status === 409) {
      return "warning";
    }

    if (error.status >= 500) {
      return "error";
    }

    if (error.status === 404) {
      return "info";
    }
  }

  return "error";
}
