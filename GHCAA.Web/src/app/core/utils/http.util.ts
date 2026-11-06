import { HttpHeaders, HttpParams } from '@angular/common/http';

export const SKIP_ERROR_NOTIFY_HEADER = 'X-Skip-Error-Notify';

/**
 * Returns HttpHeaders configured with X-Skip-Error-Notify if silent is true,
 * or undefined if silent is false/undefined.
 */
export function getSilentHeaders(silent?: boolean): HttpHeaders | undefined {
    return silent ? new HttpHeaders().set(SKIP_ERROR_NOTIFY_HEADER, 'true') : undefined;
}

/**
 * Returns HttpHeaders configured with X-Skip-Error-Notify header.
 */
export function silentHeaders(): HttpHeaders {
    return new HttpHeaders().set(SKIP_ERROR_NOTIFY_HEADER, 'true');
}

/**
 * The message an error reply carries, or the fallback. `detail` comes first. A model-validation
 * 400 has no detail and a generic title, so the first entry of its `errors` map comes before the
 * title (84.56). Mobile `ApiException.serverMessage` reads the same order.
 */
export function serverErrorMessage(err: unknown, fallback: string): string {
    const body = (err as { error?: unknown } | null)?.error;
    if (!body || typeof body !== 'object') return fallback;
    const b = body as Record<string, unknown>;
    const text = (v: unknown) => (typeof v === 'string' && v ? v : null);
    const detail = text(b['detail']);
    if (detail) return detail;
    const errors = b['errors'];
    if (errors && typeof errors === 'object') {
        for (const entry of Object.values(errors)) {
            const first = text(Array.isArray(entry) ? entry[0] : entry);
            if (first) return first;
        }
    }
    return text(b['title']) ?? text(b['message']) ?? text(b['error']) ?? fallback;
}

/**
 * Builds HttpParams from a key-value map, omitting undefined, null, or empty string values.
 */
export function buildHttpParams(params: Record<string, string | number | boolean | null | undefined>): HttpParams {
    let httpParams = new HttpParams();
    for (const [key, value] of Object.entries(params)) {
        if (value !== undefined && value !== null && value !== '') {
            httpParams = httpParams.set(key, String(value));
        }
    }
    return httpParams;
}
