import { ComponentFixture, TestBed } from '@angular/core/testing';
import { AdminLayout } from './admin-layout';
import { AuthService } from '../../core/services/auth.service';
import { NavService } from '../../core/services/nav.service';
import { OrgConfigService } from '../../core/services/org-config.service';
import { provideRouter } from '@angular/router';
import { signal } from '@angular/core';
import { vi, describe, it, expect, beforeEach } from 'vitest';

describe('AdminLayout', () => {
  let component: AdminLayout;
  let fixture: ComponentFixture<AdminLayout>;
  let authMock: any;
  let navMock: any;
  let orgConfigMock: any;

  const mockSections = [
    {
      name: 'Overview',
      items: [{ path: '/admin/dashboard', label: 'Dashboard', icon: 'dashboard' }]
    },
    {
      name: 'Membership',
      items: [
        { path: '/admin/approvals', label: 'Approvals', icon: 'approvals' },
        { path: '/admin/members', label: 'All Members', icon: 'members' }
      ]
    },
    {
      name: 'Finance & Fundraising',
      items: [
        { path: '/admin/ledger', label: 'Financial Ledger', icon: 'ledger' },
        { path: '/admin/payments/fees', label: 'Fee Policy', icon: 'fee-policy' }
      ]
    }
  ];

  beforeEach(async () => {
    authMock = {
      currentUser: signal({ role: 'SuperAdmin', fullName: 'Super Admin', memberId: 'm-1' })
    };

    navMock = {
      adminNavSections: signal(mockSections),
      labelFor: vi.fn().mockReturnValue('Control Panel')
    };

    orgConfigMock = {
      config: signal({ branding: { shortName: 'GHCAA', logoUrl: '' } })
    };

    await TestBed.configureTestingModule({
      imports: [AdminLayout],
      providers: [
        provideRouter([]),
        { provide: AuthService, useValue: authMock },
        { provide: NavService, useValue: navMock },
        { provide: OrgConfigService, useValue: orgConfigMock }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(AdminLayout);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('creates the admin layout component', () => {
    expect(component).toBeTruthy();
  });

  it('renders all sections and items when search query is empty', () => {
    const sections = component.filteredNavSections();
    expect(sections.length).toBe(3);
    const compiled = fixture.nativeElement as HTMLElement;
    const sectionLabels = compiled.querySelectorAll('.nav-section-label');
    expect(sectionLabels.length).toBe(3);
  });

  it('filters sections and items matching query by item label', () => {
    component.menuSearch.set('fee');
    fixture.detectChanges();

    const sections = component.filteredNavSections();
    expect(sections.length).toBe(1);
    expect(sections[0].name).toBe('Finance & Fundraising');
    expect(sections[0].items.length).toBe(1);
    expect(sections[0].items[0].label).toBe('Fee Policy');

    const compiled = fixture.nativeElement as HTMLElement;
    const items = compiled.querySelectorAll('.nav-body .nav-item .nav-label');
    expect(items.length).toBe(1);
    expect(items[0].textContent).toContain('Fee Policy');
  });

  it('filters sections when search matches section name', () => {
    component.menuSearch.set('membership');
    fixture.detectChanges();

    const sections = component.filteredNavSections();
    expect(sections.length).toBe(1);
    expect(sections[0].name).toBe('Membership');
    expect(sections[0].items.length).toBe(2);
  });

  it('renders empty state when search matches no menu items', () => {
    component.menuSearch.set('nonexistent query 123');
    fixture.detectChanges();

    const sections = component.filteredNavSections();
    expect(sections.length).toBe(0);

    const compiled = fixture.nativeElement as HTMLElement;
    const emptyState = compiled.querySelector('.nav-empty-state');
    expect(emptyState).toBeTruthy();
    expect(emptyState?.textContent).toContain('No menu items found');
  });

  it('clears menu search on clearMenuSearch()', () => {
    component.menuSearch.set('fee');
    fixture.detectChanges();
    expect(component.filteredNavSections().length).toBe(1);

    component.clearMenuSearch();
    fixture.detectChanges();

    expect(component.menuSearch()).toBe('');
    expect(component.filteredNavSections().length).toBe(3);
  });

  it('updates menuSearch when the shared search bar emits', () => {
    const input = (fixture.nativeElement as HTMLElement).querySelector('.nav-search-container input') as HTMLInputElement;
    input.value = 'ledger';
    input.dispatchEvent(new Event('input'));
    fixture.detectChanges();

    expect(component.menuSearch()).toBe('ledger');
    expect(component.filteredNavSections().length).toBe(1);
    expect(component.filteredNavSections()[0].items[0].label).toBe('Financial Ledger');
  });

  // Every admin path starts with /admin/, so matching on paths made these show the whole menu.
  it.each(['admin', 'min', '/admin/', '/'])('does not match items through their path for "%s"', query => {
    component.menuSearch.set(query);
    expect(component.filteredNavSections()).toEqual([]);
  });

  it('clears the search on Escape', () => {
    component.menuSearch.set('fee');
    fixture.detectChanges();
    const input = (fixture.nativeElement as HTMLElement).querySelector('.nav-search-container input') as HTMLInputElement;
    input.dispatchEvent(new KeyboardEvent('keydown', { key: 'Escape', bubbles: true }));
    expect(component.menuSearch()).toBe('');
  });

  it('shows no empty-search message when the menu is empty and nothing is typed', () => {
    navMock.adminNavSections.set([]);
    fixture.detectChanges();
    expect((fixture.nativeElement as HTMLElement).querySelector('.nav-empty-state')).toBeNull();
  });

  it('toggles mobile sidebar state', () => {
    expect(component.isSidebarOpen()).toBe(false);
    component.toggleSidebar();
    expect(component.isSidebarOpen()).toBe(true);
    component.toggleSidebar();
    expect(component.isSidebarOpen()).toBe(false);
  });
});
