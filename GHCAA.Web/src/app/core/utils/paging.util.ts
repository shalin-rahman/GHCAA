// Offset paging shifts rows when one is added between loads, so the next page can repeat a row already shown.
export function appendById<T extends { id: unknown }>(list: T[], next: T[]): T[] {
  const loaded = new Set(list.map(item => item.id));
  return [...list, ...next.filter(item => !loaded.has(item.id))];
}
