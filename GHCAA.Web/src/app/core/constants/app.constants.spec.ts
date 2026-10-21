import { MEMBERSHIP_TYPES, MEMBERSHIP_TYPE_OPTIONS, getMembershipTypeLabel, getPositionLabel, getCurrentECLabel } from './app.constants';

/**
 * Work Package 35. These two lists and the helper below are the single source of truth for rendering a
 * MembershipType on the web, and they must stay in lockstep with the enum in
 * GHCAA.Domain/Enums.cs:
 *
 *   public enum MembershipType { Founding, Executive, General, Associate, Honorary, Advisory, Guest }
 *
 * Three components previously kept their own copies of this list; two of them labelled index 6
 * 'Life' (a value the enum has never had) and one omitted Guest entirely, hiding real members.
 * These tests fail the suite if the lists drift again.
 */
describe('MembershipType constants (Work Package 35)', () => {
    // Mirrors GHCAA.Domain/Enums.cs. Update this only when the C# enum itself changes.
    const DOMAIN_ENUM = ['Founding', 'Executive', 'General', 'Associate', 'Honorary', 'Advisory', 'Guest'];

    it('MEMBERSHIP_TYPES covers the domain enum, in order', () => {
        expect(MEMBERSHIP_TYPES).toHaveLength(DOMAIN_ENUM.length);
        expect(MEMBERSHIP_TYPES).toEqual(DOMAIN_ENUM.map(n => `${n} Member`));
    });

    it('MEMBERSHIP_TYPE_OPTIONS carries the same values and labels', () => {
        expect(MEMBERSHIP_TYPE_OPTIONS.map(o => o.value)).toEqual(DOMAIN_ENUM);
        expect(MEMBERSHIP_TYPE_OPTIONS.map(o => o.label)).toEqual(MEMBERSHIP_TYPES);
    });

    it('never labels any ordinal "Life"', () => {
        expect(MEMBERSHIP_TYPES.join('|')).not.toContain('Life');
        expect(MEMBERSHIP_TYPE_OPTIONS.map(o => o.label).join('|')).not.toContain('Life');
    });

    describe('getMembershipTypeLabel', () => {
        it('resolves every numeric ordinal', () => {
            DOMAIN_ENUM.forEach((_, i) => expect(getMembershipTypeLabel(i)).toBe(MEMBERSHIP_TYPES[i]));
        });

        it('resolves the enum name to the full display label', () => {
            expect(getMembershipTypeLabel('Guest')).toBe('Guest Member');
            expect(getMembershipTypeLabel('Founding')).toBe('Founding Member');
        });

        it('resolves a numeric ordinal sent as a string', () => {
            expect(getMembershipTypeLabel('6')).toBe('Guest Member');
        });

        it('falls back to General for missing values', () => {
            expect(getMembershipTypeLabel(null as any)).toBe('General');
            expect(getMembershipTypeLabel(undefined as any)).toBe('General');
            expect(getMembershipTypeLabel('')).toBe('General');
            expect(getMembershipTypeLabel(99)).toBe('General');
        });

        it('passes an unrecognised name through rather than mislabelling it', () => {
            // Better a visibly odd string than a confidently wrong tier.
            expect(getMembershipTypeLabel('SomethingNew')).toBe('SomethingNew');
        });
    });
});

describe('FR-34: position labels come from the EcPositions table', () => {
    it('FR-34: shows the name the API sends, including one an admin renamed', () => {
        expect(getPositionLabel({ positionName: 'Senior Vice President' })).toBe('Senior Vice President');
    });

    it('FR-34: shows nothing when the record has no name instead of guessing from the key', () => {
        expect(getPositionLabel({ positionName: null })).toBe('');
        expect(getPositionLabel({ position: 'GeneralSecretary' } as any)).toBe('');
        expect(getPositionLabel(null)).toBe('');
    });

    it('FR-34: the current label reads the open record', () => {
        const history = [
            { isCurrent: false, endDate: '2024-01-01', positionName: 'Treasurer' },
            { isCurrent: true, endDate: null, positionName: 'Patron' },
        ];
        expect(getCurrentECLabel(history)).toBe('Patron');
        expect(getCurrentECLabel([])).toBe('');
    });
});
