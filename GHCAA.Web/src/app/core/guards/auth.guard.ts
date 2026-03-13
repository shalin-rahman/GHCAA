import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

/**
 * Functional guard to protect portal routes
 */
export const authGuard = () => {
    const auth = inject(AuthService);
    const router = inject(Router);

    if (auth.isAuthenticated()) {
        return true;
    }

    // Redirect to login if not authenticated
    return router.parseUrl('/login');
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
