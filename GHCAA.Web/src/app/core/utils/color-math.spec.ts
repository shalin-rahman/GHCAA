import { describe, it, expect } from 'vitest';
import {
    hexToRgb, rgbToHex, rgbString, rgbToHsl, hslToRgb,
    adjustLightness, contrastRatio, darkenForContrast
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
});
