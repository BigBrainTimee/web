export function sanitizeActivityPayload(payload) {
  const result = { ...payload };
  const time = typeof result.activityTime === 'string' ? result.activityTime.trim() : result.activityTime;

  if (!time) {
    delete result.activityTime;
  } else {
    result.activityTime = time.length === 5 ? `${time}:00` : time;
  }

  if (result.description === '') delete result.description;
  if (result.destinationId === '' || result.destinationId == null) delete result.destinationId;

  return result;
}
