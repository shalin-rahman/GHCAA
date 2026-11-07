/**
 * HSL color math for deriving the gold-accent CSS tokens (styles.scss) from a
 * profile's single configured --accent-color, instead of leaving them as
 * hardcoded literals tuned only for GHC's default gold.
 */

export interface Rgb {
  r: number;
  g: number;
  b: number;
}

export function parseHexColor(value: string): Rgb | null {
  const clean = value.trim().replace(/^#/, '');
  if (!/^(?:[0-9a-f]{3}|[0-9a-f]{6})$/i.test(clean)) return null;

  const full = clean.length === 3 ? clean.split('').map(c => c + c).join('') : clean;
  const num = parseInt(full, 16);
  return { r: (num >> 16) & 255, g: (num >> 8) & 255, b: num & 255 };
}

export function hexToRgb(hex: string): Rgb {
  const clean = hex.trim().replace('#', '');
  const full = clean.length === 3 ? clean.split('').map(c => c + c).join('') : clean;
  const num = parseInt(full, 16);
  return { r: (num >> 16) & 255, g: (num >> 8) & 255, b: num & 255 };
}

/** WCAG AA minimum contrast for normal-size text. */
export const MIN_TEXT_CONTRAST = 4.5;

/** The two label colors a filled accent can carry; they match --text-on-accent's default and white. */
export const DARK_LABEL = '#171717';
export const LIGHT_LABEL = '#ffffff';

/** Lightest surface accent text sits on in each theme (light: --bg-color, dark: --card-bg). */
export const LIGHT_TEXT_SURFACE = '#ffffff';
export const DARK_TEXT_SURFACE = '#1b1f24';

/** Lightness points added to the branding primary in dark mode; #121212 becomes #1a1a1a. */
export const DARK_PRIMARY_LIFT = 3;

export function rgbToHex({ r, g, b }: Rgb): string {
  const toHex = (c: number) => Math.round(clamp(c, 0, 255)).toString(16).padStart(2, '0');
  return `#${toHex(r)}${toHex(g)}${toHex(b)}`;
}

export function rgbString({ r, g, b }: Rgb): string {
  return `${Math.round(r)}, ${Math.round(g)}, ${Math.round(b)}`;
}

interface Hsl {
  h: number;
  s: number;
  l: number;
}

function clamp(n: number, min: number, max: number): number {
  return Math.min(max, Math.max(min, n));
}

export function rgbToHsl({ r, g, b }: Rgb): Hsl {
  const rn = r / 255, gn = g / 255, bn = b / 255;
  const max = Math.max(rn, gn, bn), min = Math.min(rn, gn, bn);
  const l = (max + min) / 2;
  const diff = max - min;
  if (diff === 0) return { h: 0, s: 0, l: l * 100 };

  const s = diff / (1 - Math.abs(2 * l - 1));
  let h: number;
  switch (max) {
    case rn: h = 60 * (((gn - bn) / diff) % 6); break;
    case gn: h = 60 * ((bn - rn) / diff + 2); break;
    default: h = 60 * ((rn - gn) / diff + 4);
  }
  if (h < 0) h += 360;
  return { h, s: s * 100, l: l * 100 };
}

export function hslToRgb({ h, s, l }: Hsl): Rgb {
  const sn = s / 100, ln = l / 100;
  const c = (1 - Math.abs(2 * ln - 1)) * sn;
  const x = c * (1 - Math.abs(((h / 60) % 2) - 1));
  const m = ln - c / 2;
  let rp = 0, gp = 0, bp = 0;
  if (h < 60) { rp = c; gp = x; }
  else if (h < 120) { rp = x; gp = c; }
  else if (h < 180) { gp = c; bp = x; }
  else if (h < 240) { gp = x; bp = c; }
  else if (h < 300) { rp = x; bp = c; }
  else { rp = c; bp = x; }
  return { r: (rp + m) * 255, g: (gp + m) * 255, b: (bp + m) * 255 };
}

/** Adjusts lightness by `deltaL` percentage points (positive lightens, negative darkens). */
export function adjustLightness(hex: string, deltaL: number): string {
  const hsl = rgbToHsl(hexToRgb(hex));
  hsl.l = clamp(hsl.l + deltaL, 0, 100);
  return rgbToHex(hslToRgb(hsl));
}

function relativeLuminance({ r, g, b }: Rgb): number {
  const channel = (c: number) => {
    const cn = c / 255;
    return cn <= 0.03928 ? cn / 12.92 : Math.pow((cn + 0.055) / 1.055, 2.4);
  };
  return 0.2126 * channel(r) + 0.7152 * channel(g) + 0.0722 * channel(b);
}

/** WCAG contrast ratio between two colors, 1 (no contrast) to 21 (max). */
export function contrastRatio(hexA: string, hexB: string): number {
  const lA = relativeLuminance(hexToRgb(hexA));
  const lB = relativeLuminance(hexToRgb(hexB));
  return ratioFromLuminance(lA, lB);
}

export function safeContrastRatio(hexA: string, hexB: string): number | null {
  const colorA = parseHexColor(hexA);
  const colorB = parseHexColor(hexB);
  if (!colorA || !colorB) return null;

  const lA = relativeLuminance(colorA);
  const lB = relativeLuminance(colorB);
  return ratioFromLuminance(lA, lB);
}

function ratioFromLuminance(lA: number, lB: number): number {
  const lighter = Math.max(lA, lB);
  const darker = Math.min(lA, lB);
  return (lighter + 0.05) / (darker + 0.05);
}

/**
 * Darkens `hex` in HSL steps until it hits `minRatio` contrast against `backgroundHex`,
 * so a custom accent color still passes AA (4.5:1 for text) as text on a light surface —
 * the same floor --accent-gold-dark/--tier-founding were manually tuned to hit for GHC's
 * default gold. Gives up at L=0 (pure black) rather than looping forever on light-on-light
 * accents no amount of darkening can separate from the background.
 */
export function darkenForContrast(hex: string, backgroundHex: string, minRatio: number): string {
  let candidate = hex;
  let hsl = rgbToHsl(hexToRgb(candidate));
  while (contrastRatio(candidate, backgroundHex) < minRatio && hsl.l > 0) {
    hsl.l = clamp(hsl.l - 2, 0, 100);
    candidate = rgbToHex(hslToRgb(hsl));
  }
  return candidate;
}

/** The mirror of darkenForContrast for text on a dark surface. Stops at white. */
export function lightenForContrast(hex: string, backgroundHex: string, minRatio: number): string {
  let candidate = hex;
  const hsl = rgbToHsl(hexToRgb(candidate));
  while (contrastRatio(candidate, backgroundHex) < minRatio && hsl.l < 100) {
    hsl.l = clamp(hsl.l + 2, 0, 100);
    candidate = rgbToHex(hslToRgb(hsl));
  }
  return candidate;
}

/**
 * Picks the dark or white label, whichever reads better on a filled `backgroundHex`.
 * Pass every stop of a gradient fill and the label is judged on its worst stop.
 */
export function readableTextOn(backgroundHex: string, ...moreBackgrounds: string[]): string {
  const fills = [backgroundHex, ...moreBackgrounds];
  return worstContrast(DARK_LABEL, fills) >= worstContrast(LIGHT_LABEL, fills) ? DARK_LABEL : LIGHT_LABEL;
}

function worstContrast(label: string, fills: string[]): number {
  return Math.min(...fills.map(fill => contrastRatio(label, fill)));
}

/** How far the accent gradient's light and dark stops sit from the accent, in HSL lightness points. */
export const ACCENT_GRADIENT_SPREAD = 8;

export interface AccentGradient {
  /** accent, lighter stop, darker stop, in paint order */
  stops: [string, string, string];
  label: string;
}

/**
 * The stops and label for a filled accent button. The full spread is +8 / -12. A mid-tone
 * accent can leave no label at 4.5:1 across that range, so the spread shrinks a point at a
 * time until one fits. If even a flat fill cannot reach 4.5:1, the flat fill is returned
 * with the better label, since changing the tenant's hue is not ours to do.
 */
export function accentGradient(accent: string, minRatio = MIN_TEXT_CONTRAST): AccentGradient {
  let result: AccentGradient = { stops: [accent, accent, accent], label: readableTextOn(accent) };
  for (let spread = ACCENT_GRADIENT_SPREAD; spread >= 0; spread--) {
    const stops: [string, string, string] = [
      accent, adjustLightness(accent, spread), adjustLightness(accent, -1.5 * spread)
    ];
    const label = readableTextOn(...stops);
    result = { stops, label };
    if (worstContrast(label, stops) >= minRatio) break;
  }
  return result;
}
