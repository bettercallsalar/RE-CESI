import { Badge, Box, Button, HStack, Stack, Text } from "@chakra-ui/react";

interface PendingResourceCardProps {
  kind: "Article" | "Evenement";
  href: string;
  title: string;
  description: string | null;
  authorLabel: string;
  createdAtLabel: string;
  visibilityLabel: string;
  extraDetails?: string;
}

export function PendingResourceCard({
  kind,
  href,
  title,
  description,
  authorLabel,
  createdAtLabel,
  visibilityLabel,
  extraDetails
}: PendingResourceCardProps) {
  return (
    <Stack bg="white" border="1px solid" borderColor="canvas.200" borderRadius="16px" minH="220px" p={{ base: 5, md: 6 }} spacing={5}>
      <Stack spacing={3}>
        <HStack align="start" justify="space-between" spacing={4}>
          <Badge bg="#FEEBC8" color="#9C4221" fontSize="12px" px={2.5} py={1} rounded="full">
            {kind} en attente
          </Badge>
          <Text color="ink.500" fontSize={{ base: "13px", md: "14px" }}>
            {createdAtLabel}
          </Text>
        </HStack>

        <Stack spacing={1.5}>
          <Text color="ink.800" fontSize={{ base: "20px", md: "22px" }} fontWeight="700">
            {title}
          </Text>
          <Text color="ink.500" fontSize={{ base: "15px", md: "16px" }}>
            Par {authorLabel}
          </Text>
          <Text color="ink.500" fontSize={{ base: "15px", md: "16px" }}>
            Visibilite {visibilityLabel}
          </Text>
          {extraDetails ? (
            <Text color="ink.500" fontSize={{ base: "15px", md: "16px" }}>
              {extraDetails}
            </Text>
          ) : null}
        </Stack>

        <Text color="ink.500" fontSize={{ base: "15px", md: "16px" }} minH="48px">
          {description || "Aucune description fournie."}
        </Text>
      </Stack>

      <HStack justify="space-between" spacing={4}>
        <Box>
          <Text color="brand.500" fontSize={{ base: "14px", md: "15px" }} fontWeight="700">
            Validation requise
          </Text>
        </Box>
        <Button as="a" href={href}>
          Voir le detail
        </Button>
      </HStack>
    </Stack>
  );
}
