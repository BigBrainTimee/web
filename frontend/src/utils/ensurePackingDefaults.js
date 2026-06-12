import { DEFAULT_PACKING_TITLES } from '../constants/checklistDefaults';

export async function ensurePackingDefaults(createItem, items) {
  const existing = new Set(items.map((item) => item.title.trim().toLowerCase()));
  const missing = DEFAULT_PACKING_TITLES
    .map((title, index) => ({ title, sortOrder: index + 1 }))
    .filter((entry) => !existing.has(entry.title.toLowerCase()));

  if (missing.length === 0) {
    return items;
  }

  await Promise.all(missing.map((entry) => createItem(entry)));
  return null;
}
