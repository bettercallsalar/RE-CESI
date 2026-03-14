import { httpClient } from "@/shared/api/httpClient";
import type { FollowState, PaginatedFollowUsersResponse } from "@/features/follows/types/follows.types";
import { buildQueryString } from "@/shared/lib/http/buildQueryString";

interface FollowsQuery {
  page?: number;
  pageSize?: number;
}

export const followsService = {
  getFollowers(idUser: number, query: FollowsQuery = {}) {
    return httpClient.get<PaginatedFollowUsersResponse>(`/api/follows/${idUser}/followers${buildQueryString(query)}`);
  },
  getFollowing(idUser: number, query: FollowsQuery = {}) {
    return httpClient.get<PaginatedFollowUsersResponse>(`/api/follows/${idUser}/following${buildQueryString(query)}`);
  },
  getOwnFollowing(token: string, query: FollowsQuery = {}) {
    return httpClient.get<PaginatedFollowUsersResponse>(`/api/follows/me/following${buildQueryString(query)}`, { token });
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
