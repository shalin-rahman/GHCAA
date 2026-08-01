import { Injectable, inject, signal, computed } from '@angular/core';
import { AuthService } from './auth.service';
import { OrgConfigService } from './org-config.service';
import { OrgConfig } from '../models/org-config.model';

export interface NavItem {
    path: string;
    label: string;
    icon: string;
    mobileVisible?: boolean;
    exact?: boolean;
    fragment?: string;
    roles?: string[];
    section?: string;
    // 29F.4: when set, the item is only shown if this OrgConfig feature flag is enabled.
    feature?: keyof OrgConfig['features'];
}

// All portal navigation items
// TODO 30.13: each item carries a `section` bucket (Overview / Community / Directory /
// Career / My Account) so the sidebar can render grouped headers like the admin panel.
const ALL_NAV_ITEMS: NavItem[] = [
    { path: '/portal/dashboard', label: 'Dashboard', icon: 'dashboard', mobileVisible: true, section: 'Overview' },
    { path: '/portal/news', label: 'News', icon: 'news', mobileVisible: true, section: 'Overview' },
    { path: '/portal/events', label: 'Events', icon: 'events', mobileVisible: true, feature: 'enableEvents', section: 'Overview' },
    { path: '/portal/assistant', label: 'Assistance', icon: 'assistant', mobileVisible: true, section: 'Overview' },
    { path: '/portal/messages', label: 'Messaging', icon: 'messages', section: 'Community' },
    { path: '/portal/forum', label: 'Discussions', icon: 'forum', feature: 'enableForum', section: 'Community' },
    { path: '/portal/governance', label: 'Governance', icon: 'governance', section: 'Community' },
    { path: '/portal/directory', label: 'Alumni Directory', icon: 'directory', feature: 'enablePublicDirectory', section: 'Directory' },
    { path: '/portal/gallery', label: 'Event Gallery', icon: 'gallery', feature: 'enableGallery', section: 'Directory' },
    { path: '/portal/jobs', label: 'Job Hub', icon: 'jobs', mobileVisible: true, feature: 'enableJobHub', section: 'Career' },
    { path: '/portal/id-card', label: 'Digital ID', icon: 'id-card', feature: 'enableDigitalIdCard', section: 'My Account' },
    { path: '/portal/payments', label: 'Payments', icon: 'payments', mobileVisible: true, section: 'My Account' },
    { path: '/portal/articles', label: 'My Articles', icon: 'articles', feature: 'enableMagazine', section: 'My Account' },
    { path: '/portal/profile', label: 'My Profile', icon: 'profile', mobileVisible: true, section: 'My Account' },
];

// Admin panel navigation items
const ADMIN_NAV_ITEMS: NavItem[] = [
    { path: '/admin/dashboard', label: 'Dashboard', icon: 'dashboard', section: 'Overview' },

    { path: '/admin/approvals', label: 'Approvals', icon: 'approvals', section: 'Membership' },
    { path: '/admin/members', label: 'All Members', icon: 'members', section: 'Membership' },
    { path: '/admin/members/ec', label: 'Executive Committee', icon: 'ec', section: 'Membership' },

    { path: '/admin/news', label: 'News Posts', icon: 'news', section: 'Content' },
    { path: '/admin/events', label: 'Manage Events', icon: 'events', section: 'Content' },
    { path: '/admin/gallery', label: 'Gallery Albums', icon: 'gallery', section: 'Content' },
    { path: '/admin/comm', label: 'Communications', icon: 'comm', section: 'Content' },
    { path: '/admin/article-approvals', label: 'Submission Review', icon: 'article-approvals', section: 'Content' },
    { path: '/admin/contact-messages', label: 'Portal Enquiries', icon: 'contact-messages', section: 'Content' },
    { path: '/admin/themes', label: 'Special Themes', icon: 'themes', section: 'Content' },

    { path: '/admin/ledger', label: 'Financial Ledger', icon: 'ledger', roles: ['SuperAdmin'], section: 'Finance & Tools' },
    { path: '/admin/payments', label: 'Payment Settings', icon: 'payment-settings', roles: ['SuperAdmin'], section: 'Finance & Tools' },
    { path: '/admin/payments/fees', label: 'Fee Policy', icon: 'fee-policy', roles: ['SuperAdmin'], section: 'Finance & Tools' },
    { path: '/admin/roles', label: 'User Roles', icon: 'roles', roles: ['SuperAdmin'], section: 'Finance & Tools' },
    { path: '/admin/audit', label: 'Audit Logs', icon: 'audit', roles: ['SuperAdmin'], section: 'Finance & Tools' },
    { path: '/admin/org-config', label: 'Org Config', icon: 'org-config', roles: ['SuperAdmin'], section: 'Finance & Tools' }
];

@Injectable({ providedIn: 'root' })
export class NavService {
    private auth = inject(AuthService);
    private orgConfig = inject(OrgConfigService);

    /** Returns nav items the current user is allowed to see in the Portal */
    portalNavItems = computed<NavItem[]>(() => {
        const user = this.auth.currentUser();
        if (!user) return [];
        // 29F.4: honor OrgConfig feature flags — items tied to a disabled feature are
        // hidden (was returning the full list regardless of the tenant's configuration).
        return ALL_NAV_ITEMS.filter(item =>
            !item.feature || this.orgConfig.isFeatureEnabled(item.feature)
        );
    });

    /** Mobile quick-nav items (subset) */
    mobileNavItems = computed<NavItem[]>(() =>
        this.portalNavItems().filter(i => i.mobileVisible)
    );

    /** TODO 30.13: grouped portal items for the sidebar, mirroring adminNavSections. */
    portalNavSections = computed(() => {
        const items = this.portalNavItems();
        return [
            { name: 'Overview', items: items.filter(i => i.section === 'Overview') },
            { name: 'Community', items: items.filter(i => i.section === 'Community') },
            { name: 'Directory', items: items.filter(i => i.section === 'Directory') },
            { name: 'Career', items: items.filter(i => i.section === 'Career') },
            { name: 'My Account', items: items.filter(i => i.section === 'My Account') }
        ].filter(s => s.items.length > 0);
    });

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

    /**
     * 30.11: single source of truth for "what should the header/breadcrumb/browser-tab
     * title say for this URL" — centralizes the match-by-path logic that admin-layout
     * and portal-layout each used to duplicate inline, so nav label and page title can
     * never drift apart again (they're now the same lookup, not two copies of it).
     */
    labelFor(url: string, scope: 'admin' | 'portal'): string {
        if (scope === 'admin') {
            const match = this.adminNavItems().find(x => url.includes(x.path));
            return match?.label ?? 'Control Panel';
        }
        const match = this.portalNavItems().find(x => url.includes(x.path.replace('/portal/', '')));
        return match?.label ?? 'Dashboard';
    }

    /** Whether the current user has admin access */
    isAdmin = computed(() => {
        const role = this.auth.currentUser()?.role;
        return role === 'Admin' || role === 'SuperAdmin';
    });

    isSuperAdmin = computed(() => this.auth.currentUser()?.role === 'SuperAdmin');
}
