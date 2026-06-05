export function createDestination(data) {
  return {
    id: data.id,
    travelPlanId: data.travelPlanId,
    name: data.name,
    location: data.location,
    arrivalDate: data.arrivalDate,
    departureDate: data.departureDate,
    description: data.description ?? '',
  };
}
