export type MessageTone = "success" | "error" | "warning" | "info";

export interface FeedbackMessage {
  tone: MessageTone;
  title?: string;
  message: string;
}
