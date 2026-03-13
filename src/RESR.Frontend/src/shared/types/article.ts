export type ArticleVisibility = "PUBLIC" | "PRIVATE";

export interface Category {
  idCategory: number;
  name: string;
}

export interface Article {
  idResource: number;
  idArticle: number;
  title: string;
  description: string | null;
  type: string;
  visibility: ArticleVisibility;
  createdAt: string;
  modifiedAt: string | null;
  idUser: number;
  idCategory: number;
  content: string;
  isApproved: boolean;
}

export interface PaginatedResponse<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}
