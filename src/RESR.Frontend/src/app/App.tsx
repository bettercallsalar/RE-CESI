import { useEffect, useState } from "react";
import { ArticleDetailPage } from "@/features/articles/pages/ArticleDetailPage";
import { ArticlesPage } from "@/features/articles/pages/ArticlesPage";
import { CreateArticlePage } from "@/features/articles/pages/CreateArticlePage";
import { EditArticlePage } from "@/features/articles/pages/EditArticlePage";
import { MyArticlesPage } from "@/features/articles/pages/MyArticlesPage";
import { CreateEventPage } from "@/features/events/pages/CreateEventPage";
import { EditEventPage } from "@/features/events/pages/EditEventPage";
import { EventDetailPage } from "@/features/events/pages/EventDetailPage";
import { EventsPage } from "@/features/events/pages/EventsPage";
import { MyEventsPage } from "@/features/events/pages/MyEventsPage";
import { useAuth } from "@/features/auth/hooks/useAuth";
import { LoginPage } from "@/features/auth/pages/LoginPage";
import { ProfilePage } from "@/features/profile/pages/ProfilePage";
import { HomePage } from "@/pages/HomePage";
import { AppLoader } from "@/shared/ui/AppLoader";

function App() {
  const { status } = useAuth();
  const [pathname, setPathname] = useState(() => window.location.pathname);
  const articleDetailMatch = pathname.match(/^\/articles\/(\d+)$/);
  const articleEditMatch = pathname.match(/^\/articles\/(\d+)\/modifier$/);
  const eventDetailMatch = pathname.match(/^\/events\/(\d+)$/);
  const eventEditMatch = pathname.match(/^\/events\/(\d+)\/modifier$/);

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
    if (pathname === "/articles/nouveau" && status === "unauthenticated") {
      window.history.replaceState({}, "", "/login");
      setPathname("/login");
    }
    if (pathname === "/mes-articles" && status === "unauthenticated") {
      window.history.replaceState({}, "", "/login");
      setPathname("/login");
    }
    if (pathname === "/events/nouveau" && status === "unauthenticated") {
      window.history.replaceState({}, "", "/login");
      setPathname("/login");
    }
    if (pathname === "/mes-events" && status === "unauthenticated") {
      window.history.replaceState({}, "", "/login");
      setPathname("/login");
    }
    if (articleEditMatch && status === "unauthenticated") {
      window.history.replaceState({}, "", "/login");
      setPathname("/login");
    }
    if (eventEditMatch && status === "unauthenticated") {
      window.history.replaceState({}, "", "/login");
      setPathname("/login");
    }
  }, [articleEditMatch, eventEditMatch, pathname, status]);

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

  if (pathname === "/articles") {
    return <ArticlesPage />;
  }

  if (pathname === "/events") {
    return <EventsPage />;
  }

  if (pathname === "/articles/nouveau") {
    if (status === "loading") {
      return <AppLoader label="Vérification de votre session" />;
    }

    if (status === "authenticated") {
      return <CreateArticlePage />;
    }

    return <AppLoader label="Redirection vers la connexion" />;
  }

  if (pathname === "/mes-articles") {
    if (status === "loading") {
      return <AppLoader label="Chargement de vos articles" />;
    }

    if (status === "authenticated") {
      return <MyArticlesPage />;
    }

    return <AppLoader label="Redirection vers la connexion" />;
  }

  if (pathname === "/events/nouveau") {
    if (status === "loading") {
      return <AppLoader label="Verification de votre session" />;
    }

    if (status === "authenticated") {
      return <CreateEventPage />;
    }

    return <AppLoader label="Redirection vers la connexion" />;
  }

  if (pathname === "/mes-events") {
    if (status === "loading") {
      return <AppLoader label="Chargement de vos evenements" />;
    }

    if (status === "authenticated") {
      return <MyEventsPage />;
    }

    return <AppLoader label="Redirection vers la connexion" />;
  }

  if (articleEditMatch) {
    if (status === "loading") {
      return <AppLoader label="Vérification de votre session" />;
    }

    if (status === "authenticated") {
      return <EditArticlePage idResource={Number(articleEditMatch[1])} />;
    }

    return <AppLoader label="Redirection vers la connexion" />;
  }

  if (articleDetailMatch) {
    return <ArticleDetailPage idResource={Number(articleDetailMatch[1])} />;
  }

  if (eventEditMatch) {
    if (status === "loading") {
      return <AppLoader label="Verification de votre session" />;
    }

    if (status === "authenticated") {
      return <EditEventPage idResource={Number(eventEditMatch[1])} />;
    }

    return <AppLoader label="Redirection vers la connexion" />;
  }

  if (eventDetailMatch) {
    return <EventDetailPage idResource={Number(eventDetailMatch[1])} />;
  }

  return <HomePage />;
}

export default App;
