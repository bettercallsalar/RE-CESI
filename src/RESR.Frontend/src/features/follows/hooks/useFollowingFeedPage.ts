import { useEffect, useState } from "react";
import { articlesService } from "@/features/articles/services/articles.service";
import { useAuth } from "@/features/auth/hooks/useAuth";
import { eventsService } from "@/features/events/services/events.service";
import { followsService } from "@/features/follows/services/follows.service";
import type { FollowUser } from "@/features/follows/types/follows.types";
import { collectAllPages } from "@/shared/lib/api/collectAllPages";
import type { Article, Category } from "@/shared/types/article";
import type { Event } from "@/shared/types/event";
import type { FeedbackMessage } from "@/shared/ui/feedback/message.types";
import { createErrorMessage, showFormMessage } from "@/shared/lib/feedback/showFormMessage";

const API_PAGE_SIZE = 100;

function sortResourcesByDate<T extends { createdAt: string }>(items: T[]) {
  return [...items].sort((left, right) => new Date(right.createdAt).getTime() - new Date(left.createdAt).getTime());
}

function dedupeByResourceId<T extends { idResource: number }>(items: T[]) {
  const seen = new Map<number, T>();

  for (const item of items) {
    seen.set(item.idResource, item);
  }

  return Array.from(seen.values());
}

export function useFollowingFeedPage() {
  const { token, user } = useAuth();
  const [categories, setCategories] = useState<Category[]>([]);
  const [followingUsers, setFollowingUsers] = useState<FollowUser[]>([]);
  const [articles, setArticles] = useState<Article[]>([]);
  const [events, setEvents] = useState<Event[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [message, setMessage] = useState<FeedbackMessage | null>(null);

  useEffect(() => {
    let cancelled = false;

    async function load() {
      if (!token || !user) {
        setCategories([]);
        setFollowingUsers([]);
        setArticles([]);
        setEvents([]);
        setIsLoading(false);
        return;
      }

      setIsLoading(true);

      try {
        const [categoriesResponse, followingUsersResponse] = await Promise.all([
          articlesService.getCategories().catch(() => []),
          collectAllPages((page) => followsService.getOwnFollowing(token, { page, pageSize: API_PAGE_SIZE }))
        ]);

        if (cancelled) {
          return;
        }

        setCategories(categoriesResponse);
        setFollowingUsers(followingUsersResponse);

        if (followingUsersResponse.length === 0) {
          setArticles([]);
          setEvents([]);
          showFormMessage(setMessage, null);
          return;
        }

        const [articlesCollections, eventsCollections] = await Promise.all([
          Promise.all(
            followingUsersResponse.map((followedUser) =>
              collectAllPages((page) =>
                articlesService.getPublicArticles({
                  page,
                  pageSize: API_PAGE_SIZE,
                  idUser: followedUser.idUser
                })
              )
            )
          ),
          Promise.all(
            followingUsersResponse.map((followedUser) =>
              collectAllPages((page) =>
                eventsService.getPublicEvents({
                  page,
                  pageSize: API_PAGE_SIZE,
                  idUser: followedUser.idUser
                })
              )
            )
          )
        ]);

        if (cancelled) {
          return;
        }

        setArticles(sortResourcesByDate(dedupeByResourceId(articlesCollections.flat())));
        setEvents(sortResourcesByDate(dedupeByResourceId(eventsCollections.flat())));
        showFormMessage(setMessage, null);
      } catch (loadError) {
        if (!cancelled) {
          setCategories([]);
          setFollowingUsers([]);
          setArticles([]);
          setEvents([]);
          showFormMessage(setMessage, createErrorMessage(loadError, "Chargement impossible"));
        }
      } finally {
        if (!cancelled) {
          setIsLoading(false);
        }
      }
    }

    void load();

    return () => {
      cancelled = true;
    };
  }, [token, user]);

  return {
    categories,
    followingUsers,
    articles,
    events,
    isLoading,
    message
  };
}
