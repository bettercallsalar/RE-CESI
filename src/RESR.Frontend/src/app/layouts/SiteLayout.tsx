import type { PropsWithChildren, ReactNode } from "react";
import { Box, Container, Stack, Text } from "@chakra-ui/react";
import { SiteFooter } from "@/shared/ui/site/SiteFooter";
import { SiteHeader } from "@/shared/ui/site/SiteHeader";

interface SiteLayoutProps extends PropsWithChildren {
  isAuthenticated?: boolean;
  intro?: ReactNode;
}

export function SiteLayout({ children, intro, isAuthenticated = false }: SiteLayoutProps) {
  return (
    <Box minH="100vh">
      <Container maxW="1600px" px={{ base: 4, md: 8, xl: 12 }}>
        <SiteHeader isAuthenticated={isAuthenticated} />

        <Stack align="center" mb={{ base: 8, md: 10 }} spacing={1}>
          {intro ?? (
            <>
              <Text fontSize={{ base: "13px", md: "14px" }} fontWeight="600" textAlign="center">
                Bienvenue sur (RE) Sources Relationnelles !
              </Text>
              <Text color="ink.500" fontSize={{ base: "11px", md: "12px" }} textAlign="center">
                La plateforme d'échange préférée des français
              </Text>
            </>
          )}
        </Stack>

        {children}

        <SiteFooter />
      </Container>
    </Box>
  );
}
