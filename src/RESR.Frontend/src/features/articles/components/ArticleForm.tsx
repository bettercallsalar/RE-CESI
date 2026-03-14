import {
  Box,
  Button,
  Card,
  CardBody,
  FormControl,
  FormLabel,
  Heading,
  Select,
  Skeleton,
  Stack,
  Text,
} from "@chakra-ui/react";
import { useEffect, useState } from "react";
import { RichTextEditor } from "@/shared/ui/forms/RichTextEditor";
import { MessageBanner } from "@/shared/ui/feedback/MessageBanner";
import { ResourceDescriptionField } from "@/shared/ui/forms/resource/ResourceDescriptionField";
import { ResourceImagesField } from "@/shared/ui/forms/resource/ResourceImagesField";
import { ResourceTitleField } from "@/shared/ui/forms/resource/ResourceTitleField";
import type { ArticleFormValues } from "@/features/articles/types/article.types";
import type { Category, ResourceFile } from "@/shared/types/article";
import type { FeedbackMessage } from "@/shared/ui/feedback/message.types";

interface ArticleFormProps {
  mode: "create" | "edit";
  values: ArticleFormValues;
  categories: Category[];
  isLoadingCategories: boolean;
  isSubmitting: boolean;
  message: FeedbackMessage | null;
  existingFiles?: ResourceFile[];
  updateField: <K extends keyof ArticleFormValues>(
    field: K,
    value: ArticleFormValues[K],
  ) => void;
  submit: () => Promise<void>;
}

export function ArticleForm({
  mode,
  values,
  categories,
  isLoadingCategories,
  isSubmitting,
  message,
  existingFiles = [],
  updateField,
  submit,
}: ArticleFormProps) {
  const [previewUrls, setPreviewUrls] = useState<
    Array<{ name: string; url: string }>
  >([]);

  useEffect(() => {
    const nextUrls = values.images.map((image) => ({
      name: image.name,
      url: URL.createObjectURL(image),
    }));

    setPreviewUrls(nextUrls);

    return () => {
      nextUrls.forEach((preview) => URL.revokeObjectURL(preview.url));
    };
  }, [values.images]);

  const isEditMode = mode === "edit";

  return (
    <Card
      bg="white"
      border="1px solid"
      borderColor="canvas.200"
      rounded={{ base: "12px", md: "16px" }}
      shadow="md">
      <CardBody p={{ base: 7, md: 8 }}>
        <Stack
          as="form"
          gap={4}
          onSubmit={(event) => {
            event.preventDefault();
            void submit();
          }}>
          <Box>
            <Heading
              color="brand.500"
              fontSize={{ base: "28px", md: "32px" }}
              lineHeight="1.15">
              {isEditMode ? "Modifier l'article" : "Créer un article"}
            </Heading>
            <Text
              color="ink.500"
              fontSize={{ base: "16px", md: "17px" }}
              mt={3}>
              {isEditMode
                ? "Mettez à jour le contenu, la catégorie ou la visibilité de votre article. Si vous ajoutez de nouvelles images, elles remplaceront les images actuelles."
                : "Rédigez votre contenu puis envoyez-le pour validation. Les articles publics deviennent visibles après approbation."}
            </Text>
          </Box>

          <ResourceTitleField
              value={values.title}
              onChange={(value) => updateField("title", value)}
              placeholder="Titre de l'article"
          />

          <ResourceDescriptionField
            value={values.description}
            onChange={(value) => updateField("description", value)}
            placeholder="Résumé visible dans les listes publiques"
          />

          <FormControl isRequired>
            <FormLabel
              color="ink.800"
              fontSize={{ base: "15px", md: "16px" }}
              fontWeight="700">
              Catégorie
            </FormLabel>
            {isLoadingCategories ? (
              <Skeleton height="48px" />
            ) : (
              <Select
                value={values.idCategory}
                onChange={(event) =>
                  updateField(
                    "idCategory",
                    event.target.value ? Number(event.target.value) : "",
                  )
                }>
                <option value="">Choisir une catégorie</option>
                {categories.map((category) => (
                  <option key={category.idCategory} value={category.idCategory}>
                    {category.name}
                  </option>
                ))}
              </Select>
            )}
          </FormControl>

          <FormControl isRequired>
            <FormLabel
              color="ink.800"
              fontSize={{ base: "15px", md: "16px" }}
              fontWeight="700">
              Visibilité
            </FormLabel>
            <Select
              value={values.visibility}
              onChange={(event) =>
                updateField(
                  "visibility",
                  event.target.value as "PUBLIC" | "PRIVATE",
                )
              }>
              <option value="PUBLIC">Public</option>
              <option value="PRIVATE">Privé</option>
            </Select>
          </FormControl>

          <ResourceImagesField
            existingFiles={
              isEditMode && previewUrls.length === 0 ? existingFiles : []
            }
            previewUrls={previewUrls}
            defaultImageSelection={values.defaultImageSelection}
            onDefaultImageSelectionChange={(value) =>
              updateField("defaultImageSelection", value)
            }
            onFilesChange={(files) => updateField("images", files)}
            existingLabel="Images actuelles"
            previewLabel={
              isEditMode ? "Nouvelles images envoyées" : "Aperçu des images"
            }
          />

          <FormControl isRequired>
            <FormLabel
              color="ink.800"
              fontSize={{ base: "15px", md: "16px" }}
              fontWeight="700">
              Contenu
            </FormLabel>
            <RichTextEditor
              helperText="Vous pouvez mettre en forme le texte avec du gras, de l'italique, des titres, des listes, des citations et des liens."
              minH={{ base: "180px", md: "240px" }}
              onChange={(nextValue) => updateField("content", nextValue)}
              placeholder="Rédigez ici le contenu complet de votre article"
              value={values.content}
            />
          </FormControl>

          {message ? (
            <MessageBanner
              message={message.message}
              title={message.title}
              tone={message.tone}
            />
          ) : null}

          <Button
            alignSelf={{ base: "stretch", md: "end" }}
            h="46px"
            isLoading={isSubmitting}
            loadingText={isEditMode ? "Enregistrement..." : "Envoi en cours"}
            minW={{ base: "100%", md: "220px" }}
            type="submit">
            {isEditMode ? "Enregistrer les changements" : "Envoyer l'article"}
          </Button>
        </Stack>
      </CardBody>
    </Card>
  );
}
