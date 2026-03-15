import { useId } from "react";
import { FormControl, FormLabel, HStack, Stack, Switch, Text } from "@chakra-ui/react";
import { useAccessibility } from "@/features/accessibility/providers/AccessibilityProvider";

interface AccessibilityModeToggleProps {
  compact?: boolean;
}

export function AccessibilityModeToggle({ compact = false }: AccessibilityModeToggleProps) {
  const id = useId();
  const { isContrastMode, setContrastMode } = useAccessibility();

  return (
    <FormControl display="block">
      <HStack
        align="center"
        bg={compact ? "canvas.100" : "white"}
        border="1px solid"
        borderColor="canvas.200"
        borderRadius={compact ? "12px" : "16px"}
        justify="space-between"
        minH={compact ? "44px" : "64px"}
        px={compact ? 3 : 4}
        py={compact ? 2 : 3}
        spacing={4}
      >
        <Stack minW={0} spacing={compact ? 0.5 : 1}>
          <FormLabel color="ink.800" fontSize={compact ? "14px" : "15px"} fontWeight="700" htmlFor={id} m={0}>
            Contraste renforce
          </FormLabel>
          {!compact ? (
            <Text color="ink.500" fontSize="14px" lineHeight="1.5">
              Renforce les contrastes et facilite la distinction visuelle des elements interactifs.
            </Text>
          ) : null}
        </Stack>

        <Switch
          colorScheme="brand"
          id={id}
          isChecked={isContrastMode}
          onChange={(event) => setContrastMode(event.target.checked)}
          size="md"
        />
      </HStack>
    </FormControl>
  );
}
