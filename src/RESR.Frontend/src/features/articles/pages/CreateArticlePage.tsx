import { Box, Card, CardBody, Heading, ListItem, Stack, Text, UnorderedList } from "@chakra-ui/react";
import { SiteLayout } from "@/app/layouts/SiteLayout";
import { CreateArticleForm } from "@/features/articles/components/CreateArticleForm";

export function CreateArticlePage() {
  return (
    <SiteLayout headerVariant="authenticated">
      <Stack spacing={{ base: 6, md: 8 }}>
        <Box maxW="980px">
          <Heading color="ink.800" fontSize={{ base: "28px", md: "34px" }} lineHeight="1.15">
            Préparer une publication claire
          </Heading>
          <Text color="ink.500" fontSize={{ base: "16px", md: "17px" }} mt={4}>
            Cette page permet de proposer un nouvel article depuis votre compte. Renseignez un titre précis, une catégorie cohérente et un contenu directement exploitable.
          </Text>
        </Box>

        <Card bg="white" border="1px solid" borderColor="canvas.200" rounded="16px" shadow="sm">
          <CardBody p={{ base: 6, md: 7 }}>
            <Stack spacing={4}>
              <Heading color="brand.500" fontSize={{ base: "22px", md: "24px" }}>
                Rappels utiles
              </Heading>
              <UnorderedList color="ink.500" fontSize={{ base: "15px", md: "16px" }} lineHeight="1.7" pl={5} spacing={2}>
                <ListItem>Le titre doit être court et compréhensible rapidement.</ListItem>
                <ListItem>La description sert d'accroche dans les listes publiques.</ListItem>
                <ListItem>Choisissez l'image par défaut qui sera utilisée dans les cartes publiques.</ListItem>
                <ListItem>Un article public reste en attente tant qu'il n'a pas été validé.</ListItem>
              </UnorderedList>
            </Stack>
          </CardBody>
        </Card>

        <CreateArticleForm />
      </Stack>
    </SiteLayout>
  );
}
