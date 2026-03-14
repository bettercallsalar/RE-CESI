import { httpClient } from "@/shared/api/httpClient";
import type { Permission, Role } from "@/features/admin/types/admin.types";
import type { PaginatedUsersResponse } from "@/shared/types/user";

interface ManageableUsersQuery {
  page?: number;
  pageSize?: number;
}

function buildManageableUsersQuery(query: ManageableUsersQuery) {
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
  addPermissionToRole(token: string, idRole: number, idPermission: number) {
    return httpClient.post<void>(`/api/roles/${idRole}/permissions/${idPermission}`, undefined, { token });
  },
  removePermissionFromRole(token: string, idRole: number, idPermission: number) {
    return httpClient.delete<void>(`/api/roles/${idRole}/permissions/${idPermission}`, { token });
  },
  getManageableUsers(token: string, query: ManageableUsersQuery = {}) {
    return httpClient.get<PaginatedUsersResponse>(`/api/users/manageable${buildManageableUsersQuery(query)}`, { token });
  },
  setManageableUserBanStatus(token: string, idUser: number, isBanned: boolean) {
    return httpClient.patch<void>(`/api/users/manageable/${idUser}/ban`, { isBanned }, { token });
  },
  banManageableUser(token: string, idUser: number) {
    return httpClient.delete<void>(`/api/users/manageable/${idUser}/ban`, { token });
  }
};
