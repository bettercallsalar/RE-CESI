import { Button, HStack, Text } from "@chakra-ui/react";

interface PaginationControlsProps {
  page: number;
  totalPages: number;
  isLoading: boolean;
  onPrevious: () => void;
  onNext: () => void;
}

export function PaginationControls({
  page,
  totalPages,
  isLoading,
  onPrevious,
  onNext
}: PaginationControlsProps) {
  if (totalPages <= 1) {
    return null;
  }

  return (
    <HStack justify="space-between" pt={6} spacing={4}>
      <Button isDisabled={isLoading || page <= 1} onClick={onPrevious} variant="outline">
        Page precedente
      </Button>
      <Text color="ink.500" fontSize={{ base: "14px", md: "15px" }}>
        Page {page} sur {totalPages}
      </Text>
      <Button isDisabled={isLoading || page >= totalPages} onClick={onNext} variant="outline">
        Page suivante
      </Button>
    </HStack>
  );
}
