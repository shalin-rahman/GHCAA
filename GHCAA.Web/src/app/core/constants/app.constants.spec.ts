import { computed } from '@angular/core';
import { ORG_CONFIG_FALLBACK } from '../config/org-config-fallback.generated';
import { getMembershipTypeOptions, getMemberCategoryOptions, getMembershipTypeLabel, getPositionLabel, getCurrentECLabel, getCategoryLabel, setOrgLabelMaps } from './app.constants';

const PACK = ORG_CONFIG_FALLBACK.localization!.locales!['en'];

// The value lists must match the C# enums in GHCAA.Domain/Enums.cs, in order, because the API
// sends ordinals. Labels come from the profile pack, never from a list kept here.
describe('MembershipType constants (Work Package 35)', () => {
    const DOMAIN_ENUM = ['Founding', 'Executive', 'General', 'Associate', 'Honorary', 'Advisory', 'Guest'];
    const CATEGORY_ENUM = ['None', 'LifelongPatron', 'Sponsor', 'Advisor', 'Mentor', 'Recruiter',
        'Active', 'Volunteer', 'Contributor', 'Guest', 'Student'];

    it('getMembershipTypeOptions() follows the enum and takes labels from the pack', () => {
        expect(getMembershipTypeOptions().map(o => o.value)).toEqual(DOMAIN_ENUM);
        expect(getMembershipTypeOptions().map(o => o.label)).toEqual(DOMAIN_ENUM.map(k => PACK.membershipTypeLabels![k]));
    });

    it('getMemberCategoryOptions() follows the enum and takes labels from the pack', () => {
        expect(getMemberCategoryOptions().map(o => o.value)).toEqual(CATEGORY_ENUM);
        expect(getMemberCategoryOptions().find(o => o.value === 'Active')?.label).toBe(PACK.memberCategoryLabels!['Active']);
    });

    it('never labels any ordinal "Life"', () => {
        expect(getMembershipTypeOptions().map(o => o.label).join('|')).not.toContain('Life');
    });

    describe('getMembershipTypeLabel', () => {
        it('resolves every numeric ordinal', () => {
            DOMAIN_ENUM.forEach((k, i) => expect(getMembershipTypeLabel(i)).toBe(PACK.membershipTypeLabels![k]));
        });

        it('resolves the enum name to the full display label', () => {
            expect(getMembershipTypeLabel('Guest')).toBe('Guest Member');
            expect(getMembershipTypeLabel('Founding')).toBe('Founding Member');
        });

        it('resolves a numeric ordinal sent as a string', () => {
            expect(getMembershipTypeLabel('6')).toBe('Guest Member');
        });

        it('falls back to the General label for missing values', () => {
            const general = PACK.membershipTypeLabels!['General'];
            expect(getMembershipTypeLabel(null as any)).toBe(general);
            expect(getMembershipTypeLabel(undefined as any)).toBe(general);
            expect(getMembershipTypeLabel('')).toBe(general);
            expect(getMembershipTypeLabel(99)).toBe(general);
        });

        it('passes an unrecognised name through rather than mislabelling it', () => {
            // Better a visibly odd string than a confidently wrong tier.
            expect(getMembershipTypeLabel('SomethingNew')).toBe('SomethingNew');
        });
    });
});

describe('84.77: type and category labels read the loaded OrgConfig', () => {
    afterEach(() => setOrgLabelMaps(null));

    it('uses the OrgConfig label for a key the pack has', () => {
        setOrgLabelMaps({ membershipTypeLabels: { Guest: 'Visitor' }, memberCategoryLabels: { Active: 'Core Member' } });
        expect(getMembershipTypeLabel('Guest')).toBe('Visitor');
        expect(getCategoryLabel('Active')).toBe('Core Member');
        expect(getCategoryLabel(6)).toBe('Core Member');
    });

    it('uses the build-time pack before the config loads and shows the key when a label is missing', () => {
        setOrgLabelMaps(undefined);
        expect(getCategoryLabel('Active')).toBe('Active');
        expect(getCategoryLabel('LifelongPatron')).toBe('Lifelong Patron');
        expect(getCategoryLabel(null)).toBe('None');
        expect(getCategoryLabel('SomethingNew')).toBe('SomethingNew');
    });

    it('keeps the build-time label for a key the server pack leaves out', () => {
        setOrgLabelMaps({ membershipTypeLabels: { Founding: 'Founder' }, memberCategoryLabels: {} });
        expect(getMembershipTypeLabel('Founding')).toBe('Founder');
        expect(getMembershipTypeLabel('Guest')).toBe(PACK.membershipTypeLabels!['Guest']);
        expect(getCategoryLabel('LifelongPatron')).toBe('Lifelong Patron');
    });

    it('treats a negative ordinal as out of range', () => {
        setOrgLabelMaps({ membershipTypeLabels: { General: 'Ordinary' }, memberCategoryLabels: { None: 'Nil' } });
        expect(getCategoryLabel(-1)).toBe('Nil');
        expect(getCategoryLabel('-1')).toBe('Nil');
        expect(getMembershipTypeLabel(-1)).toBe('Ordinary');
        expect(getMembershipTypeLabel('-3')).toBe('Ordinary');
        expect(getMembershipTypeLabel(null)).toBe('Ordinary');
    });

    it('the option lists carry the loaded labels', () => {
        setOrgLabelMaps({ membershipTypeLabels: { Guest: 'Visitor' }, memberCategoryLabels: { Active: 'Core Member' } });
        expect(getMembershipTypeOptions().find(o => o.value === 'Guest')?.label).toBe('Visitor');
        expect(getMemberCategoryOptions().find(o => o.value === 'Active')?.label).toBe('Core Member');
    });

    it('uses the OrgConfig None label for an ordinal past the enum', () => {
        setOrgLabelMaps({ memberCategoryLabels: { None: 'কোনোটি নয়' } });
        expect(getCategoryLabel(99)).toBe('কোনোটি নয়');
        setOrgLabelMaps(null);
        expect(getCategoryLabel('99')).toBe('None');
    });

    it('a computed that read a label recomputes when the config arrives', () => {
        const label = computed(() => getCategoryLabel('Active') + ' / ' + getMembershipTypeLabel('Guest'));
        expect(label()).toBe('Active / Guest Member');
        setOrgLabelMaps({ membershipTypeLabels: { Guest: 'Visitor' }, memberCategoryLabels: { Active: 'Core Member' } });
        expect(label()).toBe('Core Member / Visitor');
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
