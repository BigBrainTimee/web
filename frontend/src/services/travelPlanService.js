import { createDestination as mapDestination } from '../models/Destination';
import { createTravelPlan as mapTravelPlan } from '../models/TravelPlan';
import { createActivity as mapActivity } from '../models/Activity';
import { createChecklistItem as mapChecklistItem } from '../models/ChecklistItem';
import { apiRequest } from './apiClient';

const BASE = '/api/travel/travel-plans';

export async function getTravelPlans(token) {
  const data = await apiRequest(BASE, { token });
  return data.map(mapTravelPlan);
}

export async function getTravelPlan(token, id) {
  const data = await apiRequest(`${BASE}/${id}`, { token });
  return mapTravelPlan(data);
}

export async function createTravelPlan(token, payload) {
  const data = await apiRequest(BASE, {
    method: 'POST',
    token,
    body: payload,
  });
  return mapTravelPlan(data);
}

export async function updateTravelPlan(token, id, payload) {
  const data = await apiRequest(`${BASE}/${id}`, {
    method: 'PUT',
    token,
    body: payload,
  });
  return mapTravelPlan(data);
}

export async function deleteTravelPlan(token, id) {
  return apiRequest(`${BASE}/${id}`, {
    method: 'DELETE',
    token,
  });
}

export async function getDestinations(token, planId) {
  const data = await apiRequest(`${BASE}/${planId}/destinations`, { token });
  return data.map(mapDestination);
}

export async function createDestination(token, planId, payload) {
  const data = await apiRequest(`${BASE}/${planId}/destinations`, {
    method: 'POST',
    token,
    body: payload,
  });
  return mapDestination(data);
}

export async function updateDestination(token, planId, destinationId, payload) {
  const data = await apiRequest(`${BASE}/${planId}/destinations/${destinationId}`, {
    method: 'PUT',
    token,
    body: payload,
  });
  return mapDestination(data);
}

export async function deleteDestination(token, planId, destinationId) {
  return apiRequest(`${BASE}/${planId}/destinations/${destinationId}`, {
    method: 'DELETE',
    token,
  });
}

export async function getActivities(token, planId) {
  const data = await apiRequest(`${BASE}/${planId}/activities`, { token });
  return data.map(mapActivity);
}

export async function createActivity(token, planId, payload) {
  const data = await apiRequest(`${BASE}/${planId}/activities`, {
    method: 'POST',
    token,
    body: payload,
  });
  return mapActivity(data);
}

export async function deleteActivity(token, planId, activityId) {
  return apiRequest(`${BASE}/${planId}/activities/${activityId}`, {
    method: 'DELETE',
    token,
  });
}

export async function getChecklistItems(token, planId) {
  const data = await apiRequest(`${BASE}/${planId}/checklist-items`, { token });
  return data.map(mapChecklistItem);
}

export async function createChecklistItem(token, planId, payload) {
  const data = await apiRequest(`${BASE}/${planId}/checklist-items`, {
    method: 'POST',
    token,
    body: payload,
  });
  return mapChecklistItem(data);
}

export async function toggleChecklistItem(token, planId, itemId) {
  const data = await apiRequest(`${BASE}/${planId}/checklist-items/${itemId}/toggle`, {
    method: 'PATCH',
    token,
  });
  return mapChecklistItem(data);
}

export async function deleteChecklistItem(token, planId, itemId) {
  return apiRequest(`${BASE}/${planId}/checklist-items/${itemId}`, {
    method: 'DELETE',
    token,
  });
}
