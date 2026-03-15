import { Button, HStack, Stack, Text } from "@chakra-ui/react";
import { useEffect, useState } from "react";
import { BsBookmarkFill } from "react-icons/bs";
import { FiBookmark } from "react-icons/fi";
import { useAuth } from "@/features/auth/hooks/useAuth";
import { marksService } from "@/features/marks/services/marks.service";
import { ApiError } from "@/shared/api/httpClient";
import { createErrorMessage, showFormMessage } from "@/shared/lib/feedback/showFormMessage";
import { AppIcon } from "@/shared/ui/icons/AppIcon";
import type { FeedbackMessage } from "@/shared/ui/feedback/message.types";

interface ReadLaterToggleButtonProps {
  idResource: number;
}

export function ReadLaterToggleButton({ idResource }: ReadLaterToggleButtonProps) {
  const { status, token, user } = useAuth();
  const [isMarked, setIsMarked] = useState(false);
  const [isLoading, setIsLoading] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [message, setMessage] = useState<FeedbackMessage | null>(null);

  useEffect(() => {
    let cancelled = false;

    async function loadMarkState() {
      if (status !== "authenticated" || !token || !user) {
        setIsMarked(false);
        setIsLoading(false);
        showFormMessage(setMessage, null);
        return;
      }

      setIsLoading(true);

      try {
        const response = await marksService.getReadLaterMark(token, idResource);

        if (cancelled) {
          return;
        }

        setIsMarked(response.isReadLater);
        showFormMessage(setMessage, null);
      } catch (loadError) {
        if (cancelled) {
          return;
        }

        if (loadError instanceof ApiError && loadError.status === 404) {
          setIsMarked(false);
          showFormMessage(setMessage, null);
          return;
        }

        showFormMessage(setMessage, createErrorMessage(loadError, "Chargement impossible"));
      } finally {
        if (!cancelled) {
          setIsLoading(false);
        }
      }
    }

    void loadMarkState();

    return () => {
      cancelled = true;
    };
  }, [idResource, status, token, user]);

  async function toggleReadLater() {
    if (!token || !user) {
      return;
    }

    setIsSubmitting(true);

    try {
      if (isMarked) {
        await marksService.unmarkAsReadLater(token, idResource);
        setIsMarked(false);
      } else {
        await marksService.markAsReadLater(token, idResource);
        setIsMarked(true);
      }

      showFormMessage(setMessage, null);
    } catch (submitError) {
      showFormMessage(setMessage, createErrorMessage(submitError, "Marquage impossible"));
    } finally {
      setIsSubmitting(false);
    }
  }

  if (status !== "authenticated") {
    return null;
  }

  return (
    <Stack spacing={1}>
      <Button
        bg={isMarked ? "brand.500" : "white"}
        color={isMarked ? "white" : "brand.500"}
        isLoading={isLoading || isSubmitting}
        onClick={() => {
          void toggleReadLater();
        }}
        variant={isMarked ? "solid" : "outline"}
      >
        <HStack spacing={2}>
          <AppIcon color={isMarked ? "white" : "brand.500"} icon={isMarked ? BsBookmarkFill : FiBookmark} size="sm" />
          <Text as="span">{isMarked ? "Enregistre" : "Lire plus tard"}</Text>
        </HStack>
      </Button>

      {message ? (
        <Text color="red.600" fontSize="12px">
          {message.message}
        </Text>
      ) : null}
    </Stack>
  );
}
