import type { ArticleVisibility, Category, Article } from "@/shared/types/article";
import type { FeedbackMessage } from "@/shared/ui/feedback/message.types";

export interface ArticleListFilters {
  keyword: string;
  idCategory: number | "";
}

export interface CreateArticlePayload {
  title: string;
  description: string | null;
  visibility: ArticleVisibility;
  idCategory: number;
  content: string;
  images: File[];
}

export interface CreateArticleFormValues {
  title: string;
  description: string;
  visibility: ArticleVisibility;
  idCategory: number | "";
  content: string;
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
