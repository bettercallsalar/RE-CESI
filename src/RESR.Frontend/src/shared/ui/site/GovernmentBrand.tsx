import { Box, Image, Link, Stack, Text } from "@chakra-ui/react";

export function GovernmentBrand() {
  return (
    <Link
      aria-label="Retour à l'accueil"
      href="/"
      _hover={{ textDecoration: "none" }}
      _focusVisible={{ boxShadow: "outline", rounded: "8px" }}
    >
      <Stack align="start" spacing={{ base: 2, md: 2.5 }}>
        <Image
          alt="République Française"
          h={{ base: "34px", sm: "40px", md: "48px" }}
          objectFit="contain"
          src="/logo_fr.png"
          w={{ base: "82px", sm: "96px", md: "112px" }}
        />
        <Box color="ink.900">
          <Text
            fontSize={{ base: "10px", sm: "11px", md: "12px" }}
            fontWeight="700"
            letterSpacing="0.03em"
            lineHeight="1.05"
            maxW={{ base: "116px", sm: "132px", md: "148px" }}
            textTransform="uppercase">
            Ministère des Solidarités et de la Santé
          </Text>
          <Text
            color="ink.500"
            fontFamily="Georgia, serif"
            fontSize={{ base: "10px", sm: "11px", md: "12px" }}
            fontStyle="italic"
            lineHeight="1.2"
            mt={1}>
            Liberté
            <br />
            Égalité
            <br />
            Fraternité
          </Text>
        </Box>
      </Stack>
    </Link>
  );
}
