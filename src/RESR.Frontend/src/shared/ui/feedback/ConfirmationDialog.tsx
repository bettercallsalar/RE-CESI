import {
  AlertDialog,
  AlertDialogBody,
  AlertDialogContent,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogOverlay,
  Button,
  Stack,
  Text
} from "@chakra-ui/react";
import { useRef } from "react";

interface ConfirmationDialogProps {
  isOpen: boolean;
  onClose: () => void;
  onConfirm: () => void | Promise<void>;
  title: string;
  description: string;
  confirmLabel: string;
  cancelLabel?: string;
  confirmColorScheme?: "red" | "brand";
  isLoading?: boolean;
  note?: string;
}

export function ConfirmationDialog({
  isOpen,
  onClose,
  onConfirm,
  title,
  description,
  confirmLabel,
  cancelLabel = "Annuler",
  confirmColorScheme = "red",
  isLoading = false,
  note
}: ConfirmationDialogProps) {
  const cancelRef = useRef<HTMLButtonElement | null>(null);
  const isDanger = confirmColorScheme === "red";

  return (
    <AlertDialog isCentered isOpen={isOpen} leastDestructiveRef={cancelRef} onClose={onClose}>
      <AlertDialogOverlay bg="blackAlpha.600" backdropFilter="blur(6px)">
        <AlertDialogContent
          bg="white"
          border="1px solid"
          borderColor={isDanger ? "red.100" : "canvas.200"}
          boxShadow="2xl"
          color="ink.800"
          mx={4}
          rounded="20px"
        >
          <Stack px={{ base: 5, md: 6 }} py={{ base: 5, md: 6 }} spacing={4}>
            <AlertDialogHeader color={isDanger ? "red.600" : "brand.500"} fontSize={{ base: "22px", md: "24px" }} fontWeight="800" px={0} py={0}>
              {title}
            </AlertDialogHeader>

            <AlertDialogBody pb={0} pt={0}>
              <Stack spacing={3}>
                <Text color="ink.800" fontSize={{ base: "15px", md: "16px" }} lineHeight="1.6">
                  {description}
                </Text>
                {note ? (
                  <Text color="ink.500" fontSize={{ base: "14px", md: "15px" }} lineHeight="1.6">
                    {note}
                  </Text>
                ) : null}
              </Stack>
            </AlertDialogBody>

            <AlertDialogFooter gap={3} pb={0} pt={0}>
              <Button ref={cancelRef} borderColor="canvas.300" color="ink.800" onClick={onClose} variant="outline">
                {cancelLabel}
              </Button>
              <Button
                _hover={isDanger ? { bg: "#9B2C2C" } : undefined}
                bg={isDanger ? "#C53030" : undefined}
                color={isDanger ? "surface.onCritical" : undefined}
                isLoading={isLoading}
                onClick={() => {
                  void onConfirm();
                }}
              >
                {confirmLabel}
              </Button>
            </AlertDialogFooter>
          </Stack>
        </AlertDialogContent>
      </AlertDialogOverlay>
    </AlertDialog>
  );
}
