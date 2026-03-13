import { useEffect, useState } from "react";
import { LoginPage } from "@/features/auth/pages/LoginPage";
import { useAuth } from "@/features/auth/hooks/useAuth";
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
  }, [pathname, status]);

  if (pathname === "/login") {
    if (status === "loading") {
      return <AppLoader label="Restoring your session" />;
    }

    return <LoginPage />;
  }

  return <HomePage />;
}

export default App;
