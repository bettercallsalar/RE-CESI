import { Box, Button, SimpleGrid, Skeleton, Stack, Text } from "@chakra-ui/react";
import { useEffect, useState } from "react";
import { SiteLayout } from "@/app/layouts/SiteLayout";
import { ArticlesGrid } from "@/features/articles/components/ArticlesGrid";
import { useLatestArticles } from "@/features/articles/hooks/useLatestArticles";
import { useAuth } from "@/features/auth/hooks/useAuth";
import { flashMessageStorage } from "@/shared/lib/storage/flashMessageStorage";
import { MessageBanner } from "@/shared/ui/feedback/MessageBanner";
import type { FeedbackMessage } from "@/shared/ui/feedback/message.types";
import { ShowcasePanel } from "@/shared/ui/site/ShowcasePanel";

export function HomePage() {
  const { status, user, signOut } = useAuth();
  const isAuthenticated = status === "authenticated";
  const [flashMessage, setFlashMessage] = useState<FeedbackMessage | null>(null);
  const {
    articles,
    categories,
    isLoading: isLoadingArticles,
    message: articlesMessage
  } = useLatestArticles(3);

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

        <Stack spacing={6}>
          <Stack
            align={{ base: "stretch", lg: "center" }}
            direction={{ base: "column", lg: "row" }}
            justify="space-between"
            spacing={4}
          >
            <Box>
              <Text color="ink.800" fontSize={{ base: "24px", md: "30px" }} fontWeight="700">
                Les 3 derniers articles
              </Text>
              <Text color="ink.500" fontSize={{ base: "15px", md: "16px" }} mt={2}>
                Une sélection des publications publiques validées les plus récentes.
              </Text>
            </Box>

            <Stack direction={{ base: "column", sm: "row" }} spacing={3}>
              <Button as="a" href="/articles" variant="outline">
                Voir tous les articles
              </Button>
              {isAuthenticated ? (
                <Button as="a" href="/articles/nouveau">
                  Publier un article
                </Button>
              ) : null}
            </Stack>
          </Stack>

          {articlesMessage ? (
            <MessageBanner message={articlesMessage.message} title={articlesMessage.title} tone={articlesMessage.tone} />
          ) : null}

          {isLoadingArticles ? (
            <SimpleGrid columns={{ base: 1, lg: 3 }} spacing={{ base: 5, md: 6 }}>
              {Array.from({ length: 3 }).map((_, index) => (
                <Skeleton borderRadius="16px" height="240px" key={index} />
              ))}
            </SimpleGrid>
          ) : (
            <ArticlesGrid
              articles={articles}
              categories={categories}
              compact
              emptyLabel="Aucun article public n'est disponible pour le moment."
            />
          )}
        </Stack>

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
