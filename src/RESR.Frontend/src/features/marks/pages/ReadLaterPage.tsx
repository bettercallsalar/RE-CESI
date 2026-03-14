import { Box, Button, Heading, HStack, SimpleGrid, Skeleton, Stack, Tab, TabList, TabPanel, TabPanels, Tabs, Text } from "@chakra-ui/react";
import { useEffect, useState } from "react";
import { SiteLayout } from "@/app/layouts/SiteLayout";
import { ArticlesGrid } from "@/features/articles/components/ArticlesGrid";
import { EventsGrid } from "@/features/events/components/EventsGrid";
import { useReadLaterPage } from "@/features/marks/hooks/useReadLaterPage";
import { navigateTo } from "@/shared/lib/navigation/navigateTo";
import { MessageBanner } from "@/shared/ui/feedback/MessageBanner";

const PAGE_SIZE = 9;

function ResourceGridSkeleton() {
  return (
    <SimpleGrid columns={{ base: 1, md: 2, xl: 3 }} spacing={{ base: 5, md: 6 }}>
      {Array.from({ length: 3 }).map((_, index) => (
        <Skeleton borderRadius="16px" height="360px" key={index} />
      ))}
    </SimpleGrid>
  );
}

function PaginationControls({
  page,
  totalPages,
  isLoading,
  onPrevious,
  onNext
}: {
  page: number;
  totalPages: number;
  isLoading: boolean;
  onPrevious: () => void;
  onNext: () => void;
}) {
  if (totalPages <= 1) {
    return null;
  }

  return (
    <HStack justify="space-between" pt={6} spacing={4}>
      <Button isDisabled={isLoading || page <= 1} onClick={onPrevious} variant="outline">
        Page precedente
      </Button>
      <Text color="ink.500" fontSize={{ base: "14px", md: "15px" }}>
        Page {page} sur {totalPages}
      </Text>
      <Button isDisabled={isLoading || page >= totalPages} onClick={onNext} variant="outline">
        Page suivante
      </Button>
    </HStack>
  );
}

function getTotalPages(totalCount: number) {
  return totalCount === 0 ? 0 : Math.ceil(totalCount / PAGE_SIZE);
}

export function ReadLaterPage() {
  const { categories, articles, events, isLoading, message } = useReadLaterPage();
  const [tabIndex, setTabIndex] = useState(0);
  const [articlesPage, setArticlesPage] = useState(1);
  const [eventsPage, setEventsPage] = useState(1);

  useEffect(() => {
    setArticlesPage(1);
  }, [articles.length]);

  useEffect(() => {
    setEventsPage(1);
  }, [events.length]);

  const articlesTotalPages = getTotalPages(articles.length);
  const eventsTotalPages = getTotalPages(events.length);
  const currentArticles = articles.slice((articlesPage - 1) * PAGE_SIZE, articlesPage * PAGE_SIZE);
  const currentEvents = events.slice((eventsPage - 1) * PAGE_SIZE, eventsPage * PAGE_SIZE);

  return (
    <SiteLayout
      headerVariant="authenticated"
      intro={
        <>
          <Text fontSize={{ base: "20px", sm: "24px", md: "30px" }} fontWeight="700" textAlign="center">
            A lire plus tard
          </Text>
          <Text color="ink.500" fontSize={{ base: "16px", sm: "17px", md: "18px" }} maxW="820px" textAlign="center">
            Retrouvez tous les articles et les events que vous avez enregistres pour les consulter plus tard.
          </Text>
        </>
      }
    >
      <Stack spacing={{ base: 7, md: 8 }}>
        {message ? <MessageBanner message={message.message} title={message.title} tone={message.tone} /> : null}

        {isLoading ? (
          <>
            <SimpleGrid columns={{ base: 1, md: 3 }} spacing={5}>
              <Skeleton borderRadius="20px" height="132px" />
              <Skeleton borderRadius="20px" height="132px" />
              <Skeleton borderRadius="20px" height="132px" />
            </SimpleGrid>
            <Skeleton borderRadius="20px" height="72px" />
            <ResourceGridSkeleton />
          </>
        ) : null}

        {!isLoading && articles.length === 0 && events.length === 0 ? (
          <Stack bg="white" border="1px solid" borderColor="canvas.200" borderRadius="20px" p={{ base: 6, md: 8 }} spacing={5}>
            <Heading color="ink.800" fontSize={{ base: "24px", md: "28px" }}>
              Aucun article ou event enregistre
            </Heading>
            <Text color="ink.500" fontSize={{ base: "15px", md: "16px" }}>
              Utilisez le signet sur un article ou un event pour le retrouver ici plus tard.
            </Text>
            <HStack spacing={3} wrap="wrap">
              <Button onClick={() => navigateTo("/articles")} variant="outline">
                Parcourir les articles
              </Button>
              <Button onClick={() => navigateTo("/events")}>
                Parcourir les events
              </Button>
            </HStack>
          </Stack>
        ) : null}

        {!isLoading && (articles.length > 0 || events.length > 0) ? (
          <>
            <SimpleGrid columns={{ base: 1, md: 3 }} spacing={5}>
              <Box bg="white" border="1px solid" borderColor="canvas.200" borderRadius="20px" p={{ base: 6, md: 7 }}>
                <Text color="ink.500" fontSize={{ base: "14px", md: "15px" }} fontWeight="700">
                  Articles et events enregistres
                </Text>
                <Heading color="ink.800" fontSize={{ base: "32px", md: "38px" }} mt={3}>
                  {articles.length + events.length}
                </Heading>
              </Box>

              <Box bg="white" border="1px solid" borderColor="canvas.200" borderRadius="20px" p={{ base: 6, md: 7 }}>
                <Text color="ink.500" fontSize={{ base: "14px", md: "15px" }} fontWeight="700">
                  Articles enregistres
                </Text>
                <Heading color="ink.800" fontSize={{ base: "32px", md: "38px" }} mt={3}>
                  {articles.length}
                </Heading>
              </Box>

              <Box bg="white" border="1px solid" borderColor="canvas.200" borderRadius="20px" p={{ base: 6, md: 7 }}>
                <Text color="ink.500" fontSize={{ base: "14px", md: "15px" }} fontWeight="700">
                  Events enregistres
                </Text>
                <Heading color="ink.800" fontSize={{ base: "32px", md: "38px" }} mt={3}>
                  {events.length}
                </Heading>
              </Box>
            </SimpleGrid>

            <Tabs index={tabIndex} onChange={setTabIndex} variant="unstyled">
              <TabList borderBottom="1px solid" borderColor="canvas.200" gap={3} overflowX="auto" pb={3}>
                <Tab
                  _selected={{ bg: "brand.500", color: "white" }}
                  bg="white"
                  border="1px solid"
                  borderColor="canvas.200"
                  borderRadius="999px"
                  color="ink.800"
                  fontSize={{ base: "14px", md: "15px" }}
                  fontWeight="700"
                  px={5}
                  py={2.5}
                >
                  Articles ({articles.length})
                </Tab>
                <Tab
                  _selected={{ bg: "brand.500", color: "white" }}
                  bg="white"
                  border="1px solid"
                  borderColor="canvas.200"
                  borderRadius="999px"
                  color="ink.800"
                  fontSize={{ base: "14px", md: "15px" }}
                  fontWeight="700"
                  px={5}
                  py={2.5}
                >
                  Events ({events.length})
                </Tab>
              </TabList>

              <TabPanels>
                <TabPanel px={0} pt={6}>
                  <ArticlesGrid
                    articles={currentArticles}
                    categories={categories}
                    compact
                    emptyLabel="Aucun article n'est actuellement enregistre pour plus tard."
                  />

                  <PaginationControls
                    isLoading={isLoading}
                    onNext={() => setArticlesPage((current) => current + 1)}
                    onPrevious={() => setArticlesPage((current) => current - 1)}
                    page={articlesPage}
                    totalPages={articlesTotalPages}
                  />
                </TabPanel>

                <TabPanel px={0} pt={6}>
                  <EventsGrid
                    categories={categories}
                    compact
                    emptyLabel="Aucun event n'est actuellement enregistre pour plus tard."
                    events={currentEvents}
                  />

                  <PaginationControls
                    isLoading={isLoading}
                    onNext={() => setEventsPage((current) => current + 1)}
                    onPrevious={() => setEventsPage((current) => current - 1)}
                    page={eventsPage}
                    totalPages={eventsTotalPages}
                  />
                </TabPanel>
              </TabPanels>
            </Tabs>
          </>
        ) : null}
      </Stack>
    </SiteLayout>
  );
}
