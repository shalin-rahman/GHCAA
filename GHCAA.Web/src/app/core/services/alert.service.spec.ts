import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { AlertService } from './alert.service';
import { API_ENDPOINTS } from '../constants/api.endpoints';
import { describe, it, expect, beforeEach, afterEach } from 'vitest';

describe('AlertService', () => {
    let service: AlertService;
    let httpMock: HttpTestingController;

    beforeEach(() => {
        TestBed.configureTestingModule({
            imports: [HttpClientTestingModule],
            providers: [AlertService]
        });
        service = TestBed.inject(AlertService);
        httpMock = TestBed.inject(HttpTestingController);
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
});
