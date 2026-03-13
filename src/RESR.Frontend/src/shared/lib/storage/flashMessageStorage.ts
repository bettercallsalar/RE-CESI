export interface FlashMessage {
  type: "success" | "error";
  message: string;
}

const FLASH_KEY = "resr.flash.message";

export const flashMessageStorage = {
  set(value: FlashMessage) {
    window.sessionStorage.setItem(FLASH_KEY, JSON.stringify(value));
  },
  take(): FlashMessage | null {
    const raw = window.sessionStorage.getItem(FLASH_KEY);

    if (!raw) {
      return null;
    }

    window.sessionStorage.removeItem(FLASH_KEY);

    try {
      return JSON.parse(raw) as FlashMessage;
    } catch {
      return null;
    }
  }
};
