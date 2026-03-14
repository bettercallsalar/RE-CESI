import { HamburgerIcon } from "@chakra-ui/icons";
import { Box, Collapse, Flex, HStack, IconButton, Link, Show, Stack, Text, useDisclosure } from "@chakra-ui/react";
import { GovernmentBrand } from "@/shared/ui/site/GovernmentBrand";

interface SiteHeaderProps {
  variant?: "public" | "authenticated";
}

function NavGlyph({ type }: { type: "chart" | "grid" | "user" }) {
  if (type === "chart") {
    return (
      <Box display="inline-flex" gap="3px" h="18px" w="18px">
        <Box alignSelf="end" bg="brand.500" h="8px" w="3px" />
        <Box alignSelf="end" bg="brand.500" h="12px" w="3px" />
        <Box alignSelf="end" bg="brand.500" h="16px" w="3px" />
      </Box>
    );
  }

  if (type === "grid") {
    return (
      <Box
        border="1px solid"
        borderColor="brand.500"
        display="grid"
        gap="2px"
        gridTemplateColumns="repeat(2, 1fr)"
        h="18px"
        p="2px"
        w="18px"
      >
        <Box bg="brand.500" />
        <Box bg="brand.500" />
        <Box bg="brand.500" />
        <Box bg="brand.500" />
      </Box>
    );
  }

  return (
    <Box border="1px solid" borderColor="brand.500" borderRadius="999px" h="18px" position="relative" w="18px">
      <Box bg="brand.500" borderRadius="999px" h="4px" left="6px" position="absolute" top="3px" w="4px" />
      <Box
        borderColor="brand.500"
        borderRadius="999px 999px 6px 6px"
        borderStyle="solid"
        borderWidth="1px 1px 0"
        h="6px"
        left="4px"
        position="absolute"
        top="8px"
        w="9px"
      />
    </Box>
  );
}

function HeaderLink({
  label,
  glyph,
  href
}: {
  label: string;
  glyph: "chart" | "grid" | "user";
  href: string;
}) {
  return (
    <Link _hover={{ textDecoration: "none", color: "ink.800" }} color="brand.500" href={href}>
      <HStack minH="44px" px={1} spacing={2.5}>
        <Text fontSize={{ base: "15px", xl: "16px" }} fontWeight="600">
          {label}
        </Text>
        <NavGlyph type={glyph} />
      </HStack>
    </Link>
  );
}

export function SiteHeader({ variant = "public" }: SiteHeaderProps) {
  const { isOpen, onToggle } = useDisclosure();
  const navigationItems =
    variant === "authenticated"
      ? [
          { label: "Mes articles", glyph: "chart" as const, href: "/mes-articles" },
          { label: "Articles", glyph: "grid" as const, href: "/articles" },
          { label: "Mon compte", glyph: "user" as const, href: "/mon-compte" }
        ]
      : [
          { label: "Statistiques", glyph: "chart" as const, href: "/" },
          { label: "Articles", glyph: "grid" as const, href: "/articles" },
          { label: "Mon compte", glyph: "user" as const, href: "/login" }
        ];

  return (
    <Box pb={{ base: 7, md: 8 }} pt={{ base: 5, md: 7 }}>
      <Flex align={{ base: "start", md: "center" }} gap={{ base: 3, md: 5 }} justify="space-between">
        <GovernmentBrand />

        <Stack align="center" flex="1" px={{ base: 2, sm: 6, md: 10 }} spacing={1}>
          <Text
            color="brand.500"
            fontSize={{ base: "18px", sm: "24px", md: "30px", xl: "34px" }}
            fontWeight="700"
            lineHeight="1.1"
            textAlign="center"
          >
            (RE) Sources Relationnelles
          </Text>
        </Stack>

        <Box minW={{ base: "48px", lg: "360px" }}>
          <Show above="lg">
            <HStack justify="flex-end" spacing={6}>
              {navigationItems.map((item) => (
                <HeaderLink glyph={item.glyph} href={item.href} key={item.label} label={item.label} />
              ))}
            </HStack>
          </Show>
          <Show below="lg">
            <Flex justify="flex-end">
              <IconButton
                aria-label="Ouvrir le menu"
                boxSize={{ base: "44px", md: "48px" }}
                color="brand.500"
                icon={<HamburgerIcon boxSize={7} />}
                minW="auto"
                onClick={onToggle}
                variant="ghost"
              />
            </Flex>
          </Show>
        </Box>
      </Flex>

      <Show below="lg">
        <Collapse animateOpacity in={isOpen}>
          <Stack
            align={{ base: "stretch", sm: "flex-end" }}
            bg="white"
            border="1px solid"
            borderColor="canvas.200"
            mt={3}
            pb={3}
            pt={3}
            px={{ base: 3, sm: 4 }}
            spacing={2}
          >
            {navigationItems.map((item) => (
              <HeaderLink glyph={item.glyph} href={item.href} key={item.label} label={item.label} />
            ))}
          </Stack>
        </Collapse>
      </Show>
    </Box>
  );
}
