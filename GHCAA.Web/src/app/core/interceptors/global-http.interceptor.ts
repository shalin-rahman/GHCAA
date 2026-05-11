import { HttpInterceptorFn, HttpErrorResponse, HttpRequest, HttpHandlerFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError, BehaviorSubject, switchMap, filter, take, Observable } from 'rxjs';
import { AuthService } from '../services/auth.service';
import { NotificationService } from '../services/notification.service';
import { environment } from '../../../environments/environment';

// 24.44: Shared refresh state — one in-flight refresh serves all concurrent 401s.
let isRefreshing = false;
const refreshDone$ = new BehaviorSubject<boolean>(false);

export const globalHttpInterceptor: HttpInterceptorFn = (req, next) => {
    const authService = inject(AuthService);
    const notify = inject(NotificationService);

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
            if (error.status === 401
                && !req.url.includes('/api/auth/login')
                && !req.url.includes('/api/auth/refresh')) {
                return handle401(outReq, next, authService, notify, error);
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

function handleError(
    error: HttpErrorResponse,
    req: HttpRequest<unknown>,
    authService: AuthService,
    notify: NotificationService
): Observable<never> {
    const skipNotify = req.headers.has('X-Skip-Error-Notify');
    let errorMessage = 'An unexpected error occurred';

    if (error.error instanceof ErrorEvent) {
        errorMessage = `Error: ${error.error.message}`;
    } else {
        switch (error.status) {
            case 401:
                errorMessage = error.error?.message || 'Invalid credentials.';
                break;
            case 403:
                errorMessage = error.error?.message || 'You do not have permission.';
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
                errorMessage = error.error?.message || errorMessage;
        }
    }

    if (!skipNotify) {
        notify.error(errorMessage);
    } else {
        console.error(`Status: ${error.status} | URL: ${req.url} | Message: ${errorMessage}`, error);
    }

    return throwError(() => error);
}
