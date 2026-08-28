import { TestBed } from '@angular/core/testing';
import { ApplicationRef } from '@angular/core';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { AlertService } from './alert.service';
import { AuthService } from './auth.service';
import { API_ENDPOINTS } from '../constants/app.constants';
import { createAuthServiceMock } from '../testing/testing-utils';
import { describe, it, expect, beforeEach, afterEach } from 'vitest';

describe('AlertService', () => {
    let service: AlertService;
    let httpMock: HttpTestingController;
    let authServiceMock: ReturnType<typeof createAuthServiceMock>;

    beforeEach(() => {
        // authChecked defaults to true in the mock (matches a cached-session fast path), so the
        // effect-gated constructor load fires immediately without needing an ApplicationRef.tick().
        authServiceMock = createAuthServiceMock({ isAuthenticated: true });
        TestBed.configureTestingModule({
            imports: [HttpClientTestingModule],
            providers: [
                AlertService,
                { provide: AuthService, useValue: authServiceMock }
            ]
        });
        service = TestBed.inject(AlertService);
        httpMock = TestBed.inject(HttpTestingController);
        // The constructor's effect() is scheduled, not run synchronously — flush it via a tick.
        TestBed.inject(ApplicationRef).tick();
    });

    afterEach(() => {
        httpMock.verify();
    });

    it('should be created', () => {
        expect(service).toBeTruthy();

        // Handle the initial loadNotifications() call from constructor
        const req = httpMock.expectOne(API_ENDPOINTS.NOTIFICATIONS.BASE);
        expect(req.request.method).toBe('GET');
        req.flush([]);
    });

    it('should fetch notifications again when loadNotifications is called', () => {
        // Handle the initial loadNotifications() call from constructor
        const req1 = httpMock.expectOne(API_ENDPOINTS.NOTIFICATIONS.BASE);
        req1.flush([]);

        // Call it again
        service.loadNotifications();
        const req2 = httpMock.expectOne(API_ENDPOINTS.NOTIFICATIONS.BASE);
        expect(req2.request.method).toBe('GET');
        req2.flush([]);
    });

    it('does not call the [Authorize] endpoint for a guest', () => {
        // Drain the outer beforeEach's own constructor-triggered request before swapping modules,
        // or afterEach's httpMock.verify() on that original controller sees it as unflushed.
        httpMock.expectOne(API_ENDPOINTS.NOTIFICATIONS.BASE).flush([]);

        authServiceMock.isAuthenticated.mockReturnValue(false);
        authServiceMock.authChecked.set(true);

        TestBed.resetTestingModule();
        TestBed.configureTestingModule({
            imports: [HttpClientTestingModule],
            providers: [
                AlertService,
                { provide: AuthService, useValue: authServiceMock }
            ]
        });
        const guestService = TestBed.inject(AlertService);
        const guestHttpMock = TestBed.inject(HttpTestingController);
        TestBed.inject(ApplicationRef).tick();

        expect(guestService).toBeTruthy();
        guestHttpMock.expectNone(API_ENDPOINTS.NOTIFICATIONS.BASE);
        guestHttpMock.verify();
    });
});
