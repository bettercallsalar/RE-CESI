import type { User } from "@/shared/types/user";

export interface LoginCredentials {
  email: string;
  password: string;
}

export interface RegisterAccountPayload {
  username: string;
  email: string;
  password: string;
  firstName: string;
  birthDate: string | null;
  bio: string | null;
  idDepartment: number;
}

export interface RegisterAccountFormValues {
  username: string;
  email: string;
  password: string;
  confirmPassword: string;
  firstName: string;
  birthDate: string;
  bio: string;
  idDepartment: number | "";
}

export interface LoginResponse {
  token: string;
}

export interface RegisterResponse {
  message: string;
  userId: number;
}

export type AuthStatus = "loading" | "authenticated" | "unauthenticated";

export interface AuthContextValue {
  status: AuthStatus;
  token: string | null;
  user: User | null;
  roleId: number | null;
  isSuperAdmin: boolean;
  permissions: string[];
  canAccessAdminDashboard: boolean;
  hasPermission: (permission: string) => boolean;
  signIn: (credentials: LoginCredentials) => Promise<void>;
  signOut: () => void;
  refreshCurrentUser: () => Promise<void>;
  setCurrentUser: (user: User) => void;
}
