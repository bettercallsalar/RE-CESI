import type { PropsWithChildren, ReactNode } from "react";
import { Box, Container, Link, Stack, Text } from "@chakra-ui/react";
import { SiteFooter } from "@/shared/ui/site/SiteFooter";
import { SiteHeader } from "@/shared/ui/site/SiteHeader";

interface SiteLayoutProps extends PropsWithChildren {
  headerVariant?: "public" | "authenticated";
  intro?: ReactNode;
}

export function SiteLayout({ children, intro, headerVariant = "public" }: SiteLayoutProps) {
  return (
    <Box minH="100vh">
      <Container maxW="none" px={{ base: 4, sm: 6, md: 8, lg: 10, xl: 12 }}>
        <Box marginInline="auto" maxW="1520px">
          <Link
            bg="brand.500"
            color="surface.onAccent"
            fontSize="15px"
            fontWeight="700"
            href="#main-content"
            left="-9999px"
            px={4}
            py={3}
            position="absolute"
            top="0"
            _focusVisible={{ left: "12px", top: "12px", zIndex: 20 }}
          >
            Aller au contenu principal
          </Link>
          <SiteHeader variant={headerVariant} />

          <Stack align="center" mb={{ base: 10, md: 12 }} spacing={2}>
            {intro ?? (
              <>
                <Text fontSize={{ base: "20px", sm: "24px", md: "30px" }} fontWeight="700" lineHeight="1.2" textAlign="center">
                  Bienvenue sur (RE) Sources Relationnelles !
                </Text>
                <Text color="ink.500" fontSize={{ base: "16px", sm: "17px", md: "18px" }} maxW="720px" textAlign="center">
                  La plateforme d'échange préférée des français
                </Text>
              </>
            )}
          </Stack>

          <Box as="main" id="main-content">
            {children}
          </Box>

          <SiteFooter />
        </Box>
      </Container>
    </Box>
  );
}
