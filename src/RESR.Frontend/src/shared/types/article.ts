export type ResourceVisibility = "PUBLIC" | "PRIVATE";
export type ArticleVisibility = ResourceVisibility;

export interface Category {
  idCategory: number;
  name: string;
}

export interface ResourceFile {
  idFile: number;
  fileName: string;
  originalName: string;
  mimeType: string;
  size: number;
  path: string;
  createdAt: string;
}

export interface ResourceAuthor {
  idUser: number;
  username: string;
  firstName: string;
}

export interface Resource {
  idResource: number;
  title: string;
  description: string | null;
  type: string;
  visibility: ResourceVisibility;
  createdAt: string;
  modifiedAt: string | null;
  deletedAt: string | null;
  idUser: number;
  author: ResourceAuthor;
  idCategory: number;
  isApproved: boolean;
  defaultImageId: number | null;
  files: ResourceFile[];
}

export interface Article extends Resource {
  idArticle: number;
  content: string;
}

export interface PaginatedResponse<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}
