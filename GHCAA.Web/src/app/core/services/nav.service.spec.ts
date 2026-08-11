import { createAuthServiceMock } from '../testing/testing-utils';
import { TestBed } from '@angular/core/testing';
import { NavService } from './nav.service';
import { AuthService } from './auth.service';
import { OrgConfigService } from './org-config.service';
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

    it('should filter admin nav items for Admin role', () => {
        authServiceMock.currentUser.mockReturnValue({ role: 'Admin' });
        const items = service.adminNavItems();
        
        // Admin shouldn't see Payment Settings or Roles (SuperAdmin only)
        expect(items.some(i => i.label === 'Payment Settings')).toBe(false);
        expect(items.some(i => i.label === 'User Roles')).toBe(false);
    });

    it('should show all admin nav items for SuperAdmin role', () => {
        authServiceMock.currentUser.mockReturnValue({ role: 'SuperAdmin' });
        const items = service.adminNavItems();
        
        expect(items.some(i => i.label === 'Payment Settings')).toBe(true);
        expect(items.some(i => i.label === 'User Roles')).toBe(true);
    });

    it('should group admin nav items into sections', () => {
        authServiceMock.currentUser.mockReturnValue({ role: 'SuperAdmin' });
        const sections = service.adminNavSections();
        
        expect(sections.length).toBeGreaterThan(0);
        expect(sections[0].name).toBe('Overview');
        expect(sections.find(s => s.name === 'Finance & Tools')).toBeDefined();
    });
});
