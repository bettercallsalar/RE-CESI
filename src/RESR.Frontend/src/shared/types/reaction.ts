export type ReactionName = "like" | "love" | "dislike";

export interface ReactionUser {
  idUser: number;
  username: string;
  firstName: string;
}

export interface ResourceReaction {
  idReaction: number;
  name: ReactionName;
  idResource: number;
  idUser: number;
  user: ReactionUser;
}

export interface CreateReactionRequest {
  name: ReactionName;
}

export interface UpdateReactionRequest {
  name: ReactionName;
}
