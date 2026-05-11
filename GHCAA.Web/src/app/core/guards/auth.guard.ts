import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

/**
 * Functional guard to protect portal routes
 */
export const authGuard = () => {
    const auth = inject(AuthService);
    const router = inject(Router);

    if (!auth.isAuthenticated()) {
        return router.parseUrl('/login');
    }

    // S7.6: Force password change before accessing any protected route.
    if (auth.currentUser()?.mustChangePassword) {
        return router.parseUrl('/portal/change-password');
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
