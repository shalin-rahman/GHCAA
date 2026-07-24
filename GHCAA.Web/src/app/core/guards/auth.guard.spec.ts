import { createAuthServiceMock } from '../testing/testing-utils';
import { TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { authGuard, adminGuard } from './auth.guard';
import { vi, describe, it, expect, beforeEach } from 'vitest';

describe('AuthGuards', () => {
    let authServiceMock: any;
    let routerMock: any;

    beforeEach(() => {
        authServiceMock = {
            isAuthenticated: vi.fn(),
            currentUser: vi.fn()
        };
        routerMock = {
            parseUrl: vi.fn().mockImplementation((url: string) => url)
        };

        TestBed.configureTestingModule({
            providers: [
                { provide: AuthService, useValue: authServiceMock },
                { provide: Router, useValue: routerMock }
            ]
        });
    });

    // Minimal RouterStateSnapshot stub — the guard only reads `.url`.
    const stateFor = (url: string) => ({ url } as any);
    const anyRoute = {} as any;

    describe('authGuard', () => {
        it('should return true if authenticated', () => {
            authServiceMock.isAuthenticated.mockReturnValue(true);
            authServiceMock.currentUser.mockReturnValue({ role: 'Member', mustChangePassword: false });
            const result = TestBed.runInInjectionContext(() => authGuard(anyRoute, stateFor('/portal/dashboard')));
            expect(result).toBe(true);
        });

        it('should redirect to login if not authenticated', () => {
            authServiceMock.isAuthenticated.mockReturnValue(false);
            const result = TestBed.runInInjectionContext(() => authGuard(anyRoute, stateFor('/portal/dashboard')));
            expect(result).toBe('/login');
            expect(routerMock.parseUrl).toHaveBeenCalledWith('/login');
        });

        it('should redirect mustChangePassword users to change-password', () => {
            authServiceMock.isAuthenticated.mockReturnValue(true);
            authServiceMock.currentUser.mockReturnValue({ role: 'Member', mustChangePassword: true });
            const result = TestBed.runInInjectionContext(() => authGuard(anyRoute, stateFor('/portal/dashboard')));
            expect(result).toBe('/portal/change-password');
        });

        it('should NOT loop-redirect when already on change-password', () => {
            authServiceMock.isAuthenticated.mockReturnValue(true);
            authServiceMock.currentUser.mockReturnValue({ role: 'Member', mustChangePassword: true });
            const result = TestBed.runInInjectionContext(() => authGuard(anyRoute, stateFor('/portal/change-password')));
            expect(result).toBe(true);
        });
    });

    describe('adminGuard', () => {
        it('should allow Admin role', () => {
            authServiceMock.currentUser.mockReturnValue({ role: 'Admin' });
            const result = TestBed.runInInjectionContext(() => adminGuard());
            expect(result).toBe(true);
        });

        it('should allow SuperAdmin role', () => {
            authServiceMock.currentUser.mockReturnValue({ role: 'SuperAdmin' });
            const result = TestBed.runInInjectionContext(() => adminGuard());
            expect(result).toBe(true);
        });

        it('should redirect non-admin users to portal dashboard', () => {
            authServiceMock.currentUser.mockReturnValue({ role: 'Member' });
            const result = TestBed.runInInjectionContext(() => adminGuard());
            expect(result).toBe('/portal/dashboard');
            expect(routerMock.parseUrl).toHaveBeenCalledWith('/portal/dashboard');
        });
    });
});
