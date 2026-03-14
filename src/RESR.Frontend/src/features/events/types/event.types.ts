import type { Category, ResourceVisibility } from "@/shared/types/article";
import type { Event } from "@/shared/types/event";
import type { Department } from "@/shared/types/user";
import type { FeedbackMessage } from "@/shared/ui/feedback/message.types";

export interface EventListFilters {
  keyword: string;
  idCategory: number | "";
  idDepartment: number | "";
  startFrom: string;
  startTo: string;
}

export interface MyEventsFilters extends EventListFilters {
  visibility: ResourceVisibility | "";
  approval: "approved" | "pending" | "";
}

export interface CreateEventPayload {
  title: string;
  description: string | null;
  visibility: ResourceVisibility;
  idCategory: number;
  subtitle: string | null;
  startDate: string;
  endDate: string | null;
  address: string | null;
  idDepartment: number | null;
  images: File[];
}

export interface UpdateEventPayload {
  title?: string;
  description?: string | null;
  visibility?: ResourceVisibility;
  idCategory?: number;
  subtitle?: string | null;
  startDate?: string;
  endDate?: string | null;
  address?: string | null;
  idDepartment?: number | null;
  replaceImages: boolean;
  images: File[];
}

export interface EventFormValues {
  title: string;
  description: string;
  subtitle: string;
  visibility: ResourceVisibility;
  idCategory: number | "";
  startDate: string;
  endDate: string;
  address: string;
  idDepartment: number | "";
  images: File[];
}

export interface EventsPageState {
  filters: EventListFilters;
  categories: Category[];
  departments: Department[];
  events: Event[];
  totalCount: number;
  page: number;
  totalPages: number;
  isLoading: boolean;
  message: FeedbackMessage | null;
}
