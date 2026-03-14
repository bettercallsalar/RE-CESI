import { Box, Button, SimpleGrid, Skeleton, Stack, Text } from "@chakra-ui/react";
import { SiteLayout } from "@/app/layouts/SiteLayout";
import { PendingResourceCard } from "@/features/admin/components/PendingResourceCard";
import { usePendingEventsPage } from "@/features/admin/hooks/usePendingEventsPage";
import { formatEventDateRange } from "@/features/events/lib/eventDates";
import { formatPublicationDate } from "@/shared/lib/dates/formatPublicationDate";
import { MessageBanner } from "@/shared/ui/feedback/MessageBanner";

export function PendingEventsPage() {
  const { events, isLoading, message } = usePendingEventsPage();

  return (
    <SiteLayout
      headerVariant="authenticated"
      intro={
        <>
          <Text fontSize={{ base: "20px", sm: "24px", md: "30px" }} fontWeight="700" textAlign="center">
            Evenements en attente de validation
          </Text>
          <Text color="ink.500" fontSize={{ base: "16px", sm: "17px", md: "18px" }} maxW="760px" textAlign="center">
            Consultez tous les evenements non approuves, puis ouvrez leur detail pour les approuver ou les retirer de la diffusion publique.
          </Text>
        </>
      }
    >
      <Stack spacing={{ base: 7, md: 8 }}>
        <Button alignSelf="start" as="a" href="/admin" variant="outline">
          Retour au tableau de bord
        </Button>

        <Text color="ink.500" fontSize={{ base: "15px", md: "16px" }}>
          {events.length > 0
            ? `${events.length} evenement${events.length > 1 ? "s" : ""} en attente de validation.`
            : "Aucun evenement en attente de validation."}
        </Text>

        {message ? <MessageBanner message={message.message} title={message.title} tone={message.tone} /> : null}

        {isLoading ? (
          <SimpleGrid columns={{ base: 1, xl: 2 }} spacing={{ base: 5, md: 6 }}>
            {Array.from({ length: 4 }).map((_, index) => (
              <Skeleton borderRadius="16px" height="220px" key={index} />
            ))}
          </SimpleGrid>
        ) : events.length > 0 ? (
          <SimpleGrid columns={{ base: 1, xl: 2 }} spacing={{ base: 5, md: 6 }}>
            {events.map((event) => (
              <PendingResourceCard
                authorLabel={event.author.username}
                createdAtLabel={`Cree le ${formatPublicationDate(event.createdAt)}`}
                description={event.description}
                extraDetails={formatEventDateRange(event.startDate, event.endDate)}
                href={`/events/${event.idResource}`}
                key={event.idResource}
                kind="Evenement"
                title={event.title}
                visibilityLabel={event.visibility === "PUBLIC" ? "publique" : "privee"}
              />
            ))}
          </SimpleGrid>
        ) : (
          <Box bg="white" border="1px solid" borderColor="canvas.200" borderRadius="16px" p={{ base: 5, md: 6 }}>
            <Text color="ink.500" fontSize={{ base: "15px", md: "16px" }}>
              Aucun evenement en attente d'approbation.
            </Text>
          </Box>
        )}
      </Stack>
    </SiteLayout>
  );
}
