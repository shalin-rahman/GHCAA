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
