import { httpClient } from "@/shared/api/httpClient";
import type { Permission, Role } from "@/features/admin/types/admin.types";

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
  }
};
