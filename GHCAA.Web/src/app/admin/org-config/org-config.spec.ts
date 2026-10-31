import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AdminOrgConfig } from './org-config';
import { ORG_CONFIG_FALLBACK } from '../../core/config/org-config-fallback.generated';
import { DEFAULT_ELECTION_SETTINGS } from '../../core/models/election.models';
import { OrgConfig } from '../../core/models/org-config.model';
import { MEMBERSHIP_TYPE_OPTIONS } from '../../core/constants/app.constants';

describe('AdminOrgConfig', () => {
  let component: AdminOrgConfig;
  let fixture: ComponentFixture<AdminOrgConfig>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AdminOrgConfig]
    })
    .compileComponents();

    fixture = TestBed.createComponent(AdminOrgConfig);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  // 37.12a
  it('fills election defaults when the stored config has no elections section', () => {
    const { elections, ...withoutElections } = structuredClone(ORG_CONFIG_FALLBACK) as OrgConfig;
    (component as any).hydrate(withoutElections);
    expect(component.config?.elections).toEqual(DEFAULT_ELECTION_SETTINGS);
  });

  it('keeps stored election values over the defaults', () => {
    const cfg = { ...structuredClone(ORG_CONFIG_FALLBACK), elections: { ...DEFAULT_ELECTION_SETTINGS, inviteLinkHours: 5 } } as OrgConfig;
    (component as any).hydrate(cfg);
    expect(component.config?.elections?.inviteLinkHours).toBe(5);
  });

  it('keeps two-person actions in enum order when toggled', () => {
    (component as any).hydrate({ ...structuredClone(ORG_CONFIG_FALLBACK), elections: { ...DEFAULT_ELECTION_SETTINGS, twoPersonActions: [] } } as OrgConfig);
    component.toggleTwoPersonAction('Declare', true);
    component.toggleTwoPersonAction('Publish', true);
    expect(component.config?.elections?.twoPersonActions).toEqual(['Publish', 'Declare']);
    component.toggleTwoPersonAction('Publish', false);
    expect(component.isTwoPersonAction('Publish')).toBe(false);
    expect(component.isTwoPersonAction('Declare')).toBe(true);
  });

  // 7.26
  it('treats a missing email provider as the server default', () => {
    const cfg = structuredClone(ORG_CONFIG_FALLBACK) as OrgConfig;
    delete cfg.workflow.emailProvider;
    (component as any).hydrate(cfg);
    expect(component.config?.workflow.emailProvider).toBeNull();
  });

  it('keeps a stored email provider', () => {
    const cfg = structuredClone(ORG_CONFIG_FALLBACK) as OrgConfig;
    cfg.workflow.emailProvider = 'Brevo';
    (component as any).hydrate(cfg);
    expect(component.config?.workflow.emailProvider).toBe('Brevo');
  });

  // 34.D20
  it('offers the default membership type as a list of the known types', async () => {
    const cfg = structuredClone(ORG_CONFIG_FALLBACK) as OrgConfig;
    cfg.workflow.defaultMembershipType = 'Associate';
    (component as any).hydrate(cfg);
    component.activeTab.set('workflow');
    fixture.detectChanges();
    await fixture.whenStable();

    const host: HTMLElement = fixture.nativeElement;
    expect(host.querySelector('input[name="defaultMembershipType"]')).toBeNull();
    const select = host.querySelector<HTMLSelectElement>('select[name="defaultMembershipType"]')!;
    expect(Array.from(select.options).map(o => o.value)).toEqual(MEMBERSHIP_TYPE_OPTIONS.map(o => o.value));
    expect(select.value).toBe('Associate');
  });
});
