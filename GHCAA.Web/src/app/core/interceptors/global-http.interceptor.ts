import { HttpInterceptorFn, HttpErrorResponse, HttpRequest, HttpHandlerFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError, BehaviorSubject, switchMap, filter, take, Observable } from 'rxjs';
import { AuthService } from '../services/auth.service';
import { NotificationService } from '../services/notification.service';
import { StepUpService } from '../services/step-up.service';
import { environment } from '../../../environments/environment';

// 24.44: Shared refresh state — one in-flight refresh serves all concurrent 401s.
let isRefreshing = false;
const refreshDone$ = new BehaviorSubject<boolean>(false);

export const globalHttpInterceptor: HttpInterceptorFn = (req, next) => {
    const authService = inject(AuthService);
    const notify = inject(NotificationService);
    const stepUp = inject(StepUpService);

    let outReq = req;

    // 1. Rewrite relative /api/ URLs to absolute.
    if (req.url.startsWith('/api/')) {
        let baseUrl = environment.apiUrl;
        if (baseUrl.endsWith('/api')) baseUrl = baseUrl.substring(0, baseUrl.length - 4);
        outReq = outReq.clone({ url: `${baseUrl}${req.url}` });
    }

    // 2. 24.39: Add withCredentials so httpOnly cookies are sent with every API request.
    //    Bearer header injection is kept for in-memory token (mobile / API clients);
    //    browser sessions rely on the httpOnly cookie set by the backend.
    if (req.url.startsWith('/api/')) {
        const token = authService.getToken();
        outReq = outReq.clone({
            withCredentials: true,
            ...(token ? { setHeaders: { Authorization: `Bearer ${token}` } } : {})
        });
    }

    return next(outReq).pipe(
        catchError((error: HttpErrorResponse) => {
            // 24.44: On 401, attempt token refresh before giving up.
            // /api/auth/me is the session-restore probe every page load fires while possibly
            // still a guest — a 401 there is the expected "not logged in" answer, not a session
            // expiry, so it must never trigger refresh→logout. /api/auth/logout is excluded too:
            // logout() itself calls it, and if that call 401s (session already gone), routing it
            // back through handle401 → refresh fails → logout() again is an infinite retry loop.
            if (error.status === 401
                && !req.url.includes('/api/auth/login')
                && !req.url.includes('/api/auth/refresh')
                && !req.url.includes('/api/auth/me')
                && !req.url.includes('/api/auth/logout')) {
                return handle401(outReq, next, authService, notify, error);
            }

            // 7.13: The action is allowed for this role but needs a fresh OTP verification.
            // Raise the step-up dialog and retry once the admin verifies; the API re-issues the
            // access-token cookie with the step_up_verified_at claim, so the retry just works.
            if (error.status === 403
                && (error.error?.code ?? error.error?.Code) === 'STEP_UP_REQUIRED') {
                return stepUp.challenge().pipe(
                    switchMap(verified => verified ? next(outReq) : throwError(() => error))
                );
            }

            return handleError(error, req, authService, notify);
        })
    );
};

function handle401(
    req: HttpRequest<unknown>,
    next: HttpHandlerFn,
    authService: AuthService,
    notify: NotificationService,
    originalError: HttpErrorResponse
): Observable<any> {
    if (isRefreshing) {
        // Queue behind the ongoing refresh.
        return refreshDone$.pipe(
            filter(done => done),
            take(1),
            switchMap(success => success ? next(req) : throwError(() => originalError))
        );
    }

    isRefreshing = true;
    refreshDone$.next(false);

    return authService.refresh().pipe(
        switchMap(success => {
            isRefreshing = false;
            refreshDone$.next(success);
            if (success) return next(req);
            authService.logout();
            return throwError(() => originalError);
        }),
        catchError(err => {
            isRefreshing = false;
            refreshDone$.next(false);
            authService.logout();
            return throwError(() => err);
        })
    );
}

// 82.4: the API returns one error shape now — RFC 7807 ProblemDetails, camelCased by the same
// JSON options as everything else — so `detail` (falling back to `title`) is the one place a
// server-supplied message lives. Older cached responses or a non-API failure won't have either,
// which is what the per-status defaults below are still for.
function serverMessage(error: HttpErrorResponse): string | undefined {
    return error.error?.detail || error.error?.title;
}

function handleError(
    error: HttpErrorResponse,
    req: HttpRequest<unknown>,
    authService: AuthService,
    notify: NotificationService
): Observable<never> {
    // A 401 on the /auth/me session-restore probe means "not logged in" — the expected answer
    // for every guest on every page load, not an error. Skip both the toast (already covered by
    // X-Skip-Error-Notify) and the console.error that branch would otherwise still emit.
    if (error.status === 401 && req.url.includes('/api/auth/me')) {
        return throwError(() => error);
    }

    const skipNotify = req.headers.has('X-Skip-Error-Notify');
    let errorMessage = 'An unexpected error occurred';

    if (error.error instanceof ErrorEvent) {
        errorMessage = `Error: ${error.error.message}`;
    } else {
        switch (error.status) {
            case 401:
                errorMessage = serverMessage(error) || 'Invalid credentials.';
                break;
            case 403:
                errorMessage = serverMessage(error) || 'You do not have permission.';
                break;
            case 404:
                errorMessage = 'The requested resource was not found.';
                break;
            case 429:
                errorMessage = 'Too many requests. Please wait a moment.';
                break;
            case 500:
                errorMessage = 'Server error. Please try again later.';
                break;
            default:
                errorMessage = serverMessage(error) || errorMessage;
        }
    }

    if (!skipNotify) {
        notify.error(errorMessage);
    } else {
        console.error(`Status: ${error.status} | URL: ${req.url} | Message: ${errorMessage}`, error);
    }

    return throwError(() => error);
}
