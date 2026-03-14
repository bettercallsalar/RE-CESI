import { Badge, Box, Button, HStack, SimpleGrid, Skeleton, Stack, Text } from "@chakra-ui/react";
import { SiteLayout } from "@/app/layouts/SiteLayout";
import { formatEventDateRange } from "@/features/events/lib/eventDates";
import { usePendingResourcesPage } from "@/features/admin/hooks/usePendingResourcesPage";
import type { Article } from "@/shared/types/article";
import type { Event } from "@/shared/types/event";
import { MessageBanner } from "@/shared/ui/feedback/MessageBanner";

function formatPublicationDate(value: string) {
  return new Intl.DateTimeFormat("fr-FR", {
    dateStyle: "long"
  }).format(new Date(value));
}

function ResourceCard({
  kind,
  href,
  title,
  description,
  authorLabel,
  createdAtLabel,
  visibilityLabel,
  extraDetails
}: {
  kind: "Article" | "Evenement";
  href: string;
  title: string;
  description: string | null;
  authorLabel: string;
  createdAtLabel: string;
  visibilityLabel: string;
  extraDetails?: string;
}) {
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
          {description || "Aucune description fournie pour cette ressource."}
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

function ArticleCards({ articles }: { articles: Article[] }) {
  return (
    <SimpleGrid columns={{ base: 1, xl: 2 }} spacing={{ base: 5, md: 6 }}>
      {articles.map((article) => (
        <ResourceCard
          authorLabel={article.author.username}
          createdAtLabel={`Cree le ${formatPublicationDate(article.createdAt)}`}
          description={article.description}
          href={`/articles/${article.idResource}`}
          key={article.idResource}
          kind="Article"
          title={article.title}
          visibilityLabel={article.visibility === "PUBLIC" ? "publique" : "privee"}
        />
      ))}
    </SimpleGrid>
  );
}

function EventCards({ events }: { events: Event[] }) {
  return (
    <SimpleGrid columns={{ base: 1, xl: 2 }} spacing={{ base: 5, md: 6 }}>
      {events.map((event) => (
        <ResourceCard
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
  );
}

export function PendingResourcesPage() {
  const { articles, events, isLoading, message, canApproveArticles, canApproveEvents } = usePendingResourcesPage();
  const totalCount = articles.length + events.length;

  return (
    <SiteLayout
      headerVariant="authenticated"
      intro={
        <>
          <Text fontSize={{ base: "20px", sm: "24px", md: "30px" }} fontWeight="700" textAlign="center">
            Ressources en attente d'approbation
          </Text>
          <Text color="ink.500" fontSize={{ base: "16px", sm: "17px", md: "18px" }} maxW="760px" textAlign="center">
            Consultez tous les articles et evenements non approuves autorises par vos permissions, puis ouvrez leur detail pour les valider.
          </Text>
        </>
      }
    >
      <Stack spacing={{ base: 7, md: 8 }}>
        <Button alignSelf="start" as="a" href="/admin" variant="outline">
          Retour au tableau de bord
        </Button>

        <Text color="ink.500" fontSize={{ base: "15px", md: "16px" }}>
          {totalCount > 0
            ? `${totalCount} ressource${totalCount > 1 ? "s" : ""} en attente d'approbation.`
            : "Aucune ressource en attente d'approbation pour les permissions presentes dans votre token."}
        </Text>

        {message ? <MessageBanner message={message.message} title={message.title} tone={message.tone} /> : null}

        {isLoading ? (
          <SimpleGrid columns={{ base: 1, xl: 2 }} spacing={{ base: 5, md: 6 }}>
            {Array.from({ length: 4 }).map((_, index) => (
              <Skeleton borderRadius="16px" height="220px" key={index} />
            ))}
          </SimpleGrid>
        ) : (
          <Stack spacing={{ base: 7, md: 8 }}>
            {canApproveArticles ? (
              <Stack spacing={4}>
                <HStack justify="space-between" spacing={4} wrap="wrap">
                  <Text color="ink.800" fontSize={{ base: "20px", md: "22px" }} fontWeight="700">
                    Articles a approuver
                  </Text>
                  <Text color="brand.500" fontSize={{ base: "14px", md: "15px" }} fontWeight="700">
                    {articles.length} article{articles.length > 1 ? "s" : ""}
                  </Text>
                </HStack>
                {articles.length > 0 ? (
                  <ArticleCards articles={articles} />
                ) : (
                  <Box bg="white" border="1px solid" borderColor="canvas.200" borderRadius="16px" p={{ base: 5, md: 6 }}>
                    <Text color="ink.500" fontSize={{ base: "15px", md: "16px" }}>
                      Aucun article en attente d'approbation.
                    </Text>
                  </Box>
                )}
              </Stack>
            ) : null}

            {canApproveEvents ? (
              <Stack spacing={4}>
                <HStack justify="space-between" spacing={4} wrap="wrap">
                  <Text color="ink.800" fontSize={{ base: "20px", md: "22px" }} fontWeight="700">
                    Evenements a approuver
                  </Text>
                  <Text color="brand.500" fontSize={{ base: "14px", md: "15px" }} fontWeight="700">
                    {events.length} evenement{events.length > 1 ? "s" : ""}
                  </Text>
                </HStack>
                {events.length > 0 ? (
                  <EventCards events={events} />
                ) : (
                  <Box bg="white" border="1px solid" borderColor="canvas.200" borderRadius="16px" p={{ base: 5, md: 6 }}>
                    <Text color="ink.500" fontSize={{ base: "15px", md: "16px" }}>
                      Aucun evenement en attente d'approbation.
                    </Text>
                  </Box>
                )}
              </Stack>
            ) : null}
          </Stack>
        )}
      </Stack>
    </SiteLayout>
  );
}
