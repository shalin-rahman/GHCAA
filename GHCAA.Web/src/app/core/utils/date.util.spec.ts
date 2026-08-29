import { describe, it, expect } from 'vitest';
import { formatPeriodRange } from './date.util';

describe('formatPeriodRange', () => {
    it('shows the real stored year range even while active', () => {
        expect(formatPeriodRange({ startDate: '2024-12-01', endDate: '2026-06-30', isActive: true }))
            .toBe('2024 - 2026');
    });

    it('shows just the start year when endDate is missing, active or not', () => {
        expect(formatPeriodRange({ startDate: '2025-01-01', isActive: true })).toBe('2025');
    });

    it('shows a real range for a concluded period spanning multiple years', () => {
        expect(formatPeriodRange({ startDate: '2015-01-01', endDate: '2017-12-31', isActive: false }))
            .toBe('2015 - 2017');
    });

    it('collapses to a single year when start and end fall in the same year', () => {
        expect(formatPeriodRange({ startDate: '2020-01-01', endDate: '2020-11-30', isActive: false }))
            .toBe('2020');
    });
});
