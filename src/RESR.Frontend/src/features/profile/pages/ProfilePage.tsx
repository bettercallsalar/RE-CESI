import { Box } from "@chakra-ui/react";
import { SiteLayout } from "@/app/layouts/SiteLayout";
import { ProfileForm } from "@/features/profile/components/ProfileForm";

export function ProfilePage() {
  return (
    <SiteLayout headerVariant="authenticated">
      <Box maxW="960px" mx="auto" w="100%">
        <ProfileForm />
      </Box>
    </SiteLayout>
  );
}
