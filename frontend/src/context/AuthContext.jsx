import { createContext, useContext, useEffect, useMemo, useState } from 'react';
import * as authService from '../services/authService';

const AuthContext = createContext(null);

const TOKEN_KEY = 'travel_planner_token';
const USER_KEY = 'travel_planner_user';

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

function persistSession(token, user) {
  localStorage.setItem(TOKEN_KEY, token);
  localStorage.setItem(USER_KEY, JSON.stringify(user));
}

function clearSession() {
  localStorage.removeItem(TOKEN_KEY);
  localStorage.removeItem(USER_KEY);
}

export function AuthProvider({ children }) {
  const [token, setToken] = useState(() => localStorage.getItem(TOKEN_KEY));
  const [user, setUser] = useState(() => readCachedUser());
  const [loading, setLoading] = useState(() => Boolean(localStorage.getItem(TOKEN_KEY)));

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
        if (err?.name === 'AbortError') return;
        clearSession();
        setToken(null);
        setUser(null);
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
      persistSession(result.token, result.user);
      setToken(result.token);
      setUser(result.user);
      return result;
    },
    async register(payload) {
      const result = await authService.registerUser(payload);
      persistSession(result.token, result.user);
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
