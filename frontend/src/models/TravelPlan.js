export function createTravelPlan(data) {
  return {
    id: data.id,
    userId: data.userId,
    name: data.name,
    description: data.description ?? '',
    startDate: data.startDate,
    endDate: data.endDate,
    plannedBudget: data.plannedBudget,
    notes: data.notes ?? '',
    createdAt: data.createdAt,
    updatedAt: data.updatedAt,
  };
}
