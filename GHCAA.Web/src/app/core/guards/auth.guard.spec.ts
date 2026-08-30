import { createAuthServiceMock } from '../testing/testing-utils';
import { TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { AuthService } from '../services/auth.service';
import { authGuard, adminGuard, memberGuard } from './auth.guard';
import { vi, describe, it, expect, beforeEach } from 'vitest';

describe('AuthGuards', () => {
    let authServiceMock: ReturnType<typeof createAuthServiceMock>;
    let routerMock: any;

    beforeEach(() => {
        authServiceMock = createAuthServiceMock();
        routerMock = {
            parseUrl: vi.fn().mockImplementation((url: string) => url),
            createUrlTree: vi.fn().mockImplementation((commands: any[], _extras?: any) => commands[0])
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

    // Guards now return an Observable that waits for authChecked() — authChecked defaults to
    // `true` in the mock so these resolve on the same microtask, matching the pre-existing
    // "cached session already restored" fast path.
    const runGuard = (result: any) => firstValueFrom(result);

    describe('authGuard', () => {
        it('should return true if authenticated', async () => {
            (authServiceMock.isAuthenticated as any).mockReturnValue(true);
            (authServiceMock.currentUser as any).mockReturnValue({ role: 'Member', mustChangePassword: false });
            const result = await runGuard(TestBed.runInInjectionContext(() => authGuard(anyRoute, stateFor('/portal/dashboard'))));
            expect(result).toBe(true);
        });

        it('should redirect to login if not authenticated', async () => {
            (authServiceMock.isAuthenticated as any).mockReturnValue(false);
            const result = await runGuard(TestBed.runInInjectionContext(() => authGuard(anyRoute, stateFor('/portal/dashboard'))));
            expect(result).toBe('/login');
            expect(routerMock.createUrlTree).toHaveBeenCalledWith(['/login'], { queryParams: { returnUrl: '/portal/dashboard' } });
        });

        it('should redirect mustChangePassword users to change-password', async () => {
            (authServiceMock.isAuthenticated as any).mockReturnValue(true);
            (authServiceMock.currentUser as any).mockReturnValue({ role: 'Member', mustChangePassword: true });
            const result = await runGuard(TestBed.runInInjectionContext(() => authGuard(anyRoute, stateFor('/portal/dashboard'))));
            expect(result).toBe('/portal/change-password');
        });

        it('should NOT loop-redirect when already on change-password', async () => {
            (authServiceMock.isAuthenticated as any).mockReturnValue(true);
            (authServiceMock.currentUser as any).mockReturnValue({ role: 'Member', mustChangePassword: true });
            const result = await runGuard(TestBed.runInInjectionContext(() => authGuard(anyRoute, stateFor('/portal/change-password'))));
            expect(result).toBe(true);
        });

        it('waits for authChecked before deciding (new-tab/deep-link race)', async () => {
            // authChecked starts false (restore still in flight) — the guard must not resolve
            // until it flips true, even though isAuthenticated() would otherwise read a stale
            // "guest" snapshot at the moment the guard runs.
            authServiceMock.authChecked.set(false);
            (authServiceMock.isAuthenticated as any).mockReturnValue(true);
            (authServiceMock.currentUser as any).mockReturnValue({ role: 'Member', mustChangePassword: false });

            const pending = runGuard(TestBed.runInInjectionContext(() => authGuard(anyRoute, stateFor('/portal/dashboard'))));
            let settled = false;
            pending.then(() => { settled = true; });

            await Promise.resolve();
            expect(settled).toBe(false);

            authServiceMock.authChecked.set(true);
            const result = await pending;
            expect(result).toBe(true);
        });
    });

    describe('adminGuard', () => {
        it('should allow Admin role', async () => {
            (authServiceMock.currentUser as any).mockReturnValue({ role: 'Admin' });
            const result = await runGuard(TestBed.runInInjectionContext(() => adminGuard()));
            expect(result).toBe(true);
        });

        it('should allow SuperAdmin role', async () => {
            (authServiceMock.currentUser as any).mockReturnValue({ role: 'SuperAdmin' });
            const result = await runGuard(TestBed.runInInjectionContext(() => adminGuard()));
            expect(result).toBe(true);
        });

        it('should redirect non-admin users to portal dashboard', async () => {
            (authServiceMock.currentUser as any).mockReturnValue({ role: 'Member' });
            const result = await runGuard(TestBed.runInInjectionContext(() => adminGuard()));
            expect(result).toBe('/portal/dashboard');
            expect(routerMock.parseUrl).toHaveBeenCalledWith('/portal/dashboard');
        });
    });

    // 58.1: an Admin/SuperAdmin account with no linked Member record previously reached every
    // /portal/* page (authGuard alone only checks isAuthenticated()); those pages assume a real
    // memberId server-side, so a memberless admin landed on broken pages instead of being routed
    // somewhere that actually applies to them.
    describe('memberGuard', () => {
        it('allows a user with a memberId through', async () => {
            (authServiceMock.currentUser as any).mockReturnValue({ role: 'Member', memberId: 42 });
            const result = await runGuard(TestBed.runInInjectionContext(() => memberGuard(anyRoute, stateFor('/portal/profile'))));
            expect(result).toBe(true);
        });

        it('redirects a memberless admin to the admin dashboard', async () => {
            (authServiceMock.currentUser as any).mockReturnValue({ role: 'SuperAdmin', memberId: undefined });
            const result = await runGuard(TestBed.runInInjectionContext(() => memberGuard(anyRoute, stateFor('/portal/payments'))));
            expect(result).toBe('/admin/dashboard');
            expect(routerMock.parseUrl).toHaveBeenCalledWith('/admin/dashboard');
        });

        it('lets a memberless admin through to change-password specifically, to avoid a redirect loop with authGuard', async () => {
            (authServiceMock.currentUser as any).mockReturnValue({ role: 'SuperAdmin', memberId: undefined });
            const result = await runGuard(TestBed.runInInjectionContext(() => memberGuard(anyRoute, stateFor('/portal/change-password'))));
            expect(result).toBe(true);
        });
    });
});
