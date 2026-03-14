import { Badge, Box, Card, CardBody, HStack, Heading, Image, Stack, Text } from "@chakra-ui/react";
import { getPrimaryArticleImage, stripHtml } from "@/features/articles/lib/articleContent";
import { getResourceFileUrl } from "@/shared/lib/assets/getResourceFileUrl";
import type { Article } from "@/shared/types/article";

interface ArticleCardProps {
  article: Article;
  categoryName?: string;
  compact?: boolean;
  href?: string;
  ctaLabel?: string;
  showStatusBadges?: boolean;
}

function formatArticleDate(value: string) {
  return new Intl.DateTimeFormat("fr-FR", {
    dateStyle: "long"
  }).format(new Date(value));
}

function getExcerpt(article: Article) {
  const base = article.description?.trim() || stripHtml(article.content);

  if (base.length <= 180) {
    return base;
  }

  return `${base.slice(0, 177).trim()}...`;
}

export function ArticleCard({
  article,
  categoryName,
  compact = false,
  href = `/articles/${article.idResource}`,
  ctaLabel = "Lire l'article",
  showStatusBadges = false
}: ArticleCardProps) {
  const firstImage = getPrimaryArticleImage(article);
  const coverImageSrc = firstImage ? getResourceFileUrl(firstImage.path) : "/article-placeholder.svg";
  const visibilityLabel = article.visibility === "PUBLIC" ? "Public" : "Privé";
  const approvalLabel = article.isApproved ? "Validé" : "En attente";

  return (
    <Card
      as="a"
      href={href}
      bg="white"
      border="1px solid"
      borderColor="canvas.200"
      display="block"
      h="100%"
      rounded={{ base: "12px", md: "16px" }}
      shadow="sm"
      transition="border-color 0.2s ease, transform 0.2s ease, box-shadow 0.2s ease"
      _hover={{ borderColor: "brand.500", boxShadow: "md", transform: "translateY(-2px)", textDecoration: "none" }}
    >
      <Box bg="canvas.200" h={compact ? "180px" : "220px"} overflow="hidden">
        <Image alt={article.title} h="100%" objectFit="cover" src={coverImageSrc} w="100%" />
      </Box>
      <CardBody p={{ base: 5, md: 6 }}>
        <Stack h="100%" spacing={compact ? 3 : 4}>
          <HStack flexWrap="wrap" spacing={3}>
            <Badge bg="canvas.200" color="ink.800" fontSize="12px" px={2.5} py={1} rounded="full">
              {formatArticleDate(article.createdAt)}
            </Badge>
            {categoryName ? (
              <Badge bg="white" border="1px solid" borderColor="brand.500" color="brand.500" fontSize="12px" px={2.5} py={1} rounded="full">
                {categoryName}
              </Badge>
            ) : null}
            {showStatusBadges ? (
              <Badge bg={article.visibility === "PUBLIC" ? "brand.500" : "ink.800"} color="white" fontSize="12px" px={2.5} py={1} rounded="full">
                {visibilityLabel}
              </Badge>
            ) : null}
            {showStatusBadges ? (
              <Badge
                bg={article.isApproved ? "white" : "canvas.200"}
                border="1px solid"
                borderColor={article.isApproved ? "brand.500" : "canvas.300"}
                color={article.isApproved ? "brand.500" : "ink.800"}
                fontSize="12px"
                px={2.5}
                py={1}
                rounded="full"
              >
                {approvalLabel}
              </Badge>
            ) : null}
          </HStack>

          <Heading color="ink.800" fontSize={compact ? { base: "18px", md: "20px" } : { base: "20px", md: "24px" }} lineHeight="1.25">
            {article.title}
          </Heading>

          <Text color="ink.500" fontSize={{ base: "15px", md: "16px" }} lineHeight="1.65" noOfLines={compact ? 4 : 5}>
            {getExcerpt(article)}
          </Text>

          <Text color="brand.500" fontSize={{ base: "14px", md: "15px" }} fontWeight="700" mt="auto">
            {ctaLabel}
          </Text>
        </Stack>
      </CardBody>
    </Card>
  );
}
