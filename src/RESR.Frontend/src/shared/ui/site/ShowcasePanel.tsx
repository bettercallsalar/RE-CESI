import { Box, Button, Heading, Stack } from "@chakra-ui/react";

interface ShowcasePanelProps {
  title: string;
  minHeight?: { base: string; md: string; lg?: string };
}

export function ShowcasePanel({ title, minHeight = { base: "220px", md: "280px", lg: "320px" } }: ShowcasePanelProps) {
  return (
    <Stack spacing={3}>
      <Heading color="ink.800" fontSize={{ base: "18px", sm: "20px", md: "22px" }} fontWeight="700">
        {title}
      </Heading>
      <Box
        bg="canvas.200"
        border="1px solid"
        borderColor="canvas.200"
        minH={minHeight}
        position="relative"
        rounded={{ base: "10px", md: "12px" }}
      >
        <Button
          bottom={{ base: 4, md: 5 }}
          fontSize={{ base: "14px", md: "15px" }}
          h="44px"
          minW="108px"
          position="absolute"
          right={{ base: 4, md: 5 }}
        >
          Voir +
        </Button>
      </Box>
    </Stack>
  );
}
