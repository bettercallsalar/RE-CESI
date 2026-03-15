import { useRef, useState } from "react";
import {
  AlertDialog,
  AlertDialogBody,
  AlertDialogContent,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogOverlay,
  Box,
  Button,
  Checkbox,
  FormControl,
  FormLabel,
  HStack,
  Input,
  Select,
  SimpleGrid,
  Skeleton,
  Stack,
  Text,
  useDisclosure
} from "@chakra-ui/react";
import { SiteLayout } from "@/app/layouts/SiteLayout";
import { EventsGrid } from "@/features/events/components/EventsGrid";
import { useMyEventsPage } from "@/features/events/hooks/useMyEventsPage";
import { MessageBanner } from "@/shared/ui/feedback/MessageBanner";
import { DatePickerField } from "@/shared/ui/forms/DatePickerField";
import type { Event } from "@/shared/types/event";

export function MyEventsPage() {
  const {
    filters,
    categories,
    departments,
    events,
    isLoading,
    isDeleting,
    message,
    page,
    totalPages,
    totalCount,
    updateFilter,
    submitFilters,
    goToPage,
    deleteEvent
  } = useMyEventsPage();
  const { isOpen, onOpen, onClose } = useDisclosure();
  const cancelRef = useRef<HTMLButtonElement | null>(null);
  const [eventToDelete, setEventToDelete] = useState<Event | null>(null);
  const [isDeleteConfirmed, setIsDeleteConfirmed] = useState(false);

  function openDeleteDialog(event: Event) {
    setEventToDelete(event);
    setIsDeleteConfirmed(false);
    onOpen();
  }

  function closeDeleteDialog() {
    if (isDeleting) {
      return;
    }

    setEventToDelete(null);
    setIsDeleteConfirmed(false);
    onClose();
  }

  async function confirmDelete() {
    if (!eventToDelete) {
      return;
    }

    await deleteEvent(eventToDelete.idResource);
    closeDeleteDialog();
  }

  return (
    <SiteLayout
      headerVariant="authenticated"
      intro={
        <>
          <Text fontSize={{ base: "20px", sm: "24px", md: "30px" }} fontWeight="700" textAlign="center">
            Mes evenements
          </Text>
          <Text color="ink.500" fontSize={{ base: "16px", sm: "17px", md: "18px" }} maxW="820px" textAlign="center">
            Retrouvez tous vos evenements, qu'ils soient publics ou prives, valides, en attente de validation ou deja supprimes.
          </Text>
        </>
      }
    >
      <Stack spacing={{ base: 8, md: 10 }}>
        <Stack align={{ base: "stretch", lg: "end" }} direction={{ base: "column", lg: "row" }} justify="space-between" spacing={5}>
          <Text color="ink.500" fontSize={{ base: "15px", md: "16px" }}>
            {totalCount > 0
              ? `${totalCount} evenement${totalCount > 1 ? "s" : ""} trouve${totalCount > 1 ? "s" : ""}.`
              : "Vous n'avez encore aucun evenement, meme supprime."}
          </Text>

          <Button as="a" href="/events/nouveau">
            Creer un evenement
          </Button>
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
              Visibilite
            </FormLabel>
            <Select value={filters.visibility} onChange={(event) => updateFilter("visibility", event.target.value as "PUBLIC" | "PRIVATE" | "")}>
              <option value="">Toutes</option>
              <option value="PUBLIC">Public</option>
              <option value="PRIVATE">Prive</option>
            </Select>
          </FormControl>

          <FormControl>
            <FormLabel color="ink.800" fontSize={{ base: "15px", md: "16px" }} fontWeight="700">
              Validation
            </FormLabel>
            <Select value={filters.approval} onChange={(event) => updateFilter("approval", event.target.value as "approved" | "pending" | "")}>
              <option value="">Tous les statuts</option>
              <option value="approved">Valide</option>
              <option value="pending">En attente</option>
            </Select>
          </FormControl>

          <DatePickerField
            label="A partir du"
            max={filters.startTo || undefined}
            onChange={(value) => updateFilter("startFrom", value)}
            value={filters.startFrom}
          />

          <DatePickerField
            label="Jusqu'au"
            min={filters.startFrom || undefined}
            onChange={(value) => updateFilter("startTo", value)}
            value={filters.startTo}
          />

          <Stack align={{ base: "stretch", xl: "end" }} gridColumn={{ base: "auto", xl: "span 5" }} justify="end">
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
          <EventsGrid
            categories={categories}
            emptyLabel="Aucun de vos evenements ne correspond aux filtres selectionnes."
            events={events}
            resolveActions={(event) =>
              event.deletedAt
                ? [
                    { href: `/events/${event.idResource}`, label: "Voir", variant: "outline" },
                    {
                      label: "Evenement supprime",
                      variant: "solid",
                      tone: "dangerSoft",
                      isDisabled: true
                    }
                  ]
                : [
                    { href: `/events/${event.idResource}`, label: "Voir", variant: "outline" },
                    { href: `/events/${event.idResource}/modifier`, label: "Modifier" },
                    { label: "Supprimer", onClick: () => openDeleteDialog(event), tone: "danger" }
                  ]
            }
            showStatusBadges
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
            Page precedente
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

        <AlertDialog isCentered isOpen={isOpen} leastDestructiveRef={cancelRef} onClose={closeDeleteDialog}>
          <AlertDialogOverlay bg="blackAlpha.600" backdropFilter="blur(6px)">
            <AlertDialogContent bg="white" border="1px solid" borderColor="red.100" boxShadow="2xl" color="ink.800" mx={4} rounded="20px">
              <Box bg="linear-gradient(135deg, #fff5f5 0%, #ffffff 70%)" borderTopLeftRadius="20px" borderTopRightRadius="20px" px={6} pt={6}>
                <AlertDialogHeader color="red.600" fontSize={{ base: "22px", md: "24px" }} fontWeight="800" px={0} py={0}>
                  Confirmation de suppression
                </AlertDialogHeader>
                <Text color="ink.500" fontSize={{ base: "14px", md: "15px" }} mt={2} pb={5}>
                  Cette action retire definitivement l'evenement de votre espace public et prive.
                </Text>
              </Box>

              <AlertDialogBody pb={5} pt={2}>
                <Stack spacing={4}>
                  <Box bg="red.50" border="1px solid" borderColor="red.100" rounded="14px" px={4} py={4}>
                    <Text color="ink.800" fontSize={{ base: "16px", md: "17px" }} lineHeight="1.7">
                      Voulez-vous vraiment supprimer l'evenement {eventToDelete ? `"${eventToDelete.title}"` : ""} ? Cette suppression est irreversible et l'evenement ne pourra pas etre restaure.
                    </Text>
                  </Box>

                  <Checkbox colorScheme="red" isChecked={isDeleteConfirmed} onChange={(event) => setIsDeleteConfirmed(event.target.checked)}>
                    <Text color="ink.800" fontSize={{ base: "14px", md: "15px" }}>
                      Je confirme que cet evenement ne pourra pas etre restaure.
                    </Text>
                  </Checkbox>
                </Stack>
              </AlertDialogBody>

              <AlertDialogFooter gap={3} pb={6} pt={0}>
                <Button ref={cancelRef} borderColor="canvas.300" color="ink.800" onClick={closeDeleteDialog} variant="outline">
                  Annuler
                </Button>
                <Button
                  bg="red.500"
                  color="surface.onCritical"
                  isDisabled={!isDeleteConfirmed}
                  isLoading={isDeleting}
                  loadingText="Suppression"
                  onClick={() => {
                    void confirmDelete();
                  }}
                  _hover={{ bg: "red.600" }}
                >
                  Supprimer definitivement
                </Button>
              </AlertDialogFooter>
            </AlertDialogContent>
          </AlertDialogOverlay>
        </AlertDialog>
      </Stack>
    </SiteLayout>
  );
}
