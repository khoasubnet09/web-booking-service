"use client";

import { api } from "@/lib/api";

const AUTH_STORAGE_KEY = "service_booking_auth";

export type UserRole = "Admin" | "Customer";

export type AuthUser = {
  userId: string;
  email: string;
  fullName: string;
  role: UserRole;
  accessToken: string;
  expiresAt: string;
};

export type LoginResponse = AuthUser;

export function getStoredAuth(): AuthUser | null {
  if (typeof window === "undefined") {
    return null;
  }

  const rawValue = window.localStorage.getItem(AUTH_STORAGE_KEY);
  if (!rawValue) {
    return null;
  }

  try {
    const user = JSON.parse(rawValue) as AuthUser;
    if (new Date(user.expiresAt).getTime() <= Date.now()) {
      clearStoredAuth();
      return null;
    }

    return user;
  } catch {
    clearStoredAuth();
    return null;
  }
}

export function setStoredAuth(user: AuthUser) {
  window.localStorage.setItem(AUTH_STORAGE_KEY, JSON.stringify(user));
  window.dispatchEvent(new Event("auth-changed"));
}

export function clearStoredAuth() {
  window.localStorage.removeItem(AUTH_STORAGE_KEY);
  window.dispatchEvent(new Event("auth-changed"));
}

export async function login(email: string, password: string) {
  const user = await api<LoginResponse>("/auth/login", {
    method: "POST",
    body: JSON.stringify({ email, password }),
  });

  setStoredAuth(user);
  return user;
}

export async function authApi<T>(path: string, init?: RequestInit): Promise<T> {
  const auth = getStoredAuth();

  if (!auth) {
    throw new Error("Bạn cần đăng nhập trước khi thực hiện thao tác này.");
  }

  return api<T>(path, {
    ...init,
    headers: {
      Authorization: `Bearer ${auth.accessToken}`,
      ...init?.headers,
    },
  });
}
