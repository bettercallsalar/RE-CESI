import { createContext, useEffect, useMemo, useState, type PropsWithChildren } from "react";
import { tokenStorage } from "@/shared/lib/storage/tokenStorage";
import type { User } from "@/shared/types/user";
import { authService } from "@/features/auth/services/auth.service";
import type { AuthContextValue, AuthStatus, LoginCredentials } from "@/features/auth/types/auth.types";
import { getAuthTokenClaims, isSuperAdminToken } from "@/shared/lib/auth/tokenClaims";
import { adminDashboardPermissions } from "@/shared/lib/auth/permissionNames";

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
          applyCurrentUserProfile(profile, storedToken);
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
    applyCurrentUserProfile(profile, response.token);
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
    applyCurrentUserProfile(profile, token);
  }

  function setCurrentUser(nextUser: User) {
    setUser(nextUser);
    setStatus("authenticated");
  }

  function applyCurrentUserProfile(profile: User, activeToken: string) {
    if (profile.isBanned) {
      tokenStorage.clear();
      setToken(null);
      setUser(null);
      setStatus("unauthenticated");
      return;
    }

    setToken(activeToken);
    setUser(profile);
    setStatus("authenticated");
  }

  const { permissions, roleId } = getAuthTokenClaims(token);
  const isSuperAdmin = isSuperAdminToken(token);
  const normalizedPermissions = [...new Set(permissions)];
  const permissionSet = new Set(normalizedPermissions.map((permission) => permission.toLowerCase()));
  const hasPermission = (permission: string) => permissionSet.has(permission.toLowerCase());
  const canAccessAdminDashboard = isSuperAdmin || adminDashboardPermissions.some((permission) => hasPermission(permission));

  const value = useMemo<AuthContextValue>(
    () => ({
      status,
      token,
      user,
      roleId,
      isSuperAdmin,
      permissions: normalizedPermissions,
      canAccessAdminDashboard,
      hasPermission,
      signIn,
      signOut,
      refreshCurrentUser,
      setCurrentUser
    }),
    [canAccessAdminDashboard, isSuperAdmin, normalizedPermissions, roleId, status, token, user]
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}
