import { Box, Skeleton, Stack, Text } from "@chakra-ui/react";
import { EventForm } from "@/features/events/components/EventForm";
import { useEditEventForm } from "@/features/events/hooks/useEditEventForm";
import { MessageBanner } from "@/shared/ui/feedback/MessageBanner";

interface EditEventFormProps {
  idResource: number;
}

export function EditEventForm({ idResource }: EditEventFormProps) {
  const {
    values,
    categories,
    departments,
    existingFiles,
    canEdit,
    isLoading,
    isLoadingCategories,
    isLoadingDepartments,
    isSubmitting,
    message,
    updateField,
    submit
  } = useEditEventForm(idResource);

  if (isLoading || !values) {
    if (!isLoading && message) {
      return <MessageBanner message={message.message} title={message.title} tone={message.tone} />;
    }

    return (
      <Stack spacing={4}>
        <Skeleton borderRadius="16px" height="72px" />
        <Skeleton borderRadius="16px" height="540px" />
      </Stack>
    );
  }

  if (!canEdit) {
    return (
      <Box bg="white" border="1px solid" borderColor="canvas.200" rounded="16px" px={{ base: 6, md: 7 }} py={{ base: 7, md: 8 }}>
        <Stack spacing={4}>
          <MessageBanner
            message="Seul l'auteur de cet evenement peut acceder au mode edition."
            title="Acces refuse"
            tone="warning"
          />
          <Text color="ink.500" fontSize={{ base: "15px", md: "16px" }}>
            Revenez a la fiche evenement pour consulter ses informations.
          </Text>
        </Stack>
      </Box>
    );
  }

  return (
    <EventForm
      categories={categories}
      departments={departments}
      existingFiles={existingFiles}
      isLoadingCategories={isLoadingCategories}
      isLoadingDepartments={isLoadingDepartments}
      isSubmitting={isSubmitting}
      message={message}
      mode="edit"
      submit={submit}
      updateField={updateField}
      values={values}
    />
  );
}
