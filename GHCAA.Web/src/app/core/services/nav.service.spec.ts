import { createAuthServiceMock } from '../testing/testing-utils';
import { TestBed } from '@angular/core/testing';
import { NavService } from './nav.service';
import { AuthService } from './auth.service';
import { vi, describe, it, expect, beforeEach } from 'vitest';

describe('NavService', () => {
    let service: NavService;
    let authServiceMock: any;

    beforeEach(() => {
        authServiceMock = {
            currentUser: vi.fn(),
            isAuthenticated: vi.fn().mockReturnValue(true)
        };

        TestBed.configureTestingModule({
            providers: [
                NavService,
                { provide: AuthService, useValue: authServiceMock }
            ]
        });
        service = TestBed.inject(NavService);
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
