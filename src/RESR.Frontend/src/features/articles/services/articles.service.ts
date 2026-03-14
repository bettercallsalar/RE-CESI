import { httpClient } from "@/shared/api/httpClient";
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

function buildQuery(query: ArticleQuery) {
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

  if (query.idUser) {
    params.set("idUser", String(query.idUser));
  }

  if (query.visibility) {
    params.set("visibility", query.visibility);
  }

  if (query.isApproved !== undefined) {
    params.set("isApproved", String(query.isApproved));
  }

  if (query.createdFrom) {
    params.set("createdFrom", query.createdFrom);
  }

  if (query.createdTo) {
    params.set("createdTo", query.createdTo);
  }

  const raw = params.toString();
  return raw ? `?${raw}` : "";
}

export const articlesService = {
  getPublicArticles(query: ArticleQuery = {}) {
    return httpClient.get<PaginatedResponse<Article>>(`/api/articles${buildQuery(query)}`);
  },
  getArticleById(idResource: number) {
    return httpClient.get<Article>(`/api/articles/${idResource}`);
  },
  getOwnArticleById(token: string, idResource: number) {
    return httpClient.get<Article>(`/api/articles/me/${idResource}`, { token });
  },
  getOwnArticles(token: string, idUser: number, query: ArticleQuery = {}) {
    return httpClient.get<PaginatedResponse<Article>>(`/api/articles/${idUser}/my-articles${buildQuery(query)}`, { token });
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
