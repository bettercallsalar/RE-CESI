import { useEffect, useState } from "react";
import { LoginPage } from "@/features/auth/pages/LoginPage";
import { useAuth } from "@/features/auth/hooks/useAuth";
import { ProfilePage } from "@/features/profile/pages/ProfilePage";
import { HomePage } from "@/pages/HomePage";
import { AppLoader } from "@/shared/ui/AppLoader";

function App() {
  const { status } = useAuth();
  const [pathname, setPathname] = useState(() => window.location.pathname);

  useEffect(() => {
    const handlePopState = () => setPathname(window.location.pathname);

    window.addEventListener("popstate", handlePopState);

    return () => {
      window.removeEventListener("popstate", handlePopState);
    };
  }, []);

  useEffect(() => {
    if (pathname === "/login" && status === "authenticated") {
      window.history.replaceState({}, "", "/");
      setPathname("/");
    }
    if (pathname === "/mon-compte" && status === "unauthenticated") {
      window.history.replaceState({}, "", "/login");
      setPathname("/login");
    }
  }, [pathname, status]);

  if (pathname === "/mon-compte") {
    if (status === "loading") {
      return <AppLoader label="Chargement de votre compte" />;
    }

    if (status === "authenticated") {
      return <ProfilePage />;
    }

    return <AppLoader label="Redirection vers la connexion" />;
  }

  if (pathname === "/login") {
    if (status === "loading") {
      return <AppLoader label="Restauration de votre session" />;
    }

    return <LoginPage />;
  }

  return <HomePage />;
}

export default App;
