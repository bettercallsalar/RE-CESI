import { EventForm } from "@/features/events/components/EventForm";
import { useCreateEventForm } from "@/features/events/hooks/useCreateEventForm";

export function CreateEventForm() {
  const {
    values,
    categories,
    departments,
    isLoadingCategories,
    isLoadingDepartments,
    isSubmitting,
    message,
    updateField,
    submit
  } = useCreateEventForm();

  return (
    <EventForm
      categories={categories}
      departments={departments}
      isLoadingCategories={isLoadingCategories}
      isLoadingDepartments={isLoadingDepartments}
      isSubmitting={isSubmitting}
      message={message}
      mode="create"
      submit={submit}
      updateField={updateField}
      values={values}
    />
  );
}
