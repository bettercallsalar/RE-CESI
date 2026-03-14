import { Button, FormControl, FormLabel, HStack, Input, Select, SimpleGrid, Skeleton, Stack, Text } from "@chakra-ui/react";
import { useEffect, useState } from "react";
import { SiteLayout } from "@/app/layouts/SiteLayout";
import { useAuth } from "@/features/auth/hooks/useAuth";
import { ArticlesGrid } from "@/features/articles/components/ArticlesGrid";
import { useArticlesPage } from "@/features/articles/hooks/useArticlesPage";
import { flashMessageStorage } from "@/shared/lib/storage/flashMessageStorage";
import { MessageBanner } from "@/shared/ui/feedback/MessageBanner";
import { DatePickerField } from "@/shared/ui/forms/DatePickerField";
import type { FeedbackMessage } from "@/shared/ui/feedback/message.types";

export function ArticlesPage() {
  const { status } = useAuth();
  const { filters, categories, articles, isLoading, message, page, totalPages, totalCount, updateFilter, submitFilters, goToPage } = useArticlesPage();
  const [flashMessage, setFlashMessage] = useState<FeedbackMessage | null>(null);

  useEffect(() => {
    setFlashMessage(flashMessageStorage.take());
  }, []);

  return (
    <SiteLayout
      headerVariant={status === "authenticated" ? "authenticated" : "public"}
      intro={
        <>
          <Text fontSize={{ base: "20px", sm: "24px", md: "30px" }} fontWeight="700" textAlign="center">
            Articles publiés
          </Text>
          <Text color="ink.500" fontSize={{ base: "16px", sm: "17px", md: "18px" }} maxW="760px" textAlign="center">
            Consultez les dernières publications validées et filtrez-les par mot-clé, catégorie ou période.
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
            {totalCount > 0 ? `${totalCount} article${totalCount > 1 ? "s" : ""} disponible${totalCount > 1 ? "s" : ""}.` : "Aucun article public pour le moment."}
          </Text>

          {status === "authenticated" ? (
            <Button as="a" href="/articles/nouveau">
              Publier un article
            </Button>
          ) : status === "unauthenticated" ? (
            <Button as="a" href="/login" variant="outline">
              Se connecter pour publier
            </Button>
          ) : null}
        </Stack>

        <SimpleGrid bg="white" border="1px solid" borderColor="canvas.200" columns={{ base: 1, md: 2, xl: 5 }} gap={5} p={{ base: 5, md: 6 }} rounded="16px">
          <FormControl>
            <FormLabel color="ink.800" fontSize={{ base: "15px", md: "16px" }} fontWeight="700">
              Rechercher
            </FormLabel>
            <Input
              placeholder="Titre ou contenu"
              value={filters.keyword}
              onChange={(event) => updateFilter("keyword", event.target.value)}
            />
          </FormControl>

          <FormControl>
            <FormLabel color="ink.800" fontSize={{ base: "15px", md: "16px" }} fontWeight="700">
              Catégorie
            </FormLabel>
            <Select
              value={filters.idCategory}
              onChange={(event) => updateFilter("idCategory", event.target.value ? Number(event.target.value) : "")}
            >
              <option value="">Toutes les catégories</option>
              {categories.map((category) => (
                <option key={category.idCategory} value={category.idCategory}>
                  {category.name}
                </option>
              ))}
            </Select>
          </FormControl>

          <DatePickerField
            label="Publie depuis"
            max={filters.createdTo || undefined}
            onChange={(value) => updateFilter("createdFrom", value)}
            value={filters.createdFrom}
          />

          <DatePickerField
            label="Publie jusqu'au"
            min={filters.createdFrom || undefined}
            onChange={(value) => updateFilter("createdTo", value)}
            value={filters.createdTo}
          />

          <Stack align={{ base: "stretch", xl: "end" }} justify="end">
            <Button onClick={() => {
              void submitFilters();
            }}>
              Appliquer les filtres
            </Button>
          </Stack>
        </SimpleGrid>

        {flashMessage ? (
          <MessageBanner
            message={flashMessage.message}
            onClose={() => setFlashMessage(null)}
            title={flashMessage.title}
            tone={flashMessage.tone}
          />
        ) : null}

        {message ? <MessageBanner message={message.message} title={message.title} tone={message.tone} /> : null}

        {isLoading ? (
          <SimpleGrid columns={{ base: 1, md: 2, xl: 3 }} spacing={{ base: 5, md: 6 }}>
            {Array.from({ length: 3 }).map((_, index) => (
              <Skeleton borderRadius="16px" height="260px" key={index} />
            ))}
          </SimpleGrid>
        ) : (
          <ArticlesGrid articles={articles} categories={categories} emptyLabel="Aucun article ne correspond aux filtres sélectionnés." />
        )}

        <HStack justify="space-between" spacing={4}>
          <Button isDisabled={page <= 1 || isLoading} onClick={() => {
            void goToPage(page - 1);
          }} variant="outline">
            Page précédente
          </Button>
          <Text color="ink.500" fontSize={{ base: "14px", md: "15px" }}>
            Page {page} {totalPages > 0 ? `sur ${totalPages}` : ""}
          </Text>
          <Button isDisabled={isLoading || totalPages === 0 || page >= totalPages} onClick={() => {
            void goToPage(page + 1);
          }} variant="outline">
            Page suivante
          </Button>
        </HStack>
      </Stack>
    </SiteLayout>
  );
}
