import { httpClient } from "@/shared/api/httpClient";
import type { Department, User } from "@/shared/types/user";
import type { LoginCredentials, LoginResponse, RegisterAccountPayload, RegisterResponse } from "@/features/auth/types/auth.types";

export const authService = {
  signIn(credentials: LoginCredentials) {
    return httpClient.post<LoginResponse>("/api/login", credentials);
  },
  register(payload: RegisterAccountPayload) {
    return httpClient.post<RegisterResponse>("/api/users/register", payload);
  },
  getDepartments() {
    return httpClient.get<Department[]>("/api/departments");
  },
  getCurrentUser(token: string) {
    return httpClient.get<User>("/api/users/me", { token });
  }
};
