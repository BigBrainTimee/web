export function getEstimatedActivities(activities = []) {
  return activities
    .filter((activity) => activity.estimatedCost != null && activity.estimatedCost > 0)
    .sort((a, b) => {
      if (a.activityDate === b.activityDate) {
        return a.name.localeCompare(b.name);
      }
      return a.activityDate.localeCompare(b.activityDate);
    });
}

export function getEstimatedTotalFromActivities(activities = []) {
  return getEstimatedActivities(activities).reduce(
    (sum, activity) => sum + activity.estimatedCost,
    0,
  );
}

export function enrichBudgetSummary(summary, activities = []) {
  if (!summary) {
    return null;
  }

  const totalSpent = Number(summary.totalSpent ?? 0);
  const plannedBudget = Number(summary.plannedBudget ?? 0);
  const totalEstimated = activities.length > 0
    ? getEstimatedTotalFromActivities(activities)
    : Number(summary.totalEstimated ?? 0);
  const totalUsed = totalSpent + totalEstimated;
  const remaining = plannedBudget - totalSpent - totalEstimated;

  return {
    ...summary,
    plannedBudget,
    totalSpent,
    totalEstimated,
    totalUsed,
    remaining,
  };
}
