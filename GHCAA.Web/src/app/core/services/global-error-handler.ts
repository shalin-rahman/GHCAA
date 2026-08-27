import { ErrorHandler, Injectable, NgZone } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';

// Catches uncaught errors from component/template code and RxJS subscriptions
// that the global-http.interceptor never sees (e.g. thrown outside an HTTP call).
@Injectable()
export class GlobalErrorHandler implements ErrorHandler {
    handleError(error: unknown): void {
        // HTTP errors already have context/telemetry from global-http.interceptor.
        if (error instanceof HttpErrorResponse) {
            return;
        }

        const actual = (error as { rejection?: unknown })?.rejection ?? error;
        console.error('Unhandled application error:', actual);
    }
}
