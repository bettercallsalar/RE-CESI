import { Button, HStack, Skeleton, Stack, Text } from "@chakra-ui/react";
import { useEffect, useState } from "react";
import { FiHeart, FiThumbsDown, FiThumbsUp } from "react-icons/fi";
import type { IconType } from "react-icons";
import { useAuth } from "@/features/auth/hooks/useAuth";
import { reactionsService } from "@/shared/api/reactions.service";
import { createErrorMessage, createInfoMessage, showFormMessage } from "@/shared/lib/feedback/showFormMessage";
import type { ResourceReaction, ReactionName } from "@/shared/types/reaction";
import { AppIcon } from "@/shared/ui/icons/AppIcon";
import type { FeedbackMessage } from "@/shared/ui/feedback/message.types";

interface ResourceReactionsProps {
  idResource: number;
}

interface ReactionOption {
  name: ReactionName;
  label: string;
  icon: IconType;
  activeBg: string;
  activeBorderColor: string;
  activeColor: string;
}

const reactionOptions: ReactionOption[] = [
  {
    name: "like",
    label: "J'aime",
    icon: FiThumbsUp,
    activeBg: "green.50",
    activeBorderColor: "green.300",
    activeColor: "green.700"
  },
  {
    name: "love",
    label: "J'adore",
    icon: FiHeart,
    activeBg: "pink.50",
    activeBorderColor: "pink.300",
    activeColor: "pink.700"
  },
  {
    name: "dislike",
    label: "Je n'aime pas",
    icon: FiThumbsDown,
    activeBg: "orange.50",
    activeBorderColor: "orange.300",
    activeColor: "orange.700"
  }
];

function getReactionOption(name: ReactionName) {
  return reactionOptions.find((option) => option.name === name) ?? null;
}

function getReactionCount(reactions: ResourceReaction[], name: ReactionName) {
  return reactions.filter((reaction) => reaction.name === name).length;
}

function getMessageColor(tone: FeedbackMessage["tone"]) {
  if (tone === "success") {
    return "green.600";
  }

  if (tone === "warning") {
    return "orange.600";
  }

  if (tone === "error") {
    return "red.600";
  }

  return "ink.500";
}

function sortReactions(reactions: ResourceReaction[]) {
  return [...reactions].sort((left, right) => left.idReaction - right.idReaction);
}

export function ResourceReactions({ idResource }: ResourceReactionsProps) {
  const { status, token, user } = useAuth();
  const [reactions, setReactions] = useState<ResourceReaction[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [pendingReaction, setPendingReaction] = useState<ReactionName | "remove" | null>(null);
  const [message, setMessage] = useState<FeedbackMessage | null>(null);
  const isAuthenticated = status === "authenticated";
  const isAuthLoading = status === "loading";
  const currentUserReaction = user ? reactions.find((reaction) => reaction.idUser === user.idUser) ?? null : null;

  useEffect(() => {
    let cancelled = false;

    async function loadReactions() {
      setIsLoading(true);

      try {
        const response = await reactionsService.getByResource(idResource);

        if (cancelled) {
          return;
        }

        setReactions(sortReactions(response));
        showFormMessage(setMessage, null);
      } catch (loadError) {
        if (!cancelled) {
          setReactions([]);
          showFormMessage(setMessage, createErrorMessage(loadError, "Chargement impossible"));
        }
      } finally {
        if (!cancelled) {
          setIsLoading(false);
        }
      }
    }

    void loadReactions();

    return () => {
      cancelled = true;
    };
  }, [idResource]);

  async function handleReactionSelection(name: ReactionName) {
    if (!isAuthenticated || !token || !user) {
      showFormMessage(setMessage, createInfoMessage("Connectez-vous pour reagir a cette ressource.", "Connexion requise"));
      return;
    }

    const existingReaction = currentUserReaction;
    const isRemovingCurrentReaction = existingReaction?.name === name;
    setPendingReaction(isRemovingCurrentReaction ? "remove" : name);

    try {
      if (existingReaction && isRemovingCurrentReaction) {
        await reactionsService.delete(token, existingReaction.idReaction);
        setReactions((current) => current.filter((reaction) => reaction.idReaction !== existingReaction.idReaction));
        showFormMessage(setMessage, null);
        return;
      }

      if (existingReaction) {
        const updatedReaction = await reactionsService.update(token, existingReaction.idReaction, { name });
        setReactions((current) =>
          current.map((reaction) => (reaction.idReaction === updatedReaction.idReaction ? updatedReaction : reaction))
        );
        showFormMessage(setMessage, null);
        return;
      }

      const createdReaction = await reactionsService.create(token, idResource, { name });
      setReactions((current) => sortReactions([...current, createdReaction]));
      showFormMessage(setMessage, null);
    } catch (submitError) {
      showFormMessage(setMessage, createErrorMessage(submitError, "Reaction impossible"));
    } finally {
      setPendingReaction(null);
    }
  }

  return (
    <Stack align="start" spacing={1.5}>
      {isLoading ? (
        <HStack spacing={2}>
          <Skeleton borderRadius="full" height="36px" width="56px" />
          <Skeleton borderRadius="full" height="36px" width="56px" />
          <Skeleton borderRadius="full" height="36px" width="56px" />
        </HStack>
      ) : (
        <HStack spacing={2} wrap="wrap">
          {reactionOptions.map((option) => {
            const count = getReactionCount(reactions, option.name);
            const isActive = currentUserReaction?.name === option.name;
            const isPending = pendingReaction === option.name || (pendingReaction === "remove" && isActive);

            return (
              <Button
                bg={isActive ? option.activeBg : "transparent"}
                border="1px solid"
                borderColor={isActive ? option.activeBorderColor : "canvas.200"}
                color={isActive ? option.activeColor : "ink.700"}
                h="36px"
                isDisabled={isAuthLoading}
                isLoading={isPending}
                key={option.name}
                minW="0"
                onClick={() => {
                  void handleReactionSelection(option.name);
                }}
                px={3}
                title={option.label}
                variant="outline"
              >
                <HStack spacing={1.5}>
                  <AppIcon
                    bg={isActive ? "white" : "canvas.100"}
                    borderRadius="full"
                    color={isActive ? option.activeColor : "ink.500"}
                    icon={option.icon}
                    iconSize="16px"
                    size="sm"
                  />
                  <Text as="span" fontSize="13px" fontWeight="700" lineHeight="1">
                    {count}
                  </Text>
                </HStack>
              </Button>
            );
          })}
        </HStack>
      )}

      {message ? (
        <Text color={getMessageColor(message.tone)} fontSize="12px">
          {message.message}
        </Text>
      ) : null}
    </Stack>
  );
}
