import { Box } from "@chakra-ui/react";
import { SiteLayout } from "@/app/layouts/SiteLayout";
import { RegisterForm } from "@/features/auth/components/RegisterForm";

export function RegisterPage() {
  return (
    <SiteLayout headerVariant="public">
      <Box maxW="960px" mx="auto" w="100%">
        <RegisterForm />
      </Box>
    </SiteLayout>
  );
}
