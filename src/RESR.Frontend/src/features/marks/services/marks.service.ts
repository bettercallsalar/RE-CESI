import { httpClient } from "@/shared/api/httpClient";
import type { Mark, PaginatedMarksResponse } from "@/features/marks/types/marks.types";

interface MarksQuery {
  page?: number;
  pageSize?: number;
}

function buildQuery(query: MarksQuery) {
  const params = new URLSearchParams();

  if (query.page) {
    params.set("page", String(query.page));
  }

  if (query.pageSize) {
    params.set("pageSize", String(query.pageSize));
  }

  const raw = params.toString();
  return raw ? `?${raw}` : "";
}

export const marksService = {
  getReadLaterMarks(token: string, query: MarksQuery = {}) {
    return httpClient.get<PaginatedMarksResponse>(`/api/marks/readLater${buildQuery(query)}`, { token });
  },

  getReadLaterMark(token: string, idResource: number) {
    return httpClient.get<Mark>(`/api/marks/readLater/${idResource}`, { token });
  },

  markAsReadLater(token: string, idResource: number) {
    return httpClient.post<Mark>(`/api/marks/readLater/${idResource}`, undefined, { token });
  },

  unmarkAsReadLater(token: string, idResource: number) {
    return httpClient.delete<void>(`/api/marks/readLater/${idResource}`, { token });
  }
};
