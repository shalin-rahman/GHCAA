import { createAuthServiceMock, createNotificationServiceMock } from '../../core/testing/testing-utils';
import { signal } from '@angular/core';
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
            getEventsPage: vi.fn().mockReturnValue(of({ items: [], totalItems: 0, totalPages: 0, page: 1, pageSize: 20 })),
            getEventById: vi.fn(),
            getUpcomingEvents: vi.fn().mockReturnValue(of([])),
            getMyRegistrations: vi.fn().mockReturnValue(of([])),
            registerForEvent: vi.fn().mockReturnValue(of({ success: true })),
            getRegistrationForInvitation: vi.fn().mockReturnValue(of({}))
        };

        gatewaysServiceMock = {
            initiatePayment: vi.fn().mockReturnValue(of({ success: true, gatewayUrl: 'http://pay.com' }))
        };

        notificationServiceMock = createNotificationServiceMock();

        activatedRouteMock = {
            snapshot: { queryParamMap: { get: vi.fn().mockReturnValue(null) } }
        };

        const authServiceMock = {
            isAuthenticated: vi.fn().mockReturnValue(false),
            currentUser: vi.fn().mockReturnValue(null),
            getToken: vi.fn().mockReturnValue(null),
            // Real signal (not vi.fn()) — whenAuthenticated() reads it directly.
            authChecked: signal(true),
            // Mirrors AuthService.whenAuthenticated: a synchronous check-and-call is enough here.
            whenAuthenticated: vi.fn((callback: () => void) => {
                if (authServiceMock.authChecked() && authServiceMock.isAuthenticated()) callback();
            })
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

    it('should load the first page of events on init', () => {
        expect(eventsServiceMock.getEventsPage).toHaveBeenCalledWith(1, 20);
    });

    // NFR-P1: events load a page at a time.
    it('appends the next page until none is left', () => {
        eventsServiceMock.getEventsPage
            .mockReturnValueOnce(of({ items: [{ id: 1 }], totalItems: 2, totalPages: 2, page: 1, pageSize: 20 }))
            .mockReturnValueOnce(of({ items: [{ id: 2 }], totalItems: 2, totalPages: 2, page: 2, pageSize: 20 }));

        component.loadEvents();
        component.loadNextPage();

        expect(eventsServiceMock.getEventsPage).toHaveBeenLastCalledWith(2, 20);
        expect(component.events().map(e => e.id)).toEqual([1, 2]);
        expect(component.hasMore()).toBe(false);
    });

    it('fetches a deep-linked event that is not on the first page', () => {
        vi.useFakeTimers();
        const openSpy = vi.spyOn(component, 'openRegisterModal').mockImplementation(() => {});
        eventsServiceMock.getEventById.mockReturnValue(of({ id: 42, isActive: true, startDate: '2026-11-01T10:00:00Z' }));

        (component as any).resolveDeepLink('42');
        vi.advanceTimersByTime(200);

        expect(eventsServiceMock.getEventById).toHaveBeenCalledWith(42);
        expect(openSpy).toHaveBeenCalledWith(expect.objectContaining({ id: 42 }));
        expect(component.events()[0].id).toBe(42);
        vi.useRealTimers();
    });

    // NFR-P1: the registration form renders inside the event's row, so the row must exist.
    it('lists a deep-linked event once when its page loads later', () => {
        eventsServiceMock.getEventsPage
            .mockReturnValueOnce(of({ items: [{ id: 1 }], totalItems: 2, totalPages: 2, page: 1, pageSize: 20 }))
            .mockReturnValueOnce(of({ items: [{ id: 42, isActive: true, startDate: '2026-11-01T10:00:00Z' }], totalItems: 2, totalPages: 2, page: 2, pageSize: 20 }));
        eventsServiceMock.getEventById.mockReturnValue(of({ id: 42, isActive: true, startDate: '2026-11-01T10:00:00Z' }));
        vi.spyOn(component, 'openRegisterModal').mockImplementation(() => {});

        component.loadEvents();
        (component as any).resolveDeepLink('42');
        component.loadNextPage();

        expect(component.events().map(e => e.id)).toEqual([42, 1]);
    });

    it('ignores a deep link to an inactive event', () => {
        vi.useFakeTimers();
        const openSpy = vi.spyOn(component, 'openRegisterModal').mockImplementation(() => {});
        eventsServiceMock.getEventById.mockReturnValue(of({ id: 7, isActive: false }));

        (component as any).resolveDeepLink('7');
        vi.advanceTimersByTime(200);

        expect(openSpy).not.toHaveBeenCalled();
        vi.useRealTimers();
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
