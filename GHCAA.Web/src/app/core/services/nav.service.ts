import { Injectable, inject, signal, computed } from '@angular/core';
import { AuthService } from './auth.service';

export interface NavItem {
    path: string;
    label: string;
    icon: string;
    adminOnly?: boolean;
    mobileVisible?: boolean;
}

// All portal navigation items
const ALL_NAV_ITEMS: NavItem[] = [
    { path: '/portal/dashboard', label: 'Dashboard', icon: '📊', mobileVisible: true },
    { path: '/portal/news', label: 'News Hub', icon: '📰', mobileVisible: true },
    { path: '/portal/events', label: 'Events', icon: '🎟️' },
    { path: '/portal/assistant', label: 'AI Assistant', icon: '✨', mobileVisible: true },
    { path: '/portal/messages', label: 'Messaging', icon: '💬' },
    { path: '/portal/jobs', label: 'Job Hub', icon: '💼', mobileVisible: true },
    { path: '/portal/directory', label: 'Alumni Directory', icon: '🔍' },
    { path: '/portal/gallery', label: 'Event Gallery', icon: '🖼️' },
    { path: '/portal/governance', label: 'Governance', icon: '⚖️' },
    { path: '/portal/id-card', label: 'Digital ID', icon: '🆔' },
    { path: '/portal/profile', label: 'My Profile', icon: '👤', mobileVisible: true },
];

// Admin panel navigation items
const ADMIN_NAV_ITEMS: NavItem[] = [
    { path: '/admin/dashboard', label: 'Dashboard', icon: '📊' },
    { path: '/admin/approvals', label: 'Approvals', icon: '📝' },
    { path: '/admin/members', label: 'All Members', icon: '👥' },
    { path: '/admin/news', label: 'News Posts', icon: '📰' },
    { path: '/admin/ledger', label: 'Financial Ledger', icon: '📖' },
];

@Injectable({ providedIn: 'root' })
export class NavService {
    private auth = inject(AuthService);

    /** Returns nav items the current user is allowed to see in the Portal */
    portalNavItems = computed<NavItem[]>(() => {
        const user = this.auth.currentUser();
        if (!user) return [];
        // All authenticated users get all portal nav items
        return ALL_NAV_ITEMS;
    });

    /** Mobile quick-nav items (subset) */
    mobileNavItems = computed<NavItem[]>(() =>
        this.portalNavItems().filter(i => i.mobileVisible)
    );

    /** Returns admin nav items (only available to Admin/SuperAdmin) */
    adminNavItems = computed<NavItem[]>(() => {
        const user = this.auth.currentUser();
        if (!user || !['Admin', 'SuperAdmin'].includes(user.role)) return [];
        return ADMIN_NAV_ITEMS;
    });

    /** Whether the current user has admin access */
    isAdmin = computed(() => {
        const role = this.auth.currentUser()?.role;
        return role === 'Admin' || role === 'SuperAdmin';
    });

    isSuperAdmin = computed(() => this.auth.currentUser()?.role === 'SuperAdmin');
}
