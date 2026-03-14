import type { Category, PaginatedResponse, ResourceAuthor, ResourceFile, ResourceVisibility } from "@/shared/types/article";
import type { Department } from "@/shared/types/user";

export interface Event {
  idResource: number;
  idEvent: number;
  title: string;
  description: string | null;
  type: string;
  visibility: ResourceVisibility;
  createdAt: string;
  modifiedAt: string | null;
  idUser: number;
  author: ResourceAuthor;
  idCategory: number;
  subtitle: string | null;
  startDate: string;
  endDate: string | null;
  address: string | null;
  department: Department | null;
  isApproved: boolean;
  defaultImageId: number | null;
  files: ResourceFile[];
  deletedAt: string | null;
}

export type EventPaginatedResponse = PaginatedResponse<Event>;
export type EventCategory = Category;
