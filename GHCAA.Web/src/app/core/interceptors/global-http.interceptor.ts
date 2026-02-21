import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';
import { AuthService } from '../services/auth.service';
import { NotificationService } from '../services/notification.service';

export const globalHttpInterceptor: HttpInterceptorFn = (req, next) => {
    const authService = inject(AuthService);
    const notify = inject(NotificationService);
    const token = authService.getToken();

    // 1. Inject Bearer Token
    let authReq = req;
    if (token) {
        authReq = req.clone({
            setHeaders: { Authorization: `Bearer ${token}` }
        });
    }

    // 2. Handle Responses and Errors
    return next(authReq).pipe(
        catchError((error: HttpErrorResponse) => {
            let errorMessage = 'An unexpected error occurred';

            if (error.error instanceof ErrorEvent) {
                // Client-side error
                errorMessage = `Error: ${error.error.message}`;
            } else {
                // Server-side error
                switch (error.status) {
                    case 401:
                    case 403:
                        errorMessage = 'Session expired. Please login again.';
                        authService.logout();
                        break;
                    case 404:
                        errorMessage = 'The requested resource was not found.';
                        break;
                    case 500:
                        errorMessage = 'Server error. Please try again later.';
                        break;
                    default:
                        errorMessage = error.error?.message || errorMessage;
                }
            }

            notify.error(errorMessage);
            return throwError(() => error);
        })
    );
};
