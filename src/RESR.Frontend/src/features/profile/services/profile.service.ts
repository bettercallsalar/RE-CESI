import { httpClient } from "@/shared/api/httpClient";
import type { Department, User } from "@/shared/types/user";
import type { UpdateOwnProfilePayload } from "@/features/profile/types/profile.types";

export const profileService = {
  getDepartments() {
    return httpClient.get<Department[]>("/api/departments");
  },
  updateOwnProfile(token: string, payload: UpdateOwnProfilePayload) {
    return httpClient.patch<User>("/api/users/modify-profile", payload, { token });
  },
  deleteOwnProfile(token: string) {
    return httpClient.delete<void>("/api/users/me", { token });
  }
};
