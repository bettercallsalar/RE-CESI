import { Box, Card, CardBody, Heading, ListItem, Stack, Text, UnorderedList } from "@chakra-ui/react";
import { SiteLayout } from "@/app/layouts/SiteLayout";
import { EditArticleForm } from "@/features/articles/components/EditArticleForm";

interface EditArticlePageProps {
  idResource: number;
}

export function EditArticlePage({ idResource }: EditArticlePageProps) {
  return (
    <SiteLayout headerVariant="authenticated">
      <Stack spacing={{ base: 6, md: 8 }}>
        <Box maxW="980px">
          <Heading color="ink.800" fontSize={{ base: "28px", md: "34px" }} lineHeight="1.15">
            Mettre à jour votre publication
          </Heading>
          <Text color="ink.500" fontSize={{ base: "16px", md: "17px" }} mt={4}>
            Vous pouvez corriger le texte, modifier la catégorie, changer l'image par défaut ou remplacer les images de l'article.
          </Text>
        </Box>

        <Card bg="white" border="1px solid" borderColor="canvas.200" rounded="16px" shadow="sm">
          <CardBody p={{ base: 6, md: 7 }}>
            <Stack spacing={4}>
              <Heading color="brand.500" fontSize={{ base: "22px", md: "24px" }}>
                Conseils avant enregistrement
              </Heading>
              <UnorderedList color="ink.500" fontSize={{ base: "15px", md: "16px" }} lineHeight="1.7" pl={5} spacing={2}>
                <ListItem>Vérifiez que le titre reste court et compréhensible.</ListItem>
                <ListItem>La description doit rester utile dans les listes publiques.</ListItem>
                <ListItem>Vous pouvez changer l'image par défaut sans remplacer toutes les images.</ListItem>
                <ListItem>Si vous envoyez de nouvelles images, elles remplaceront l'ensemble des images existantes.</ListItem>
              </UnorderedList>
            </Stack>
          </CardBody>
        </Card>

        <EditArticleForm idResource={idResource} />
      </Stack>
    </SiteLayout>
  );
}
