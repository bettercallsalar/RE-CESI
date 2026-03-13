import { Box, SimpleGrid, Text } from "@chakra-ui/react";
import type { Article, Category } from "@/shared/types/article";
import { ArticleCard } from "@/features/articles/components/ArticleCard";

interface ArticlesGridProps {
  articles: Article[];
  categories: Category[];
  emptyLabel: string;
  compact?: boolean;
}

export function ArticlesGrid({ articles, categories, emptyLabel, compact = false }: ArticlesGridProps) {
  function getCategoryName(idCategory: number) {
    return categories.find((category) => category.idCategory === idCategory)?.name;
  }

  if (articles.length === 0) {
    return (
      <Box bg="white" border="1px solid" borderColor="canvas.200" rounded="16px" px={{ base: 5, md: 6 }} py={{ base: 6, md: 7 }}>
        <Text color="ink.500" fontSize={{ base: "15px", md: "16px" }}>
          {emptyLabel}
        </Text>
      </Box>
    );
  }

  return (
    <SimpleGrid columns={{ base: 1, md: 2, xl: 3 }} spacing={{ base: 5, md: 6 }}>
      {articles.map((article) => (
        <ArticleCard article={article} categoryName={getCategoryName(article.idCategory)} compact={compact} key={article.idResource} />
      ))}
    </SimpleGrid>
  );
}
