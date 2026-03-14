import { Button, HStack, Stack, Text, Textarea } from "@chakra-ui/react";
import { useState } from "react";

interface CommentComposerProps {
  label: string;
  placeholder: string;
  submitLabel: string;
  isDisabled?: boolean;
  isSubmitting?: boolean;
  onSubmit: (content: string) => Promise<boolean>;
  onCancel?: () => void;
  autoFocus?: boolean;
  minHeight?: string;
}

export function CommentComposer({
  label,
  placeholder,
  submitLabel,
  isDisabled = false,
  isSubmitting = false,
  onSubmit,
  onCancel,
  autoFocus = false,
  minHeight = "132px"
}: CommentComposerProps) {
  const [content, setContent] = useState("");
  const remainingCharacters = 2000 - content.length;
  const trimmedContent = content.trim();

  async function handleSubmit() {
    if (!trimmedContent) {
      return;
    }

    const isSuccessful = await onSubmit(trimmedContent);

    if (isSuccessful) {
      setContent("");
    }
  }

  return (
    <Stack bg="canvas.100" border="1px solid" borderColor="canvas.200" p={{ base: 4, md: 5 }} rounded="18px" spacing={3}>
      <Text color="ink.800" fontSize={{ base: "15px", md: "16px" }} fontWeight="700">
        {label}
      </Text>

      <Textarea
        aria-label={label}
        autoFocus={autoFocus}
        bg="white"
        borderColor="canvas.300"
        maxLength={2000}
        minH={minHeight}
        onChange={(event) => setContent(event.target.value)}
        placeholder={placeholder}
        resize="vertical"
        value={content}
      />

      <HStack justify="space-between" spacing={3} wrap="wrap">
        <Text color={remainingCharacters <= 120 ? "orange.500" : "ink.500"} fontSize="13px">
          {remainingCharacters} caracteres restants
        </Text>

        <HStack spacing={3}>
          {onCancel ? (
            <Button isDisabled={isDisabled} onClick={onCancel} size="sm" variant="ghost">
              Annuler
            </Button>
          ) : null}

          <Button
            isDisabled={isDisabled || !trimmedContent}
            isLoading={isSubmitting}
            onClick={() => void handleSubmit()}
            size="sm"
          >
            {submitLabel}
          </Button>
        </HStack>
      </HStack>
    </Stack>
  );
}
