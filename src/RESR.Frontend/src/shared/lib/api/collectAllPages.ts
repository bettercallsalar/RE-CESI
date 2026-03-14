interface PaginatedItemsResponse<T> {
  items: T[];
  totalPages: number;
}

export async function collectAllPages<T>(
  loadPage: (page: number) => Promise<PaginatedItemsResponse<T>>
): Promise<T[]> {
  const firstPage = await loadPage(1);
  const items = [...firstPage.items];

  for (let page = 2; page <= firstPage.totalPages; page += 1) {
    const nextPage = await loadPage(page);
    items.push(...nextPage.items);
  }

  return items;
}
