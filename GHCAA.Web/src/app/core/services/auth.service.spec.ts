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
        TestBed.configureTestingModule({
            imports: [HttpClientTestingModule, RouterTestingModule.withRoutes([])],
            providers: [AuthService]
        });
        service = TestBed.inject(AuthService);
        httpMock = TestBed.inject(HttpTestingController);
        router = TestBed.inject(Router);
        vi.spyOn(router, 'navigate');

        localStorage.clear();
    });

    afterEach(() => {
        httpMock.verify();
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
            expect(localStorage.getItem('user_session')).toBeTruthy();
        });

        const req = httpMock.expectOne(API_ENDPOINTS.AUTH.LOGIN);
        expect(req.request.method).toBe('POST');
        expect(req.request.body).toEqual(credentials);
        req.flush(mockResponse);
    });

    it('should logout and clear session', () => {
        // Set initial session
        const mockUser: User = { token: 'token', memberId: 1, username: 'user', role: 'Member' };
        localStorage.setItem('user_session', JSON.stringify(mockUser));
        
        // Mock a re-initialization manually for testing logout logic separately since session is populated in constructor typically, 
        // we will directly call logout.
        service.logout();

        expect(service.isAuthenticated()).toBe(false);
        expect(service.getToken()).toBeNull();
        expect(localStorage.getItem('user_session')).toBeNull();
        expect(router.navigate).toHaveBeenCalledWith(['/login']);
    });


});
