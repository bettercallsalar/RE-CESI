import { Badge, Box, Button, Card, CardBody, HStack, Heading, Image, Stack, Text } from "@chakra-ui/react";
import { getPrimaryArticleImage, stripHtml } from "@/features/articles/lib/articleContent";
import { getResourceFileUrl } from "@/shared/lib/assets/getResourceFileUrl";
import { navigateTo } from "@/shared/lib/navigation/navigateTo";
import type { Article } from "@/shared/types/article";

export interface ArticleCardAction {
  label: string;
  href?: string;
  onClick?: () => void;
  variant?: "solid" | "outline";
  tone?: "default" | "danger" | "dangerSoft";
  isDisabled?: boolean;
}

interface ArticleCardProps {
  article: Article;
  categoryName?: string;
  compact?: boolean;
  href?: string;
  ctaLabel?: string;
  actions?: ArticleCardAction[];
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

function getAuthorLabel(article: Article) {
  const username = article.author?.username?.trim();
  const firstName = article.author?.firstName?.trim();

  return username || firstName || `Utilisateur #${article.idUser}`;
}
export function ArticleCard({
  article,
  categoryName,
  compact = false,
  href = `/articles/${article.idResource}`,
  ctaLabel = "Lire l'article",
  actions,
  showStatusBadges = false
}: ArticleCardProps) {
  const firstImage = getPrimaryArticleImage(article);
  const coverImageSrc = firstImage ? getResourceFileUrl(firstImage.path) : "/article-placeholder.svg";
  const visibilityLabel = article.visibility === "PUBLIC" ? "Public" : "Privé";
  const approvalLabel = article.isApproved ? "Validé" : "En attente";
  const deletedLabel = article.deletedAt ? `Supprimé le ${formatArticleDate(article.deletedAt)}` : null;
  const isLinkedCard = !actions?.length;

  return (
    <Card
      as={isLinkedCard ? "a" : undefined}
      href={isLinkedCard ? href : undefined}
      bg="white"
      border="1px solid"
      borderColor="canvas.200"
      display="flex"
      flexDirection="column"
      h="100%"
      rounded={{ base: "12px", md: "16px" }}
      shadow="sm"
      transition="border-color 0.2s ease, transform 0.2s ease, box-shadow 0.2s ease"
      _hover={{ borderColor: "brand.500", boxShadow: "md", transform: "translateY(-2px)", textDecoration: "none" }}
    >
      <Box bg="canvas.200" h={compact ? "180px" : "220px"} overflow="hidden">
        <Image alt={article.title} h="100%" objectFit="cover" src={coverImageSrc} w="100%" />
      </Box>
      <CardBody display="flex" flex="1" flexDirection="column" p={{ base: 5, md: 6 }}>
        <Stack flex="1" spacing={compact ? 3 : 4}>
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
              <Badge bg={article.visibility === "PUBLIC" ? "brand.500" : "surface.strong"} color={article.visibility === "PUBLIC" ? "surface.onAccent" : "surface.onStrong"} fontSize="12px" px={2.5} py={1} rounded="full">
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
            {showStatusBadges && deletedLabel ? (
              <Badge bg="red.500" color="surface.onCritical" fontSize="12px" px={2.5} py={1} rounded="full">
                {deletedLabel}
              </Badge>
            ) : null}
          </HStack>

          <Heading color="ink.800" fontSize={compact ? { base: "18px", md: "20px" } : { base: "20px", md: "24px" }} lineHeight="1.25">
            {article.title}
          </Heading>

          <Text color="ink.500" fontSize={{ base: "13px", md: "14px" }} fontWeight="600">
            Par {getAuthorLabel(article)}
          </Text>
          <Text color="ink.500" fontSize={{ base: "15px", md: "16px" }} lineHeight="1.65" noOfLines={compact ? 4 : 5}>
            {getExcerpt(article)}
          </Text>

          {actions?.length ? (
            <HStack align="stretch" mt="auto" pt={3} spacing={3} wrap="nowrap">
              {actions.map((action) => (
                <Button
                  key={`${action.label}-${action.href ?? "action"}`}
                  bg={action.tone === "danger" ? "red.500" : undefined}
                  borderColor={action.tone === "dangerSoft" ? "red.200" : undefined}
                  color={action.tone === "danger" ? "surface.onCritical" : action.tone === "dangerSoft" ? "red.300" : undefined}
                  flexShrink={0}
                  isDisabled={action.isDisabled}
                  onClick={() => {
                    if (action.onClick) {
                      action.onClick();
                      return;
                    }

                    if (action.href) {
                      navigateTo(action.href);
                    }
                  }}
                  size="sm"
                  variant={action.variant ?? "outline"}
                  _hover={
                    action.tone === "danger"
                      ? { bg: "red.600" }
                      : action.tone === "dangerSoft"
                        ? { bg: "red.50" }
                        : undefined
                  }
                  _disabled={
                    action.tone === "dangerSoft"
                      ? { bg: "red.100", borderColor: "red.200", color: "surface.onCritical", opacity: 1, cursor: "not-allowed" }
                      : undefined
                  }
                >
                  {action.label}
                </Button>
              ))}
            </HStack>
          ) : (
            <Text color="brand.500" fontSize={{ base: "14px", md: "15px" }} fontWeight="700" mt="auto">
              {ctaLabel}
            </Text>
          )}
        </Stack>
      </CardBody>
    </Card>
  );
}
