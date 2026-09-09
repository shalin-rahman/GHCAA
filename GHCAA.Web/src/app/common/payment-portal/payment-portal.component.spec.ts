// @vitest-environment jsdom
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { signal } from '@angular/core';
import { PaymentPortalComponent } from './payment-portal.component';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { RegistrationService } from '../../core/services/registration.service';
import { FinancialService } from '../../core/services/financial.service';
import { AuthService } from '../../core/services/auth.service';
import { of } from 'rxjs';

import { vi } from 'vitest';

describe('PaymentPortalComponent', () => {
  let component: PaymentPortalComponent;
  let fixture: ComponentFixture<PaymentPortalComponent>;
  let mockFinancialService: any;
  let mockAuthService: any;

  beforeEach(async () => {
    mockFinancialService = {
      getSavedMethods: vi.fn().mockReturnValue(of([])),
      deleteSavedMethod: vi.fn().mockReturnValue(of({}))
    };
    mockAuthService = {
      isAuthenticated: vi.fn().mockReturnValue(true),
      // Real signal (not vi.fn()) — whenAuthenticated() reads it directly.
      authChecked: signal(true),
      // Mirrors AuthService.whenAuthenticated: a synchronous check-and-call is enough here.
      whenAuthenticated: vi.fn((callback: () => void) => {
        if (mockAuthService.authChecked() && mockAuthService.isAuthenticated()) callback();
      })
    };

    await TestBed.configureTestingModule({
      imports: [PaymentPortalComponent, HttpClientTestingModule],
      providers: [
        { provide: FinancialService, useValue: mockFinancialService },
        { provide: AuthService, useValue: mockAuthService }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(PaymentPortalComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should load saved methods on init if authenticated', () => {
    expect(mockFinancialService.getSavedMethods).toHaveBeenCalled();
  });

  it('should switch tabs', () => {
    component.activePortalTab.set('all');
    expect(component.activePortalTab()).toBe('all');
  });

  it('should emit methodSelected when a method is clicked', () => {
    const emitSpy = vi.spyOn(component.methodSelected, 'emit');
    const mockMethod = { id: 1, displayName: 'BKash' };
    component.selectMethod(mockMethod as any);
    expect(emitSpy).toHaveBeenCalledWith(mockMethod);
  });
});
