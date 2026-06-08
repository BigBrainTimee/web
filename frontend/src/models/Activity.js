export function createActivity(data) {
  return {
    id: data.id,
    travelPlanId: data.travelPlanId,
    destinationId: data.destinationId ?? null,
    activityDate: data.activityDate,
    activityTime: data.activityTime ?? null,
    name: data.name,
    location: data.location ?? '',
    description: data.description ?? '',
    estimatedCost: data.estimatedCost ?? null,
    status: data.status,
  };
}
