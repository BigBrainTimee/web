import { createUser } from '../models/User';
import { apiRequest } from './apiClient';

const BASE = '/api/auth/users';

export async function getUsers(token) {
  const data = await apiRequest(BASE, { token });
  return data.map(createUser);
}

export async function updateUserRole(token, userId, role) {
  const data = await apiRequest(`${BASE}/${userId}/role`, {
    method: 'PUT',
    token,
    body: { role },
  });
  return createUser(data);
}

export async function deleteUser(token, userId) {
  return apiRequest(`${BASE}/${userId}`, {
    method: 'DELETE',
    token,
  });
}
