import { inject } from '@angular/core';
import { ActivatedRouteSnapshot, Router, RouterStateSnapshot } from '@angular/router';
import { AuthService } from '../services/auth.service';

const CHANGE_PASSWORD_URL = '/portal/change-password';

/**
 * Functional guard to protect portal routes
 */
export const authGuard = (_route: ActivatedRouteSnapshot, state: RouterStateSnapshot) => {
    const auth = inject(AuthService);
    const router = inject(Router);

    if (!auth.isAuthenticated()) {
        return router.parseUrl('/login');
    }

    // S7.6: Force password change before accessing any protected route.
    // 29A.1: Never redirect the change-password page onto itself — that route is where we send
    // the user, so allowing it here prevents an infinite guard→redirect loop (and a 404 when the
    // route was previously missing entirely).
    if (auth.currentUser()?.mustChangePassword && !state.url.startsWith(CHANGE_PASSWORD_URL)) {
        return router.parseUrl(CHANGE_PASSWORD_URL);
    }

    return true;
};

/**
 * Functional guard for admin-only routes
 */
export const adminGuard = () => {
    const auth = inject(AuthService);
    const router = inject(Router);

    const role = auth.currentUser()?.role;
    if (role === 'Admin' || role === 'SuperAdmin') {
        return true;
    }

    return router.parseUrl('/portal/dashboard');
};

/**
 * Functional guard for super-admin-only routes
 */
export const superAdminGuard = () => {
    const auth = inject(AuthService);
    const router = inject(Router);

    const role = auth.currentUser()?.role;
    if (role === 'SuperAdmin') {
        return true;
    }

    return router.parseUrl('/admin/dashboard');
};
