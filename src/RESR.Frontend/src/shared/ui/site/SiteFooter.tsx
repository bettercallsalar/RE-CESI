import { Box, HStack, Link, SimpleGrid, Stack, Text } from "@chakra-ui/react";

function SocialBadge({ label }: { label: string }) {
  return (
    <Box
      alignItems="center"
      border="1px solid"
      borderColor="brand.500"
      color="brand.500"
      display="inline-flex"
      fontSize="10px"
      fontWeight="700"
      h="20px"
      justifyContent="center"
      minW="20px"
      px={1}
    >
      {label}
    </Box>
  );
}

export function SiteFooter() {
  return (
    <SimpleGrid borderTop="1px solid" borderColor="blackAlpha.100" columns={{ base: 1, md: 2 }} gap={8} mt={{ base: 14, md: 18 }} pt={8}>
      <Stack align={{ base: "center", md: "start" }} spacing={3}>
        <Text color="brand.500" fontSize="11px" fontWeight="600">
          Nous suivre sur les réseaux sociaux
        </Text>
        <HStack spacing={3}>
          <SocialBadge label="X" />
          <SocialBadge label="f" />
          <SocialBadge label="in" />
          <SocialBadge label="▶" />
        </HStack>
      </Stack>

      <Stack align={{ base: "center", md: "start" }} spacing={2}>
        <Text color="brand.500" fontSize="11px" fontWeight="600">
          Légal & Contact
        </Text>
        <Link color="brand.500" fontSize="10px">
          Mentions légales
        </Link>
        <Link color="brand.500" fontSize="10px">
          Données personnelles et cookies
        </Link>
        <Link color="brand.500" fontSize="10px">
          Conditions générales d'utilisation
        </Link>
        <Link color="brand.500" fontSize="10px">
          Contacts
        </Link>
      </Stack>
    </SimpleGrid>
  );
}
