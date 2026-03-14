import { httpClient } from "@/shared/api/httpClient";
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

function buildQuery(query: EventQuery) {
  const params = new URLSearchParams();

  if (query.page) {
    params.set("page", String(query.page));
  }

  if (query.pageSize) {
    params.set("pageSize", String(query.pageSize));
  }

  if (query.keyword) {
    params.set("keyword", query.keyword);
  }

  if (query.idCategory) {
    params.set("idCategory", String(query.idCategory));
  }

  if (query.idDepartment) {
    params.set("idDepartment", String(query.idDepartment));
  }

  if (query.idUser) {
    params.set("idUser", String(query.idUser));
  }

  if (query.visibility) {
    params.set("visibility", query.visibility);
  }

  if (query.isApproved !== undefined) {
    params.set("isApproved", String(query.isApproved));
  }

  if (query.startFrom) {
    params.set("startFrom", query.startFrom);
  }

  if (query.startTo) {
    params.set("startTo", query.startTo);
  }

  const raw = params.toString();
  return raw ? `?${raw}` : "";
}

export const eventsService = {
  getPublicEvents(query: EventQuery = {}) {
    return httpClient.get<EventPaginatedResponse>(`/api/events${buildQuery(query)}`);
  },
  getEventById(idResource: number) {
    return httpClient.get<Event>(`/api/events/${idResource}`);
  },
  getOwnEventById(token: string, idResource: number) {
    return httpClient.get<Event>(`/api/events/me/${idResource}`, { token });
  },
  getOwnEvents(token: string, idUser: number, query: EventQuery = {}) {
    return httpClient.get<EventPaginatedResponse>(`/api/events/${idUser}/my-events${buildQuery(query)}`, { token });
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

  if ("replaceImages" in payload) {
    formData.append("replaceImages", String(payload.replaceImages));
  }

  for (const image of payload.images) {
    formData.append("images", image);
  }

  return formData;
}
