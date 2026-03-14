import type { ResourceAuthor } from "@/shared/types/article";

export interface ResourceComment {
  idComment: number;
  content: string;
  createdAt: string;
  modifiedAt: string | null;
  deletedAt: string | null;
  idResource: number;
  idUser: number;
  author: ResourceAuthor;
  idParentComment: number | null;
}

export interface CreateResourceCommentRequest {
  content: string;
  idParentComment?: number | null;
}
