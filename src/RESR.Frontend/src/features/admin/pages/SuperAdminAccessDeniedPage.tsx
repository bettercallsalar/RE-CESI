import { Button, Stack, Text } from "@chakra-ui/react";
import { SiteLayout } from "@/app/layouts/SiteLayout";
import { MessageBanner } from "@/shared/ui/feedback/MessageBanner";

export function SuperAdminAccessDeniedPage() {
  return (
    <SiteLayout headerVariant="authenticated">
      <Stack maxW="760px" spacing={6}>
        <MessageBanner
          message="Cette section est reservee au role SuperAdmin. Votre token actuel ne permet pas d'ouvrir la gestion des roles."
          title="Acces refuse"
          tone="warning"
        />
        <Text color="ink.500" fontSize={{ base: "15px", md: "16px" }}>
          Seul un compte avec le role SuperAdmin peut consulter la liste des roles et modifier leurs permissions.
        </Text>
        <Button alignSelf="start" as="a" href="/admin" variant="outline">
          Retour au tableau de bord
        </Button>
      </Stack>
    </SiteLayout>
  );
}
