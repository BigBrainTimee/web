import { createActivity as mapActivity } from '../models/Activity';
import { createChecklistItem as mapChecklistItem } from '../models/ChecklistItem';
import { createDestination as mapDestination } from '../models/Destination';
import { createShareLink as mapShareLink } from '../models/ShareLink';
import { createTravelPlan as mapTravelPlan } from '../models/TravelPlan';
import { sanitizeActivityPayload } from '../utils/activityPayload';
import { apiDownload, apiRequest } from './apiClient';

const planPath = (userId, planId) => `/api/travel/admin/users/${userId}/travel-plans/${planId}`;
const userPath = (userId) => `/api/travel/admin/users/${userId}`;

export async function getUserTravelPlans(token, userId) {
  const data = await apiRequest(`${userPath(userId)}/travel-plans`, { token });
  return data.map(mapTravelPlan);
}

export async function createUserTravelPlan(token, userId, payload) {
  const data = await apiRequest(`${userPath(userId)}/travel-plans`, {
    method: 'POST',
    token,
    body: payload,
  });
  return mapTravelPlan(data);
}

export async function getUserTravelPlan(token, userId, planId) {
  const data = await apiRequest(planPath(userId, planId), { token });
  return mapTravelPlan(data);
}

export async function updateUserTravelPlan(token, userId, planId, payload) {
  const data = await apiRequest(planPath(userId, planId), {
    method: 'PUT',
    token,
    body: payload,
  });
  return mapTravelPlan(data);
}

export async function deleteUserTravelPlan(token, userId, planId) {
  return apiRequest(planPath(userId, planId), {
    method: 'DELETE',
    token,
  });
}

export async function downloadUserPlanReport(token, userId, planId) {
  const { blob, fileName } = await apiDownload(`${planPath(userId, planId)}/report/pdf`, { token });
  const url = URL.createObjectURL(blob);
  const link = document.createElement('a');
  link.href = url;
  link.download = fileName;
  link.click();
  URL.revokeObjectURL(url);
}

export async function getUserDestinations(token, userId, planId) {
  const data = await apiRequest(`${planPath(userId, planId)}/destinations`, { token });
  return data.map(mapDestination);
}

export async function createUserDestination(token, userId, planId, payload) {
  const data = await apiRequest(`${planPath(userId, planId)}/destinations`, {
    method: 'POST',
    token,
    body: payload,
  });
  return mapDestination(data);
}

export async function updateUserDestination(token, userId, planId, destinationId, payload) {
  const data = await apiRequest(`${planPath(userId, planId)}/destinations/${destinationId}`, {
    method: 'PUT',
    token,
    body: payload,
  });
  return mapDestination(data);
}

export async function deleteUserDestination(token, userId, planId, destinationId) {
  return apiRequest(`${planPath(userId, planId)}/destinations/${destinationId}`, {
    method: 'DELETE',
    token,
  });
}

export async function getUserActivities(token, userId, planId) {
  const data = await apiRequest(`${planPath(userId, planId)}/activities`, { token });
  return data.map(mapActivity);
}

export async function createUserActivity(token, userId, planId, payload) {
  const data = await apiRequest(`${planPath(userId, planId)}/activities`, {
    method: 'POST',
    token,
    body: sanitizeActivityPayload(payload),
  });
  return mapActivity(data);
}

export async function updateUserActivity(token, userId, planId, activityId, payload) {
  const data = await apiRequest(`${planPath(userId, planId)}/activities/${activityId}`, {
    method: 'PUT',
    token,
    body: sanitizeActivityPayload(payload),
  });
  return mapActivity(data);
}

export async function deleteUserActivity(token, userId, planId, activityId) {
  return apiRequest(`${planPath(userId, planId)}/activities/${activityId}`, {
    method: 'DELETE',
    token,
  });
}

export async function getUserChecklistItems(token, userId, planId) {
  const data = await apiRequest(`${planPath(userId, planId)}/checklist-items`, { token });
  return data.map(mapChecklistItem);
}

export async function createUserChecklistItem(token, userId, planId, payload) {
  const data = await apiRequest(`${planPath(userId, planId)}/checklist-items`, {
    method: 'POST',
    token,
    body: payload,
  });
  return mapChecklistItem(data);
}

export async function toggleUserChecklistItem(token, userId, planId, itemId) {
  const data = await apiRequest(`${planPath(userId, planId)}/checklist-items/${itemId}/toggle`, {
    method: 'PATCH',
    token,
  });
  return mapChecklistItem(data);
}

export async function deleteUserChecklistItem(token, userId, planId, itemId) {
  return apiRequest(`${planPath(userId, planId)}/checklist-items/${itemId}`, {
    method: 'DELETE',
    token,
  });
}

export async function getUserShareLinks(token, userId, planId) {
  const data = await apiRequest(`${planPath(userId, planId)}/share-links`, { token });
  return data.map(mapShareLink);
}

export async function createUserShareLink(token, userId, planId, payload) {
  const data = await apiRequest(`${planPath(userId, planId)}/share-links`, {
    method: 'POST',
    token,
    body: payload,
  });
  return mapShareLink(data);
}

export async function deleteUserShareLink(token, userId, planId, linkId) {
  return apiRequest(`${planPath(userId, planId)}/share-links/${linkId}`, {
    method: 'DELETE',
    token,
  });
}

export function createAdminShareApi(token, userId, planId) {
  return {
    getShareLinks: (authToken, ignoredPlanId) => getUserShareLinks(authToken ?? token, userId, planId),
    createShareLink: (authToken, ignoredPlanId, payload) => createUserShareLink(authToken ?? token, userId, planId, payload),
    deleteShareLink: (authToken, ignoredPlanId, linkId) => deleteUserShareLink(authToken ?? token, userId, planId, linkId),
  };
}
