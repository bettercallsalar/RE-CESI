import { createContext, useEffect, useMemo, useState, type PropsWithChildren } from "react";
import { tokenStorage } from "@/shared/lib/storage/tokenStorage";
import type { User } from "@/shared/types/user";
import { authService } from "@/features/auth/services/auth.service";
import type { AuthContextValue, AuthStatus, LoginCredentials } from "@/features/auth/types/auth.types";

export const AuthContext = createContext<AuthContextValue | undefined>(undefined);

export function AuthProvider({ children }: PropsWithChildren) {
  const [status, setStatus] = useState<AuthStatus>("loading");
  const [token, setToken] = useState<string | null>(null);
  const [user, setUser] = useState<User | null>(null);

  useEffect(() => {
    let cancelled = false;

    async function restoreSession() {
      const storedToken = tokenStorage.get();

      if (!storedToken) {
        if (!cancelled) {
          setStatus("unauthenticated");
        }
        return;
      }

      if (!token) {
        setToken(storedToken);
      }

      try {
        const profile = await authService.getCurrentUser(storedToken);

        if (!cancelled) {
          setUser(profile);
          setStatus("authenticated");
        }
      } catch {
        tokenStorage.clear();

        if (!cancelled) {
          setToken(null);
          setUser(null);
          setStatus("unauthenticated");
        }
      }
    }

    void restoreSession();

    return () => {
      cancelled = true;
    };
  }, []);

  async function signIn(credentials: LoginCredentials) {
    const response = await authService.signIn(credentials);
    tokenStorage.set(response.token);
    setToken(response.token);

    const profile = await authService.getCurrentUser(response.token);
    setUser(profile);
    setStatus("authenticated");
  }

  function signOut() {
    tokenStorage.clear();
    setToken(null);
    setUser(null);
    setStatus("unauthenticated");
  }

  async function refreshCurrentUser() {
    if (!token) {
      setUser(null);
      setStatus("unauthenticated");
      return;
    }

    const profile = await authService.getCurrentUser(token);
    setUser(profile);
    setStatus("authenticated");
  }

  const value = useMemo<AuthContextValue>(
    () => ({
      status,
      token,
      user,
      signIn,
      signOut,
      refreshCurrentUser
    }),
    [status, token, user]
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}
