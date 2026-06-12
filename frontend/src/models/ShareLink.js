export function parseApiDate(value) {
  if (!value) return null;
  if (typeof value === 'string') {
    const hasTimezone = value.endsWith('Z') || /[+-]\d{2}:\d{2}$/.test(value);
    return new Date(hasTimezone ? value : `${value}Z`);
  }
  return new Date(value);
}

export function isShareLinkExpired(expiresAt) {
  if (!expiresAt) return false;
  const expiry = parseApiDate(expiresAt);
  return !Number.isNaN(expiry.getTime()) && expiry.getTime() <= Date.now();
}

export function formatApiDate(value) {
  const date = parseApiDate(value);
  if (!date || Number.isNaN(date.getTime())) return '';
  return date.toLocaleString('sr-RS');
}

export function createShareLink(data) {
  return {
    id: data.id,
    travelPlanId: data.travelPlanId,
    token: data.token,
    accessType: data.accessType,
    createdAt: data.createdAt,
    expiresAt: data.expiresAt ?? null,
  };
}

export function createSharedPlan(data) {
  return {
    accessType: data.accessType,
    canEdit: data.canEdit,
    plan: {
      id: data.plan.id,
      name: data.plan.name,
      description: data.plan.description ?? '',
      startDate: data.plan.startDate,
      endDate: data.plan.endDate,
      plannedBudget: data.plan.plannedBudget,
      notes: data.plan.notes ?? '',
    },
    destinations: (data.destinations ?? []).map((d) => ({
      id: d.id,
      name: d.name,
      location: d.location,
      arrivalDate: d.arrivalDate,
      departureDate: d.departureDate,
      description: d.description ?? '',
    })),
    activities: (data.activities ?? []).map((a) => ({
      id: a.id,
      name: a.name,
      activityDate: a.activityDate,
      activityTime: a.activityTime ?? null,
      location: a.location ?? '',
      description: a.description ?? '',
      status: a.status,
      estimatedCost: a.estimatedCost ?? null,
    })),
    checklistItems: (data.checklistItems ?? []).map((c) => ({
      id: c.id,
      title: c.title,
      isCompleted: c.isCompleted,
      sortOrder: c.sortOrder,
    })),
    budgetSummary: data.budgetSummary
      ? {
          travelPlanId: data.budgetSummary.travelPlanId,
          plannedBudget: data.budgetSummary.plannedBudget,
          totalSpent: data.budgetSummary.totalSpent,
          remaining: data.budgetSummary.remaining,
          byCategory: (data.budgetSummary.byCategory ?? []).map((item) => ({
            category: item.category,
            amount: item.amount,
          })),
        }
      : null,
    expenses: (data.expenses ?? []).map((e) => ({
      id: e.id,
      travelPlanId: e.travelPlanId,
      name: e.name,
      category: e.category,
      amount: e.amount,
      expenseDate: e.expenseDate,
      description: e.description ?? '',
    })),
  };
}

export function buildShareUrl(token) {
  const configuredBase = import.meta.env.VITE_SHARE_BASE_URL?.replace(/\/$/, '');
  const base = configuredBase || window.location.origin;
  return `${base}/shared/${token}`;
}
