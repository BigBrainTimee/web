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

const DEFAULT_TIMEOUT_MS = 30000;

export async function apiRequest(path, options = {}) {
  const { token, body, method = 'GET', headers = {}, signal, timeoutMs = DEFAULT_TIMEOUT_MS } = options;

  const requestHeaders = {
    ...headers,
  };

  if (body !== undefined) {
    requestHeaders['Content-Type'] = 'application/json';
  }

  if (token) {
    requestHeaders.Authorization = `Bearer ${token}`;
  }

  const timeoutController = new AbortController();
  const timeoutId = setTimeout(() => timeoutController.abort(), timeoutMs);

  if (signal) {
    if (signal.aborted) {
      clearTimeout(timeoutId);
      throw new DOMException('Aborted', 'AbortError');
    }
    signal.addEventListener('abort', () => timeoutController.abort(), { once: true });
  }

  let response;
  try {
    response = await fetch(`${API_URL}${path}`, {
      method,
      headers: requestHeaders,
      body: body !== undefined ? JSON.stringify(body) : undefined,
      signal: timeoutController.signal,
    });
  } catch (err) {
    if (err?.name === 'AbortError') {
      if (signal?.aborted) {
        throw err;
      }
      throw new ApiError('Zahtev je istekao. Proveri da li su backend servisi pokrenuti.', 408);
    }
    throw err;
  } finally {
    clearTimeout(timeoutId);
  }

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
