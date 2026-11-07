import { describe, it, expect } from 'vitest';
import {
    hexToRgb, rgbToHex, rgbString, rgbToHsl, hslToRgb,
    adjustLightness, contrastRatio, darkenForContrast, parseHexColor, safeContrastRatio,
    lightenForContrast, readableTextOn, accentGradient, DARK_LABEL, LIGHT_LABEL, DARK_TEXT_SURFACE
} from './color-math';

describe('color-math', () => {
    describe('hexToRgb', () => {
        it('parses a 6-digit hex', () => {
            expect(hexToRgb('#c5a059')).toEqual({ r: 197, g: 160, b: 89 });
        });

        it('expands a 3-digit hex', () => {
            expect(hexToRgb('#fff')).toEqual({ r: 255, g: 255, b: 255 });
        });

        it('works without a leading #', () => {
            expect(hexToRgb('000000')).toEqual({ r: 0, g: 0, b: 0 });
        });

        it('ignores spaces around a pasted value', () => {
            expect(hexToRgb('  #c5a059 ')).toEqual({ r: 197, g: 160, b: 89 });
        });
    });

    describe('parseHexColor', () => {
        it.each([
            ['#abc', { r: 170, g: 187, b: 204 }],
            ['#a1B2c3', { r: 161, g: 178, b: 195 }],
            ['ABC', { r: 170, g: 187, b: 204 }],
            ['  #c5a059  ', { r: 197, g: 160, b: 89 }]
        ])('parses %s', (value, expected) => {
            expect(parseHexColor(value)).toEqual(expected);
        });

        it.each(['', '#', '#12', '#12345', '#1234567', '#gggggg', 'red', 'rgb(0,0,0)'])(
            'rejects %s',
            value => expect(parseHexColor(value)).toBeNull()
        );
    });

    describe('rgbToHex / rgbString', () => {
        it('round-trips through hexToRgb', () => {
            expect(rgbToHex(hexToRgb('#c5a059'))).toBe('#c5a059');
        });

        it('formats an rgb tuple as a comma string', () => {
            expect(rgbString({ r: 197, g: 160, b: 89 })).toBe('197, 160, 89');
        });
    });

    describe('rgbToHsl / hslToRgb', () => {
        it('round-trips a chromatic color', () => {
            const rgb = hexToRgb('#c5a059');
            const hsl = rgbToHsl(rgb);
            const back = hslToRgb(hsl);
            expect(rgbToHex(back)).toBe('#c5a059');
        });

        it('gives hue 0 and saturation 0 for a grayscale color', () => {
            const hsl = rgbToHsl(hexToRgb('#808080'));
            expect(hsl.h).toBe(0);
            expect(hsl.s).toBe(0);
        });
    });

    describe('adjustLightness', () => {
        it('lightens when deltaL is positive', () => {
            const lighter = adjustLightness('#c5a059', 10);
            expect(rgbToHsl(hexToRgb(lighter)).l).toBeGreaterThan(rgbToHsl(hexToRgb('#c5a059')).l);
        });

        it('darkens when deltaL is negative', () => {
            const darker = adjustLightness('#c5a059', -10);
            expect(rgbToHsl(hexToRgb(darker)).l).toBeLessThan(rgbToHsl(hexToRgb('#c5a059')).l);
        });

        it('clamps at full white rather than overshooting', () => {
            expect(adjustLightness('#ffffff', 50)).toBe('#ffffff');
        });

        it('clamps at full black rather than undershooting', () => {
            expect(adjustLightness('#000000', -50)).toBe('#000000');
        });
    });

    describe('contrastRatio', () => {
        it('is 21:1 for pure black on white', () => {
            expect(contrastRatio('#000000', '#ffffff')).toBeCloseTo(21, 0);
        });

        it('is 1:1 for identical colors', () => {
            expect(contrastRatio('#c5a059', '#c5a059')).toBeCloseTo(1, 5);
        });

        it('does not depend on argument order', () => {
            expect(contrastRatio('#c5a059', '#ffffff')).toBeCloseTo(contrastRatio('#ffffff', '#c5a059'), 10);
        });
    });

    describe('safeContrastRatio', () => {
        it('calculates the same ratio as contrastRatio for valid hex colors', () => {
            expect(safeContrastRatio('#c5a059', '#ffffff')).toBeCloseTo(
                contrastRatio('#c5a059', '#ffffff'),
                10
            );
        });

        it.each(['', '#', '#12', '#12345', '#1234567', '#gggggg', 'red', 'rgb(0,0,0)'])(
            'returns null for invalid first or second color %s',
            value => {
                expect(safeContrastRatio(value, '#ffffff')).toBeNull();
                expect(safeContrastRatio('#000000', value)).toBeNull();
            }
        );
    });

    describe('darkenForContrast', () => {
        it('darkens a color that starts below the target ratio', () => {
            const result = darkenForContrast('#c5a059', '#ffffff', 4.5);
            expect(result).not.toBe('#c5a059');
            expect(contrastRatio(result, '#ffffff')).toBeGreaterThanOrEqual(4.5);
        });

        it('leaves a color unchanged if it already clears the target ratio', () => {
            expect(darkenForContrast('#000000', '#ffffff', 4.5)).toBe('#000000');
        });

        it('bottoms out at black rather than looping forever on an unreachable ratio', () => {
            const result = darkenForContrast('#ffffff', '#ffffff', 21);
            expect(result).toBe('#000000');
        });
    });

    describe('lightenForContrast', () => {
        it('lightens a dark accent until it reads on the dark card', () => {
            const result = lightenForContrast('#2f6f4f', DARK_TEXT_SURFACE, 4.5);
            expect(result).not.toBe('#2f6f4f');
            expect(contrastRatio(result, DARK_TEXT_SURFACE)).toBeGreaterThanOrEqual(4.5);
        });

        it('leaves the default gold alone, since it already passes on the dark card', () => {
            expect(lightenForContrast('#c5a059', DARK_TEXT_SURFACE, 4.5)).toBe('#c5a059');
        });

        it('tops out at white rather than looping forever on an unreachable ratio', () => {
            expect(lightenForContrast('#000000', '#000000', 21)).toBe('#ffffff');
        });
    });

    describe('readableTextOn', () => {
        it('puts the dark label on the default gold', () => {
            expect(readableTextOn('#c5a059')).toBe(DARK_LABEL);
        });

        it('puts the white label on a dark green accent', () => {
            expect(readableTextOn('#2f6f4f')).toBe(LIGHT_LABEL);
        });

        it.each(['#ffffff', '#ffd700', '#000000', '#1e3a8a'])('reaches 4.5:1 on %s', bg => {
            expect(contrastRatio(readableTextOn(bg), bg)).toBeGreaterThanOrEqual(4.5);
        });

        it('judges a gradient by its worst stop, not its first', () => {
            expect(readableTextOn('#6b6b6b')).toBe(LIGHT_LABEL);
            expect(readableTextOn('#6b6b6b', '#a0a0a0')).toBe(DARK_LABEL);
        });

        it('gives the same answer for one stop repeated', () => {
            expect(readableTextOn('#2f6f4f', '#2f6f4f')).toBe(readableTextOn('#2f6f4f'));
        });
    });

    describe('accentGradient', () => {
        const worst = (g: { stops: string[]; label: string }) =>
            Math.min(...g.stops.map(s => contrastRatio(g.label, s)));

        it('keeps the full +8 / -12 spread for the default gold', () => {
            const g = accentGradient('#c5a059');
            expect(g.stops).toEqual(['#c5a059', adjustLightness('#c5a059', 8), adjustLightness('#c5a059', -12)]);
            expect(g.label).toBe(DARK_LABEL);
        });

        it('narrows the spread for a mid-tone green until the label reaches 4.5:1', () => {
            const g = accentGradient('#2e7d32');
            expect(g.stops[1]).not.toBe(adjustLightness('#2e7d32', 8));
            expect(g.stops[1]).not.toBe('#2e7d32');
            expect(worst(g)).toBeGreaterThanOrEqual(4.5);
        });

        it('falls back to a flat fill when no spread can reach 4.5:1', () => {
            // mid grey: white is about 4.4:1 and dark about 4.2:1 on the flat fill itself
            const g = accentGradient('#787878');
            expect(g.stops).toEqual(['#787878', '#787878', '#787878']);
            expect(g.label).toBe(readableTextOn('#787878'));
        });

        it.each(['#000000', '#ffffff', '#1e3a8a', '#ffd700'])('stays at 4.5:1 for %s', accent => {
            expect(worst(accentGradient(accent))).toBeGreaterThanOrEqual(4.5);
        });
    });
});
