import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AdminOrgConfig } from './org-config';

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
});
