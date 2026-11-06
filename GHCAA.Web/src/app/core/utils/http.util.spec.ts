import { describe, expect, it } from 'vitest';
import { buildHttpParams, getSilentHeaders, serverErrorMessage, silentHeaders, SKIP_ERROR_NOTIFY_HEADER } from './http.util';

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

    describe('serverErrorMessage (84.56)', () => {
        const reply = (error: unknown) => ({ error });

        it('gives the first errors entry of a validation reply, not the generic title', () => {
            expect(serverErrorMessage(reply({
                title: 'One or more validation errors occurred.',
                errors: { Title: ['The Title field is required.'], CompanyName: ['x'] }
            }), 'fb')).toBe('The Title field is required.');
        });

        it('puts detail before errors, and errors before title, message and error', () => {
            expect(serverErrorMessage(reply({ detail: 'd', errors: { X: ['x'] }, title: 't' }), 'fb')).toBe('d');
            expect(serverErrorMessage(reply({ title: 't', message: 'm' }), 'fb')).toBe('t');
            expect(serverErrorMessage(reply({ message: 'm', error: 'e' }), 'fb')).toBe('m');
            expect(serverErrorMessage(reply({ error: 'e' }), 'fb')).toBe('e');
        });

        it('skips empty errors entries and empty strings', () => {
            expect(serverErrorMessage(reply({ detail: '', errors: { X: [], Y: '' }, title: 't' }), 'fb')).toBe('t');
        });

        it('returns the fallback for a missing, plain-text or unknown body', () => {
            expect(serverErrorMessage(null, 'fb')).toBe('fb');
            expect(serverErrorMessage(reply(null), 'fb')).toBe('fb');
            expect(serverErrorMessage(reply('plain text'), 'fb')).toBe('fb');
            expect(serverErrorMessage(reply({ status: 400 }), 'fb')).toBe('fb');
        });
    });
});
