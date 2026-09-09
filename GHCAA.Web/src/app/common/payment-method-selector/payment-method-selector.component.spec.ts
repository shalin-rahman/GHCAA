// @vitest-environment jsdom
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { PaymentMethodSelectorComponent } from './payment-method-selector.component';
import { PaymentConfigService } from '../../core/services/payment-config.service';
import { of } from 'rxjs';

describe('PaymentMethodSelectorComponent', () => {
  let component: PaymentMethodSelectorComponent;
  let fixture: ComponentFixture<PaymentMethodSelectorComponent>;
  let paymentServiceMock: any;

  beforeEach(async () => {
    paymentServiceMock = {
       getActivePaymentMethods: vi.fn().mockReturnValue(of([
           { id: 1, displayName: 'bKash Wallet', method: 'BKash', gateway: 'None' },
           { id: 2, displayName: 'MasterCard', method: 'CreditCard', gateway: 'SSLCommerz' }
       ]))
    };

    await TestBed.configureTestingModule({
      imports: [PaymentMethodSelectorComponent],
      providers: [
        { provide: PaymentConfigService, useValue: paymentServiceMock }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(PaymentMethodSelectorComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should correctly identify brand types', () => {
     const bkashMethod = { displayName: 'bKash Official', method: 'BKash' };
     expect(component.isBrand(bkashMethod as any, 'bkash')).toBe(true);
     expect(component.isBrand(bkashMethod as any, 'nagad')).toBe(false);

     const cardMethod = { displayName: 'Visa Card', method: 'CreditCard' };
     expect(component.isBrand(cardMethod as any, 'card')).toBe(true);
  });

  it('should resolve correct asset URLs for brands', () => {
      const bkashMethod = { displayName: 'bKash Wallet', method: 'BKash' };
      expect(component.getLogoUrl(bkashMethod as any)).toContain('bkash.svg');
      
      const nagadMethod = { displayName: 'Nagad Pay', method: 'Nagad' };
      expect(component.getLogoUrl(nagadMethod as any)).toContain('nagad.svg');
      
      const customMethod = { displayName: 'Custom Method', icon: '💰' };
      expect(component.getLogoUrl(customMethod as any)).toContain('data:image/svg+xml');
  });

  it('should emit event when method is selected', () => {
      const spy = vi.spyOn(component.methodSelected, 'emit');
      const method = { id: 1, displayName: 'bKash' };
      component.selectMethod(method as any);
      expect(spy).toHaveBeenCalledWith(method);
      expect(component.selectedMethodId).toBe(1);
  });
});
