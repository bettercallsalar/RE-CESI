import { useRef, useState } from "react";
import {
  Alert,
  AlertDialog,
  AlertDialogBody,
  AlertDialogContent,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogOverlay,
  AlertIcon,
  Box,
  Button,
  Checkbox,
  Heading,
  Stack,
  Text,
  useDisclosure
} from "@chakra-ui/react";

interface DeleteAccountSectionProps {
  isDeleting: boolean;
  error: string | null;
  onDelete: () => Promise<void>;
}

export function DeleteAccountSection({ isDeleting, error, onDelete }: DeleteAccountSectionProps) {
  const { isOpen, onOpen, onClose } = useDisclosure();
  const cancelRef = useRef<HTMLButtonElement | null>(null);
  const [isConfirmed, setIsConfirmed] = useState(false);

  async function handleDelete() {
    await onDelete();
    onClose();
    setIsConfirmed(false);
  }

  return (
    <Stack bg="white" border="1px solid" borderColor="red.100" rounded="16px" spacing={5} p={{ base: 6, md: 8 }}>
      <Box>
        <Heading color="red.600" fontSize={{ base: "24px", md: "28px" }}>
          Supprimer mon compte
        </Heading>
        <Text color="ink.500" fontSize={{ base: "15px", md: "16px" }} mt={3}>
          Cette action est définitive. Une fois votre compte supprimé, il ne pourra pas être restauré.
        </Text>
      </Box>

      {error ? (
        <Alert borderRadius="8px" status="error">
          <AlertIcon />
          {error}
        </Alert>
      ) : null}

      <Button alignSelf="start" colorScheme="red" onClick={onOpen} variant="outline">
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
                colorScheme="red"
                isDisabled={!isConfirmed}
                isLoading={isDeleting}
                loadingText="Suppression"
                onClick={() => {
                  void handleDelete();
                }}
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
