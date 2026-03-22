import { Box, HStack, Link, SimpleGrid, Stack, Text } from "@chakra-ui/react";

function SocialBadge({ label }: { label: string }) {
  return (
    <Box
      aria-hidden="true"
      alignItems="center"
      border="1px solid"
      borderColor="brand.500"
      color="brand.500"
      display="inline-flex"
      fontSize="13px"
      fontWeight="700"
      h="36px"
      justifyContent="center"
      minW="36px"
      px={2}
      rounded="md"
    >
      {label}
    </Box>
  );
}

export function SiteFooter() {
  return (
    <SimpleGrid borderTop="1px solid" borderColor="canvas.200" columns={{ base: 1, md: 2 }} gap={{ base: 8, md: 12 }} mt={{ base: 16, md: 20 }} pt={{ base: 8, md: 10 }}>
      <Stack align={{ base: "center", md: "start" }} spacing={4}>
        <Text color="brand.500" fontSize={{ base: "15px", md: "16px" }} fontWeight="700">
          Nous suivre sur les réseaux sociaux
        </Text>
        <HStack spacing={4}>
          <SocialBadge label="X" />
          <SocialBadge label="f" />
          <SocialBadge label="in" />
          <SocialBadge label="▶" />
        </HStack>
      </Stack>

      <Stack align={{ base: "center", md: "start" }} spacing={3}>
        <Text color="brand.500" fontSize={{ base: "15px", md: "16px" }} fontWeight="700">
          Légal & Contact
        </Text>
        <Link alignItems="center" color="brand.500" display="inline-flex" fontSize={{ base: "14px", md: "15px" }} href="/mentions-legales" minH="44px">
          Mentions légales
        </Link>
        <Link alignItems="center" color="brand.500" display="inline-flex" fontSize={{ base: "14px", md: "15px" }} href="/accessibilite" minH="44px">
          Accessibilité
        </Link>
        <Link alignItems="center" color="brand.500" display="inline-flex" fontSize={{ base: "14px", md: "15px" }} href="/donnees-personnelles-cookies" minH="44px">
          Données personnelles et cookies
        </Link>
        <Link alignItems="center" color="brand.500" display="inline-flex" fontSize={{ base: "14px", md: "15px" }} href="/conditions-generales-utilisation" minH="44px">
          Conditions générales d'utilisation
        </Link>
        <Link alignItems="center" color="brand.500" display="inline-flex" fontSize={{ base: "14px", md: "15px" }} href="/contact" minH="44px">
          Contacts
        </Link>
      </Stack>
    </SimpleGrid>
  );
}
