import type { Event } from "@/shared/types/event";

function formatDate(value: string, options: Intl.DateTimeFormatOptions) {
  return new Intl.DateTimeFormat("fr-FR", options).format(new Date(value));
}

export function formatEventPublishedDate(value: string) {
  return formatDate(value, { dateStyle: "long" });
}

export function formatEventDateTime(value: string) {
  return formatDate(value, {
    dateStyle: "long",
    timeStyle: "short"
  });
}

export function formatEventDateRange(startDate: string, endDate?: string | null) {
  if (!endDate) {
    return formatEventDateTime(startDate);
  }

  const start = new Date(startDate);
  const end = new Date(endDate);
  const sameDay = start.toDateString() === end.toDateString();

  if (sameDay) {
    return `${formatDate(startDate, { dateStyle: "long" })} de ${formatDate(startDate, { timeStyle: "short" })} a ${formatDate(endDate, { timeStyle: "short" })}`;
  }

  return `${formatEventDateTime(startDate)} au ${formatEventDateTime(endDate)}`;
}

export function formatEventDateTimeInput(value?: string | null) {
  if (!value) {
    return "";
  }

  const date = new Date(value);
  const year = date.getFullYear();
  const month = String(date.getMonth() + 1).padStart(2, "0");
  const day = String(date.getDate()).padStart(2, "0");
  const hours = String(date.getHours()).padStart(2, "0");
  const minutes = String(date.getMinutes()).padStart(2, "0");

  return `${year}-${month}-${day}T${hours}:${minutes}`;
}

export function getPrimaryEventImage(event: { files: Event["files"] }) {
  return event.files[0] ?? null;
}

export function getEventExcerpt(event: Event) {
  const base = event.description?.trim() || event.subtitle?.trim() || event.address?.trim() || "Aucune description fournie.";

  if (base.length <= 180) {
    return base;
  }

  return `${base.slice(0, 177).trim()}...`;
}
