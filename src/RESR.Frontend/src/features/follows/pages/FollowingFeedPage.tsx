import { Badge, Box, Button, Heading, HStack, SimpleGrid, Skeleton, Stack, Tab, TabList, TabPanel, TabPanels, Tabs, Text } from "@chakra-ui/react";
import { useEffect, useState } from "react";
import { SiteLayout } from "@/app/layouts/SiteLayout";
import { ArticlesGrid } from "@/features/articles/components/ArticlesGrid";
import { EventsGrid } from "@/features/events/components/EventsGrid";
import { useFollowingFeedPage } from "@/features/follows/hooks/useFollowingFeedPage";
import { getUserProfileHref } from "@/features/profile/lib/getUserProfileHref";
import { navigateTo } from "@/shared/lib/navigation/navigateTo";
import { MessageBanner } from "@/shared/ui/feedback/MessageBanner";
import { ContentGridSkeleton } from "@/shared/ui/content/ContentGridSkeleton";
import { PaginationControls } from "@/shared/ui/pagination/PaginationControls";

const PAGE_SIZE = 9;
function getTotalPages(totalCount: number) {
  return totalCount === 0 ? 0 : Math.ceil(totalCount / PAGE_SIZE);
}

export function FollowingFeedPage() {
  const { categories, followingUsers, articles, events, isLoading, message } = useFollowingFeedPage();
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
            Suivis
          </Text>
          <Text color="ink.500" fontSize={{ base: "16px", sm: "17px", md: "18px" }} maxW="820px" textAlign="center">
            Retrouvez en un seul endroit les articles et les evenements publics des utilisateurs que vous suivez.
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
            <ContentGridSkeleton />
          </>
        ) : null}

        {!isLoading && followingUsers.length === 0 ? (
          <Stack bg="white" border="1px solid" borderColor="canvas.200" borderRadius="20px" p={{ base: 6, md: 8 }} spacing={5}>
            <Heading color="ink.800" fontSize={{ base: "24px", md: "28px" }}>
              Aucun utilisateur suivi
            </Heading>
            <Text color="ink.500" fontSize={{ base: "15px", md: "16px" }}>
              Commencez par suivre des utilisateurs depuis leur profil pour voir ici leurs articles et leurs events publics.
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

        {!isLoading && followingUsers.length > 0 ? (
          <>
            <SimpleGrid columns={{ base: 1, md: 3 }} spacing={5}>
              <Box bg="white" border="1px solid" borderColor="canvas.200" borderRadius="20px" p={{ base: 6, md: 7 }}>
                <Text color="ink.500" fontSize={{ base: "14px", md: "15px" }} fontWeight="700">
                  Utilisateurs suivis
                </Text>
                <Heading color="ink.800" fontSize={{ base: "32px", md: "38px" }} mt={3}>
                  {followingUsers.length}
                </Heading>
              </Box>

              <Box bg="white" border="1px solid" borderColor="canvas.200" borderRadius="20px" p={{ base: 6, md: 7 }}>
                <Text color="ink.500" fontSize={{ base: "14px", md: "15px" }} fontWeight="700">
                  Articles disponibles
                </Text>
                <Heading color="ink.800" fontSize={{ base: "32px", md: "38px" }} mt={3}>
                  {articles.length}
                </Heading>
              </Box>

              <Box bg="white" border="1px solid" borderColor="canvas.200" borderRadius="20px" p={{ base: 6, md: 7 }}>
                <Text color="ink.500" fontSize={{ base: "14px", md: "15px" }} fontWeight="700">
                  Events disponibles
                </Text>
                <Heading color="ink.800" fontSize={{ base: "32px", md: "38px" }} mt={3}>
                  {events.length}
                </Heading>
              </Box>
            </SimpleGrid>

            <Stack bg="white" border="1px solid" borderColor="canvas.200" borderRadius="20px" p={{ base: 5, md: 6 }} spacing={4}>
              <Text color="ink.800" fontSize={{ base: "15px", md: "16px" }} fontWeight="700">
                Utilisateurs suivis
              </Text>
              <HStack spacing={3} wrap="wrap">
                {followingUsers.map((followedUser) => (
                  <Badge
                    as="a"
                    bg="canvas.100"
                    border="1px solid"
                    borderColor="canvas.200"
                    color="brand.500"
                    fontSize="13px"
                    href={getUserProfileHref(followedUser.idUser)}
                    key={followedUser.idUser}
                    px={3}
                    py={2}
                    rounded="full"
                  >
                    {followedUser.firstName} @{followedUser.username}
                  </Badge>
                ))}
              </HStack>
            </Stack>

            <Tabs index={tabIndex} onChange={setTabIndex} variant="unstyled">
              <TabList borderBottom="1px solid" borderColor="canvas.200" gap={3} overflowX="auto" pb={3}>
                <Tab
                  _selected={{ bg: "brand.500", color: "surface.onAccent" }}
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
                  _selected={{ bg: "brand.500", color: "surface.onAccent" }}
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
                    emptyLabel="Aucun article public n'est encore disponible dans vos suivis."
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
                    emptyLabel="Aucun event public n'est encore disponible dans vos suivis."
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
