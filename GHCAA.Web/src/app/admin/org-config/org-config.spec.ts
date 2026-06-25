import { ComponentFixture, TestBed } from '@angular/core/testing';

import { OrgConfig } from './org-config';

describe('OrgConfig', () => {
  let component: OrgConfig;
  let fixture: ComponentFixture<OrgConfig>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [OrgConfig]
    })
    .compileComponents();

    fixture = TestBed.createComponent(OrgConfig);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
