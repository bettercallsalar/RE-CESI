import { Box, Card, CardBody, Heading, ListItem, Stack, Text, UnorderedList } from "@chakra-ui/react";
import { SiteLayout } from "@/app/layouts/SiteLayout";
import { EditEventForm } from "@/features/events/components/EditEventForm";

interface EditEventPageProps {
  idResource: number;
}

export function EditEventPage({ idResource }: EditEventPageProps) {
  return (
    <SiteLayout headerVariant="authenticated">
      <Stack spacing={{ base: 6, md: 8 }}>
        <Box maxW="980px">
          <Heading color="ink.800" fontSize={{ base: "28px", md: "34px" }} lineHeight="1.15">
            Mettre a jour votre evenement
          </Heading>
          <Text color="ink.500" fontSize={{ base: "16px", md: "17px" }} mt={4}>
            Vous pouvez corriger les informations pratiques, ajuster les dates, changer la categorie ou remplacer les images de l'evenement.
          </Text>
        </Box>

        <Card bg="white" border="1px solid" borderColor="canvas.200" rounded="16px" shadow="sm">
          <CardBody p={{ base: 6, md: 7 }}>
            <Stack spacing={4}>
              <Heading color="brand.500" fontSize={{ base: "22px", md: "24px" }}>
                Conseils avant enregistrement
              </Heading>
              <UnorderedList color="ink.500" fontSize={{ base: "15px", md: "16px" }} lineHeight="1.7" pl={5} spacing={2}>
                <ListItem>Verifiez que les horaires et l'adresse correspondent encore a la realite.</ListItem>
                <ListItem>Le sous-titre doit completer le titre sans le repeter.</ListItem>
                <ListItem>Le departement aide les utilisateurs a retrouver votre evenement.</ListItem>
                <ListItem>Si vous envoyez de nouvelles images, elles remplaceront l'ensemble des images existantes.</ListItem>
              </UnorderedList>
            </Stack>
          </CardBody>
        </Card>

        <EditEventForm idResource={idResource} />
      </Stack>
    </SiteLayout>
  );
}
