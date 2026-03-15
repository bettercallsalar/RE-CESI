export interface FollowUser {
  idUser: number;
  username: string;
  firstName: string;
}

export interface FollowState {
  idFollower: number;
  idFollowing: number;
  isFollowing: boolean;
}

export interface PaginatedFollowUsersResponse {
  items: FollowUser[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}
