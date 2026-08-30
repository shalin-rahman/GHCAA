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

        // 54.2: a tab left open across a deploy still holds the old build's lazy-chunk hash
        // references; navigating to a lazy route in that stale tab requests a chunk filename
        // that no longer exists post-deploy. index.html itself is already served no-cache
        // (see GHCAA.API/Program.cs), which fixes every *new* page load, but can't help a tab
        // that's already running — the only fix there is a reload to pick up the new build.
        const message = (actual as { message?: unknown })?.message ?? actual;
        const text = typeof message === 'string' ? message : String(message);
        if (/Failed to fetch dynamically imported module|ChunkLoadError|Loading chunk .* failed/i.test(text)) {
            this.reloadOnceForStaleChunk();
            return;
        }

        console.error('Unhandled application error:', actual);
    }

    private reloadOnceForStaleChunk(): void {
        // Guarded by sessionStorage so a genuinely broken chunk (not just a stale deploy)
        // doesn't reload-loop forever — one retry, then give up and surface it normally.
        const key = 'ghcaa-stale-chunk-reload';
        try {
            if (sessionStorage.getItem(key)) {
                console.error('Chunk load failed again after a reload — not retrying further.');
                return;
            }
            sessionStorage.setItem(key, '1');
        } catch {
            // sessionStorage unavailable (private browsing, etc.) — fall through to reload anyway.
        }
        window.location.reload();
    }
}
