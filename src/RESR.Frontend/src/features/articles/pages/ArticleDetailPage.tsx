import { Badge, Box, Button, Card, CardBody, Heading, HStack, Image, Skeleton, Stack, Text } from "@chakra-ui/react";
import { useEffect, useState } from "react";
import { SiteLayout } from "@/app/layouts/SiteLayout";
import { useAuth } from "@/features/auth/hooks/useAuth";
import { getPrimaryArticleImage, sanitizeArticleHtml } from "@/features/articles/lib/articleContent";
import { useArticleDetail } from "@/features/articles/hooks/useArticleDetail";
import { getResourceFileUrl } from "@/shared/lib/assets/getResourceFileUrl";
import { navigateTo } from "@/shared/lib/navigation/navigateTo";
import { MessageBanner } from "@/shared/ui/feedback/MessageBanner";

interface ArticleDetailPageProps {
  idResource: number;
}

function formatArticleDate(value: string) {
  return new Intl.DateTimeFormat("fr-FR", {
    dateStyle: "long"
  }).format(new Date(value));
}

export function ArticleDetailPage({ idResource }: ArticleDetailPageProps) {
  const { status } = useAuth();
  const { article, categoryName, isLoading, message, canEdit } = useArticleDetail(idResource);
  const [currentImageIndex, setCurrentImageIndex] = useState(0);

  const albumImages = article?.files ?? [];
  const preferredImage = article ? getPrimaryArticleImage(article) : null;
  const currentImage = albumImages[currentImageIndex] ?? preferredImage ?? null;

  useEffect(() => {
    if (!article) {
      setCurrentImageIndex(0);
      return;
    }

    const preferredIndex = preferredImage
      ? article.files.findIndex((file) => file.idFile === preferredImage.idFile)
      : 0;

    setCurrentImageIndex(preferredIndex >= 0 ? preferredIndex : 0);
  }, [article, preferredImage]);

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

        {!isLoading && article ? (
          <>
            <Stack spacing={4}>
              <HStack flexWrap="wrap" spacing={3}>
                <Badge bg="canvas.200" color="ink.800" fontSize="12px" px={2.5} py={1} rounded="full">
                  {formatArticleDate(article.createdAt)}
                </Badge>
                {categoryName ? (
                  <Badge bg="white" border="1px solid" borderColor="brand.500" color="brand.500" fontSize="12px" px={2.5} py={1} rounded="full">
                    {categoryName}
                  </Badge>
                ) : null}
              </HStack>

              <Heading color="ink.800" fontSize={{ base: "30px", md: "40px" }} lineHeight="1.1">
                {article.title}
              </Heading>

              {article.description ? (
                <Text color="ink.500" fontSize={{ base: "17px", md: "19px" }} lineHeight="1.7" maxW="980px">
                  {article.description}
                </Text>
              ) : null}

              <HStack align="center" flexWrap="wrap" justify="space-between" spacing={4}>
                <Text color="ink.500" fontSize={{ base: "14px", md: "15px" }}>
                  Publication proposée par l'utilisateur #{article.idUser}
                </Text>

                <HStack spacing={3}>
                  <Button onClick={() => navigateTo("/articles")} variant="outline">
                    Retour aux articles
                  </Button>
                  {canEdit ? (
                    <Button onClick={() => navigateTo(`/articles/${article.idResource}/modifier`)}>
                      Modifier l'article
                    </Button>
                  ) : null}
                </HStack>
              </HStack>
            </Stack>

            {currentImage ? (
              <Stack bg="white" border="1px solid" borderColor="canvas.200" py={{ base: 4, md: 6 }} px={{ base: 4, md: 6 }} rounded="18px" spacing={4}>
                <HStack justify="space-between" spacing={4} wrap="wrap">
                  <Text color="ink.500" fontSize={{ base: "14px", md: "15px" }}>
                    {albumImages.length > 0 ? `Image ${currentImageIndex + 1} sur ${albumImages.length}` : "Illustration de l'article"}
                  </Text>

                  {albumImages.length > 1 ? (
                    <HStack spacing={3}>
                      <Button onClick={goToPreviousImage} variant="outline">
                        Image précédente
                      </Button>
                      <Button onClick={goToNextImage}>
                        Image suivante
                      </Button>
                    </HStack>
                  ) : null}
                </HStack>

                <Box textAlign="center">
                  <Image
                    alt={currentImage.originalName || article.title}
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

            <Card bg="white" border="1px solid" borderColor="canvas.200" rounded="18px" shadow="sm">
              <CardBody p={{ base: 6, md: 8 }}>
                <Box
                  className="article-content"
                  color="ink.800"
                  dangerouslySetInnerHTML={{ __html: sanitizeArticleHtml(article.content) }}
                  fontSize={{ base: "16px", md: "17px" }}
                  lineHeight="1.85"
                />
              </CardBody>
            </Card>
          </>
        ) : null}
      </Stack>
    </SiteLayout>
  );
}
