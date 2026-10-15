import { TestBed } from '@angular/core/testing';
import { HttpClient, HttpHeaders, HttpXsrfTokenExtractor, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { describe, it, expect, beforeEach, afterEach, vi } from 'vitest';
import { of } from 'rxjs';
import { globalHttpInterceptor } from './global-http.interceptor';
import { StepUpService } from '../services/step-up.service';
import { AuthService } from '../services/auth.service';
import { NotificationService } from '../services/notification.service';
import { XSRF } from '../constants/app.constants';

// NFR-S1: the refresh and step-up responses set a new XSRF-TOKEN cookie. A retried write must send
// the new value, or XsrfMiddleware rejects it with 403 (seen live on PUT /api/admin/members/{id}).
describe('globalHttpInterceptor: XSRF header on retry', () => {
    let httpMock: HttpTestingController;
    let http: HttpClient;
    let cookieToken: string | null;
    let stepUpMock: { challenge: ReturnType<typeof vi.fn> };

    beforeEach(() => {
        cookieToken = 'old';
        stepUpMock = { challenge: vi.fn() };

        TestBed.configureTestingModule({
            providers: [
                provideHttpClient(withInterceptors([globalHttpInterceptor])),
                provideHttpClientTesting(),
                { provide: HttpXsrfTokenExtractor, useValue: { getToken: () => cookieToken } },
                { provide: StepUpService, useValue: stepUpMock },
                {
                    provide: AuthService,
                    useValue: {
                        getToken: () => null,
                        refresh: vi.fn(() => { cookieToken = 'new'; return of(true); }),
                        logout: vi.fn()
                    }
                },
                { provide: NotificationService, useValue: { error: vi.fn() } }
            ]
        });

        httpMock = TestBed.inject(HttpTestingController);
        http = TestBed.inject(HttpClient);
    });

    afterEach(() => httpMock.verify());

    const withHeader = () => new HttpHeaders({ [XSRF.HEADER]: 'old' });

    it('sends the refreshed token when a write is retried after a 401', () => {
        http.put('/api/admin/members/573', {}, { headers: withHeader() }).subscribe();

        httpMock.expectOne(r => r.url.endsWith('/api/admin/members/573'))
            .flush(null, { status: 401, statusText: 'Unauthorized' });

        const retry = httpMock.expectOne(r => r.url.endsWith('/api/admin/members/573'));
        expect(retry.request.headers.get(XSRF.HEADER)).toBe('new');
        retry.flush({ ok: true });
    });

    it('sends the current token when a write is retried after step-up', () => {
        stepUpMock.challenge.mockImplementation(() => { cookieToken = 'new'; return of(true); });

        http.delete('/api/admin/members/5', { headers: withHeader() }).subscribe();

        httpMock.expectOne(r => r.url.endsWith('/api/admin/members/5')).flush(
            { code: 'STEP_UP_REQUIRED' },
            { status: 403, statusText: 'Forbidden' }
        );

        const retry = httpMock.expectOne(r => r.url.endsWith('/api/admin/members/5'));
        expect(retry.request.headers.get(XSRF.HEADER)).toBe('new');
        retry.flush({ ok: true });
    });

    it('leaves a retry alone when the cookie is gone', () => {
        cookieToken = null;
        const auth = TestBed.inject(AuthService) as unknown as { refresh: ReturnType<typeof vi.fn> };
        auth.refresh.mockReturnValue(of(true));

        http.put('/api/admin/members/573', {}, { headers: withHeader() }).subscribe();

        httpMock.expectOne(r => r.url.endsWith('/api/admin/members/573'))
            .flush(null, { status: 401, statusText: 'Unauthorized' });

        const retry = httpMock.expectOne(r => r.url.endsWith('/api/admin/members/573'));
        expect(retry.request.headers.get(XSRF.HEADER)).toBe('old');
        retry.flush({ ok: true });
    });
});
