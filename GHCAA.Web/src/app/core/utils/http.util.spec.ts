import { describe, expect, it } from 'vitest';
import { buildHttpParams, getSilentHeaders, silentHeaders, SKIP_ERROR_NOTIFY_HEADER } from './http.util';

describe('http.util', () => {
    describe('getSilentHeaders', () => {
        it('should return HttpHeaders with X-Skip-Error-Notify header when silent is true', () => {
            const headers = getSilentHeaders(true);
            expect(headers).toBeDefined();
            expect(headers?.get(SKIP_ERROR_NOTIFY_HEADER)).toBe('true');
        });

        it('should return undefined when silent is false or undefined', () => {
            expect(getSilentHeaders(false)).toBeUndefined();
            expect(getSilentHeaders()).toBeUndefined();
        });
    });

    describe('silentHeaders', () => {
        it('should return HttpHeaders with X-Skip-Error-Notify set to true', () => {
            const headers = silentHeaders();
            expect(headers.get(SKIP_ERROR_NOTIFY_HEADER)).toBe('true');
        });
    });

    describe('buildHttpParams', () => {
        it('should construct HttpParams omitting undefined, null, and empty string values', () => {
            const params = buildHttpParams({
                page: 1,
                pageSize: 10,
                eventId: 'evt-123',
                category: undefined,
                filter: null,
                empty: '',
                active: true
            });

            expect(params.get('page')).toBe('1');
            expect(params.get('pageSize')).toBe('10');
            expect(params.get('eventId')).toBe('evt-123');
            expect(params.get('active')).toBe('true');
            expect(params.has('category')).toBe(false);
            expect(params.has('filter')).toBe(false);
            expect(params.has('empty')).toBe(false);
        });
    });
});
