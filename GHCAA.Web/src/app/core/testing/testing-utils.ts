import { of } from 'rxjs';
import { vi } from 'vitest';

/**
 * Creates a standard mock for the AuthService.
 * You can override any default return value by passing it in.
 */
export function createAuthServiceMock(overrides: any = {}) {
  return {
    isAuthenticated: vi.fn().mockReturnValue(overrides.isAuthenticated ?? true),
    hasRole: vi.fn().mockReturnValue(overrides.hasRole ?? true),
    currentUser: vi.fn().mockReturnValue(overrides.currentUser ?? { id: 1, name: 'Test User' }),
    login: vi.fn().mockReturnValue(of((overrides.loginResponse ?? {}))),
    logout: vi.fn()
  };
}

/**
 * Creates a standard mock for the AlertService.
 */
export function createAlertServiceMock() {
  return {
    success: vi.fn(),
    error: vi.fn(),
    info: vi.fn(),
    warning: vi.fn()
  };
}

/**
 * Creates a standard mock for the NotificationService.
 */
export function createNotificationServiceMock() {
  return {
    getNotifications: vi.fn().mockReturnValue(of([])),
    markAsRead: vi.fn().mockReturnValue(of({})),
    markAllAsRead: vi.fn().mockReturnValue(of({}))
  };
}
