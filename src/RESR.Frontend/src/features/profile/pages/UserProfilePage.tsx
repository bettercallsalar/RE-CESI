import { Badge, Box, Button, Grid, GridItem, Heading, HStack, SimpleGrid, Skeleton, Stack, Tab, TabList, TabPanel, TabPanels, Tabs, Text } from "@chakra-ui/react";
import { useState } from "react";
import { SiteLayout } from "@/app/layouts/SiteLayout";
import { ArticlesGrid } from "@/features/articles/components/ArticlesGrid";
import { EventsGrid } from "@/features/events/components/EventsGrid";
import { useUserProfilePage } from "@/features/profile/hooks/useUserProfilePage";
import { navigateTo } from "@/shared/lib/navigation/navigateTo";
import { MessageBanner } from "@/shared/ui/feedback/MessageBanner";

interface UserProfilePageProps {
  idUser: number;
}

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

function buildProfileTitle(firstName: string, username: string) {
  return firstName.trim() || username.trim() || "Profil utilisateur";
}

export function UserProfilePage({ idUser }: UserProfilePageProps) {
  const [tabIndex, setTabIndex] = useState(0);
  const {
    profile,
    categories,
    articles,
    articlesPage,
    articlesTotalPages,
    articlesTotalCount,
    events,
    eventsPage,
    eventsTotalPages,
    eventsTotalCount,
    followersCount,
    followingCount,
    isLoading,
    isArticlesLoading,
    isEventsLoading,
    isOwnProfile,
    isFollowing,
    isFollowSubmitting,
    message,
    goToArticlesPage,
    goToEventsPage,
    followUser,
    unfollowUser
  } = useUserProfilePage(idUser);

  const profileTitle = profile ? buildProfileTitle(profile.firstName, profile.username) : "Profil utilisateur";

  return (
    <SiteLayout
      headerVariant="authenticated"
      intro={
        <>
          <Text fontSize={{ base: "20px", sm: "24px", md: "30px" }} fontWeight="700" textAlign="center">
            {profile ? `Profil de ${profileTitle}` : "Profil utilisateur"}
          </Text>
          <Text color="ink.500" fontSize={{ base: "16px", sm: "17px", md: "18px" }} maxW="760px" textAlign="center">
            Consultez les publications et evenements publics partages par ce membre de la plateforme.
          </Text>
        </>
      }
    >
      <Stack spacing={{ base: 7, md: 8 }}>
        {message ? <MessageBanner message={message.message} title={message.title} tone={message.tone} /> : null}

        {isLoading ? (
          <Grid alignItems="stretch" gap={{ base: 5, lg: 6 }} templateColumns={{ base: "1fr", xl: "1.1fr 0.9fr" }}>
            <GridItem>
              <Skeleton borderRadius="20px" height="280px" />
            </GridItem>
            <GridItem>
              <SimpleGrid columns={{ base: 1, md: 2 }} spacing={5}>
                <Skeleton borderRadius="20px" height="132px" />
                <Skeleton borderRadius="20px" height="132px" />
              </SimpleGrid>
            </GridItem>
          </Grid>
        ) : null}

        {!isLoading && !profile ? (
          <Stack bg="white" border="1px solid" borderColor="canvas.200" borderRadius="20px" p={{ base: 6, md: 8 }} spacing={5}>
            <Heading color="ink.800" fontSize={{ base: "24px", md: "28px" }}>
              Profil indisponible
            </Heading>
            <Text color="ink.500" fontSize={{ base: "15px", md: "16px" }}>
              Le profil de l'utilisateur {idUser} est introuvable ou n'est plus disponible.
            </Text>
            <HStack spacing={3} wrap="wrap">
              <Button onClick={() => navigateTo("/articles")} variant="outline">
                Retour aux articles
              </Button>
              <Button onClick={() => navigateTo("/events")}>
                Parcourir les events
              </Button>
            </HStack>
          </Stack>
        ) : null}

        {!isLoading && profile ? (
          <>
            <Grid alignItems="start" gap={{ base: 5, lg: 6 }} templateColumns={{ base: "1fr", xl: "1.1fr 0.9fr" }}>
              <GridItem>
                <Stack bg="white" border="1px solid" borderColor="canvas.200" borderRadius="20px" h="100%" p={{ base: 6, md: 8 }} spacing={5}>
                  <HStack align="start" justify="space-between" spacing={4} wrap="wrap">
                    <Stack spacing={2}>
                      <Heading color="ink.800" fontSize={{ base: "28px", md: "34px" }}>
                        {profileTitle}
                      </Heading>
                      <Text color="brand.500" fontSize={{ base: "16px", md: "17px" }} fontWeight="700">
                        @{profile.username}
                      </Text>
                    </Stack>

                    <Badge
                      alignSelf="start"
                      bg={profile.isVerified ? "brand.500" : "canvas.200"}
                      color={profile.isVerified ? "white" : "ink.800"}
                      fontSize="12px"
                      px={3}
                      py={1.5}
                      rounded="full"
                    >
                      {profile.isVerified ? "Compte verifie" : "Compte non verifie"}
                    </Badge>
                  </HStack>

                  {!isOwnProfile ? (
                    <HStack spacing={3} wrap="wrap">
                      <Button
                        isDisabled={isFollowSubmitting}
                        onClick={() => {
                          if (isFollowing) {
                            void unfollowUser();
                            return;
                          }

                          void followUser();
                        }}
                        variant={isFollowing ? "outline" : "solid"}
                      >
                        {isFollowing ? "Ne plus suivre" : "Suivre"}
                      </Button>
                    </HStack>
                  ) : null}

                  <Stack spacing={2}>
                    <Text color="ink.800" fontSize={{ base: "15px", md: "16px" }} fontWeight="700">
                      Departement
                    </Text>
                    <Text color="ink.500" fontSize={{ base: "15px", md: "16px" }}>
                      {profile.department.code} - {profile.department.name}
                    </Text>
                  </Stack>

                  <Stack flex="1" spacing={2}>
                    <Text color="ink.800" fontSize={{ base: "15px", md: "16px" }} fontWeight="700">
                      Bio
                    </Text>
                    <Text color="ink.500" fontSize={{ base: "15px", md: "16px" }} lineHeight="1.7">
                      {profile.bio?.trim() || "Aucune presentation n'a encore ete renseignee pour ce profil."}
                    </Text>
                  </Stack>

                  {isOwnProfile ? (
                    <HStack spacing={3} wrap="wrap">
                      <Button onClick={() => navigateTo("/mon-compte")} variant="outline">
                        Modifier mon compte
                      </Button>
                      <Button onClick={() => navigateTo("/mes-articles")}>
                        Gerer mes articles
                      </Button>
                    </HStack>
                  ) : null}
                </Stack>
              </GridItem>

              <GridItem>
                <SimpleGrid columns={{ base: 1, md: 2 }} spacing={5}>
                  <Box bg="white" border="1px solid" borderColor="canvas.200" borderRadius="20px" p={{ base: 6, md: 7 }}>
                    <Text color="ink.500" fontSize={{ base: "14px", md: "15px" }} fontWeight="700">
                      Articles publics
                    </Text>
                    <Heading color="ink.800" fontSize={{ base: "32px", md: "38px" }} mt={3}>
                      {articlesTotalCount}
                    </Heading>
                    <Text color="ink.500" fontSize={{ base: "14px", md: "15px" }} mt={2}>
                      Ressources visibles actuellement sur la plateforme.
                    </Text>
                  </Box>

                  <Box bg="white" border="1px solid" borderColor="canvas.200" borderRadius="20px" p={{ base: 6, md: 7 }}>
                    <Text color="ink.500" fontSize={{ base: "14px", md: "15px" }} fontWeight="700">
                      Events publics
                    </Text>
                    <Heading color="ink.800" fontSize={{ base: "32px", md: "38px" }} mt={3}>
                      {eventsTotalCount}
                    </Heading>
                    <Text color="ink.500" fontSize={{ base: "14px", md: "15px" }} mt={2}>
                      Evenements proposes par cet utilisateur et actuellement consultables.
                    </Text>
                  </Box>

                  <Box bg="white" border="1px solid" borderColor="canvas.200" borderRadius="20px" p={{ base: 6, md: 7 }}>
                    <Text color="ink.500" fontSize={{ base: "14px", md: "15px" }} fontWeight="700">
                      Abonnes
                    </Text>
                    <Heading color="ink.800" fontSize={{ base: "32px", md: "38px" }} mt={3}>
                      {followersCount}
                    </Heading>
                    <Text color="ink.500" fontSize={{ base: "14px", md: "15px" }} mt={2}>
                      Membres qui suivent actuellement ce profil.
                    </Text>
                  </Box>

                  <Box bg="white" border="1px solid" borderColor="canvas.200" borderRadius="20px" p={{ base: 6, md: 7 }}>
                    <Text color="ink.500" fontSize={{ base: "14px", md: "15px" }} fontWeight="700">
                      Abonnements
                    </Text>
                    <Heading color="ink.800" fontSize={{ base: "32px", md: "38px" }} mt={3}>
                      {followingCount}
                    </Heading>
                    <Text color="ink.500" fontSize={{ base: "14px", md: "15px" }} mt={2}>
                      Profils que cet utilisateur suit actuellement.
                    </Text>
                  </Box>
                </SimpleGrid>
              </GridItem>
            </Grid>

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
                  Articles ({articlesTotalCount})
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
                  Events ({eventsTotalCount})
                </Tab>
              </TabList>

              <TabPanels>
                <TabPanel px={0} pt={6}>
                  {isArticlesLoading ? (
                    <ResourceGridSkeleton />
                  ) : (
                    <ArticlesGrid
                      articles={articles}
                      categories={categories}
                      compact
                      emptyLabel={`Aucun article public n'est disponible pour ${profile.username}.`}
                    />
                  )}

                  <PaginationControls
                    isLoading={isArticlesLoading}
                    onNext={() => {
                      void goToArticlesPage(articlesPage + 1);
                    }}
                    onPrevious={() => {
                      void goToArticlesPage(articlesPage - 1);
                    }}
                    page={articlesPage}
                    totalPages={articlesTotalPages}
                  />
                </TabPanel>

                <TabPanel px={0} pt={6}>
                  {isEventsLoading ? (
                    <ResourceGridSkeleton />
                  ) : (
                    <EventsGrid
                      categories={categories}
                      compact
                      emptyLabel={`Aucun event public n'est disponible pour ${profile.username}.`}
                      events={events}
                    />
                  )}

                  <PaginationControls
                    isLoading={isEventsLoading}
                    onNext={() => {
                      void goToEventsPage(eventsPage + 1);
                    }}
                    onPrevious={() => {
                      void goToEventsPage(eventsPage - 1);
                    }}
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
