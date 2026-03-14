import { Badge, Box, Button, Card, CardBody, Heading, HStack, Image, SimpleGrid, Skeleton, Stack, Text } from "@chakra-ui/react";
import { useEffect, useState } from "react";
import { SiteLayout } from "@/app/layouts/SiteLayout";
import { useAuth } from "@/features/auth/hooks/useAuth";
import { formatEventDateRange, formatEventPublishedDate, getPrimaryEventImage } from "@/features/events/lib/eventDates";
import { useEventDetail } from "@/features/events/hooks/useEventDetail";
import { getResourceFileUrl } from "@/shared/lib/assets/getResourceFileUrl";
import { navigateTo } from "@/shared/lib/navigation/navigateTo";
import { CommentsSection } from "@/shared/ui/comments/CommentsSection";
import { MessageBanner } from "@/shared/ui/feedback/MessageBanner";

interface EventDetailPageProps {
  idResource: number;
}

function getAuthorLabel(idUser: number, firstName?: string, username?: string) {
  return username?.trim() || firstName?.trim() || `utilisateur #${idUser}`;
}

export function EventDetailPage({ idResource }: EventDetailPageProps) {
  const { status } = useAuth();
  const { event, categoryName, isLoading, isApproving, message, canEdit, canApprove, approveEvent } = useEventDetail(idResource);
  const [currentImageIndex, setCurrentImageIndex] = useState(0);

  const albumImages = event?.files ?? [];
  const preferredImage = event ? getPrimaryEventImage(event) : null;
  const currentImage = albumImages[currentImageIndex] ?? preferredImage ?? null;

  useEffect(() => {
    if (!event) {
      setCurrentImageIndex(0);
      return;
    }

    const preferredIndex = preferredImage
      ? event.files.findIndex((file) => file.idFile === preferredImage.idFile)
      : 0;

    setCurrentImageIndex(preferredIndex >= 0 ? preferredIndex : 0);
  }, [event, preferredImage]);

  function goToPreviousImage() {
    if (albumImages.length <= 1) {
      return;
    }

    setCurrentImageIndex((current) => (current === 0 ? albumImages.length - 1 : current - 1));
  }

  function goToNextImage() {
    if (albumImages.length <= 1) {
      return;
    }

    setCurrentImageIndex((current) => (current === albumImages.length - 1 ? 0 : current + 1));
  }

  return (
    <SiteLayout headerVariant={status === "authenticated" ? "authenticated" : "public"}>
      <Stack spacing={{ base: 8, md: 10 }}>
        {isLoading ? (
          <Stack spacing={5}>
            <Skeleton borderRadius="16px" height="72px" />
            <Skeleton borderRadius="16px" height="420px" />
            <Skeleton borderRadius="16px" height="260px" />
          </Stack>
        ) : null}

        {!isLoading && message ? <MessageBanner message={message.message} title={message.title} tone={message.tone} /> : null}

        {!isLoading && event ? (
          <>
            <Stack spacing={4}>
              <HStack flexWrap="wrap" spacing={3}>
                <Badge bg="canvas.200" color="ink.800" fontSize="12px" px={2.5} py={1} rounded="full">
                  Cree le {formatEventPublishedDate(event.createdAt)}
                </Badge>
                {categoryName ? (
                  <Badge bg="white" border="1px solid" borderColor="brand.500" color="brand.500" fontSize="12px" px={2.5} py={1} rounded="full">
                    {categoryName}
                  </Badge>
                ) : null}
                {event.department ? (
                  <Badge bg="white" border="1px solid" borderColor="canvas.300" color="ink.800" fontSize="12px" px={2.5} py={1} rounded="full">
                    {event.department.code} - {event.department.name}
                  </Badge>
                ) : null}
                {event.deletedAt ? (
                  <Badge bg="red.500" color="white" fontSize="12px" px={2.5} py={1} rounded="full">
                    Supprime le {formatEventPublishedDate(event.deletedAt)}
                  </Badge>
                ) : null}
                {!event.deletedAt && !event.isApproved ? (
                  <Badge bg="#FEEBC8" color="#9C4221" fontSize="12px" px={2.5} py={1} rounded="full">
                    En attente d'approbation
                  </Badge>
                ) : null}
              </HStack>

              <Heading color="ink.800" fontSize={{ base: "30px", md: "40px" }} lineHeight="1.1">
                {event.title}
              </Heading>

              {event.subtitle ? (
                <Text color="brand.500" fontSize={{ base: "18px", md: "20px" }} fontWeight="700">
                  {event.subtitle}
                </Text>
              ) : null}

              {event.description ? (
                <Text color="ink.500" fontSize={{ base: "17px", md: "19px" }} lineHeight="1.7" maxW="980px">
                  {event.description}
                </Text>
              ) : null}

              <HStack align="center" flexWrap="wrap" justify="space-between" spacing={4}>
                <Text color="ink.500" fontSize={{ base: "14px", md: "15px" }}>
                  Evenement propose par {getAuthorLabel(event.idUser, event.author?.firstName, event.author?.username)}
                </Text>

                <HStack spacing={3}>
                  {canApprove && !event.isApproved ? (
                    <Button as="a" href="/admin/resources/pending" variant="outline">
                      Retour aux validations
                    </Button>
                  ) : null}
                  <Button onClick={() => navigateTo("/events")} variant="outline">
                    Retour aux evenements
                  </Button>
                  {canEdit ? (
                    <Button onClick={() => navigateTo(`/events/${event.idResource}/modifier`)}>
                      Modifier l'evenement
                    </Button>
                  ) : null}
                  {canApprove && !event.isApproved ? (
                    <Button
                      isDisabled={isApproving}
                      onClick={() => {
                        void approveEvent();
                      }}
                    >
                      Approuver l'evenement
                    </Button>
                  ) : null}
                </HStack>
              </HStack>
            </Stack>

            <SimpleGrid columns={{ base: 1, lg: 3 }} spacing={5}>
              <Card bg="white" border="1px solid" borderColor="canvas.200" rounded="18px" shadow="sm">
                <CardBody>
                  <Stack spacing={2}>
                    <Text color="ink.500" fontSize="14px" fontWeight="700">
                      Quand
                    </Text>
                    <Text color="ink.800" fontSize={{ base: "16px", md: "17px" }} lineHeight="1.6">
                      {formatEventDateRange(event.startDate, event.endDate)}
                    </Text>
                  </Stack>
                </CardBody>
              </Card>

              <Card bg="white" border="1px solid" borderColor="canvas.200" rounded="18px" shadow="sm">
                <CardBody>
                  <Stack spacing={2}>
                    <Text color="ink.500" fontSize="14px" fontWeight="700">
                      Ou
                    </Text>
                    <Text color="ink.800" fontSize={{ base: "16px", md: "17px" }} lineHeight="1.6">
                      {event.address || "Adresse non renseignee"}
                    </Text>
                  </Stack>
                </CardBody>
              </Card>

              <Card bg="white" border="1px solid" borderColor="canvas.200" rounded="18px" shadow="sm">
                <CardBody>
                  <Stack spacing={2}>
                    <Text color="ink.500" fontSize="14px" fontWeight="700">
                      Visibilite
                    </Text>
                    <Text color="ink.800" fontSize={{ base: "16px", md: "17px" }} lineHeight="1.6">
                      {event.visibility === "PUBLIC" ? "Public" : "Prive"} {event.isApproved ? "• valide" : "• en attente de validation"}
                    </Text>
                  </Stack>
                </CardBody>
              </Card>
            </SimpleGrid>

            {currentImage ? (
              <Stack bg="white" border="1px solid" borderColor="canvas.200" px={{ base: 4, md: 6 }} py={{ base: 4, md: 6 }} rounded="18px" spacing={4}>
                <HStack justify="space-between" spacing={4} wrap="wrap">
                  <Text color="ink.500" fontSize={{ base: "14px", md: "15px" }}>
                    {albumImages.length > 0 ? `Image ${currentImageIndex + 1} sur ${albumImages.length}` : "Illustration de l'evenement"}
                  </Text>

                  {albumImages.length > 1 ? (
                    <HStack spacing={3}>
                      <Button onClick={goToPreviousImage} variant="outline">
                        Image precedente
                      </Button>
                      <Button onClick={goToNextImage}>
                        Image suivante
                      </Button>
                    </HStack>
                  ) : null}
                </HStack>

                <Box textAlign="center">
                  <Image
                    alt={currentImage.originalName || event.title}
                    display="inline-block"
                    h="auto"
                    maxW="100%"
                    src={getResourceFileUrl(currentImage.path)}
                    w={{ base: "100%", lg: "75vw" }}
                  />
                </Box>

                {albumImages.length > 1 ? (
                  <HStack align="stretch" overflowX="auto" pb={1} spacing={3}>
                    {albumImages.map((file, index) => (
                      <Box
                        bg={index === currentImageIndex ? "canvas.100" : "white"}
                        border="1px solid"
                        borderColor={index === currentImageIndex ? "brand.500" : "canvas.200"}
                        cursor="pointer"
                        flex="0 0 144px"
                        key={file.idFile}
                        onClick={() => setCurrentImageIndex(index)}
                        px={2}
                        py={2}
                        rounded="12px"
                        transition="border-color 0.2s ease, background-color 0.2s ease"
                      >
                        <Image
                          alt={file.originalName}
                          display="block"
                          h="96px"
                          mx="auto"
                          objectFit="contain"
                          src={getResourceFileUrl(file.path)}
                          w="100%"
                        />
                        <Text color="ink.500" fontSize="12px" mt={2} noOfLines={2} textAlign="center">
                          {file.originalName}
                        </Text>
                      </Box>
                    ))}
                  </HStack>
                ) : null}
              </Stack>
            ) : null}

            {!event.deletedAt ? <CommentsSection idResource={event.idResource} resourceOwnerId={event.idUser} /> : null}
          </>
        ) : null}
      </Stack>
    </SiteLayout>
  );
}
