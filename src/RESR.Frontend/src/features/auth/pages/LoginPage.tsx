import { Grid, GridItem } from "@chakra-ui/react";
import { SiteLayout } from "@/app/layouts/SiteLayout";
import { LoginForm } from "@/features/auth/components/LoginForm";
import { ShowcasePanel } from "@/shared/ui/site/ShowcasePanel";

export function LoginPage() {
  return (
    <SiteLayout headerVariant="public">
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
