import { TestBed } from '@angular/core/testing';
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
});
