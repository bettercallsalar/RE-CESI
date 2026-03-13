import { Box, Button, Flex, HStack, Link, Show, Stack, Text } from "@chakra-ui/react";
import { HamburgerIcon } from "@chakra-ui/icons";
import { GovernmentBrand } from "@/shared/ui/site/GovernmentBrand";

interface SiteHeaderProps {
  isAuthenticated?: boolean;
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

function HeaderLink({ label, glyph }: { label: string; glyph: "chart" | "grid" | "user" }) {
  return (
    <Link _hover={{ textDecoration: "none", color: "brand.600" }} color="brand.500">
      <HStack spacing={2}>
        <Text fontSize="11px" fontWeight="600">
          {label}
        </Text>
        <NavGlyph type={glyph} />
      </HStack>
    </Link>
  );
}

export function SiteHeader({ isAuthenticated = false }: SiteHeaderProps) {
  return (
    <Flex align="start" justify="space-between" pb={6} pt={{ base: 4, md: 6 }}>
      <GovernmentBrand />

      <Stack align="center" flex="1" px={{ base: 3, md: 10 }} spacing={1}>
        <Text color="brand.500" fontSize={{ base: "14px", md: "18px" }} fontWeight="700" textAlign="center">
          (RE) Sources Relationnelles
        </Text>
      </Stack>

      <Box minW={{ base: "56px", md: "220px" }}>
        <Show above="md">
          {isAuthenticated ? (
            <HStack justify="flex-end" spacing={5}>
              <HeaderLink glyph="chart" label="Statistiques" />
              <HeaderLink glyph="grid" label="Ressources" />
              <HeaderLink glyph="user" label="MonCompte" />
            </HStack>
          ) : (
            <Flex justify="flex-end">
              <Button aria-label="Menu" color="brand.500" minW="auto" variant="ghost">
                <HamburgerIcon boxSize={6} />
              </Button>
            </Flex>
          )}
        </Show>
        <Show below="md">
          <Flex justify="flex-end">
            <Button aria-label="Menu" color="brand.500" minW="auto" variant="ghost">
              <HamburgerIcon boxSize={6} />
            </Button>
          </Flex>
        </Show>
      </Box>
    </Flex>
  );
}
