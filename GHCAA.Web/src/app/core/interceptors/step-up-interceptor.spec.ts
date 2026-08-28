import { TestBed } from '@angular/core/testing';
import { HttpClient, HttpErrorResponse, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { describe, it, expect, beforeEach, afterEach, vi } from 'vitest';
import { of } from 'rxjs';
import { globalHttpInterceptor } from './global-http.interceptor';
import { StepUpService } from '../services/step-up.service';
import { AuthService } from '../services/auth.service';
import { NotificationService } from '../services/notification.service';

// 7.13: the interceptor must turn a 403 STEP_UP_REQUIRED into an OTP challenge and then
// replay the original request, rather than surfacing a bare "no permission" error.
describe('globalHttpInterceptor — step-up branch', () => {
    let httpMock: HttpTestingController;
    let http: HttpClient;
    let stepUpMock: { challenge: ReturnType<typeof vi.fn> };

    beforeEach(() => {
        stepUpMock = { challenge: vi.fn() };

        TestBed.configureTestingModule({
            providers: [
                provideHttpClient(withInterceptors([globalHttpInterceptor])),
                provideHttpClientTesting(),
                { provide: StepUpService, useValue: stepUpMock },
                { provide: AuthService, useValue: { getToken: () => null, refresh: vi.fn(), logout: vi.fn() } },
                { provide: NotificationService, useValue: { error: vi.fn() } }
            ]
        });

        httpMock = TestBed.inject(HttpTestingController);
        http = TestBed.inject(HttpClient);
    });

    afterEach(() => httpMock.verify());

    it('challenges and retries the original request once verified', () => {
        stepUpMock.challenge.mockReturnValue(of(true));
        const onNext = vi.fn();

        http.delete('/api/admin/members/5').subscribe(onNext);

        httpMock.expectOne(r => r.url.endsWith('/api/admin/members/5')).flush(
            { code: 'STEP_UP_REQUIRED', message: 'Additional verification is required.' },
            { status: 403, statusText: 'Forbidden' }
        );

        expect(stepUpMock.challenge).toHaveBeenCalled();

        // The retry is a fresh request that now succeeds.
        httpMock.expectOne(r => r.url.endsWith('/api/admin/members/5')).flush({ ok: true });
        expect(onNext).toHaveBeenCalledWith({ ok: true });
    });

    it('propagates the original error when the admin cancels', () => {
        stepUpMock.challenge.mockReturnValue(of(false));
        const onError = vi.fn();

        http.delete('/api/admin/members/5').subscribe({ error: onError });

        httpMock.expectOne(r => r.url.endsWith('/api/admin/members/5')).flush(
            { code: 'STEP_UP_REQUIRED', message: 'Additional verification is required.' },
            { status: 403, statusText: 'Forbidden' }
        );

        expect(onError).toHaveBeenCalled();
        expect(onError.mock.calls[0][0]).toBeInstanceOf(HttpErrorResponse);
    });

    it('does not challenge on an ordinary 403', () => {
        const onError = vi.fn();

        http.delete('/api/admin/members/5').subscribe({ error: onError });

        httpMock.expectOne(r => r.url.endsWith('/api/admin/members/5')).flush(
            { message: 'You do not have permission.' },
            { status: 403, statusText: 'Forbidden' }
        );

        expect(stepUpMock.challenge).not.toHaveBeenCalled();
        expect(onError).toHaveBeenCalled();
    });
});
