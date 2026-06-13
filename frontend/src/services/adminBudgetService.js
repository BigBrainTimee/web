import { createExpense as mapExpense, createBudgetSummary as mapBudgetSummary } from '../models/Expense';
import { apiRequest } from './apiClient';

const planPath = (userId, planId) => `/api/budget/admin/users/${userId}/travel-plans/${planId}`;

export async function getUserExpenses(token, userId, planId) {
  const data = await apiRequest(`${planPath(userId, planId)}/expenses`, { token });
  return data.map(mapExpense);
}

export async function createUserExpense(token, userId, planId, payload) {
  const data = await apiRequest(`${planPath(userId, planId)}/expenses`, {
    method: 'POST',
    token,
    body: payload,
  });
  return mapExpense(data);
}

export async function deleteUserExpense(token, userId, planId, expenseId) {
  return apiRequest(`${planPath(userId, planId)}/expenses/${expenseId}`, {
    method: 'DELETE',
    token,
  });
}

export async function getUserBudgetSummary(token, userId, planId) {
  const data = await apiRequest(`${planPath(userId, planId)}/summary`, { token });
  return mapBudgetSummary(data);
}
