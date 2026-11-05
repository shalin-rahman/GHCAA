import { Injectable, inject, signal, computed } from '@angular/core';
import { AuthService } from './auth.service';
import { OrgConfigService } from './org-config.service';
import { OrgConfig } from '../models/org-config.model';
import { ELECTION_STAFF_ROLES } from '../constants/app.constants';

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
    { path: '/portal/assistant', label: 'Assistance', icon: 'assistant', section: 'Overview' },
    { path: '/portal/messages', label: 'Messaging', icon: 'messages', section: 'Community' },
    { path: '/portal/forum', label: 'Discussions', icon: 'forum', feature: 'enableForum', section: 'Community' },
    { path: '/portal/governance', label: 'Governance', icon: 'governance', section: 'Community' },
    { path: '/portal/election', label: 'Association Election', icon: 'governance', section: 'Community' },
    { path: '/portal/requests', label: 'My Requests', icon: 'messages', section: 'Community' },
    { path: '/portal/polls', label: 'Polls', icon: 'polls', feature: 'enablePolls', section: 'Community' },
    { path: '/portal/directory', label: 'Alumni Directory', icon: 'directory', feature: 'enablePublicDirectory', section: 'Directory' },
    { path: '/portal/gallery', label: 'Event Gallery', icon: 'gallery', feature: 'enableGallery', section: 'Directory' },
    { path: '/portal/jobs', label: 'Job Hub', icon: 'jobs', feature: 'enableJobHub', section: 'Career' },
    { path: '/portal/id-card', label: 'Digital ID', icon: 'id-card', feature: 'enableDigitalIdCard', section: 'My Account' },
    { path: '/portal/payments', label: 'Payments', icon: 'payments', section: 'My Account' },
    { path: '/portal/giving', label: 'My Giving', icon: 'payments', feature: 'enableFundraising', section: 'My Account' },
    { path: '/portal/articles', label: 'My Articles', icon: 'articles', feature: 'enableMagazine', section: 'My Account' },
    { path: '/portal/communications', label: 'My Communications', icon: 'comm', section: 'My Account' },
    { path: '/portal/profile', label: 'My Profile', icon: 'profile', mobileVisible: true, section: 'My Account' },
    { path: '/officials', label: 'Election Officials', icon: 'governance', exact: true, roles: [...ELECTION_STAFF_ROLES], section: 'Officials' },
    { path: '/officials/my-appointments', label: 'My Appointments', icon: 'governance', roles: [...ELECTION_STAFF_ROLES], section: 'Officials' },
];

// Sidebar order of the admin sections. An item's section must be one of these, or the
// item never renders, so the type below makes a typo a compile error.
export const ADMIN_NAV_SECTIONS = [
    'Overview',
    'Membership',
    'Content & Media',
    'Reviews & Inquiries',
    'Communications',
    'Elections',
    'Finance & Fundraising',
    'System & Settings'
] as const;

export type AdminNavSection = typeof ADMIN_NAV_SECTIONS[number];

interface AdminNavItem extends NavItem {
    section: AdminNavSection;
}

// Admin panel navigation items
const ADMIN_NAV_ITEMS: AdminNavItem[] = [
    { path: '/admin/dashboard', label: 'Dashboard', icon: 'dashboard', section: 'Overview' },

    { path: '/admin/approvals', label: 'Approvals', icon: 'approvals', section: 'Membership' },
    { path: '/admin/members', label: 'All Members', icon: 'members', section: 'Membership' },
    { path: '/admin/members/ec', label: 'Executive Committee', icon: 'ec', section: 'Membership' },

    { path: '/admin/news', label: 'News & Notices', icon: 'news', section: 'Content & Media' },
    { path: '/admin/events', label: 'Manage Events', icon: 'events', section: 'Content & Media' },
    { path: '/admin/gallery', label: 'Gallery Albums', icon: 'gallery', section: 'Content & Media' },
    { path: '/admin/site-content', label: 'Site Content', icon: 'site-content', section: 'Content & Media' },
    { path: '/admin/themes', label: 'Special Themes', icon: 'themes', section: 'Content & Media' },

    { path: '/admin/article-approvals', label: 'Submission Review', icon: 'article-approvals', section: 'Reviews & Inquiries' },
    { path: '/admin/gallery-approvals', label: 'Gallery Review', icon: 'gallery-approvals', section: 'Reviews & Inquiries' },
    { path: '/admin/job-approvals', label: 'Job Review', icon: 'job-approvals', section: 'Reviews & Inquiries' },
    { path: '/admin/contact-messages', label: 'Portal Enquiries', icon: 'contact-messages', section: 'Reviews & Inquiries' },
    { path: '/admin/mentorship', label: 'Mentorship Requests', icon: 'governance', section: 'Reviews & Inquiries' },

    { path: '/admin/comm', label: 'Communications', icon: 'comm', section: 'Communications' },
    { path: '/admin/polls', label: 'Polls', icon: 'polls', section: 'Communications' },

    { path: '/admin/elections', label: 'Elections', icon: 'governance', section: 'Elections' },
    { path: '/admin/election-content', label: 'Election Content', icon: 'governance', section: 'Elections' },
    { path: '/admin/election-personas', label: 'Election Personas', icon: 'governance', roles: ['SuperAdmin'], section: 'Elections' },

    { path: '/admin/ledger', label: 'Financial Ledger', icon: 'ledger', roles: ['SuperAdmin'], section: 'Finance & Fundraising' },
    { path: '/admin/payments', label: 'Payment Settings', icon: 'payment-settings', roles: ['SuperAdmin'], section: 'Finance & Fundraising' },
    { path: '/admin/payments/fees', label: 'Fee Policy', icon: 'fee-policy', roles: ['SuperAdmin'], section: 'Finance & Fundraising' },
    { path: '/admin/campaigns', label: 'Fundraising Campaigns', icon: 'ledger', feature: 'enableFundraising', section: 'Finance & Fundraising' },

    { path: '/admin/org-config', label: 'Org Config', icon: 'org-config', roles: ['SuperAdmin'], section: 'System & Settings' },
    { path: '/admin/roles', label: 'User Roles', icon: 'roles', roles: ['SuperAdmin'], section: 'System & Settings' },
    { path: '/admin/audit', label: 'Audit Logs', icon: 'audit', roles: ['SuperAdmin'], section: 'System & Settings' },
    { path: '/admin/error-logs', label: 'Error Logs', icon: 'audit', roles: ['SuperAdmin'], section: 'System & Settings' },
    { path: '/admin/dev-tracker', label: 'Developer Options', icon: 'audit', roles: ['SuperAdmin'], section: 'System & Settings' }
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
        const roles = user.roles ?? [user.role];
        return ALL_NAV_ITEMS.filter(item =>
            (!item.feature || this.orgConfig.isFeatureEnabled(item.feature)) &&
            (!item.roles || item.roles.some(r => roles.includes(r)))
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
            { name: 'My Account', items: items.filter(i => i.section === 'My Account') },
            { name: 'Officials', items: items.filter(i => i.section === 'Officials') }
        ].filter(s => s.items.length > 0);
    });

    /** Returns admin nav items (only available to Admin/SuperAdmin) */
    adminNavItems = computed<NavItem[]>(() => {
        const user = this.auth.currentUser();
        if (!user || !['Admin', 'SuperAdmin'].includes(user.role)) return [];
        return ADMIN_NAV_ITEMS.filter(item => {
            if (!item.roles) return true;
            const roles = user.roles ?? [user.role];
            return item.roles.some(r => roles.includes(r));
        });
    });

    /** Grouped admin items for safe UI rendering */
    adminNavSections = computed(() => {
        const items = this.adminNavItems();
        return ADMIN_NAV_SECTIONS
            .map(name => ({ name, items: items.filter(i => i.section === name) }))
            .filter(s => s.items.length > 0);
    });

    /**
     * 30.11: single source of truth for "what should the header/breadcrumb/browser-tab
     * title say for this URL" — centralizes the match-by-path logic that admin-layout
     * and portal-layout each used to duplicate inline, so nav label and page title can
     * never drift apart again (they're now the same lookup, not two copies of it).
     */
    labelFor(url: string, scope: 'admin' | 'portal'): string {
        // 82.32: `.find()` on an ordered list returned the FIRST matching path, so a nav item whose
        // path is a prefix of another (e.g. /admin/members before /admin/members/ec) always won —
        // Executive Committee's header/tab read "All Members". Picking the longest matching path
        // makes the more specific route win regardless of list order.
        if (scope === 'admin') {
            const matches = this.adminNavItems().filter(x => url.includes(x.path));
            const match = matches.sort((a, b) => b.path.length - a.path.length)[0];
            return match?.label ?? 'Control Panel';
        }
        const matches = this.portalNavItems().filter(x => url.includes(x.path.replace('/portal/', '')));
        const match = matches.sort((a, b) => b.path.length - a.path.length)[0];
        return match?.label ?? 'Dashboard';
    }

    /** Whether the current user has admin access */
    isAdmin = computed(() => {
        const role = this.auth.currentUser()?.role;
        return role === 'Admin' || role === 'SuperAdmin';
    });

    isSuperAdmin = computed(() => this.auth.currentUser()?.role === 'SuperAdmin');
}
