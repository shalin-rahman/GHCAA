import { Injectable, inject, signal, computed } from '@angular/core';
import { AuthService } from './auth.service';

export interface NavItem {
    path: string;
    label: string;
    icon: string;
    mobileVisible?: boolean;
    exact?: boolean;
    fragment?: string;
    roles?: string[];
    section?: string;
}

// All portal navigation items
const ALL_NAV_ITEMS: NavItem[] = [
    { path: '/portal/dashboard', label: 'Dashboard', icon: '📊', mobileVisible: true },
    { path: '/portal/news', label: 'News', icon: '📰', mobileVisible: true },
    { path: '/portal/events', label: 'Events', icon: '🎟️', mobileVisible: true },
    { path: '/portal/assistant', label: 'Assistance', icon: '✨', mobileVisible: true },
    { path: '/portal/messages', label: 'Messaging', icon: '💬' },
    { path: '/portal/forum', label: 'Discussions', icon: '🗣️' },
    { path: '/portal/jobs', label: 'Job Hub', icon: '💼', mobileVisible: true },
    { path: '/portal/directory', label: 'Alumni Directory', icon: '🔍' },
    { path: '/portal/gallery', label: 'Event Gallery', icon: '🖼️' },
    { path: '/portal/governance', label: 'Governance', icon: '⚖️' },
    { path: '/portal/id-card', label: 'Digital ID', icon: '🆔' },
    { path: '/portal/payments', label: 'Payments', icon: '💰', mobileVisible: true },
    { path: '/portal/articles', label: 'My Articles', icon: '✍️' },
    { path: '/portal/profile', label: 'My Profile', icon: '👤', mobileVisible: true },
];

// Admin panel navigation items
const ADMIN_NAV_ITEMS: NavItem[] = [
    { path: '/admin/dashboard', label: 'Dashboard', icon: '📊', section: 'Overview' },
    
    { path: '/admin/approvals', label: 'Approvals', icon: '📝', section: 'Membership' },
    { path: '/admin/members', label: 'All Members', icon: '👥', section: 'Membership' },
    { path: '/admin/members/ec', label: 'Executive Committee', icon: '🎗️', section: 'Membership' },
    
    { path: '/admin/news', label: 'News Posts', icon: '📰', section: 'Content' },
    { path: '/admin/events', label: 'Manage Events', icon: '🗓️', section: 'Content' },
    { path: '/admin/gallery', label: 'Gallery Albums', icon: '🖼️', section: 'Content' },
    { path: '/admin/comm', label: 'Communications', icon: '✉️', section: 'Content' },
    { path: '/admin/article-approvals', label: 'Submission Review', icon: '✅', section: 'Content' },
    { path: '/admin/contact-messages', label: 'Portal Enquiries', icon: '📥', section: 'Content' },
    { path: '/admin/themes', label: 'Special Themes', icon: '🎨', section: 'Content' },
    
    { path: '/admin/ledger', label: 'Financial Ledger', icon: '📖', roles: ['SuperAdmin'], section: 'Finance & Tools' },
    { path: '/admin/payments', label: 'Payment Settings', icon: '💳', roles: ['SuperAdmin'], section: 'Finance & Tools' },
    { path: '/admin/payments/fees', label: 'Fee Policy', icon: '🧾', roles: ['SuperAdmin'], section: 'Finance & Tools' },
    { path: '/admin/roles', label: 'User Roles', icon: '🛡️', roles: ['SuperAdmin'], section: 'Finance & Tools' },
    { path: '/admin/audit', label: 'Audit Logs', icon: '📜', roles: ['SuperAdmin'], section: 'Finance & Tools' },
    { path: '/admin/org-config', label: 'Org Config', icon: '⚙️', roles: ['SuperAdmin'], section: 'Finance & Tools' }
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
        return ADMIN_NAV_ITEMS.filter(item => {
            if (!item.roles) return true;
            return item.roles.includes(user.role);
        });
    });

    /** Grouped admin items for safe UI rendering */
    adminNavSections = computed(() => {
        const items = this.adminNavItems();
        return [
            { name: 'Overview', items: items.filter(i => i.section === 'Overview') },
            { name: 'Membership', items: items.filter(i => i.section === 'Membership') },
            { name: 'Content', items: items.filter(i => i.section === 'Content') },
            { name: 'Finance & Tools', items: items.filter(i => i.section === 'Finance & Tools') }
        ].filter(s => s.items.length > 0);
    });

    /** Whether the current user has admin access */
    isAdmin = computed(() => {
        const role = this.auth.currentUser()?.role;
        return role === 'Admin' || role === 'SuperAdmin';
    });

    isSuperAdmin = computed(() => this.auth.currentUser()?.role === 'SuperAdmin');
}
