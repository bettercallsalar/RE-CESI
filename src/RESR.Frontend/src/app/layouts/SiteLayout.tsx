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
            color="white"
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

          {intro ? (
            <Stack align="center" mb={{ base: 10, md: 12 }} spacing={2}>
              {intro}
            </Stack>
          ) : null}

          <Box as="main" id="main-content">
            {children}
          </Box>

          <SiteFooter />
        </Box>
      </Container>
    </Box>
  );
}
