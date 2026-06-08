import { createExpense as mapExpense, createBudgetSummary as mapBudgetSummary } from '../models/Expense';
import { apiRequest } from './apiClient';

const BASE = '/api/budget/travel-plans';

export async function getExpenses(token, planId) {
  const data = await apiRequest(`${BASE}/${planId}/expenses`, { token });
  return data.map(mapExpense);
}

export async function createExpense(token, planId, payload) {
  const data = await apiRequest(`${BASE}/${planId}/expenses`, {
    method: 'POST',
    token,
    body: payload,
  });
  return mapExpense(data);
}

export async function updateExpense(token, planId, expenseId, payload) {
  const data = await apiRequest(`${BASE}/${planId}/expenses/${expenseId}`, {
    method: 'PUT',
    token,
    body: payload,
  });
  return mapExpense(data);
}

export async function deleteExpense(token, planId, expenseId) {
  return apiRequest(`${BASE}/${planId}/expenses/${expenseId}`, {
    method: 'DELETE',
    token,
  });
}

export async function getBudgetSummary(token, planId) {
  const data = await apiRequest(`${BASE}/${planId}/summary`, { token });
  return mapBudgetSummary(data);
}
