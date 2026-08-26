import { httpClient } from "@/shared/api/httpClient";
import type { CreateReactionRequest, ResourceReaction, UpdateReactionRequest } from "@/shared/types/reaction";

export const reactionsService = {
  getByResource(idResource: number) {
    return httpClient.get<ResourceReaction[]>(`/api/reactions/resources/${idResource}`);
  },

  create(token: string, idResource: number, request: CreateReactionRequest) {
    return httpClient.post<ResourceReaction>(`/api/reactions/resources/${idResource}`, request, { token });
  },

  update(token: string, idReaction: number, request: UpdateReactionRequest) {
    return httpClient.patch<ResourceReaction>(`/api/reactions/${idReaction}`, request, { token });
  },

  delete(token: string, idReaction: number) {
    return httpClient.delete<void>(`/api/reactions/${idReaction}`, { token });
  }
};
