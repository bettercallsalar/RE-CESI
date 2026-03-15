import type { Category, PaginatedResponse, Resource, ResourceVisibility } from "@/shared/types/article";
import type { Department } from "@/shared/types/user";

export interface Event extends Resource {
  idEvent: number;
  visibility: ResourceVisibility;
  subtitle: string | null;
  startDate: string;
  endDate: string | null;
  address: string | null;
  department: Department | null;
}

export type EventPaginatedResponse = PaginatedResponse<Event>;
export type EventCategory = Category;
