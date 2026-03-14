import {
  Box,
  Button,
  FormControl,
  FormLabel,
  HStack,
  Image,
  Input,
  SimpleGrid,
  Stack,
  Text,
} from "@chakra-ui/react";
import type { ResourceFile } from "@/shared/types/article";
import { getResourceFileUrl } from "@/shared/lib/assets/getResourceFileUrl";

interface ResourceImagesFieldProps {
  existingFiles?: ResourceFile[];
  previewUrls: Array<{ name: string; url: string }>;
  defaultImageSelection: string;
  onDefaultImageSelectionChange: (value: string) => void;
  onFilesChange: (files: File[]) => void;
  existingLabel?: string;
  previewLabel?: string;
}

export function ResourceImagesField({
  existingFiles = [],
  previewUrls,
  defaultImageSelection,
  onDefaultImageSelectionChange,
  onFilesChange,
  existingLabel = "Images actuelles",
  previewLabel = "Aperçu des images",
}: ResourceImagesFieldProps) {
  const showExistingFiles = existingFiles.length > 0 && previewUrls.length === 0;
  const totalVisibleImages =
    (showExistingFiles ? existingFiles.length : 0) + previewUrls.length;

  return (
    <FormControl>
      <FormLabel
        color="ink.800"
        fontSize={{ base: "15px", md: "16px" }}
        fontWeight="700">
        Images
      </FormLabel>
      <Input
        accept="image/*"
        multiple
        onChange={(event) => onFilesChange(Array.from(event.target.files ?? []))}
        p={1.5}
        type="file"
      />
      <Text color="ink.500" fontSize={{ base: "13px", md: "14px" }} mt={2}>
        Jusqu'à 6 images, 5 Mo maximum par image.
      </Text>
      {totalVisibleImages > 1 ? (
        <Text
          color="ink.800"
          fontSize={{ base: "14px", md: "15px" }}
          fontWeight="700"
          mt={3}>
          Choisissez l'image par défaut si plusieurs images sont présentes.
        </Text>
      ) : null}

      {showExistingFiles ? (
        <Box mt={4}>
          <Text
            color="ink.800"
            fontSize={{ base: "14px", md: "15px" }}
            fontWeight="700"
            mb={3}>
            {existingLabel}
          </Text>
          <SimpleGrid columns={{ base: 2, md: 3 }} spacing={4}>
            {existingFiles.map((file) => {
              const isDefault = defaultImageSelection === `existing:${file.idFile}`;

              return (
                <Box
                  bg="white"
                  border="2px solid"
                  borderColor={isDefault ? "brand.500" : "canvas.200"}
                  key={file.idFile}
                  overflow="hidden"
                  rounded="12px">
                  <Image
                    alt={file.originalName}
                    h="120px"
                    objectFit="cover"
                    src={getResourceFileUrl(file.path)}
                    w="100%"
                  />
                  <Stack px={3} py={3} spacing={3}>
                    <Text color="ink.500" fontSize="12px" wordBreak="break-word">
                      {file.originalName}
                    </Text>
                    <HStack justify="space-between" spacing={3}>
                      <Button
                        onClick={() =>
                          onDefaultImageSelectionChange(`existing:${file.idFile}`)
                        }
                        size="sm"
                        variant={isDefault ? "solid" : "outline"}>
                        {isDefault ? "Image par défaut" : "Définir par défaut"}
                      </Button>
                    </HStack>
                  </Stack>
                </Box>
              );
            })}
          </SimpleGrid>
        </Box>
      ) : null}

      {previewUrls.length > 0 ? (
        <Box mt={4}>
          <Text
            color="ink.800"
            fontSize={{ base: "14px", md: "15px" }}
            fontWeight="700"
            mb={3}>
            {previewLabel}
          </Text>
          <SimpleGrid columns={{ base: 2, md: 3 }} spacing={4}>
            {previewUrls.map((preview, index) => {
              const isDefault = defaultImageSelection === `new:${index}`;

              return (
                <Box
                  bg="white"
                  border="2px solid"
                  borderColor={isDefault ? "brand.500" : "canvas.200"}
                  key={preview.name}
                  overflow="hidden"
                  rounded="12px">
                  <Image
                    alt={preview.name}
                    h="120px"
                    objectFit="cover"
                    src={preview.url}
                    w="100%"
                  />
                  <Stack px={3} py={3} spacing={3}>
                    <Text color="ink.500" fontSize="12px" wordBreak="break-word">
                      {preview.name}
                    </Text>
                    <HStack justify="space-between" spacing={3}>
                      <Button
                        onClick={() => onDefaultImageSelectionChange(`new:${index}`)}
                        size="sm"
                        variant={isDefault ? "solid" : "outline"}>
                        {isDefault ? "Image par défaut" : "Définir par défaut"}
                      </Button>
                    </HStack>
                  </Stack>
                </Box>
              );
            })}
          </SimpleGrid>
        </Box>
      ) : null}
    </FormControl>
  );
}
