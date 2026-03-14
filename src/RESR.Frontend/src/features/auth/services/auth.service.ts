import { httpClient } from "@/shared/api/httpClient";
import type { User } from "@/shared/types/user";
import type { LoginCredentials, LoginResponse } from "@/features/auth/types/auth.types";

export const authService = {
  signIn(credentials: LoginCredentials) {
    return httpClient.post<LoginResponse>("/api/login", credentials);
  },
  getCurrentUser(token: string) {
    return httpClient.get<User>("/api/users/me", { token });
  }
};
