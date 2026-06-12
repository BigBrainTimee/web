import { createContext, useContext, useEffect, useMemo, useState } from 'react';
import { ApiError } from '../services/apiClient';
import * as authService from '../services/authService';

const AuthContext = createContext(null);

const TOKEN_KEY = 'travel_planner_token';
const USER_KEY = 'travel_planner_user';
const EXPIRES_KEY = 'travel_planner_expires_at';

function readCachedUser() {
  const raw = localStorage.getItem(USER_KEY);
  if (!raw) return null;

  try {
    return JSON.parse(raw);
  } catch {
    localStorage.removeItem(USER_KEY);
    return null;
  }
}

function readExpiresAt() {
  const raw = localStorage.getItem(EXPIRES_KEY);
  if (!raw) return null;

  const expiresAt = new Date(raw);
  return Number.isNaN(expiresAt.getTime()) ? null : expiresAt;
}

function isSessionExpired(expiresAt) {
  if (!expiresAt) return false;
  return expiresAt.getTime() <= Date.now();
}

function persistSession(token, user, expiresAt) {
  localStorage.setItem(TOKEN_KEY, token);
  localStorage.setItem(USER_KEY, JSON.stringify(user));
  if (expiresAt) {
    localStorage.setItem(EXPIRES_KEY, expiresAt);
  }
}

function clearSession() {
  localStorage.removeItem(TOKEN_KEY);
  localStorage.removeItem(USER_KEY);
  localStorage.removeItem(EXPIRES_KEY);
}

function readStoredSession() {
  const token = localStorage.getItem(TOKEN_KEY);
  const user = readCachedUser();
  const expiresAt = readExpiresAt();

  if (!token || !user) {
    return { token: null, user: null, expiresAt: null };
  }

  if (isSessionExpired(expiresAt)) {
    clearSession();
    return { token: null, user: null, expiresAt: null };
  }

  return { token, user, expiresAt };
}

function shouldInvalidateSession(error) {
  if (error?.name === 'AbortError') {
    return false;
  }

  if (error instanceof ApiError) {
    return error.status === 401 || error.status === 403 || error.status === 404;
  }

  return false;
}

export function AuthProvider({ children }) {
  const initialSession = readStoredSession();
  const [token, setToken] = useState(initialSession.token);
  const [user, setUser] = useState(initialSession.user);
  const [loading, setLoading] = useState(() => Boolean(initialSession.token));

  useEffect(() => {
    if (!token) {
      setUser(null);
      setLoading(false);
      return undefined;
    }

    const cachedUser = readCachedUser();
    if (cachedUser) {
      setUser(cachedUser);
      setLoading(false);
    }

    const controller = new AbortController();

    async function validateSession() {
      try {
        const currentUser = await authService.getCurrentUser(token, controller.signal);
        setUser(currentUser);
        localStorage.setItem(USER_KEY, JSON.stringify(currentUser));
      } catch (err) {
        if (shouldInvalidateSession(err)) {
          clearSession();
          setToken(null);
          setUser(null);
        }
      } finally {
        setLoading(false);
      }
    }

    validateSession();

    return () => controller.abort();
  }, [token]);

  const value = useMemo(() => ({
    token,
    user,
    loading,
    isAuthenticated: Boolean(token && user),
    async login(credentials) {
      const result = await authService.loginUser(credentials);
      persistSession(result.token, result.user, result.expiresAt);
      setToken(result.token);
      setUser(result.user);
      return result;
    },
    async register(payload) {
      const result = await authService.registerUser(payload);
      persistSession(result.token, result.user, result.expiresAt);
      setToken(result.token);
      setUser(result.user);
      return result;
    },
    logout() {
      clearSession();
      setToken(null);
      setUser(null);
    },
  }), [token, user, loading]);

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth must be used within AuthProvider');
  }
  return context;
}
