import { useRef, useState } from "react";
import {
  AlertDialog,
  AlertDialogBody,
  AlertDialogContent,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogOverlay,
  Box,
  Button,
  Checkbox,
  Heading,
  Stack,
  Text,
  useDisclosure
} from "@chakra-ui/react";
import { MessageBanner } from "@/shared/ui/feedback/MessageBanner";
import type { FeedbackMessage } from "@/shared/ui/feedback/message.types";

interface DeleteAccountSectionProps {
  isDeleting: boolean;
  message: FeedbackMessage | null;
  onDelete: () => Promise<void>;
}

export function DeleteAccountSection({ isDeleting, message, onDelete }: DeleteAccountSectionProps) {
  const { isOpen, onOpen, onClose } = useDisclosure();
  const cancelRef = useRef<HTMLButtonElement | null>(null);
  const [isConfirmed, setIsConfirmed] = useState(false);

  async function handleDelete() {
    await onDelete();
    onClose();
    setIsConfirmed(false);
  }

  return (
    <Stack bg="white" border="1px solid" borderColor="canvas.200" rounded="16px" spacing={5} p={{ base: 6, md: 8 }}>
      <Box>
        <Heading color="ink.800" fontSize={{ base: "24px", md: "28px" }}>
          Supprimer mon compte
        </Heading>
        <Text color="ink.500" fontSize={{ base: "15px", md: "16px" }} mt={3}>
          Cette action est définitive. Une fois votre compte supprimé, il ne pourra pas être restauré.
        </Text>
      </Box>

      {message ? (
        <MessageBanner message={message.message} title={message.title} tone={message.tone} />
      ) : null}

      <Button alignSelf="start" onClick={onOpen} variant="outline">
        Supprimer définitivement
      </Button>

      <AlertDialog isCentered isOpen={isOpen} leastDestructiveRef={cancelRef} onClose={onClose}>
        <AlertDialogOverlay>
          <AlertDialogContent>
            <AlertDialogHeader fontSize="lg" fontWeight="700">
              Confirmation de suppression
            </AlertDialogHeader>

            <AlertDialogBody>
              <Stack spacing={4}>
                <Text>
                  Voulez-vous vraiment supprimer votre compte ? Cette suppression est irréversible.
                </Text>
                <Checkbox isChecked={isConfirmed} onChange={(event) => setIsConfirmed(event.target.checked)}>
                  Je confirme que mon compte ne pourra pas être restauré.
                </Checkbox>
              </Stack>
            </AlertDialogBody>

            <AlertDialogFooter gap={3}>
              <Button ref={cancelRef} onClick={onClose} variant="outline">
                Annuler
              </Button>
              <Button
                bg="red.600"
                color="surface.onCritical"
                isDisabled={!isConfirmed}
                isLoading={isDeleting}
                loadingText="Suppression"
                onClick={() => {
                  void handleDelete();
                }}
                _hover={{ bg: "red.700", opacity: 0.96 }}
              >
                Supprimer
              </Button>
            </AlertDialogFooter>
          </AlertDialogContent>
        </AlertDialogOverlay>
      </AlertDialog>
    </Stack>
  );
}
