import { signal } from '@angular/core';
import { of } from 'rxjs';
import { vi } from 'vitest';

/**
 * Creates a standard mock for the AuthService.
 * You can override any default return value by passing it in.
 */
export function createAuthServiceMock(overrides: any = {}) {
  const isAuthenticated = vi.fn().mockReturnValue(overrides.isAuthenticated ?? true);
  // A real signal (not vi.fn()) — the auth guards feed this through toObservable(), which
  // requires actual Signal internals.
  const authChecked = signal(overrides.authChecked ?? true);
  return {
    isAuthenticated,
    hasRole: vi.fn().mockReturnValue(overrides.hasRole ?? true),
    currentUser: vi.fn().mockReturnValue(overrides.currentUser ?? { id: 1, name: 'Test User' }),
    authChecked,
    // Mirrors AuthService.whenAuthenticated: a synchronous check-and-call at registration time is
    // enough for tests, which set up mock state before construction rather than mutating it after.
    whenAuthenticated: vi.fn((callback: () => void) => {
      if (authChecked() && isAuthenticated()) callback();
    }),
    login: vi.fn().mockReturnValue(of((overrides.loginResponse ?? {}))),
    logout: vi.fn()
  };
}

/**
 * Creates a standard mock for the NotificationService (the toast service —
 * success/error/info/warning). Components only ever call a subset of these,
 * so providing all four is safe even where a spec only asserts on one or two.
 */
export function createNotificationServiceMock() {
  return {
    success: vi.fn(),
    error: vi.fn(),
    info: vi.fn(),
    warning: vi.fn()
  };
}
