import type { PropsWithChildren } from "react";
import { ChakraProvider } from "@chakra-ui/react";
import { theme } from "@/app/theme";
import { AuthProvider } from "@/features/auth/providers/AuthProvider";

export function AppProviders({ children }: PropsWithChildren) {
  return (
    <ChakraProvider theme={theme}>
      <AuthProvider>{children}</AuthProvider>
    </ChakraProvider>
  );
}
