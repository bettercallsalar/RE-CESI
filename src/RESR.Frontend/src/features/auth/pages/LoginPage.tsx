import { Grid, GridItem, Text } from "@chakra-ui/react";
import { SiteLayout } from "@/app/layouts/SiteLayout";
import { LoginForm } from "@/features/auth/components/LoginForm";
import { ShowcasePanel } from "@/shared/ui/site/ShowcasePanel";

export function LoginPage() {
  return (
    <SiteLayout
      headerVariant="public"
      intro={
        <>
          <Text fontSize={{ base: "12px", sm: "13px", md: "14px" }} fontWeight="600" textAlign="center">
            Bienvenue sur (RE) Sources Relationnelles !
          </Text>
          <Text color="ink.500" fontSize={{ base: "10px", sm: "11px", md: "12px" }} textAlign="center">
            La plateforme d'échange préférée des français
          </Text>
        </>
      }
    >
      <Grid alignItems="start" gap={{ base: 6, lg: 10 }} templateColumns={{ base: "1fr", xl: "1.35fr 0.75fr" }}>
        <GridItem>
          <ShowcasePanel minHeight={{ base: "280px", md: "360px", lg: "420px" }} title="Titre Article" />
        </GridItem>
        <GridItem>
          <LoginForm />
        </GridItem>
      </Grid>
    </SiteLayout>
  );
}
