import { createUser as mapUser } from '../models/User';
import { apiRequest } from './apiClient';

const BASE = '/api/auth/users';

export async function getUsers(token) {
  const data = await apiRequest(BASE, { token });
  return data.map(mapUser);
}

export async function getUser(token, userId) {
  const data = await apiRequest(`${BASE}/${userId}`, { token });
  return mapUser(data);
}

export async function addUser(token, payload) {
  const data = await apiRequest(BASE, {
    method: 'POST',
    token,
    body: payload,
  });
  return mapUser(data);
}

export async function updateUserRole(token, userId, role) {
  const data = await apiRequest(`${BASE}/${userId}/role`, {
    method: 'PUT',
    token,
    body: { role },
  });
  return mapUser(data);
}

export async function deleteUser(token, userId) {
  return apiRequest(`${BASE}/${userId}`, {
    method: 'DELETE',
    token,
  });
}
