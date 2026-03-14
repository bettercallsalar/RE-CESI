interface PaginatedItems<T> {
  items: T[];
  totalPages: number;
}

export async function loadAllPaginatedItems<T>(fetchPage: (page: number) => Promise<PaginatedItems<T>>) {
  const firstPage = await fetchPage(1);
  const items = [...firstPage.items];

  if (firstPage.totalPages <= 1) {
    return items;
  }

  const remainingPages = await Promise.all(
    Array.from({ length: firstPage.totalPages - 1 }, (_, index) => fetchPage(index + 2))
  );

  for (const page of remainingPages) {
    items.push(...page.items);
  }

  return items;
}

export function sortByCreatedAtDesc<T extends { createdAt: string }>(items: T[]) {
  return [...items].sort((left, right) => Date.parse(right.createdAt) - Date.parse(left.createdAt));
}
