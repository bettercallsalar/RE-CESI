import { httpClient } from "@/shared/api/httpClient";
import type { CreateResourceCommentRequest, ResourceComment } from "@/shared/types/comment";

class CommentsService {
  async getCommentsByResource(idResource: number) {
    return httpClient.get<ResourceComment[]>(`/api/comments/resources/${idResource}`);
  }

  async createComment(token: string, idResource: number, request: CreateResourceCommentRequest) {
    return httpClient.post<ResourceComment>(
      `/api/comments/resources/${idResource}`,
      {
        content: request.content,
        idParentComment: request.idParentComment ?? null
      },
      { token }
    );
  }

  async deleteComment(token: string, idComment: number) {
    return httpClient.delete<void>(`/api/comments/${idComment}`, { token });
  }

  async deleteCommentForModeration(token: string, idComment: number) {
    return httpClient.delete<void>(`/api/comments/moderation/${idComment}`, { token });
  }
}

export const commentsService = new CommentsService();
