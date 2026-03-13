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
          <Text fontSize={{ base: "20px", sm: "24px", md: "30px" }} fontWeight="700" textAlign="center">
            Bienvenue sur (RE) Sources Relationnelles !
          </Text>
          <Text color="ink.500" fontSize={{ base: "16px", sm: "17px", md: "18px" }} maxW="720px" textAlign="center">
            La plateforme d'échange préférée des français
          </Text>
        </>
      }
    >
      <Grid alignItems="start" gap={{ base: 8, lg: 10, xl: 12 }} templateColumns={{ base: "1fr", xl: "1.3fr 0.8fr" }}>
        <GridItem>
          <ShowcasePanel minHeight={{ base: "280px", md: "380px", lg: "460px" }} title="Titre Article" />
        </GridItem>
        <GridItem>
          <LoginForm />
        </GridItem>
      </Grid>
    </SiteLayout>
  );
}
