import { describe, expect, it } from 'vitest';
import { HumanizePipe } from './humanize.pipe';

describe('HumanizePipe', () => {
    const pipe = new HumanizePipe();

    it('spaces out a PascalCase role name', () => {
        expect(pipe.transform('ElectionPersona')).toBe('Election Persona');
        expect(pipe.transform('SuperAdmin')).toBe('Super Admin');
    });

    it('capitalises a camelCase key', () => {
        expect(pipe.transform('showOnPublicBoard')).toBe('Show On Public Board');
    });

    it('keeps an acronym together', () => {
        expect(pipe.transform('ECMember')).toBe('EC Member');
    });

    it('leaves a single word or an already spaced name alone', () => {
        expect(pipe.transform('Admin')).toBe('Admin');
        expect(pipe.transform('Election Official')).toBe('Election Official');
    });

    it('returns an empty string for null, undefined or empty input', () => {
        expect(pipe.transform(null)).toBe('');
        expect(pipe.transform(undefined)).toBe('');
        expect(pipe.transform('')).toBe('');
    });
});
