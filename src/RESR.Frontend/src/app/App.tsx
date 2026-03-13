import { LoginPage } from "@/features/auth/pages/LoginPage";
import { useAuth } from "@/features/auth/hooks/useAuth";
import { HomePage } from "@/pages/HomePage";
import { AppLoader } from "@/shared/ui/AppLoader";

function App() {
  const { status } = useAuth();

  if (status === "loading") {
    return <AppLoader label="Restoring your session" />;
  }

  if (status === "authenticated") {
    return <HomePage />;
  }

  return <LoginPage />;
}

export default App;
