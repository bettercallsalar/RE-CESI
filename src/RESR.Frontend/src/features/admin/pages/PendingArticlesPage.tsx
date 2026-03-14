import { Box, Button, SimpleGrid, Skeleton, Stack, Text } from "@chakra-ui/react";
import { SiteLayout } from "@/app/layouts/SiteLayout";
import { PendingResourceCard } from "@/features/admin/components/PendingResourceCard";
import { usePendingArticlesPage } from "@/features/admin/hooks/usePendingArticlesPage";
import { MessageBanner } from "@/shared/ui/feedback/MessageBanner";

function formatPublicationDate(value: string) {
  return new Intl.DateTimeFormat("fr-FR", {
    dateStyle: "long"
  }).format(new Date(value));
}

export function PendingArticlesPage() {
  const { articles, isLoading, message } = usePendingArticlesPage();

  return (
    <SiteLayout
      headerVariant="authenticated"
      intro={
        <>
          <Text fontSize={{ base: "20px", sm: "24px", md: "30px" }} fontWeight="700" textAlign="center">
            Articles en attente de validation
          </Text>
          <Text color="ink.500" fontSize={{ base: "16px", sm: "17px", md: "18px" }} maxW="760px" textAlign="center">
            Consultez tous les articles non approuves, puis ouvrez leur detail pour les approuver ou les retirer de la diffusion publique.
          </Text>
        </>
      }
    >
      <Stack spacing={{ base: 7, md: 8 }}>
        <Button alignSelf="start" as="a" href="/admin" variant="outline">
          Retour au tableau de bord
        </Button>

        <Text color="ink.500" fontSize={{ base: "15px", md: "16px" }}>
          {articles.length > 0
            ? `${articles.length} article${articles.length > 1 ? "s" : ""} en attente de validation.`
            : "Aucun article en attente de validation."}
        </Text>

        {message ? <MessageBanner message={message.message} title={message.title} tone={message.tone} /> : null}

        {isLoading ? (
          <SimpleGrid columns={{ base: 1, xl: 2 }} spacing={{ base: 5, md: 6 }}>
            {Array.from({ length: 4 }).map((_, index) => (
              <Skeleton borderRadius="16px" height="220px" key={index} />
            ))}
          </SimpleGrid>
        ) : articles.length > 0 ? (
          <SimpleGrid columns={{ base: 1, xl: 2 }} spacing={{ base: 5, md: 6 }}>
            {articles.map((article) => (
              <PendingResourceCard
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
        ) : (
          <Box bg="white" border="1px solid" borderColor="canvas.200" borderRadius="16px" p={{ base: 5, md: 6 }}>
            <Text color="ink.500" fontSize={{ base: "15px", md: "16px" }}>
              Aucun article en attente d'approbation.
            </Text>
          </Box>
        )}
      </Stack>
    </SiteLayout>
  );
}
