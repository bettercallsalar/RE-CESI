import { useEffect, useState } from "react";
import { articlesService } from "@/features/articles/services/articles.service";
import { useAuth } from "@/features/auth/hooks/useAuth";
import { eventsService } from "@/features/events/services/events.service";
import { followsService } from "@/features/follows/services/follows.service";
import { profileService } from "@/features/profile/services/profile.service";
import type { PublicUserProfile } from "@/features/profile/types/profile.types";
import type { Category, PaginatedResponse, Article } from "@/shared/types/article";
import type { Event } from "@/shared/types/event";
import type { FeedbackMessage } from "@/shared/ui/feedback/message.types";
import { createErrorMessage, createSuccessMessage, showFormMessage } from "@/shared/lib/feedback/showFormMessage";

const PAGE_SIZE = 9;

interface PaginatedState<T> {
  items: T[];
  page: number;
  totalPages: number;
  totalCount: number;
  isLoading: boolean;
}

function createInitialPaginatedState<T>(): PaginatedState<T> {
  return {
    items: [],
    page: 1,
    totalPages: 0,
    totalCount: 0,
    isLoading: true
  };
}

function toPaginatedState<T>(response: PaginatedResponse<T>): PaginatedState<T> {
  return {
    items: response.items,
    page: response.page,
    totalPages: response.totalPages,
    totalCount: response.totalCount,
    isLoading: false
  };
}

export function useUserProfilePage(idUser: number) {
  const { token, user } = useAuth();
  const [profile, setProfile] = useState<PublicUserProfile | null>(null);
  const [categories, setCategories] = useState<Category[]>([]);
  const [articlesState, setArticlesState] = useState<PaginatedState<Article>>(createInitialPaginatedState<Article>());
  const [eventsState, setEventsState] = useState<PaginatedState<Event>>(createInitialPaginatedState<Event>());
  const [isProfileLoading, setIsProfileLoading] = useState(true);
  const [followersCount, setFollowersCount] = useState(0);
  const [followingCount, setFollowingCount] = useState(0);
  const [isFollowing, setIsFollowing] = useState(false);
  const [isFollowSubmitting, setIsFollowSubmitting] = useState(false);
  const [message, setMessage] = useState<FeedbackMessage | null>(null);

  useEffect(() => {
    if (!token) {
      setProfile(null);
      setCategories([]);
      setArticlesState({ ...createInitialPaginatedState<Article>(), isLoading: false });
      setEventsState({ ...createInitialPaginatedState<Event>(), isLoading: false });
      setFollowersCount(0);
      setFollowingCount(0);
      setIsFollowing(false);
      setIsProfileLoading(false);
      return;
    }

    const activeToken = token;
    let cancelled = false;

    async function load() {
      setIsProfileLoading(true);
      setArticlesState((current) => ({ ...current, isLoading: true }));
      setEventsState((current) => ({ ...current, isLoading: true }));

      try {
        const [profileResponse, categoriesResponse, articlesResponse, eventsResponse, followersResponse, followingResponse, followStateResponse] = await Promise.all([
          profileService.getUserProfile(activeToken, idUser),
          articlesService.getCategories().catch(() => []),
          articlesService.getPublicArticles({ page: 1, pageSize: PAGE_SIZE, idUser }),
          eventsService.getPublicEvents({ page: 1, pageSize: PAGE_SIZE, idUser }),
          followsService.getFollowers(idUser, { page: 1, pageSize: 1 }).catch(() => null),
          followsService.getFollowing(idUser, { page: 1, pageSize: 1 }).catch(() => null),
          user?.idUser && user.idUser !== idUser
            ? followsService.getOwnFollowingState(activeToken, idUser).catch(() => null)
            : Promise.resolve(null)
        ]);

        if (cancelled) {
          return;
        }

        setProfile(profileResponse);
        setCategories(categoriesResponse);
        setArticlesState(toPaginatedState(articlesResponse));
        setEventsState(toPaginatedState(eventsResponse));
        setFollowersCount(followersResponse?.totalCount ?? 0);
        setFollowingCount(followingResponse?.totalCount ?? 0);
        setIsFollowing(followStateResponse?.isFollowing ?? false);
        showFormMessage(setMessage, null);
      } catch (loadError) {
        if (cancelled) {
          return;
        }

        setProfile(null);
        setCategories([]);
        setArticlesState({ ...createInitialPaginatedState<Article>(), isLoading: false });
        setEventsState({ ...createInitialPaginatedState<Event>(), isLoading: false });
        setFollowersCount(0);
        setFollowingCount(0);
        setIsFollowing(false);
        showFormMessage(setMessage, createErrorMessage(loadError, "Chargement impossible"));
      } finally {
        if (!cancelled) {
          setIsProfileLoading(false);
        }
      }
    }

    void load();

    return () => {
      cancelled = true;
    };
  }, [idUser, token, user?.idUser]);

  async function goToArticlesPage(nextPage: number) {
    if (!token || nextPage < 1 || (articlesState.totalPages > 0 && nextPage > articlesState.totalPages)) {
      return;
    }

    setArticlesState((current) => ({ ...current, isLoading: true }));

    try {
      const response = await articlesService.getPublicArticles({
        page: nextPage,
        pageSize: PAGE_SIZE,
        idUser
      });

      setArticlesState(toPaginatedState(response));
      showFormMessage(setMessage, null);
    } catch (loadError) {
      setArticlesState((current) => ({ ...current, isLoading: false }));
      showFormMessage(setMessage, createErrorMessage(loadError, "Chargement impossible"));
    }
  }

  async function goToEventsPage(nextPage: number) {
    if (!token || nextPage < 1 || (eventsState.totalPages > 0 && nextPage > eventsState.totalPages)) {
      return;
    }

    setEventsState((current) => ({ ...current, isLoading: true }));

    try {
      const response = await eventsService.getPublicEvents({
        page: nextPage,
        pageSize: PAGE_SIZE,
        idUser
      });

      setEventsState(toPaginatedState(response));
      showFormMessage(setMessage, null);
    } catch (loadError) {
      setEventsState((current) => ({ ...current, isLoading: false }));
      showFormMessage(setMessage, createErrorMessage(loadError, "Chargement impossible"));
    }
  }

  async function followUser() {
    if (!token || user?.idUser === idUser) {
      return false;
    }

    setIsFollowSubmitting(true);

    try {
      await followsService.followUser(token, idUser);
      setIsFollowing(true);
      setFollowersCount((current) => current + 1);
      showFormMessage(setMessage, createSuccessMessage("Vous suivez maintenant cet utilisateur."));
      return true;
    } catch (submitError) {
      showFormMessage(setMessage, createErrorMessage(submitError, "Abonnement impossible"));
      return false;
    } finally {
      setIsFollowSubmitting(false);
    }
  }

  async function unfollowUser() {
    if (!token || user?.idUser === idUser) {
      return false;
    }

    setIsFollowSubmitting(true);

    try {
      await followsService.unfollowUser(token, idUser);
      setIsFollowing(false);
      setFollowersCount((current) => Math.max(0, current - 1));
      showFormMessage(setMessage, createSuccessMessage("Vous ne suivez plus cet utilisateur."));
      return true;
    } catch (submitError) {
      showFormMessage(setMessage, createErrorMessage(submitError, "Desabonnement impossible"));
      return false;
    } finally {
      setIsFollowSubmitting(false);
    }
  }

  return {
    profile,
    categories,
    articles: articlesState.items,
    articlesPage: articlesState.page,
    articlesTotalPages: articlesState.totalPages,
    articlesTotalCount: articlesState.totalCount,
    events: eventsState.items,
    eventsPage: eventsState.page,
    eventsTotalPages: eventsState.totalPages,
    eventsTotalCount: eventsState.totalCount,
    followersCount,
    followingCount,
    isLoading: isProfileLoading,
    isArticlesLoading: articlesState.isLoading,
    isEventsLoading: eventsState.isLoading,
    isOwnProfile: user?.idUser === profile?.idUser,
    isFollowing,
    isFollowSubmitting,
    message,
    goToArticlesPage,
    goToEventsPage,
    followUser,
    unfollowUser
  };
}
