import type { PropsWithChildren, ReactNode } from "react";
import { Box, Container, Stack, Text } from "@chakra-ui/react";
import { SiteFooter } from "@/shared/ui/site/SiteFooter";
import { SiteHeader } from "@/shared/ui/site/SiteHeader";

interface SiteLayoutProps extends PropsWithChildren {
  headerVariant?: "public" | "authenticated";
  intro?: ReactNode;
}

export function SiteLayout({ children, intro, headerVariant = "public" }: SiteLayoutProps) {
  return (
    <Box minH="100vh">
      <Container maxW="none" px={{ base: 3, sm: 5, md: 8, lg: 10, xl: 12 }}>
        <Box marginInline="auto" maxW="1520px">
          <SiteHeader variant={headerVariant} />

          <Stack align="center" mb={{ base: 8, md: 10 }} spacing={1}>
            {intro ?? (
              <>
                <Text fontSize={{ base: "12px", sm: "13px", md: "14px" }} fontWeight="600" textAlign="center">
                  Bienvenue sur (RE) Sources Relationnelles !
                </Text>
                <Text color="ink.500" fontSize={{ base: "10px", sm: "11px", md: "12px" }} textAlign="center">
                  La plateforme d'échange préférée des français
                </Text>
              </>
            )}
          </Stack>

          {children}

          <SiteFooter />
        </Box>
      </Container>
    </Box>
  );
}
