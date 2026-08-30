import { describe, it, expect } from 'vitest';
import { formatPeriodRange, getEventStatus, getEventStatusMeta } from './date.util';

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

// 54.6/58.6: event lifecycle status is computed from dates, not just the admin's IsActive flag —
// added after that flag alone was found to stay "Active" forever, even long past an event's end.
describe('getEventStatus', () => {
    const YEAR_3000 = '3000-01-01';
    const YEAR_2000 = '2000-01-01';

    it('is Unpublished when isActive is false, regardless of dates', () => {
        expect(getEventStatus({ isActive: false, startDate: YEAR_2000, endDate: YEAR_3000 })).toBe('Unpublished');
    });

    it('is Upcoming when now is before startDate', () => {
        expect(getEventStatus({ isActive: true, startDate: YEAR_3000, endDate: YEAR_3000 })).toBe('Upcoming');
    });

    it('is Ended when now is after endDate', () => {
        expect(getEventStatus({ isActive: true, startDate: YEAR_2000, endDate: YEAR_2000 })).toBe('Ended');
    });

    it('is Ongoing when now falls between startDate and endDate', () => {
        expect(getEventStatus({ isActive: true, startDate: YEAR_2000, endDate: YEAR_3000 })).toBe('Ongoing');
    });

    it('Unpublished takes priority over date-computed status even mid-event', () => {
        expect(getEventStatus({ isActive: false, startDate: YEAR_2000, endDate: YEAR_3000 })).toBe('Unpublished');
    });
});

describe('getEventStatusMeta', () => {
    it('maps each status to the expected label and central .status-badge state class', () => {
        expect(getEventStatusMeta({ isActive: false, startDate: '2000-01-01', endDate: '2000-01-01' }))
            .toEqual({ label: 'Unpublished', class: 'inactive' });
        expect(getEventStatusMeta({ isActive: true, startDate: '3000-01-01', endDate: '3000-01-01' }))
            .toEqual({ label: 'Upcoming', class: 'pending' });
        expect(getEventStatusMeta({ isActive: true, startDate: '2000-01-01', endDate: '3000-01-01' }))
            .toEqual({ label: 'Ongoing', class: 'active' });
        expect(getEventStatusMeta({ isActive: true, startDate: '2000-01-01', endDate: '2000-01-01' }))
            .toEqual({ label: 'Ended', class: 'terminated' });
    });
});
