import { createContext, useCallback, useContext, useEffect, useMemo, useState } from 'react';
import { api } from '../services/api';

interface AuthState {
  accessToken: string | null;
  userId: string | null;
}

interface AuthContextValue extends AuthState {

  register: (email: string, password: string) => Promise<void>;
  login: (email: string, password: string) => Promise<{ requiresTwoFactor: boolean; userId?: string }>;
  verifyTwoFactor: (userId: string, code: string) => Promise<void>;
  refresh: () => Promise<void>;
  logout: () => Promise<void>;
}

const AuthContext = createContext<AuthContextValue | undefined>(undefined);

export const AuthProvider = ({ children }: { children: React.ReactNode }) => {
  const [accessToken, setAccessToken] = useState<string | null>(null);
  const [userId, setUserId] = useState<string | null>(null);

  useEffect(() => {
    if (accessToken) {
      api.defaults.headers.common.Authorization = `Bearer ${accessToken}`;
    } else {
      delete api.defaults.headers.common.Authorization;
    }
  }, [accessToken]);


  const register = useCallback(async (email: string, password: string) => {
    await api.post('/auth/register', { email, password });
  }, []);

  const login = useCallback(async (email: string, password: string) => {
    const response = await api.post('/auth/login', { email, password });
    if (response.data.requiresTwoFactor) {
      return { requiresTwoFactor: true, userId: response.data.userId };
    }

    setAccessToken(response.data.accessToken);
    setUserId(null);
    return { requiresTwoFactor: false };
  }, []);

  const verifyTwoFactor = useCallback(async (id: string, code: string) => {
    const response = await api.post('/auth/login/2fa', { userId: id, code });
    setAccessToken(response.data.accessToken);
    setUserId(null);
  }, []);

  const refresh = useCallback(async () => {
    const response = await api.post('/auth/refresh');
    setAccessToken(response.data.accessToken);
  }, []);

  const logout = useCallback(async () => {
    await api.post('/auth/logout');
    setAccessToken(null);
    setUserId(null);
  }, []);

  const value = useMemo(
    () => ({ accessToken, userId, register, login, verifyTwoFactor, refresh, logout }),
    [accessToken, userId, register, login, verifyTwoFactor, refresh, logout]
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
};

export const useAuth = () => {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth must be used within AuthProvider');
  }
  return context;
};
