import { translateApiMessage } from '../utils/apiMessages';

const API_URL = import.meta.env.VITE_API_URL;

export class ApiError extends Error {
  constructor(message, status, details) {
    super(message);
    this.name = 'ApiError';
    this.status = status;
    this.details = details;
  }
}

async function parseError(response) {
  try {
    const data = await response.json();
    if (data.message) {
      return translateApiMessage(data.message);
    }
    if (data.title && data.errors) {
      return translateApiMessage(Object.values(data.errors).flat().join(' '));
    }
    return translateApiMessage(data.title || 'Request failed.');
  } catch {
    return translateApiMessage('Request failed.');
  }
}

export async function apiRequest(path, options = {}) {
  const { token, body, method = 'GET', headers = {}, signal } = options;

  const requestHeaders = {
    ...headers,
  };

  if (body !== undefined) {
    requestHeaders['Content-Type'] = 'application/json';
  }

  if (token) {
    requestHeaders.Authorization = `Bearer ${token}`;
  }

  const response = await fetch(`${API_URL}${path}`, {
    method,
    headers: requestHeaders,
    body: body !== undefined ? JSON.stringify(body) : undefined,
    signal,
  });

  if (!response.ok) {
    const message = await parseError(response);
    throw new ApiError(message, response.status);
  }

  if (response.status === 204) {
    return null;
  }

  return response.json();
}

export async function apiDownload(path, options = {}) {
  const { token, method = 'GET', headers = {} } = options;

  const requestHeaders = { ...headers };

  if (token) {
    requestHeaders.Authorization = `Bearer ${token}`;
  }

  const response = await fetch(`${API_URL}${path}`, {
    method,
    headers: requestHeaders,
  });

  if (!response.ok) {
    const message = await parseError(response);
    throw new ApiError(message, response.status);
  }

  const blob = await response.blob();
  const disposition = response.headers.get('Content-Disposition') ?? '';
  const fileNameMatch = disposition.match(/filename="?([^";]+)"?/i);
  const fileName = fileNameMatch?.[1] ?? 'download.pdf';

  return { blob, fileName };
}
