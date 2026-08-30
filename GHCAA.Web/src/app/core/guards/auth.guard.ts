import { inject } from '@angular/core';
import { toObservable } from '@angular/core/rxjs-interop';
import { ActivatedRouteSnapshot, Router, RouterStateSnapshot } from '@angular/router';
import { filter, map, take } from 'rxjs';
import { AuthService } from '../services/auth.service';
import { ROUTES } from '../constants/app.constants';

const CHANGE_PASSWORD_URL = '/portal/change-password';

// AuthService's /auth/me session restore is deferred (afterNextRender — see auth.service.ts),
// so on a fresh page load / new tab `isAuthenticated()`/`currentUser()` can still read their
// pre-restore snapshot (usually "guest") at the exact moment a guard runs. Waiting for
// `authChecked()` to flip true — it's already `true` immediately whenever a cached session was
// found, so this never adds a wait for the common case — avoids bouncing a genuinely logged-in
// member (valid cookie, new tab / empty sessionStorage) to /login before the restore lands.
function waitForAuthChecked(auth: AuthService) {
    return toObservable(auth.authChecked).pipe(filter(Boolean), take(1));
}

/**
 * Functional guard to protect portal routes
 */
export const authGuard = (_route: ActivatedRouteSnapshot, state: RouterStateSnapshot) => {
    const auth = inject(AuthService);
    const router = inject(Router);

    return waitForAuthChecked(auth).pipe(
        map(() => {
            if (!auth.isAuthenticated()) {
                // 29D.8: Preserve the attempted URL so login can return the user to where they were
                // headed instead of always dumping them on the dashboard.
                return router.createUrlTree([ROUTES.LOGIN], { queryParams: { returnUrl: state.url } });
            }

            // S7.6: Force password change before accessing any protected route.
            // 29A.1: Never redirect the change-password page onto itself — that route is where we
            // send the user, so allowing it here prevents an infinite guard→redirect loop (and a
            // 404 when the route was previously missing entirely).
            if (auth.currentUser()?.mustChangePassword && !state.url.startsWith(CHANGE_PASSWORD_URL)) {
                return router.parseUrl(CHANGE_PASSWORD_URL);
            }

            return true;
        })
    );
};

/**
 * Functional guard for member-only portal routes. An Admin/SuperAdmin account created without a
 * linked Member record (e.g. via ProtectedSuperAdminSeeder) previously could still reach every
 * /portal/* page — Profile, Payments, Dashboard — since authGuard only checks isAuthenticated().
 * Those pages all assume a real memberId server-side (ProfileController.GetProfile returns 401
 * without one), so a memberless admin landed on broken/empty member pages instead of being routed
 * somewhere that actually applies to them.
 */
export const memberGuard = (_route: ActivatedRouteSnapshot, state: RouterStateSnapshot) => {
    const auth = inject(AuthService);
    const router = inject(Router);

    return waitForAuthChecked(auth).pipe(
        map(() => {
            if (auth.currentUser()?.memberId) {
                return true;
            }

            // Mirrors authGuard's own change-password special-case: a memberless admin forced
            // into a password change must still be able to reach that one page, or authGuard's
            // redirect there and this guard's redirect away from /portal would loop forever.
            if (state.url.startsWith(CHANGE_PASSWORD_URL)) {
                return true;
            }

            return router.parseUrl(ROUTES.ADMIN_DASHBOARD);
        })
    );
};

/**
 * Functional guard for admin-only routes
 */
export const adminGuard = () => {
    const auth = inject(AuthService);
    const router = inject(Router);

    return waitForAuthChecked(auth).pipe(
        map(() => {
            const role = auth.currentUser()?.role;
            if (role === 'Admin' || role === 'SuperAdmin') {
                return true;
            }

            return router.parseUrl(ROUTES.PORTAL_DASHBOARD);
        })
    );
};

/**
 * Functional guard for super-admin-only routes
 */
export const superAdminGuard = () => {
    const auth = inject(AuthService);
    const router = inject(Router);

    return waitForAuthChecked(auth).pipe(
        map(() => {
            const role = auth.currentUser()?.role;
            if (role === 'SuperAdmin') {
                return true;
            }

            return router.parseUrl(ROUTES.ADMIN_DASHBOARD);
        })
    );
};
