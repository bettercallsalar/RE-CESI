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
  Text,
  Textarea
} from "@chakra-ui/react";
import { useCreateArticleForm } from "@/features/articles/hooks/useCreateArticleForm";
import { MessageBanner } from "@/shared/ui/feedback/MessageBanner";
import { RichTextEditor } from "@/shared/ui/forms/RichTextEditor";

export function CreateArticleForm() {
  const { values, categories, isLoadingCategories, isSubmitting, message, updateField, submit } = useCreateArticleForm();

  return (
    <Card bg="white" border="1px solid" borderColor="canvas.200" rounded={{ base: "12px", md: "16px" }} shadow="md">
      <CardBody p={{ base: 7, md: 8 }}>
        <Stack as="form" gap={4} onSubmit={(event) => {
          event.preventDefault();
          void submit();
        }}>
          <Box>
            <Heading color="brand.500" fontSize={{ base: "28px", md: "32px" }} lineHeight="1.15">
              Créer un article
            </Heading>
            <Text color="ink.500" fontSize={{ base: "16px", md: "17px" }} mt={3}>
              Rédigez votre contenu puis envoyez-le pour validation. Les articles publics deviennent visibles après approbation.
            </Text>
          </Box>

          <FormControl isRequired>
            <FormLabel color="ink.800" fontSize={{ base: "15px", md: "16px" }} fontWeight="700">
              Titre
            </FormLabel>
            <Input
              value={values.title}
              onChange={(event) => updateField("title", event.target.value)}
              placeholder="Titre de l'article"
            />
          </FormControl>

          <FormControl>
            <FormLabel color="ink.800" fontSize={{ base: "15px", md: "16px" }} fontWeight="700">
              Description
            </FormLabel>
            <Textarea
              minH="120px"
              value={values.description}
              onChange={(event) => updateField("description", event.target.value)}
              placeholder="Résumé visible dans les listes publiques"
            />
          </FormControl>

          <FormControl isRequired>
            <FormLabel color="ink.800" fontSize={{ base: "15px", md: "16px" }} fontWeight="700">
              Catégorie
            </FormLabel>
            {isLoadingCategories ? (
              <Skeleton height="48px" />
            ) : (
              <Select value={values.idCategory} onChange={(event) => updateField("idCategory", event.target.value ? Number(event.target.value) : "")}>
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
            <FormLabel color="ink.800" fontSize={{ base: "15px", md: "16px" }} fontWeight="700">
              Visibilité
            </FormLabel>
            <Select value={values.visibility} onChange={(event) => updateField("visibility", event.target.value as "PUBLIC" | "PRIVATE")}>
              <option value="PUBLIC">Public</option>
              <option value="PRIVATE">Privé</option>
            </Select>
          </FormControl>

          <FormControl isRequired>
            <FormLabel color="ink.800" fontSize={{ base: "15px", md: "16px" }} fontWeight="700">
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

          {message ? <MessageBanner message={message.message} title={message.title} tone={message.tone} /> : null}

          <Button alignSelf={{ base: "stretch", md: "end" }} h="46px" isLoading={isSubmitting} loadingText="Envoi en cours" minW={{ base: "100%", md: "190px" }} type="submit">
            Envoyer l'article
          </Button>
        </Stack>
      </CardBody>
    </Card>
  );
}
