import { Box, Button, Stack, Text } from "@chakra-ui/react";
import { useEffect, useState } from "react";
import { SiteLayout } from "@/app/layouts/SiteLayout";
import { useAuth } from "@/features/auth/hooks/useAuth";
import { ShowcasePanel } from "@/shared/ui/site/ShowcasePanel";
import { flashMessageStorage } from "@/shared/lib/storage/flashMessageStorage";
import { MessageBanner } from "@/shared/ui/feedback/MessageBanner";
import type { FeedbackMessage } from "@/shared/ui/feedback/message.types";

export function HomePage() {
  const { status, user, signOut } = useAuth();
  const isAuthenticated = status === "authenticated";
  const [flashMessage, setFlashMessage] = useState<FeedbackMessage | null>(null);

  useEffect(() => {
    setFlashMessage(flashMessageStorage.take());
  }, []);

  return (
    <SiteLayout headerVariant={isAuthenticated ? "authenticated" : "public"}>
      <Stack spacing={{ base: 10, md: 12 }}>
        {flashMessage ? (
          <MessageBanner
            message={flashMessage.message}
            onClose={() => setFlashMessage(null)}
            title={flashMessage.title ?? (flashMessage.tone === "success" ? "Succès" : "Information")}
            tone={flashMessage.tone}
          />
        ) : null}

        <ShowcasePanel minHeight={{ base: "260px", md: "380px", lg: "460px" }} title="Titre Article" />
        <ShowcasePanel minHeight={{ base: "260px", md: "380px", lg: "460px" }} title="Titre Événement" />

        <Stack
          align={{ base: "start", md: "center" }}
          direction={{ base: "column", md: "row" }}
          justify="space-between"
          pt={1}
          spacing={4}
        >
          <Box>
            {isAuthenticated ? (
              <>
                <Text color="ink.500" fontSize={{ base: "15px", md: "16px" }}>
                  Connecté en tant que {user?.firstName ?? user?.username}
                </Text>
                <Text color="ink.400" fontSize={{ base: "14px", md: "15px" }}>
                  {user?.email}
                </Text>
              </>
            ) : (
              <>
                <Text color="ink.500" fontSize={{ base: "15px", md: "16px" }}>
                  Cette page d'accueil est publique.
                </Text>
                <Text color="ink.400" fontSize={{ base: "14px", md: "15px" }} maxW="760px">
                  Accédez à votre compte uniquement si vous avez besoin d'un espace personnel.
                </Text>
              </>
            )}
          </Box>

          {isAuthenticated ? (
            <Button fontSize={{ base: "15px", md: "16px" }} h="48px" onClick={signOut} px={6} variant="outline">
              Se déconnecter
            </Button>
          ) : (
            <Button as="a" fontSize={{ base: "15px", md: "16px" }} h="48px" href="/login" px={6} variant="outline">
              Se connecter
            </Button>
          )}
        </Stack>
      </Stack>
    </SiteLayout>
  );
}
