export function createChecklistItem(data) {
  return {
    id: data.id,
    travelPlanId: data.travelPlanId,
    title: data.title,
    isCompleted: data.isCompleted,
    sortOrder: data.sortOrder,
  };
}
