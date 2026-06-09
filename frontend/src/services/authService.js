import { createUser } from '../models/User';
import { apiRequest } from './apiClient';

export async function registerUser(payload) {
  const data = await apiRequest('/api/auth/register', {
    method: 'POST',
    body: payload,
  });

  return {
    token: data.token,
    expiresAt: data.expiresAt,
    user: createUser(data.user),
  };
}

export async function loginUser(payload) {
  const data = await apiRequest('/api/auth/login', {
    method: 'POST',
    body: payload,
  });

  return {
    token: data.token,
    expiresAt: data.expiresAt,
    user: createUser(data.user),
  };
}

export async function getCurrentUser(token, signal) {
  const data = await apiRequest('/api/auth/me', { token, signal });
  return createUser(data);
}
