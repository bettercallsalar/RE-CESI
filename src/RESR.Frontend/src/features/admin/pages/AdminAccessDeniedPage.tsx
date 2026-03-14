import { Button, Stack, Text } from "@chakra-ui/react";
import { SiteLayout } from "@/app/layouts/SiteLayout";
import { MessageBanner } from "@/shared/ui/feedback/MessageBanner";

interface AdminAccessDeniedPageProps {
  message: string;
  title?: string;
}

export function AdminAccessDeniedPage({ message, title = "Acces refuse" }: AdminAccessDeniedPageProps) {
  return (
    <SiteLayout headerVariant="authenticated">
      <Stack maxW="760px" spacing={6}>
        <MessageBanner message={message} title={title} tone="warning" />
        <Text color="ink.500" fontSize={{ base: "15px", md: "16px" }}>
          Cette section d'administration n'est disponible que pour les comptes disposant des permissions requises.
        </Text>
        <Button alignSelf="start" as="a" href="/" variant="outline">
          Retour a l'accueil
        </Button>
      </Stack>
    </SiteLayout>
  );
}
