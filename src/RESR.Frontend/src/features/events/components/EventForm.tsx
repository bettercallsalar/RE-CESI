import {
  Box,
  Button,
  Card,
  CardBody,
  FormControl,
  FormLabel,
  Heading,
  Input,
  Select,
  Skeleton,
  Stack,
  Text
} from "@chakra-ui/react";
import { useEffect, useState } from "react";
import { ResourceDescriptionField } from "@/shared/ui/forms/resource/ResourceDescriptionField";
import { ResourceImagesField } from "@/shared/ui/forms/resource/ResourceImagesField";
import { ResourceTitleField } from "@/shared/ui/forms/resource/ResourceTitleField";
import { MessageBanner } from "@/shared/ui/feedback/MessageBanner";
import type { EventFormValues } from "@/features/events/types/event.types";
import type { Category, ResourceFile } from "@/shared/types/article";
import type { Department } from "@/shared/types/user";
import type { FeedbackMessage } from "@/shared/ui/feedback/message.types";

interface EventFormProps {
  mode: "create" | "edit";
  values: EventFormValues;
  categories: Category[];
  departments: Department[];
  isLoadingCategories: boolean;
  isLoadingDepartments: boolean;
  isSubmitting: boolean;
  message: FeedbackMessage | null;
  existingFiles?: ResourceFile[];
  updateField: <K extends keyof EventFormValues>(field: K, value: EventFormValues[K]) => void;
  submit: () => Promise<void>;
}

export function EventForm({
  mode,
  values,
  categories,
  departments,
  isLoadingCategories,
  isLoadingDepartments,
  isSubmitting,
  message,
  existingFiles = [],
  updateField,
  submit
}: EventFormProps) {
  const [previewUrls, setPreviewUrls] = useState<Array<{ name: string; url: string }>>([]);

  useEffect(() => {
    const nextUrls = values.images.map((image) => ({
      name: image.name,
      url: URL.createObjectURL(image)
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
      shadow="md"
    >
      <CardBody p={{ base: 7, md: 8 }}>
        <Stack
          as="form"
          gap={4}
          onSubmit={(event) => {
            event.preventDefault();
            void submit();
          }}
        >
          <Box>
            <Heading color="brand.500" fontSize={{ base: "28px", md: "32px" }} lineHeight="1.15">
              {isEditMode ? "Modifier l'evenement" : "Creer un evenement"}
            </Heading>
            <Text color="ink.500" fontSize={{ base: "16px", md: "17px" }} mt={3}>
              {isEditMode
                ? "Mettez a jour les dates, le lieu, la categorie ou la visibilite de votre evenement. Si vous ajoutez de nouvelles images, elles remplaceront les images actuelles."
                : "Renseignez les informations utiles a la participation puis envoyez votre evenement pour validation."}
            </Text>
          </Box>

          <ResourceTitleField
            onChange={(value) => updateField("title", value)}
            placeholder="Titre de l'evenement"
            value={values.title}
          />

          <ResourceDescriptionField
            onChange={(value) => updateField("description", value)}
            placeholder="Resume visible dans les listes publiques"
            value={values.description}
          />

          <FormControl>
            <FormLabel color="ink.800" fontSize={{ base: "15px", md: "16px" }} fontWeight="700">
              Sous-titre
            </FormLabel>
            <Input placeholder="Exemple: forum metier, atelier, conference..." value={values.subtitle} onChange={(event) => updateField("subtitle", event.target.value)} />
          </FormControl>

          <Stack direction={{ base: "column", md: "row" }} spacing={4}>
            <FormControl isRequired>
              <FormLabel color="ink.800" fontSize={{ base: "15px", md: "16px" }} fontWeight="700">
                Debut
              </FormLabel>
              <Input type="datetime-local" value={values.startDate} onChange={(event) => updateField("startDate", event.target.value)} />
            </FormControl>

            <FormControl>
              <FormLabel color="ink.800" fontSize={{ base: "15px", md: "16px" }} fontWeight="700">
                Fin
              </FormLabel>
              <Input type="datetime-local" value={values.endDate} onChange={(event) => updateField("endDate", event.target.value)} />
            </FormControl>
          </Stack>

          <FormControl>
            <FormLabel color="ink.800" fontSize={{ base: "15px", md: "16px" }} fontWeight="700">
              Adresse
            </FormLabel>
            <Input placeholder="Lieu ou adresse complete" value={values.address} onChange={(event) => updateField("address", event.target.value)} />
          </FormControl>

          <Stack direction={{ base: "column", md: "row" }} spacing={4}>
            <FormControl isRequired>
              <FormLabel color="ink.800" fontSize={{ base: "15px", md: "16px" }} fontWeight="700">
                Categorie
              </FormLabel>
              {isLoadingCategories ? (
                <Skeleton height="48px" />
              ) : (
                <Select
                  value={values.idCategory}
                  onChange={(event) => updateField("idCategory", event.target.value ? Number(event.target.value) : "")}
                >
                  <option value="">Choisir une categorie</option>
                  {categories.map((category) => (
                    <option key={category.idCategory} value={category.idCategory}>
                      {category.name}
                    </option>
                  ))}
                </Select>
              )}
            </FormControl>

            <FormControl>
              <FormLabel color="ink.800" fontSize={{ base: "15px", md: "16px" }} fontWeight="700">
                Departement
              </FormLabel>
              {isLoadingDepartments ? (
                <Skeleton height="48px" />
              ) : (
                <Select
                  value={values.idDepartment}
                  onChange={(event) => updateField("idDepartment", event.target.value ? Number(event.target.value) : "")}
                >
                  <option value="">Choisir un departement</option>
                  {departments.map((department) => (
                    <option key={department.idDepartment} value={department.idDepartment}>
                      {department.code} - {department.name}
                    </option>
                  ))}
                </Select>
              )}
            </FormControl>
          </Stack>

          <FormControl isRequired>
            <FormLabel color="ink.800" fontSize={{ base: "15px", md: "16px" }} fontWeight="700">
              Visibilite
            </FormLabel>
            <Select value={values.visibility} onChange={(event) => updateField("visibility", event.target.value as "PUBLIC" | "PRIVATE")}>
              <option value="PUBLIC">Public</option>
              <option value="PRIVATE">Prive</option>
            </Select>
          </FormControl>

          <ResourceImagesField
            defaultImageSelection=""
            enableDefaultSelection={false}
            existingFiles={isEditMode && previewUrls.length === 0 ? existingFiles : []}
            existingLabel="Images actuelles"
            onDefaultImageSelectionChange={() => undefined}
            onFilesChange={(files) => updateField("images", files)}
            previewLabel={isEditMode ? "Nouvelles images envoyees" : "Apercu des images"}
            previewUrls={previewUrls}
          />

          {message ? <MessageBanner message={message.message} title={message.title} tone={message.tone} /> : null}

          <Button
            alignSelf={{ base: "stretch", md: "end" }}
            h="46px"
            isLoading={isSubmitting}
            loadingText={isEditMode ? "Enregistrement..." : "Envoi en cours"}
            minW={{ base: "100%", md: "220px" }}
            type="submit"
          >
            {isEditMode ? "Enregistrer les changements" : "Envoyer l'evenement"}
          </Button>
        </Stack>
      </CardBody>
    </Card>
  );
}
