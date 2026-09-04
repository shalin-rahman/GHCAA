import { describe, it, expect } from 'vitest';
import { formatPeriodRange, getEventStatus, getEventStatusMeta } from './date.util';

describe('formatPeriodRange', () => {
    it.each([
        ['shows the real stored year range even while active', { startDate: '2024-12-01', endDate: '2026-06-30', isActive: true }, '2024 - 2026'],
        ['shows just the start year when endDate is missing, active or not', { startDate: '2025-01-01', isActive: true }, '2025'],
        ['shows a real range for a concluded period spanning multiple years', { startDate: '2015-01-01', endDate: '2017-12-31', isActive: false }, '2015 - 2017'],
        ['collapses to a single year when start and end fall in the same year', { startDate: '2020-01-01', endDate: '2020-11-30', isActive: false }, '2020']
    ] as const)('%s', (_label, period, expected) => {
        expect(formatPeriodRange(period)).toBe(expected);
    });
});

// 54.6/58.6: event lifecycle status is computed from dates, not just the admin's IsActive flag —
// added after that flag alone was found to stay "Active" forever, even long past an event's end.
describe('getEventStatus', () => {
    const YEAR_3000 = '3000-01-01';
    const YEAR_2000 = '2000-01-01';

    it.each([
        ['is Unpublished when isActive is false, regardless of dates', false, YEAR_2000, YEAR_3000, 'Unpublished'],
        ['is Upcoming when now is before startDate', true, YEAR_3000, YEAR_3000, 'Upcoming'],
        ['is Ended when now is after endDate', true, YEAR_2000, YEAR_2000, 'Ended'],
        ['is Ongoing when now falls between startDate and endDate', true, YEAR_2000, YEAR_3000, 'Ongoing']
    ] as const)('%s', (_label, isActive, startDate, endDate, expected) => {
        expect(getEventStatus({ isActive, startDate, endDate })).toBe(expected);
    });

    // Same inputs and outcome as the "is Unpublished when isActive is false" case above —
    // flagged as a genuine duplicate, kept as-is pending a call on whether to drop it.
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
