import { Box, Skeleton, Stack, Text } from "@chakra-ui/react";
import { ArticleForm } from "@/features/articles/components/ArticleForm";
import { useEditArticleForm } from "@/features/articles/hooks/useEditArticleForm";
import { MessageBanner } from "@/shared/ui/feedback/MessageBanner";

interface EditArticleFormProps {
  idResource: number;
}

export function EditArticleForm({ idResource }: EditArticleFormProps) {
  const {
    values,
    categories,
    existingFiles,
    canEdit,
    isLoading,
    isLoadingCategories,
    isSubmitting,
    message,
    updateField,
    submit
  } = useEditArticleForm(idResource);

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
            message="Seul l'auteur de cet article peut accéder au mode édition."
            title="Accès refusé"
            tone="warning"
          />
          <Text color="ink.500" fontSize={{ base: "15px", md: "16px" }}>
            Revenez à la fiche article publique pour consulter son contenu.
          </Text>
        </Stack>
      </Box>
    );
  }

  return (
    <ArticleForm
      categories={categories}
      existingFiles={existingFiles}
      isLoadingCategories={isLoadingCategories}
      isSubmitting={isSubmitting}
      message={message}
      mode="edit"
      submit={submit}
      updateField={updateField}
      values={values}
    />
  );
}
