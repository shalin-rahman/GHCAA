import { describe, expect, it } from 'vitest';
import { BatchPipe, earliestOrgRecord, hasBatchLabel } from './batch.pipe';

describe('BatchPipe', () => {
    const pipe = new BatchPipe();

    it('shows the server label when there is one', () => {
        expect(pipe.transform({ batchLabel: 'HSC (Science) 2001 / BSc (Physics) 2004', passingYear: 2001 }))
            .toBe('HSC (Science) 2001 / BSc (Physics) 2004');
    });

    it('trims the label', () => {
        expect(pipe.transform({ batchLabel: '  HSC 2001 ' })).toBe('HSC 2001');
    });

    it('falls back to the passing year when the label is null, empty or blank', () => {
        expect(pipe.transform({ batchLabel: null, passingYear: 1999 })).toBe('1999');
        expect(pipe.transform({ batchLabel: '', passingYear: 1999 })).toBe('1999');
        expect(pipe.transform({ batchLabel: '   ', passingYear: 1999 })).toBe('1999');
    });

    it('uses the certificate year when there is no passing year', () => {
        expect(pipe.transform({ ghcLastCertificatePassingYear: 1985 })).toBe('1985');
    });

    it('treats year 0 and blank years as missing', () => {
        expect(pipe.transform({ passingYear: 0, ghcLastCertificatePassingYear: '' })).toBe('N/A');
    });

    it('returns the fallback for null, undefined or empty input', () => {
        expect(pipe.transform(null)).toBe('N/A');
        expect(pipe.transform(undefined, '—')).toBe('—');
        expect(pipe.transform({})).toBe('N/A');
    });

    it('earliestOrgRecord picks the lowest college year, skips nulls and puts a missing year last', () => {
        const history = [
            null,
            { id: 1, isOrgProfile: true, passingYear: 0 },
            { id: 2, isOrgProfile: false, passingYear: 1970 },
            { id: 3, isOrgProfile: true, passingYear: 1996 },
            { id: 4, isOrgProfile: true, passingYear: 1982 },
        ];
        expect(earliestOrgRecord(history)?.id).toBe(4);
        expect(earliestOrgRecord([{ id: 1, isOrgProfile: true, passingYear: 0 }])?.id).toBe(1);
        expect(earliestOrgRecord(null)).toBeNull();
        expect(earliestOrgRecord([{ id: 2, isOrgProfile: false, passingYear: 1970 }])).toBeNull();
    });

    it('earliestOrgRecord does not reorder the input', () => {
        const history = [{ isOrgProfile: true, passingYear: 2000 }, { isOrgProfile: true, passingYear: 1990 }];
        earliestOrgRecord(history);
        expect(history[0].passingYear).toBe(2000);
    });

    it('hasBatchLabel ignores blank labels', () => {
        expect(hasBatchLabel({ batchLabel: 'HSC 2001' })).toBe(true);
        expect(hasBatchLabel({ batchLabel: '  ' })).toBe(false);
        expect(hasBatchLabel(null)).toBe(false);
    });
});
