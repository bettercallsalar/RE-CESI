import { httpClient } from "@/shared/api/httpClient";
import { buildQueryString } from "@/shared/lib/http/buildQueryString";
import type { CreateEventPayload, UpdateEventPayload } from "@/features/events/types/event.types";
import type { Category } from "@/shared/types/article";
import type { Event, EventPaginatedResponse } from "@/shared/types/event";
import type { Department } from "@/shared/types/user";

interface EventQuery {
  page?: number;
  pageSize?: number;
  keyword?: string;
  idCategory?: number;
  idDepartment?: number;
  idUser?: number;
  visibility?: "PUBLIC" | "PRIVATE";
  isApproved?: boolean;
  startFrom?: string;
  startTo?: string;
}

export const eventsService = {
  getPublicEvents(query: EventQuery = {}) {
    return httpClient.get<EventPaginatedResponse>(`/api/events${buildQueryString(query)}`);
  },
  getPendingEvents(token: string, query: EventQuery = {}) {
    return httpClient.get<EventPaginatedResponse>(`/api/events/approval/pending${buildQueryString(query)}`, { token });
  },
  getEventById(idResource: number) {
    return httpClient.get<Event>(`/api/events/${idResource}`);
  },
  getApprovalEventById(token: string, idResource: number) {
    return httpClient.get<Event>(`/api/events/approval/${idResource}`, { token });
  },
  getOwnEventById(token: string, idResource: number) {
    return httpClient.get<Event>(`/api/events/me/${idResource}`, { token });
  },
  getOwnEvents(token: string, idUser: number, query: EventQuery = {}) {
    return httpClient.get<EventPaginatedResponse>(`/api/events/${idUser}/my-events${buildQueryString(query)}`, { token });
  },
  getCategories() {
    return httpClient.get<Category[]>("/api/categories");
  },
  getDepartments() {
    return httpClient.get<Department[]>("/api/departments");
  },
  createEvent(token: string, payload: CreateEventPayload) {
    return httpClient.post<{ idResource: number }>("/api/events", toEventFormData(payload), { token });
  },
  updateEvent(token: string, idResource: number, payload: UpdateEventPayload) {
    return httpClient.patch<Event>(`/api/events/${idResource}`, toEventFormData(payload), { token });
  },
  setEventApproval(token: string, idResource: number, isApproved: boolean) {
    return httpClient.patch<Event>(`/api/events/${idResource}/approval`, { isApproved }, { token });
  },
  deleteEvent(token: string, idResource: number) {
    return httpClient.delete<void>(`/api/events/${idResource}`, { token });
  }
};

function toEventFormData(payload: CreateEventPayload | UpdateEventPayload) {
  const formData = new FormData();

  if (payload.title !== undefined) {
    formData.append("title", payload.title);
  }

  if (payload.description !== undefined) {
    formData.append("description", payload.description ?? "");
  }

  if (payload.visibility !== undefined) {
    formData.append("visibility", payload.visibility);
  }

  if (payload.idCategory !== undefined) {
    formData.append("idCategory", String(payload.idCategory));
  }

  if (payload.subtitle !== undefined) {
    formData.append("subtitle", payload.subtitle ?? "");
  }

  if (payload.startDate !== undefined) {
    formData.append("startDate", payload.startDate);
  }

  if (payload.endDate !== undefined) {
    formData.append("endDate", payload.endDate ?? "");
  }

  if (payload.address !== undefined) {
    formData.append("address", payload.address ?? "");
  }

  if (payload.idDepartment !== undefined) {
    formData.append("idDepartment", payload.idDepartment === null ? "" : String(payload.idDepartment));
  }

  if ("defaultImageIndex" in payload && typeof payload.defaultImageIndex === "number") {
    formData.append("defaultImageIndex", String(payload.defaultImageIndex));
  }

  if ("defaultImageId" in payload && typeof payload.defaultImageId === "number") {
    formData.append("defaultImageId", String(payload.defaultImageId));
  }

  if ("replaceImages" in payload) {
    formData.append("replaceImages", String(payload.replaceImages));
  }

  for (const image of payload.images) {
    formData.append("images", image);
  }

  return formData;
}
