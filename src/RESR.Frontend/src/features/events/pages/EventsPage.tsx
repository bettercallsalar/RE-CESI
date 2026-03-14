import { Button, FormControl, FormLabel, HStack, Input, Select, SimpleGrid, Skeleton, Stack, Text } from "@chakra-ui/react";
import { useEffect, useState } from "react";
import { SiteLayout } from "@/app/layouts/SiteLayout";
import { useAuth } from "@/features/auth/hooks/useAuth";
import { EventsGrid } from "@/features/events/components/EventsGrid";
import { useEventsPage } from "@/features/events/hooks/useEventsPage";
import { flashMessageStorage } from "@/shared/lib/storage/flashMessageStorage";
import { MessageBanner } from "@/shared/ui/feedback/MessageBanner";
import type { FeedbackMessage } from "@/shared/ui/feedback/message.types";

export function EventsPage() {
  const { status } = useAuth();
  const {
    filters,
    categories,
    departments,
    events,
    isLoading,
    message,
    page,
    totalPages,
    totalCount,
    updateFilter,
    submitFilters,
    goToPage
  } = useEventsPage();
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
            Evenements publies
          </Text>
          <Text color="ink.500" fontSize={{ base: "16px", sm: "17px", md: "18px" }} maxW="760px" textAlign="center">
            Consultez les prochains evenements valides et filtrez-les par mot-cle, categorie, departement ou date.
          </Text>
        </>
      }
    >
      <Stack spacing={{ base: 8, md: 10 }}>
        <Stack align={{ base: "stretch", lg: "end" }} direction={{ base: "column", lg: "row" }} justify="space-between" spacing={5}>
          <Text color="ink.500" fontSize={{ base: "15px", md: "16px" }}>
            {totalCount > 0 ? `${totalCount} evenement${totalCount > 1 ? "s" : ""} disponible${totalCount > 1 ? "s" : ""}.` : "Aucun evenement public pour le moment."}
          </Text>

          {status === "authenticated" ? (
            <Button as="a" href="/events/nouveau">
              Creer un evenement
            </Button>
          ) : status === "unauthenticated" ? (
            <Button as="a" href="/login" variant="outline">
              Se connecter pour publier
            </Button>
          ) : null}
        </Stack>

        <SimpleGrid bg="white" border="1px solid" borderColor="canvas.200" columns={{ base: 1, md: 2, xl: 5 }} gap={5} p={{ base: 5, md: 6 }} rounded="16px">
          <FormControl gridColumn={{ base: "auto", xl: "span 2" }}>
            <FormLabel color="ink.800" fontSize={{ base: "15px", md: "16px" }} fontWeight="700">
              Rechercher
            </FormLabel>
            <Input placeholder="Titre, sous-titre ou adresse" value={filters.keyword} onChange={(event) => updateFilter("keyword", event.target.value)} />
          </FormControl>

          <FormControl>
            <FormLabel color="ink.800" fontSize={{ base: "15px", md: "16px" }} fontWeight="700">
              Categorie
            </FormLabel>
            <Select value={filters.idCategory} onChange={(event) => updateFilter("idCategory", event.target.value ? Number(event.target.value) : "")}>
              <option value="">Toutes les categories</option>
              {categories.map((category) => (
                <option key={category.idCategory} value={category.idCategory}>
                  {category.name}
                </option>
              ))}
            </Select>
          </FormControl>

          <FormControl>
            <FormLabel color="ink.800" fontSize={{ base: "15px", md: "16px" }} fontWeight="700">
              Departement
            </FormLabel>
            <Select value={filters.idDepartment} onChange={(event) => updateFilter("idDepartment", event.target.value ? Number(event.target.value) : "")}>
              <option value="">Tous les departements</option>
              {departments.map((department) => (
                <option key={department.idDepartment} value={department.idDepartment}>
                  {department.code} - {department.name}
                </option>
              ))}
            </Select>
          </FormControl>

          <FormControl>
            <FormLabel color="ink.800" fontSize={{ base: "15px", md: "16px" }} fontWeight="700">
              A partir du
            </FormLabel>
            <Input type="date" value={filters.startFrom} onChange={(event) => updateFilter("startFrom", event.target.value)} />
          </FormControl>

          <FormControl>
            <FormLabel color="ink.800" fontSize={{ base: "15px", md: "16px" }} fontWeight="700">
              Jusqu'au
            </FormLabel>
            <Input type="date" value={filters.startTo} onChange={(event) => updateFilter("startTo", event.target.value)} />
          </FormControl>

          <Stack align={{ base: "stretch", xl: "end" }} gridColumn={{ base: "auto", xl: "span 5" }} justify="end">
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
          <EventsGrid categories={categories} emptyLabel="Aucun evenement ne correspond aux filtres selectionnes." events={events} />
        )}

        <HStack justify="space-between" spacing={4}>
          <Button isDisabled={page <= 1 || isLoading} onClick={() => {
            void goToPage(page - 1);
          }} variant="outline">
            Page precedente
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
