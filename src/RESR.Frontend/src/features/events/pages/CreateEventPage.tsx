import { Box, Card, CardBody, Heading, ListItem, Stack, Text, UnorderedList } from "@chakra-ui/react";
import { SiteLayout } from "@/app/layouts/SiteLayout";
import { CreateEventForm } from "@/features/events/components/CreateEventForm";

export function CreateEventPage() {
  return (
    <SiteLayout headerVariant="authenticated">
      <Stack spacing={{ base: 6, md: 8 }}>
        <Box maxW="980px">
          <Heading color="ink.800" fontSize={{ base: "28px", md: "34px" }} lineHeight="1.15">
            Preparer un evenement utile
          </Heading>
          <Text color="ink.500" fontSize={{ base: "16px", md: "17px" }} mt={4}>
            Cette page permet de proposer un nouvel evenement depuis votre compte. Renseignez des horaires fiables, un lieu clair et les informations utiles aux participants.
          </Text>
        </Box>

        <Card bg="white" border="1px solid" borderColor="canvas.200" rounded="16px" shadow="sm">
          <CardBody p={{ base: 6, md: 7 }}>
            <Stack spacing={4}>
              <Heading color="brand.500" fontSize={{ base: "22px", md: "24px" }}>
                Rappels utiles
              </Heading>
              <UnorderedList color="ink.500" fontSize={{ base: "15px", md: "16px" }} lineHeight="1.7" pl={5} spacing={2}>
                <ListItem>Le titre doit permettre d'identifier l'evenement rapidement.</ListItem>
                <ListItem>Le sous-titre peut preciser le format ou le public cible.</ListItem>
                <ListItem>Ajoutez une adresse et un departement pour faciliter la recherche.</ListItem>
                <ListItem>Un evenement public reste en attente tant qu'il n'a pas ete valide.</ListItem>
              </UnorderedList>
            </Stack>
          </CardBody>
        </Card>

        <CreateEventForm />
      </Stack>
    </SiteLayout>
  );
}
