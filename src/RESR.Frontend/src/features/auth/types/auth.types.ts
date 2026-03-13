import type { User } from "@/shared/types/user";

export interface LoginCredentials {
  email: string;
  password: string;
}

export interface LoginResponse {
  token: string;
}

export type AuthStatus = "loading" | "authenticated" | "unauthenticated";

export interface AuthContextValue {
  status: AuthStatus;
  token: string | null;
  user: User | null;
  signIn: (credentials: LoginCredentials) => Promise<void>;
  signOut: () => void;
  refreshCurrentUser: () => Promise<void>;
}
