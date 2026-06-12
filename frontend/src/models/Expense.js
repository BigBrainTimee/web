export function createExpense(data) {
  return {
    id: data.id,
    travelPlanId: data.travelPlanId,
    name: data.name,
    category: data.category,
    amount: data.amount,
    expenseDate: data.expenseDate,
    description: data.description ?? '',
  };
}

export function createBudgetSummary(data) {
  return {
    travelPlanId: data.travelPlanId,
    plannedBudget: data.plannedBudget,
    totalSpent: data.totalSpent,
    totalEstimated: data.totalEstimated ?? 0,
    remaining: data.remaining,
    byCategory: (data.byCategory ?? []).map((item) => ({
      category: item.category,
      amount: item.amount,
    })),
  };
}

export const EXPENSE_CATEGORIES = [
  'Transport',
  'Accommodation',
  'Food',
  'Tickets',
  'Shopping',
  'Other',
];

export const EXPENSE_CATEGORY_LABELS = {
  Transport: 'Prevoz',
  Accommodation: 'Smeštaj',
  Food: 'Hrana',
  Tickets: 'Karte',
  Shopping: 'Kupovina',
  Other: 'Ostalo',
};

export function getExpenseCategoryLabel(category) {
  return EXPENSE_CATEGORY_LABELS[category] ?? category;
}
