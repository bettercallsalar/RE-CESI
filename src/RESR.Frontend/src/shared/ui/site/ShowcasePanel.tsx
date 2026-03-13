import { Box, Button, Heading, Stack } from "@chakra-ui/react";

interface ShowcasePanelProps {
  title: string;
  minHeight?: { base: string; md: string; lg?: string };
}

export function ShowcasePanel({ title, minHeight = { base: "220px", md: "280px", lg: "320px" } }: ShowcasePanelProps) {
  return (
    <Stack spacing={2}>
      <Heading color="ink.900" fontSize={{ base: "11px", md: "12px" }} fontWeight="600">
        {title}
      </Heading>
      <Box bg="#d8d8d8" minH={minHeight} position="relative">
        <Button
          bottom={3}
          fontSize="10px"
          h="22px"
          minW="58px"
          position="absolute"
          right={0}
          size="xs"
        >
          Voir +
        </Button>
      </Box>
    </Stack>
  );
}
