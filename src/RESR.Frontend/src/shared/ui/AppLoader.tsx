import { Center, Spinner, Stack, Text } from "@chakra-ui/react";

interface AppLoaderProps {
  label?: string;
}

export function AppLoader({ label = "Loading" }: AppLoaderProps) {
  return (
    <Center minH="100vh">
      <Stack align="center" spacing={4}>
        <Spinner color="brand.500" size="xl" thickness="4px" />
        <Text color="ink.500" fontSize="12px">{label}</Text>
      </Stack>
    </Center>
  );
}
