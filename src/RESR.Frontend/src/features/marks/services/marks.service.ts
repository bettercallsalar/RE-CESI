import { httpClient } from "@/shared/api/httpClient";
import type { Mark, PaginatedMarksResponse } from "@/features/marks/types/marks.types";
import { buildQueryString } from "@/shared/lib/http/buildQueryString";

interface MarksQuery {
  page?: number;
  pageSize?: number;
}

export const marksService = {
  getReadLaterMarks(token: string, query: MarksQuery = {}) {
    return httpClient.get<PaginatedMarksResponse>(`/api/marks/readLater${buildQueryString(query)}`, { token });
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
