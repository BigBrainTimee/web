export const DEFAULT_PACKING_TITLES = [
  'Pasoš',
  'Karta',
  'Rezervacija smeštaja',
  'Putno osiguranje',
  'Punjač',
  'Garderoba',
];

export const CUSTOM_PACKING_SORT_ORDER = 100;

const defaultTitleSet = new Set(DEFAULT_PACKING_TITLES.map((title) => title.toLowerCase()));

export function isDefaultPackingItem(title) {
  return defaultTitleSet.has(title.trim().toLowerCase());
}

export function splitPackingItems(items) {
  const defaults = [];
  const custom = [];

  for (const item of items) {
    if (isDefaultPackingItem(item.title)) {
      defaults.push(item);
    } else {
      custom.push(item);
    }
  }

  const sortItems = (left, right) => left.sortOrder - right.sortOrder || left.id - right.id;
  defaults.sort(sortItems);
  custom.sort(sortItems);

  return { defaults, custom };
}
