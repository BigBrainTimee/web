import { createActivity as mapActivity } from '../models/Activity';
import { createChecklistItem as mapChecklistItem } from '../models/ChecklistItem';
import { createDestination as mapDestination } from '../models/Destination';
import { createExpense as mapExpense } from '../models/Expense';
import { createShareLink as mapShareLink, createSharedPlan as mapSharedPlan } from '../models/ShareLink';
import { sanitizeActivityPayload } from '../utils/activityPayload';
import { apiRequest } from './apiClient';

const PLAN_BASE = '/api/travel/travel-plans';
const SHARED_BASE = '/api/travel/shared';
const SHARED_BUDGET_BASE = '/api/budget/shared';

export async function getShareLinks(token, planId) {
  const data = await apiRequest(`${PLAN_BASE}/${planId}/share-links`, { token });
  return data.map(mapShareLink);
}

export async function createShareLink(token, planId, payload) {
  const data = await apiRequest(`${PLAN_BASE}/${planId}/share-links`, {
    method: 'POST',
    token,
    body: payload,
  });
  return mapShareLink(data);
}

export async function deleteShareLink(token, planId, linkId) {
  return apiRequest(`${PLAN_BASE}/${planId}/share-links/${linkId}`, {
    method: 'DELETE',
    token,
  });
}

export async function getSharedPlan(shareToken, authToken) {
  const data = await apiRequest(`${SHARED_BASE}/${shareToken}`, { token: authToken });
  return mapSharedPlan(data);
}

export async function addSharedDestination(shareToken, payload, authToken) {
  const data = await apiRequest(`${SHARED_BASE}/${shareToken}/destinations`, {
    method: 'POST',
    token: authToken,
    body: payload,
  });
  return mapDestination(data);
}

export async function deleteSharedDestination(shareToken, destinationId, authToken) {
  return apiRequest(`${SHARED_BASE}/${shareToken}/destinations/${destinationId}`, {
    method: 'DELETE',
    token: authToken,
  });
}

export async function updateSharedDestination(shareToken, destinationId, payload, authToken) {
  const data = await apiRequest(`${SHARED_BASE}/${shareToken}/destinations/${destinationId}`, {
    method: 'PUT',
    token: authToken,
    body: payload,
  });
  return mapDestination(data);
}

export async function addSharedActivity(shareToken, payload, authToken) {
  const data = await apiRequest(`${SHARED_BASE}/${shareToken}/activities`, {
    method: 'POST',
    token: authToken,
    body: sanitizeActivityPayload(payload),
  });
  return mapActivity(data);
}

export async function deleteSharedActivity(shareToken, activityId, authToken) {
  return apiRequest(`${SHARED_BASE}/${shareToken}/activities/${activityId}`, {
    method: 'DELETE',
    token: authToken,
  });
}

export async function updateSharedActivity(shareToken, activityId, payload, authToken) {
  const data = await apiRequest(`${SHARED_BASE}/${shareToken}/activities/${activityId}`, {
    method: 'PUT',
    token: authToken,
    body: sanitizeActivityPayload(payload),
  });
  return mapActivity(data);
}

export async function addSharedExpense(shareToken, payload, authToken) {
  const data = await apiRequest(`${SHARED_BUDGET_BASE}/${shareToken}/expenses`, {
    method: 'POST',
    token: authToken,
    body: payload,
  });
  return mapExpense(data);
}

export async function deleteSharedExpense(shareToken, expenseId, authToken) {
  return apiRequest(`${SHARED_BUDGET_BASE}/${shareToken}/expenses/${expenseId}`, {
    method: 'DELETE',
    token: authToken,
  });
}

export async function addSharedChecklistItem(shareToken, payload, authToken) {
  const data = await apiRequest(`${SHARED_BASE}/${shareToken}/checklist-items`, {
    method: 'POST',
    token: authToken,
    body: payload,
  });
  return mapChecklistItem(data);
}

export async function toggleSharedChecklistItem(shareToken, itemId, authToken) {
  const data = await apiRequest(`${SHARED_BASE}/${shareToken}/checklist-items/${itemId}/toggle`, {
    method: 'PATCH',
    token: authToken,
  });
  return mapChecklistItem(data);
}

export async function deleteSharedChecklistItem(shareToken, itemId, authToken) {
  return apiRequest(`${SHARED_BASE}/${shareToken}/checklist-items/${itemId}`, {
    method: 'DELETE',
    token: authToken,
  });
}
