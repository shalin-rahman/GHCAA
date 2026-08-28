import { ErrorHandler, Injectable } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';

// Catches uncaught errors from component/template code and RxJS subscriptions
// that the global-http.interceptor never sees (e.g. thrown outside an HTTP call).
@Injectable()
export class GlobalErrorHandler implements ErrorHandler {
    handleError(error: unknown): void {
        // Unwrap an unhandled-promise-rejection wrapper FIRST — an HttpErrorResponse thrown
        // inside a promise (e.g. a firstValueFrom()/toPromise() call site) arrives here as
        // `{ rejection: HttpErrorResponse }`, not as the HttpErrorResponse itself, so checking
        // `instanceof HttpErrorResponse` before unwrapping missed it and double-reported on top
        // of the interceptor's own toast.
        const actual = (error as { rejection?: unknown })?.rejection ?? error;

        // HTTP errors already have context/telemetry from global-http.interceptor.
        if (actual instanceof HttpErrorResponse) {
            return;
        }

        console.error('Unhandled application error:', actual);
    }
}
