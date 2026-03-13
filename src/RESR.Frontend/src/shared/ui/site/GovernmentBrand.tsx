import { Box, Image, Stack, Text } from "@chakra-ui/react";

export function GovernmentBrand() {
  return (
    <Stack align="start" spacing={1.5}>
      <Image
        alt="République Française"
        h={{ base: "24px", md: "28px" }}
        objectFit="contain"
        src="/logo_fr.png"
        w={{ base: "56px", md: "64px" }}
      />
      <Box color="ink.900">
        <Text
          fontSize={{ base: "8px", md: "9px" }}
          fontWeight="700"
          letterSpacing="0.03em"
          lineHeight="1.05"
          maxW={{ base: "78px", md: "92px" }}
          textTransform="uppercase">
          Ministère des Solidarités et de la Santé
        </Text>
        <Text
          color="ink.500"
          fontFamily="Georgia, serif"
          fontSize={{ base: "8px", md: "9px" }}
          fontStyle="italic"
          lineHeight="1.15"
          mt={1}>
          Liberté
          <br />
          Égalité
          <br />
          Fraternité
        </Text>
      </Box>
    </Stack>
  );
}
