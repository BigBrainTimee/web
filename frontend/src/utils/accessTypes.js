export const ACCESS_TYPE_LABELS = {
  View: 'Pregled',
  Edit: 'Izmena',
};

export function getAccessTypeLabel(accessType) {
  return ACCESS_TYPE_LABELS[accessType] ?? accessType;
}
