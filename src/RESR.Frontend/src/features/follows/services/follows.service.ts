import { httpClient } from "@/shared/api/httpClient";
import type { FollowState, PaginatedFollowUsersResponse } from "@/features/follows/types/follows.types";

interface FollowsQuery {
  page?: number;
  pageSize?: number;
}

function buildQuery(query: FollowsQuery = {}) {
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

export const followsService = {
  getFollowers(idUser: number, query: FollowsQuery = {}) {
    return httpClient.get<PaginatedFollowUsersResponse>(`/api/follows/${idUser}/followers${buildQuery(query)}`);
  },
  getFollowing(idUser: number, query: FollowsQuery = {}) {
    return httpClient.get<PaginatedFollowUsersResponse>(`/api/follows/${idUser}/following${buildQuery(query)}`);
  },
  getOwnFollowing(token: string, query: FollowsQuery = {}) {
    return httpClient.get<PaginatedFollowUsersResponse>(`/api/follows/me/following${buildQuery(query)}`, { token });
  },
  getOwnFollowingState(token: string, idFollowing: number) {
    return httpClient.get<FollowState>(`/api/follows/me/following/${idFollowing}`, { token });
  },
  followUser(token: string, idFollowing: number) {
    return httpClient.post<void>(`/api/follows/${idFollowing}`, undefined, { token });
  },
  unfollowUser(token: string, idFollowing: number) {
    return httpClient.delete<void>(`/api/follows/${idFollowing}`, { token });
  }
};
