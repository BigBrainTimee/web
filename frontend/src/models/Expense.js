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
