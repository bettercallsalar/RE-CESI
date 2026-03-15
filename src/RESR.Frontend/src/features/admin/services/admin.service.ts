import { httpClient } from "@/shared/api/httpClient";
import type { Permission, Role, RoleSummary } from "@/features/admin/types/admin.types";
import type { PaginatedUsersResponse } from "@/shared/types/user";

interface UsersQuery {
  page?: number;
  pageSize?: number;
  keyword?: string;
  roleIds?: number;
}

function buildUsersQuery(query: UsersQuery) {
  const params = new URLSearchParams();

  if (query.page) {
    params.set("page", String(query.page));
  }

  if (query.pageSize) {
    params.set("pageSize", String(query.pageSize));
  }

  if (query.keyword?.trim()) {
    params.set("keyword", query.keyword.trim());
  }

  if (query.roleIds) {
    params.set("roleIds", String(query.roleIds));
  }

  const raw = params.toString();
  return raw ? `?${raw}` : "";
}

export const adminService = {
  getRoles(token: string) {
    return httpClient.get<Role[]>("/api/roles", { token });
  },
  getRole(token: string, idRole: number) {
    return httpClient.get<Role>(`/api/roles/${idRole}`, { token });
  },
  getPermissions(token: string) {
    return httpClient.get<Permission[]>("/api/permissions", { token });
  },
  getAssignableRoles(token: string) {
    return httpClient.get<RoleSummary[]>("/api/roles/assignable", { token });
  },
  addPermissionToRole(token: string, idRole: number, idPermission: number) {
    return httpClient.post<void>(`/api/roles/${idRole}/permissions/${idPermission}`, undefined, { token });
  },
  removePermissionFromRole(token: string, idRole: number, idPermission: number) {
    return httpClient.delete<void>(`/api/roles/${idRole}/permissions/${idPermission}`, { token });
  },
  getUsers(token: string, query: UsersQuery = {}) {
    return httpClient.get<PaginatedUsersResponse>(`/api/users${buildUsersQuery(query)}`, { token });
  },
  updateUserRole(token: string, idUser: number, idRole: number) {
    return httpClient.patch<void>(`/api/users/${idUser}`, { idRole }, { token });
  },
  setManageableUserBanStatus(token: string, idUser: number, isBanned: boolean) {
    return httpClient.patch<void>(`/api/users/manageable/${idUser}/ban`, { isBanned }, { token });
  },
  banManageableUser(token: string, idUser: number) {
    return httpClient.delete<void>(`/api/users/manageable/${idUser}/ban`, { token });
  }
};
