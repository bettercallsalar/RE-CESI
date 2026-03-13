import { Box, Button, Stack, Text } from "@chakra-ui/react";
import { SiteLayout } from "@/app/layouts/SiteLayout";
import { useAuth } from "@/features/auth/hooks/useAuth";
import { ShowcasePanel } from "@/shared/ui/site/ShowcasePanel";

export function HomePage() {
  const { status, user, signOut } = useAuth();
  const isAuthenticated = status === "authenticated";

  return (
    <SiteLayout headerVariant={isAuthenticated ? "authenticated" : "public"}>
      <Stack spacing={7}>
        <ShowcasePanel minHeight={{ base: "250px", md: "360px", lg: "430px" }} title="Titre Article" />
        <ShowcasePanel minHeight={{ base: "250px", md: "360px", lg: "430px" }} title="Titre Événement" />

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
                <Text color="ink.500" fontSize="11px">
                  Connecté en tant que {user?.firstName ?? user?.username}
                </Text>
                <Text color="ink.400" fontSize="10px">
                  {user?.email}
                </Text>
              </>
            ) : (
              <>
                <Text color="ink.500" fontSize="11px">
                  Cette page d'accueil est publique.
                </Text>
                <Text color="ink.400" fontSize="10px">
                  Accédez à votre compte uniquement si vous avez besoin d'un espace personnel.
                </Text>
              </>
            )}
          </Box>

          {isAuthenticated ? (
            <Button fontSize="11px" h="28px" onClick={signOut} variant="outline">
              Se déconnecter
            </Button>
          ) : (
            <Button as="a" fontSize="11px" h="28px" href="/login" variant="outline">
              Se connecter
            </Button>
          )}
        </Stack>
      </Stack>
    </SiteLayout>
  );
}
