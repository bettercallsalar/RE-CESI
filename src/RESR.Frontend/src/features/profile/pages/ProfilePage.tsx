import { Grid, GridItem, Text } from "@chakra-ui/react";
import { SiteLayout } from "@/app/layouts/SiteLayout";
import { ProfileForm } from "@/features/profile/components/ProfileForm";
import { ShowcasePanel } from "@/shared/ui/site/ShowcasePanel";

export function ProfilePage() {
  return (
    <SiteLayout headerVariant="authenticated">
      <Grid alignItems="start" gap={{ base: 8, lg: 10, xl: 12 }} templateColumns={{ base: "1fr", xl: "0.95fr 1.05fr" }}>
        <GridItem>
          <Text color="ink.500" fontSize={{ base: "16px", md: "17px" }} mb={6}>
            Gérez ici vos informations personnelles, votre département et la suppression éventuelle de votre compte.
          </Text>
          <ShowcasePanel minHeight={{ base: "260px", md: "340px", lg: "420px" }} title="Mon espace utilisateur" />
        </GridItem>

        <GridItem>
          <ProfileForm />
        </GridItem>
      </Grid>
    </SiteLayout>
  );
}
