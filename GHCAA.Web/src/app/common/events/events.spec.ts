import { createAuthServiceMock } from '../../core/testing/testing-utils';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Events } from './events';
import { EventsService } from '../../core/services/events.service';
import { NotificationService } from '../../core/services/notification.service';
import { of } from 'rxjs';
import { AuthService } from '../../core/services/auth.service';
import { GatewaysService } from '../../core/services/gateways.service';

import { ActivatedRoute, provideRouter } from '@angular/router';

describe('Events Component', () => {
    let component: Events;
    let fixture: ComponentFixture<Events>;
    let eventsServiceMock: any;
    let notificationServiceMock: any;
    let activatedRouteMock: any;
    let gatewaysServiceMock: any;

    beforeEach(async () => {
        eventsServiceMock = {
            getEvents: vi.fn().mockReturnValue(of([])),
            getUpcomingEvents: vi.fn().mockReturnValue(of([])),
            getMyRegistrations: vi.fn().mockReturnValue(of([])),
            registerForEvent: vi.fn().mockReturnValue(of({ success: true })),
            getRegistrationForInvitation: vi.fn().mockReturnValue(of({}))
        };

        gatewaysServiceMock = {
            initiatePayment: vi.fn().mockReturnValue(of({ success: true, gatewayUrl: 'http://pay.com' }))
        };

        notificationServiceMock = {
            success: vi.fn(),
            error: vi.fn()
        };

        activatedRouteMock = {
            snapshot: { queryParamMap: { get: vi.fn().mockReturnValue(null) } }
        };

        const authServiceMock = {
            isAuthenticated: vi.fn().mockReturnValue(false),
            currentUser: vi.fn().mockReturnValue(null),
            getToken: vi.fn().mockReturnValue(null)
        };

        await TestBed.configureTestingModule({
            imports: [Events],
            providers: [
                { provide: EventsService, useValue: eventsServiceMock },
                { provide: NotificationService, useValue: notificationServiceMock },
                { provide: ActivatedRoute, useValue: activatedRouteMock },
                { provide: AuthService, useValue: authServiceMock },
                { provide: GatewaysService, useValue: gatewaysServiceMock },
                provideRouter([])
            ]
        }).compileComponents();

        fixture = TestBed.createComponent(Events);
        component = fixture.componentInstance;
        fixture.detectChanges();
    });

    it('should create', () => {
        expect(component).toBeTruthy();
    });

    it('should load events on init', () => {
        expect(eventsServiceMock.getEvents).toHaveBeenCalled();
    });

    it('should correctly prefix online gateway references', () => {
        const mockEvent = { id: 1, title: 'Test', requiresPayment: true, registrationFee: 500 };
        const mockMethod = { id: 1, method: 'SSLCommerz', gateway: 'SSLCommerz', isOnline: true };
        
        component.selectedEvent.set(mockEvent as any);
        component.selectedPaymentMethod.set(mockMethod as any);
        component.regForm.patchValue({ paymentReference: 'MY_REF' });
        
        component.submitRegistration();
        
        // Check that registerForEvent was called with EVT-REG- prefix if it's an online gateway
        const callArgs = eventsServiceMock.registerForEvent.mock.calls[0][0];
        expect(callArgs.paymentReference).toContain('EVT-REG-');
    });

    it('should initiate gateway for online payments', () => {
        const mockEvent = { id: 1, title: 'Test', requiresPayment: true, registrationFee: 500 };
        const mockMethod = { id: 1, method: 'SSLCommerz', gateway: 'SSLCommerz', isOnline: true };
        
        component.selectedEvent.set(mockEvent as any);
        component.selectedPaymentMethod.set(mockMethod as any);
        component.regForm.patchValue({
            guestName: 'Guest User',
            guestEmail: 'guest@test.com',
            guestMobile: '01700000000',
            contributionAmount: 500,
            paymentReference: 'TEST-REF'
        });
        
        fixture.detectChanges();
        component.submitRegistration();
        
        expect(gatewaysServiceMock.initiatePayment).toHaveBeenCalled();
    });

    it('should not initiate gateway for manual payments', () => {
        const mockEvent = { id: 1, title: 'Test', requiresPayment: true, registrationFee: 500 };
        const mockMethod = { id: 1, method: 'BKash', gateway: 'None', isOnline: false };
        
        component.selectedEvent.set(mockEvent as any);
        component.selectedPaymentMethod.set(mockMethod as any);
        component.regForm.patchValue({ paymentReference: '123456' });
        
        component.submitRegistration();
        
        expect(gatewaysServiceMock.initiatePayment).not.toHaveBeenCalled();
        expect(notificationServiceMock.success).toHaveBeenCalled();
    });

    it('should not open the registration modal for informational-only events', () => {
        const mockEvent = { id: 1, title: 'Test', requiresRegistration: false, allowNonMembers: true };
        component.openRegisterModal(mockEvent as any);
        expect(component.showModal()).toBe(false);
        expect(component.selectedEvent()).toBeNull();
    });

    it('should update validators when manual method is selected', () => {
       const mockMethod = { id: 1, requiresReference: true };
       component.onPaymentMethodSelected(mockMethod as any);
       
       const refControl = component.regForm.get('paymentReference');
       expect(refControl?.validator).toBeTruthy();
       
       const mockFreeMethod = { id: 2, requiresReference: false };
       component.onPaymentMethodSelected(mockFreeMethod as any);
       expect(refControl?.validator).toBeFalsy();
    });
});
