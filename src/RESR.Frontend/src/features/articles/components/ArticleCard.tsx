import { Badge, Box, Card, CardBody, HStack, Heading, Image, Stack, Text } from "@chakra-ui/react";
import { stripHtml } from "@/features/articles/lib/articleContent";
import { getResourceFileUrl } from "@/shared/lib/assets/getResourceFileUrl";
import type { Article } from "@/shared/types/article";

interface ArticleCardProps {
  article: Article;
  categoryName?: string;
  compact?: boolean;
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

export function ArticleCard({ article, categoryName, compact = false }: ArticleCardProps) {
  const firstImage = article.files[0];

  return (
    <Card
      bg="white"
      border="1px solid"
      borderColor="canvas.200"
      h="100%"
      rounded={{ base: "12px", md: "16px" }}
      shadow="sm"
    >
      {firstImage ? (
        <Box bg="canvas.200" h={compact ? "180px" : "220px"} overflow="hidden">
          <Image alt={article.title} h="100%" objectFit="cover" src={getResourceFileUrl(firstImage.path)} w="100%" />
        </Box>
      ) : null}
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
          </HStack>

          <Heading color="ink.800" fontSize={compact ? { base: "18px", md: "20px" } : { base: "20px", md: "24px" }} lineHeight="1.25">
            {article.title}
          </Heading>

          <Text color="ink.500" fontSize={{ base: "15px", md: "16px" }} lineHeight="1.65" noOfLines={compact ? 4 : 5}>
            {getExcerpt(article)}
          </Text>
        </Stack>
      </CardBody>
    </Card>
  );
}
