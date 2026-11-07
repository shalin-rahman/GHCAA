import { describe, expect, it } from 'vitest';
import { getNextTheme, isTheme } from './theme.config';

describe('theme.config', () => {
    describe('isTheme', () => {
        it.each([
            ['light', true],
            ['dark', true],
            ['', false],
            ['toString', false],
            [null, false]
        ])('accepts %s: %s', (value, expected) => {
            expect(isTheme(value)).toBe(expected);
        });
    });

    describe('getNextTheme', () => {
        it.each([
            ['light', 'dark'],
            ['dark', 'light']
        ] as const)('switches %s to %s', (current, expected) => {
            expect(getNextTheme(current)).toBe(expected);
        });
    });
});
