import { TestBed } from '@angular/core/testing';
import { ApplicationRef } from '@angular/core';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { RouterTestingModule } from '@angular/router/testing';
import { Router } from '@angular/router';
import { AuthService } from './auth.service';
import { API_ENDPOINTS } from '../constants/app.constants';
import { User } from '../models/auth.models';

import { vi } from 'vitest';

describe('AuthService', () => {
    let service: AuthService;
    let httpMock: HttpTestingController;
    let router: Router;

    beforeEach(() => {
        sessionStorage.clear();
        localStorage.clear();

        TestBed.configureTestingModule({
            imports: [HttpClientTestingModule, RouterTestingModule.withRoutes([])],
            providers: [AuthService]
        });
        service = TestBed.inject(AuthService);
        httpMock = TestBed.inject(HttpTestingController);
        router = TestBed.inject(Router);
        vi.spyOn(router, 'navigate');

        // The constructor's /auth/me call is deferred via afterNextRender (NG0200 guard) —
        // tick the ApplicationRef so the deferred callback fires before we expect the request.
        TestBed.inject(ApplicationRef).tick();
        // Flush the /auth/me request triggered by the constructor when no session exists.
        httpMock.expectOne(API_ENDPOINTS.AUTH.ME).flush(null, { status: 401, statusText: 'Unauthorized' });
    });

    afterEach(() => {
        httpMock.verify();
        sessionStorage.clear();
        localStorage.clear();
    });

    it('should be created', () => {
        expect(service).toBeTruthy();
    });

    it('should login and set session', () => {
        const credentials = { username: 'user', password: 'password' };
        const mockResponse = { token: 'new-token', memberId: 1, username: 'user', role: 'Admin' };

        service.login(credentials).subscribe(user => {
            expect(user.token).toBe('new-token');
            expect(user.role).toBe('Admin');
            expect(user.memberId).toBe(1);
            expect(service.isAuthenticated()).toBe(true);
            expect(service.getToken()).toBe('new-token');
            // Token is NOT stored — only display fields are persisted.
            const stored = JSON.parse(sessionStorage.getItem('user_session')!);
            expect(stored.token).toBeUndefined();
            expect(stored.username).toBe('user');
        });

        const req = httpMock.expectOne(API_ENDPOINTS.AUTH.LOGIN);
        expect(req.request.method).toBe('POST');
        expect(req.request.body).toEqual(credentials);
        req.flush(mockResponse);
    });

    it('should logout and clear session', () => {
        service.logout();

        // Flush the POST /auth/logout call.
        const req = httpMock.expectOne(API_ENDPOINTS.AUTH.LOGOUT);
        expect(req.request.method).toBe('POST');
        req.flush(null);

        expect(service.isAuthenticated()).toBe(false);
        expect(service.getToken()).toBeNull();
        expect(sessionStorage.getItem('user_session')).toBeNull();
        expect(router.navigate).toHaveBeenCalledWith(['/login']);
    });

    // FR-37.12d. Accepting an appointment adds a role the current login does not carry.
    it('reloadUser takes the new token and the roles from /auth/me', () => {
        service.login({ username: 'user', password: 'p' }).subscribe();
        httpMock.expectOne(API_ENDPOINTS.AUTH.LOGIN).flush({ token: 'old', memberId: 1, username: 'user', role: 'Member', roles: ['Member'] });

        let result: boolean | undefined;
        service.reloadUser().subscribe(ok => result = ok);
        httpMock.expectOne(API_ENDPOINTS.AUTH.REFRESH).flush({ token: 'new' });
        httpMock.expectOne(API_ENDPOINTS.AUTH.ME).flush({ username: 'user', memberId: 1, role: 'Member', roles: ['Member', 'ElectionOfficial'] });

        expect(result).toBe(true);
        expect(service.getToken()).toBe('new');
        expect(service.hasRole('ElectionOfficial')).toBe(true);
        expect(JSON.parse(sessionStorage.getItem('user_session')!).roles).toContain('ElectionOfficial');
    });

    // FR-37.12d. A second refresh would present the rotated refresh token, and the API answers
    // token reuse by ending every session of the user.
    it('refresh shares one request between callers that ask at the same time', () => {
        service.login({ username: 'user', password: 'p' }).subscribe();
        httpMock.expectOne(API_ENDPOINTS.AUTH.LOGIN).flush({ token: 'old', memberId: 1, username: 'user', role: 'Member', roles: ['Member'] });

        const results: boolean[] = [];
        service.refresh().subscribe(ok => results.push(ok));
        service.refresh().subscribe(ok => results.push(ok));
        httpMock.expectOne(API_ENDPOINTS.AUTH.REFRESH).flush({ token: 'new' });

        expect(results).toEqual([true, true]);
        expect(service.getToken()).toBe('new');

        service.refresh().subscribe();
        httpMock.expectOne(API_ENDPOINTS.AUTH.REFRESH).flush({ token: 'newer' });
        expect(service.getToken()).toBe('newer');
    });

    it('reloadUser keeps the user as they were when the refresh fails', () => {
        service.login({ username: 'user', password: 'p' }).subscribe();
        httpMock.expectOne(API_ENDPOINTS.AUTH.LOGIN).flush({ token: 'old', memberId: 1, username: 'user', role: 'Member', roles: ['Member'] });

        let result: boolean | undefined;
        service.reloadUser().subscribe(ok => result = ok);
        httpMock.expectOne(API_ENDPOINTS.AUTH.REFRESH).flush(null, { status: 401, statusText: 'Unauthorized' });

        expect(result).toBe(false);
        expect(service.getToken()).toBe('old');
        expect(service.hasRole('ElectionOfficial')).toBe(false);
    });

    it('drops stored ballot receipts on logout and keeps other settings', () => {
        localStorage.setItem('ghcaa_ballot_receipt_7_5', 'a');
        localStorage.setItem('ghcaa_ballot_receipt_8_5', 'b');
        localStorage.setItem('ghcaa_theme', 'dark');

        service.logout();
        httpMock.expectOne(API_ENDPOINTS.AUTH.LOGOUT).flush(null);

        expect(localStorage.getItem('ghcaa_ballot_receipt_7_5')).toBeNull();
        expect(localStorage.getItem('ghcaa_ballot_receipt_8_5')).toBeNull();
        expect(localStorage.getItem('ghcaa_theme')).toBe('dark');
        localStorage.removeItem('ghcaa_theme');
    });
});
