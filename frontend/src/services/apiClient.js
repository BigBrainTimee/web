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
      return data.message;
    }
    if (data.title && data.errors) {
      return Object.values(data.errors).flat().join(' ');
    }
    return data.title || 'Request failed.';
  } catch {
    return 'Request failed.';
  }
}

export async function apiRequest(path, options = {}) {
  const { token, body, method = 'GET', headers = {} } = options;

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
