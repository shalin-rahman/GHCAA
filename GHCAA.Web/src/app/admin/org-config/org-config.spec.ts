import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AdminOrgConfig } from './org-config';
import { ORG_CONFIG_FALLBACK } from '../../core/config/org-config-fallback.generated';
import { DEFAULT_ELECTION_SETTINGS } from '../../core/models/election.models';
import { OrgConfig } from '../../core/models/org-config.model';

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
});
