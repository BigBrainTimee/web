import { createDestination as mapDestination } from '../models/Destination';
import { createTravelPlan as mapTravelPlan } from '../models/TravelPlan';
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
