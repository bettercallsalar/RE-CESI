import { ArticleForm } from "@/features/articles/components/ArticleForm";
import { useCreateArticleForm } from "@/features/articles/hooks/useCreateArticleForm";

export function CreateArticleForm() {
  const { values, categories, isLoadingCategories, isSubmitting, message, updateField, submit } = useCreateArticleForm();

  return (
    <ArticleForm
      categories={categories}
      isLoadingCategories={isLoadingCategories}
      isSubmitting={isSubmitting}
      message={message}
      mode="create"
      submit={submit}
      updateField={updateField}
      values={values}
    />
  );
}
