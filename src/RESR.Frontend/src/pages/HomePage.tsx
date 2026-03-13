import { Box, Button, HStack, Stack, Text } from "@chakra-ui/react";
import { SiteLayout } from "@/app/layouts/SiteLayout";
import { useAuth } from "@/features/auth/hooks/useAuth";
import { ShowcasePanel } from "@/shared/ui/site/ShowcasePanel";

export function HomePage() {
  const { user, signOut } = useAuth();

  return (
    <SiteLayout isAuthenticated>
      <Stack spacing={7}>
        <ShowcasePanel minHeight={{ base: "250px", md: "360px", lg: "430px" }} title="Titre Article" />
        <ShowcasePanel minHeight={{ base: "250px", md: "360px", lg: "430px" }} title="Titre Événement" />

        <HStack justify="space-between" pt={1}>
          <Box>
            <Text color="ink.500" fontSize="11px">
              Connecté en tant que {user?.firstName ?? user?.username}
            </Text>
            <Text color="ink.400" fontSize="10px">
              {user?.email}
            </Text>
          </Box>
          <Button fontSize="11px" h="28px" onClick={signOut} variant="outline">
            Se déconnecter
          </Button>
        </HStack>
      </Stack>
    </SiteLayout>
  );
}
