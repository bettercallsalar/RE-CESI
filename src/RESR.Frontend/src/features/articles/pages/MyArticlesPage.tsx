import { Button, FormControl, FormLabel, HStack, Input, Select, SimpleGrid, Skeleton, Stack, Text } from "@chakra-ui/react";
import { SiteLayout } from "@/app/layouts/SiteLayout";
import { ArticlesGrid } from "@/features/articles/components/ArticlesGrid";
import { useMyArticlesPage } from "@/features/articles/hooks/useMyArticlesPage";
import { MessageBanner } from "@/shared/ui/feedback/MessageBanner";

export function MyArticlesPage() {
  const { filters, categories, articles, isLoading, message, page, totalPages, totalCount, updateFilter, submitFilters, goToPage } =
    useMyArticlesPage();

  return (
    <SiteLayout
      headerVariant="authenticated"
      intro={
        <>
          <Text fontSize={{ base: "20px", sm: "24px", md: "30px" }} fontWeight="700" textAlign="center">
            Mes articles
          </Text>
          <Text color="ink.500" fontSize={{ base: "16px", sm: "17px", md: "18px" }} maxW="820px" textAlign="center">
            Retrouvez tous vos articles, qu&apos;ils soient publics ou privés, validés ou encore en attente de validation.
          </Text>
        </>
      }
    >
      <Stack spacing={{ base: 8, md: 10 }}>
        <Stack
          align={{ base: "stretch", lg: "end" }}
          direction={{ base: "column", lg: "row" }}
          justify="space-between"
          spacing={5}
        >
          <Text color="ink.500" fontSize={{ base: "15px", md: "16px" }}>
            {totalCount > 0
              ? `${totalCount} article${totalCount > 1 ? "s" : ""} trouve${totalCount > 1 ? "s" : ""}.`
              : "Vous n'avez encore aucun article."}
          </Text>

          <Button as="a" href="/articles/nouveau">
            Créer un article
          </Button>
        </Stack>

        <SimpleGrid bg="white" border="1px solid" borderColor="canvas.200" columns={{ base: 1, md: 2, xl: 5 }} gap={5} p={{ base: 5, md: 6 }} rounded="16px">
          <FormControl gridColumn={{ base: "auto", xl: "span 2" }}>
            <FormLabel color="ink.800" fontSize={{ base: "15px", md: "16px" }} fontWeight="700">
              Rechercher
            </FormLabel>
            <Input placeholder="Titre ou contenu" value={filters.keyword} onChange={(event) => updateFilter("keyword", event.target.value)} />
          </FormControl>

          <FormControl>
            <FormLabel color="ink.800" fontSize={{ base: "15px", md: "16px" }} fontWeight="700">
              Catégorie
            </FormLabel>
            <Select value={filters.idCategory} onChange={(event) => updateFilter("idCategory", event.target.value ? Number(event.target.value) : "")}>
              <option value="">Toutes les catégories</option>
              {categories.map((category) => (
                <option key={category.idCategory} value={category.idCategory}>
                  {category.name}
                </option>
              ))}
            </Select>
          </FormControl>

          <FormControl>
            <FormLabel color="ink.800" fontSize={{ base: "15px", md: "16px" }} fontWeight="700">
              Visibilité
            </FormLabel>
            <Select value={filters.visibility} onChange={(event) => updateFilter("visibility", event.target.value as "PUBLIC" | "PRIVATE" | "")}>
              <option value="">Toutes</option>
              <option value="PUBLIC">Public</option>
              <option value="PRIVATE">Privé</option>
            </Select>
          </FormControl>

          <FormControl>
            <FormLabel color="ink.800" fontSize={{ base: "15px", md: "16px" }} fontWeight="700">
              Validation
            </FormLabel>
            <Select value={filters.approval} onChange={(event) => updateFilter("approval", event.target.value as "approved" | "pending" | "")}>
              <option value="">Tous les statuts</option>
              <option value="approved">Validé</option>
              <option value="pending">En attente</option>
            </Select>
          </FormControl>

          <Stack align={{ base: "stretch", xl: "end" }} justify="end">
            <Button onClick={() => {
              void submitFilters();
            }}>
              Appliquer les filtres
            </Button>
          </Stack>
        </SimpleGrid>

        {message ? <MessageBanner message={message.message} title={message.title} tone={message.tone} /> : null}

        {isLoading ? (
          <SimpleGrid columns={{ base: 1, md: 2, xl: 3 }} spacing={{ base: 5, md: 6 }}>
            {Array.from({ length: 3 }).map((_, index) => (
              <Skeleton borderRadius="16px" height="260px" key={index} />
            ))}
          </SimpleGrid>
        ) : (
          <ArticlesGrid
            articles={articles}
            categories={categories}
            emptyLabel="Aucun de vos articles ne correspond aux filtres sélectionnés."
            resolveHref={(article) => `/articles/${article.idResource}/modifier`}
            showStatusBadges
            ctaLabel="Voir ou modifier"
          />
        )}

        <HStack justify="space-between" spacing={4}>
          <Button
            isDisabled={page <= 1 || isLoading}
            onClick={() => {
              void goToPage(page - 1);
            }}
            variant="outline"
          >
            Page précédente
          </Button>
          <Text color="ink.500" fontSize={{ base: "14px", md: "15px" }}>
            Page {page} {totalPages > 0 ? `sur ${totalPages}` : ""}
          </Text>
          <Button
            isDisabled={isLoading || totalPages === 0 || page >= totalPages}
            onClick={() => {
              void goToPage(page + 1);
            }}
            variant="outline"
          >
            Page suivante
          </Button>
        </HStack>
      </Stack>
    </SiteLayout>
  );
}
