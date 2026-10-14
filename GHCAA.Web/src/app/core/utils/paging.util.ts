// Offset paging shifts rows when one is added between loads, so the next page can repeat a row already shown.
export function appendById<T extends { id: unknown }>(list: T[], next: T[]): T[] {
  const loaded = new Set(list.map(item => item.id));
  return [...list, ...next.filter(item => !loaded.has(item.id))];
}

// The server caps every page, so asking for 10000 rows once only ever returned the first page.
// Reads page by page until totalPages is reached or a page comes back empty.
export async function fetchAllPages<T extends { id: unknown }>(
  fetchPage: (page: number) => Promise<{ items?: T[]; totalPages?: number }>
): Promise<T[]> {
  let all: T[] = [];
  for (let page = 1; ; page++) {
    const res = await fetchPage(page);
    const items = res?.items ?? [];
    all = appendById(all, items);
    if (items.length === 0 || page >= (res?.totalPages ?? 1)) return all;
  }
}
