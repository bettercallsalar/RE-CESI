import type { ArticleVisibility, Category, Article } from "@/shared/types/article";
import type { FeedbackMessage } from "@/shared/ui/feedback/message.types";

export interface ArticleListFilters {
  keyword: string;
  idCategory: number | "";
  createdFrom: string;
  createdTo: string;
}

export interface MyArticlesFilters {
  keyword: string;
  idCategory: number | "";
  visibility: ArticleVisibility | "";
  approval: "approved" | "pending" | "";
  createdFrom: string;
  createdTo: string;
}

export interface CreateArticlePayload {
  title: string;
  description: string | null;
  visibility: ArticleVisibility;
  idCategory: number;
  content: string;
  defaultImageIndex?: number;
  images: File[];
}

export interface UpdateArticlePayload {
  title?: string;
  description?: string | null;
  visibility?: ArticleVisibility;
  idCategory?: number;
  content?: string;
  replaceImages: boolean;
  defaultImageId?: number;
  defaultImageIndex?: number;
  images: File[];
}

export interface ArticleFormValues {
  title: string;
  description: string;
  visibility: ArticleVisibility;
  idCategory: number | "";
  content: string;
  defaultImageSelection: string;
  images: File[];
}

export interface ArticlesPageState {
  filters: ArticleListFilters;
  categories: Category[];
  articles: Article[];
  totalCount: number;
  page: number;
  totalPages: number;
  isLoading: boolean;
  message: FeedbackMessage | null;
}
