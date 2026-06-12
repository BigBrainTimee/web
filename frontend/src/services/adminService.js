import { createUser as mapUser } from '../models/User';
import { apiRequest } from './apiClient';

const BASE = '/api/auth/users';

export async function getUsers(token) {
  const data = await apiRequest(BASE, { token });
  return data.map(mapUser);
}

export async function addUser(token, payload) {
  const { name, lastName, email, password, role } = payload;

  const registered = await apiRequest('/api/auth/register', {
    method: 'POST',
    body: { name, lastName, email, password },
  });

  const user = mapUser(registered.user);

  if (role !== 'User') {
    const updated = await apiRequest(`${BASE}/${user.id}/role`, {
      method: 'PUT',
      token,
      body: { role },
    });
    return mapUser(updated);
  }

  return user;
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
