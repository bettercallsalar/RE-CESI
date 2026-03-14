import { SimpleGrid, Skeleton } from "@chakra-ui/react";

interface ContentGridSkeletonProps {
  count?: number;
}

export function ContentGridSkeleton({ count = 3 }: ContentGridSkeletonProps) {
  return (
    <SimpleGrid columns={{ base: 1, md: 2, xl: 3 }} spacing={{ base: 5, md: 6 }}>
      {Array.from({ length: count }).map((_, index) => (
        <Skeleton borderRadius="16px" height="360px" key={index} />
      ))}
    </SimpleGrid>
  );
}
