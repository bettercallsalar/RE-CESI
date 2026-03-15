import { httpClient } from "@/shared/api/httpClient";
import { buildQueryString } from "@/shared/lib/http/buildQueryString";
import type { Article, Category, PaginatedResponse } from "@/shared/types/article";
import type { CreateArticlePayload, UpdateArticlePayload } from "@/features/articles/types/article.types";

interface ArticleQuery {
  page?: number;
  pageSize?: number;
  keyword?: string;
  idCategory?: number;
  idUser?: number;
  visibility?: "PUBLIC" | "PRIVATE";
  isApproved?: boolean;
  createdFrom?: string;
  createdTo?: string;
}

export const articlesService = {
  getPublicArticles(query: ArticleQuery = {}) {
    return httpClient.get<PaginatedResponse<Article>>(`/api/articles${buildQueryString(query)}`);
  },
  getPendingArticles(token: string, query: ArticleQuery = {}) {
    return httpClient.get<PaginatedResponse<Article>>(`/api/articles/approval/pending${buildQueryString(query)}`, { token });
  },
  getArticleById(idResource: number) {
    return httpClient.get<Article>(`/api/articles/${idResource}`);
  },
  getApprovalArticleById(token: string, idResource: number) {
    return httpClient.get<Article>(`/api/articles/approval/${idResource}`, { token });
  },
  getOwnArticleById(token: string, idResource: number) {
    return httpClient.get<Article>(`/api/articles/me/${idResource}`, { token });
  },
  getOwnArticles(token: string, idUser: number, query: ArticleQuery = {}) {
    return httpClient.get<PaginatedResponse<Article>>(`/api/articles/${idUser}/my-articles${buildQueryString(query)}`, { token });
  },
  getCategories() {
    return httpClient.get<Category[]>("/api/categories");
  },
  createArticle(token: string, payload: CreateArticlePayload) {
    return httpClient.post<{ idResource: number }>("/api/articles", toArticleFormData(payload), { token });
  },
  updateArticle(token: string, idResource: number, payload: UpdateArticlePayload) {
    return httpClient.patch<Article>(`/api/articles/${idResource}`, toArticleFormData(payload), { token });
  },
  setArticleApproval(token: string, idResource: number, isApproved: boolean) {
    return httpClient.patch<Article>(`/api/articles/${idResource}/approval`, { isApproved }, { token });
  },
  deleteArticle(token: string, idResource: number) {
    return httpClient.delete<void>(`/api/articles/${idResource}`, { token });
  }
};

function toArticleFormData(payload: CreateArticlePayload | UpdateArticlePayload) {
  const formData = new FormData();

  if (payload.title !== undefined) {
    formData.append("title", payload.title);
  }

  if (payload.description) {
    formData.append("description", payload.description);
  }

  if (payload.visibility !== undefined) {
    formData.append("visibility", payload.visibility);
  }

  if (payload.idCategory !== undefined) {
    formData.append("idCategory", String(payload.idCategory));
  }

  if (payload.content !== undefined) {
    formData.append("content", payload.content);
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
