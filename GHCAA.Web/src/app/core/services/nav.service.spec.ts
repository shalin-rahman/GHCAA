import { createAuthServiceMock } from '../testing/testing-utils';
import { TestBed } from '@angular/core/testing';
import { NavService, ADMIN_NAV_SECTIONS } from './nav.service';
import { AuthService } from './auth.service';
import { OrgConfigService } from './org-config.service';
import { ADMIN_ROLE, SUPER_ADMIN_ROLE } from '../constants/app.constants';
import { routes } from '../../app.routes';
import { vi, describe, it, expect, beforeEach } from 'vitest';

describe('NavService', () => {
    let service: NavService;
    let authServiceMock: any;
    let orgConfigMock: any;

    beforeEach(() => {
        authServiceMock = {
            currentUser: vi.fn(),
            isAuthenticated: vi.fn().mockReturnValue(true)
        };
        // All features enabled by default so portal nav gating is a no-op unless a test overrides.
        orgConfigMock = {
            isFeatureEnabled: vi.fn().mockReturnValue(true)
        };

        TestBed.configureTestingModule({
            providers: [
                NavService,
                { provide: AuthService, useValue: authServiceMock },
                { provide: OrgConfigService, useValue: orgConfigMock }
            ]
        });
        service = TestBed.inject(NavService);
    });

    it('should hide feature-gated portal items when the feature is disabled', () => {
        authServiceMock.currentUser.mockReturnValue({ role: 'Member' });
        // Disable Job Hub only.
        orgConfigMock.isFeatureEnabled.mockImplementation((f: string) => f !== 'enableJobHub');
        const items = service.portalNavItems();
        expect(items.some(i => i.label === 'Job Hub')).toBe(false);
        // Ungated items remain.
        expect(items.some(i => i.label === 'Dashboard')).toBe(true);
    });

    it('FR-41: should filter admin nav items for Admin role', () => {
        authServiceMock.currentUser.mockReturnValue({ role: ADMIN_ROLE });
        const items = service.adminNavItems();
        
        // Admin shouldn't see Payment Settings or Roles (SuperAdmin only)
        expect(items.some(i => i.label === 'Payment Settings')).toBe(false);
        expect(items.some(i => i.label === 'User Roles')).toBe(false);
    });

    it('FR-41: should show all admin nav items for SuperAdmin role', () => {
        authServiceMock.currentUser.mockReturnValue({ role: SUPER_ADMIN_ROLE });
        const items = service.adminNavItems();
        
        expect(items.some(i => i.label === 'Payment Settings')).toBe(true);
        expect(items.some(i => i.label === 'User Roles')).toBe(true);
    });

    it('FR-41: should group admin nav items into sections', () => {
        authServiceMock.currentUser.mockReturnValue({ role: SUPER_ADMIN_ROLE });
        const sections = service.adminNavSections();
        
        expect(sections.length).toBeGreaterThan(0);
        expect(sections[0].name).toBe('Overview');
        expect(sections.find(s => s.name === 'Finance & Fundraising')).toBeDefined();
        expect(sections.find(s => s.name === 'System & Settings')).toBeDefined();
        expect(sections.find(s => s.name === 'Elections')).toBeDefined();
        expect(sections.find(s => s.name === 'Reviews & Inquiries')).toBeDefined();
    });

    it('FR-41: lists admin sections in the declared order', () => {
        authServiceMock.currentUser.mockReturnValue({ role: SUPER_ADMIN_ROLE });
        expect(service.adminNavSections().map(s => s.name)).toEqual([...ADMIN_NAV_SECTIONS]);
    });

    it('FR-41: gives a plain Admin elections and campaigns but not personas or system pages', () => {
        authServiceMock.currentUser.mockReturnValue({ role: ADMIN_ROLE, roles: [ADMIN_ROLE] });
        const sections = service.adminNavSections();
        const labelsIn = (name: string) => sections.find(s => s.name === name)?.items.map(i => i.label) ?? [];

        expect(labelsIn('Elections')).toEqual(['Elections', 'Election Content']);
        expect(labelsIn('Finance & Fundraising')).toEqual(['Fundraising Campaigns']);
        expect(sections.some(s => s.name === 'System & Settings')).toBe(false);
    });

    it('FR-41: shows Election Personas to SuperAdmin under Elections', () => {
        authServiceMock.currentUser.mockReturnValue({ role: SUPER_ADMIN_ROLE });
        const elections = service.adminNavSections().find(s => s.name === 'Elections');
        expect(elections?.items.map(i => i.label)).toContain('Election Personas');
    });

    it('should expose election navigation to members and administrators', () => {
        authServiceMock.currentUser.mockReturnValue({ role: 'Member' });
        expect(service.portalNavItems().some(i => i.path === '/portal/election')).toBe(true);

        authServiceMock.currentUser.mockReturnValue({ role: ADMIN_ROLE });
        expect(service.adminNavItems().some(i => i.path === '/admin/elections')).toBe(true);
    });

    it('FR-39: shows an item when any role in the roles list matches', () => {
        authServiceMock.currentUser.mockReturnValue({ role: ADMIN_ROLE, roles: [ADMIN_ROLE, SUPER_ADMIN_ROLE] });
        expect(service.adminNavItems().some(i => i.label === 'User Roles')).toBe(true);
    });

    it('FR-39: hides an item when no role in the roles list matches', () => {
        authServiceMock.currentUser.mockReturnValue({ role: ADMIN_ROLE, roles: [ADMIN_ROLE] });
        expect(service.adminNavItems().some(i => i.label === 'User Roles')).toBe(false);
    });

    it('FR-39: falls back to the single role when an old session has no roles list', () => {
        authServiceMock.currentUser.mockReturnValue({ role: ADMIN_ROLE });
        expect(service.adminNavItems().some(i => i.label === 'User Roles')).toBe(false);
        expect(service.adminNavItems().some(i => i.path === '/admin/elections')).toBe(true);
    });

    // Phones only get the sidebar through the drawer, so a portal page with no nav entry
    // is unreachable there. Detail pages and change-password are opened from other pages.
    it('lists every top-level portal page in the member menu', () => {
        authServiceMock.currentUser.mockReturnValue({ role: 'Member' });
        const reachedFromOtherPages = new Set(['', 'change-password', 'forum/:id']);
        const portalPages = routes.find(r => r.path === 'portal')!.children!
            .map(r => r.path!)
            .filter(p => !reachedFromOtherPages.has(p));
        const navPaths = service.portalNavItems().map(i => i.path);

        expect(portalPages.filter(p => !navPaths.includes(`/portal/${p}`))).toEqual([]);
    });

    it('keeps the phone bottom bar to a few shortcuts', () => {
        authServiceMock.currentUser.mockReturnValue({ role: 'Member' });
        expect(service.mobileNavItems().map(i => i.path))
            .toEqual(['/portal/dashboard', '/portal/news', '/portal/events', '/portal/profile']);
    });

    it('FR-41: hides Fundraising Campaigns from an Admin when fundraising is off', () => {
        authServiceMock.currentUser.mockReturnValue({ role: ADMIN_ROLE, roles: [ADMIN_ROLE] });
        orgConfigMock.isFeatureEnabled.mockImplementation((f: string) => f !== 'enableFundraising');
        expect(service.adminNavItems().some(i => i.path === '/admin/campaigns')).toBe(false);
        expect(service.adminNavItems().some(i => i.path === '/admin/dashboard')).toBe(true);
    });

    it('FR-41: keeps Fundraising Campaigns for a SuperAdmin when fundraising is off', () => {
        authServiceMock.currentUser.mockReturnValue({ role: SUPER_ADMIN_ROLE, roles: [SUPER_ADMIN_ROLE] });
        orgConfigMock.isFeatureEnabled.mockReturnValue(false);
        const finance = service.adminNavSections().find(s => s.name === 'Finance & Fundraising');
        expect(finance?.items.map(i => i.label)).toContain('Fundraising Campaigns');
    });

    it('FR-41: keeps flagged items for a session whose roles list includes SuperAdmin', () => {
        authServiceMock.currentUser.mockReturnValue({ role: ADMIN_ROLE, roles: [ADMIN_ROLE, SUPER_ADMIN_ROLE] });
        orgConfigMock.isFeatureEnabled.mockReturnValue(false);
        expect(service.adminNavItems().some(i => i.path === '/admin/campaigns')).toBe(true);
    });

    it('FR-41: shows Fundraising Campaigns to admins when fundraising is on', () => {
        authServiceMock.currentUser.mockReturnValue({ role: ADMIN_ROLE, roles: [ADMIN_ROLE] });
        expect(service.adminNavItems().some(i => i.path === '/admin/campaigns')).toBe(true);
    });

    // A plain Admin's only finance link is Campaigns, so the whole section goes with it.
    it('FR-41: drops an admin section left empty by a disabled feature', () => {
        authServiceMock.currentUser.mockReturnValue({ role: ADMIN_ROLE, roles: [ADMIN_ROLE] });
        orgConfigMock.isFeatureEnabled.mockImplementation((f: string) => f !== 'enableFundraising');
        expect(service.adminNavSections().some(s => s.name === 'Finance & Fundraising')).toBe(false);
    });

    it('hides Polls when the polls feature is off', () => {
        authServiceMock.currentUser.mockReturnValue({ role: 'Member' });
        orgConfigMock.isFeatureEnabled.mockImplementation((f: string) => f !== 'enablePolls');
        const paths = service.portalNavItems().map(i => i.path);
        expect(paths).not.toContain('/portal/polls');
        expect(paths).toContain('/portal/communications');
    });
});
