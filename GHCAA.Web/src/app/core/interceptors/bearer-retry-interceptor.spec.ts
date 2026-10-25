import { TestBed } from '@angular/core/testing';
import { HttpClient, HttpXsrfTokenExtractor, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { describe, it, expect, beforeEach, afterEach, vi } from 'vitest';
import { of, Subject } from 'rxjs';
import { globalHttpInterceptor } from './global-http.interceptor';
import { StepUpService } from '../services/step-up.service';
import { AuthService } from '../services/auth.service';
import { NotificationService } from '../services/notification.service';

// FR-37.12d: accepting an appointment ends the old access token. The API reads the Bearer header
// before the cookie, so a retry after the refresh must carry the new token or it fails again.
describe('globalHttpInterceptor: Bearer header on retry', () => {
    let httpMock: HttpTestingController;
    let http: HttpClient;
    let token: string | null;

    beforeEach(() => {
        token = 'old';

        TestBed.configureTestingModule({
            providers: [
                provideHttpClient(withInterceptors([globalHttpInterceptor])),
                provideHttpClientTesting(),
                { provide: HttpXsrfTokenExtractor, useValue: { getToken: () => null } },
                { provide: StepUpService, useValue: { challenge: vi.fn() } },
                {
                    provide: AuthService,
                    useValue: {
                        getToken: () => token,
                        refresh: vi.fn(),
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

    function refreshTo(next: string | null): void {
        const auth = TestBed.inject(AuthService) as unknown as { refresh: ReturnType<typeof vi.fn> };
        auth.refresh.mockImplementation(() => { token = next; return of(true); });
    }

    it('sends the new token when a request is retried after a 401', () => {
        refreshTo('new');
        http.get('/api/officials/elections').subscribe();

        const first = httpMock.expectOne(r => r.url.endsWith('/api/officials/elections'));
        expect(first.request.headers.get('Authorization')).toBe('Bearer old');
        first.flush(null, { status: 401, statusText: 'Unauthorized' });

        const retry = httpMock.expectOne(r => r.url.endsWith('/api/officials/elections'));
        expect(retry.request.headers.get('Authorization')).toBe('Bearer new');
        retry.flush([]);
    });

    it('drops the old Bearer header when the refresh left no token in memory', () => {
        refreshTo(null);
        http.get('/api/officials/elections').subscribe();

        httpMock.expectOne(r => r.url.endsWith('/api/officials/elections'))
            .flush(null, { status: 401, statusText: 'Unauthorized' });

        const retry = httpMock.expectOne(r => r.url.endsWith('/api/officials/elections'));
        expect(retry.request.headers.has('Authorization')).toBe(false);
        retry.flush([]);
    });
    function auth(): { refresh: ReturnType<typeof vi.fn>; logout: ReturnType<typeof vi.fn> } {
        return TestBed.inject(AuthService) as unknown as { refresh: ReturnType<typeof vi.fn>; logout: ReturnType<typeof vi.fn> };
    }

    it('sends the new token on a request queued behind a refresh already running', () => {
        const done = new Subject<boolean>();
        auth().refresh.mockImplementation(() => done);
        http.get('/api/officials/elections').subscribe();
        http.get('/api/elections/7/seats').subscribe();

        httpMock.expectOne(r => r.url.endsWith('/api/officials/elections'))
            .flush(null, { status: 401, statusText: 'Unauthorized' });
        httpMock.expectOne(r => r.url.endsWith('/api/elections/7/seats'))
            .flush(null, { status: 401, statusText: 'Unauthorized' });
        expect(auth().refresh).toHaveBeenCalledTimes(1);

        token = 'new';
        done.next(true);
        done.complete();

        const first = httpMock.expectOne(r => r.url.endsWith('/api/officials/elections'));
        const queued = httpMock.expectOne(r => r.url.endsWith('/api/elections/7/seats'));
        expect(first.request.headers.get('Authorization')).toBe('Bearer new');
        expect(queued.request.headers.get('Authorization')).toBe('Bearer new');
        first.flush([]);
        queued.flush([]);
    });

    it('logs out and fails both the request and the one queued behind it when the refresh fails', () => {
        const done = new Subject<boolean>();
        auth().refresh.mockImplementation(() => done);
        const errors: number[] = [];
        http.get('/api/officials/elections').subscribe({ error: e => errors.push(e.status) });
        http.get('/api/elections/7/seats').subscribe({ error: e => errors.push(e.status) });

        httpMock.expectOne(r => r.url.endsWith('/api/officials/elections'))
            .flush(null, { status: 401, statusText: 'Unauthorized' });
        httpMock.expectOne(r => r.url.endsWith('/api/elections/7/seats'))
            .flush(null, { status: 401, statusText: 'Unauthorized' });

        done.next(false);
        done.complete();

        expect(auth().logout).toHaveBeenCalledTimes(1);
        expect(errors).toEqual([401, 401]);
    });
    it('keeps the session when the retry itself fails after a good refresh', () => {
        refreshTo('new');
        let status = 0;
        http.get('/api/officials/elections').subscribe({ error: e => status = e.status });

        httpMock.expectOne(r => r.url.endsWith('/api/officials/elections'))
            .flush(null, { status: 401, statusText: 'Unauthorized' });
        httpMock.expectOne(r => r.url.endsWith('/api/officials/elections'))
            .flush(null, { status: 403, statusText: 'Forbidden' });

        expect(status).toBe(403);
        expect(auth().logout).not.toHaveBeenCalled();
    });
});
