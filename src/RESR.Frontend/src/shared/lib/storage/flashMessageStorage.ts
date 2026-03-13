import type { FeedbackMessage } from "@/shared/ui/feedback/message.types";

const FLASH_KEY = "resr.flash.message";

export const flashMessageStorage = {
  set(value: FeedbackMessage) {
    window.sessionStorage.setItem(FLASH_KEY, JSON.stringify(value));
  },
  take(): FeedbackMessage | null {
    const raw = window.sessionStorage.getItem(FLASH_KEY);

    if (!raw) {
      return null;
    }

    window.sessionStorage.removeItem(FLASH_KEY);

    try {
      return JSON.parse(raw) as FeedbackMessage;
    } catch {
      return null;
    }
  }
};
