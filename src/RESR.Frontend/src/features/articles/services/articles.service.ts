import { httpClient } from "@/shared/api/httpClient";
import type { Article, Category, PaginatedResponse } from "@/shared/types/article";
import type { CreateArticlePayload } from "@/features/articles/types/article.types";

interface ArticleQuery {
  page?: number;
  pageSize?: number;
  keyword?: string;
  idCategory?: number;
  idUser?: number;
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
  getCategories() {
    return httpClient.get<Category[]>("/api/categories");
  },
  createArticle(token: string, payload: CreateArticlePayload) {
    const formData = new FormData();
    formData.append("title", payload.title);
    if (payload.description) {
      formData.append("description", payload.description);
    }
    formData.append("visibility", payload.visibility);
    formData.append("idCategory", String(payload.idCategory));
    formData.append("content", payload.content);

    for (const image of payload.images) {
      formData.append("images", image);
    }

    return httpClient.post<{ idResource: number }>("/api/articles", formData, { token });
  }
};
