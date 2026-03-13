import { Box, Collapse, Flex, HStack, IconButton, Link, Show, Stack, Text, useDisclosure } from "@chakra-ui/react";
import { HamburgerIcon } from "@chakra-ui/icons";
import { GovernmentBrand } from "@/shared/ui/site/GovernmentBrand";

interface SiteHeaderProps {
  variant?: "public" | "authenticated";
}

function NavGlyph({ type }: { type: "chart" | "grid" | "user" }) {
  if (type === "chart") {
    return (
      <Box display="inline-flex" gap="2px" h="16px" w="16px">
        <Box alignSelf="end" bg="brand.500" h="7px" w="3px" />
        <Box alignSelf="end" bg="brand.500" h="11px" w="3px" />
        <Box alignSelf="end" bg="brand.500" h="14px" w="3px" />
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
        h="16px"
        p="2px"
        w="16px"
      >
        <Box bg="brand.500" />
        <Box bg="brand.500" />
        <Box bg="brand.500" />
        <Box bg="brand.500" />
      </Box>
    );
  }

  return (
    <Box border="1px solid" borderColor="brand.500" borderRadius="999px" h="16px" position="relative" w="16px">
      <Box bg="brand.500" borderRadius="999px" h="4px" left="5px" position="absolute" top="3px" w="4px" />
      <Box
        borderColor="brand.500"
        borderRadius="999px 999px 6px 6px"
        borderStyle="solid"
        borderWidth="1px 1px 0"
        h="6px"
        left="3px"
        position="absolute"
        top="8px"
        w="8px"
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
    <Link _hover={{ textDecoration: "none", color: "brand.600" }} color="brand.500" href={href}>
      <HStack spacing={2}>
        <Text fontSize="11px" fontWeight="600">
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
          { label: "Statistiques", glyph: "chart" as const, href: "/" },
          { label: "Ressources", glyph: "grid" as const, href: "/" },
          { label: "MonCompte", glyph: "user" as const, href: "/" }
        ]
      : [
          { label: "Statistiques", glyph: "chart" as const, href: "/" },
          { label: "Ressources", glyph: "grid" as const, href: "/" },
          { label: "MonCompte", glyph: "user" as const, href: "/login" }
        ];

  return (
    <Box pb={6} pt={{ base: 4, md: 6 }}>
      <Flex align="start" gap={3} justify="space-between">
        <GovernmentBrand />

        <Stack align="center" flex="1" px={{ base: 1, sm: 4, md: 10 }} spacing={1}>
          <Text color="brand.500" fontSize={{ base: "12px", sm: "14px", md: "18px" }} fontWeight="700" textAlign="center">
            (RE) Sources Relationnelles
          </Text>
        </Stack>

        <Box minW={{ base: "40px", md: "260px" }}>
          <Show above="lg">
            <HStack justify="flex-end" spacing={5}>
              {navigationItems.map((item) => (
                <HeaderLink glyph={item.glyph} href={item.href} key={item.label} label={item.label} />
              ))}
            </HStack>
          </Show>
          <Show below="lg">
            <Flex justify="flex-end">
              <IconButton
                aria-label="Ouvrir le menu"
                color="brand.500"
                icon={<HamburgerIcon boxSize={6} />}
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
          <Stack align="flex-end" pt={4} spacing={3}>
            {navigationItems.map((item) => (
              <HeaderLink glyph={item.glyph} href={item.href} key={item.label} label={item.label} />
            ))}
          </Stack>
        </Collapse>
      </Show>
    </Box>
  );
}
