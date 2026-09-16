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

export function hexToRgb(hex: string): Rgb {
  const clean = hex.replace('#', '');
  const full = clean.length === 3 ? clean.split('').map(c => c + c).join('') : clean;
  const num = parseInt(full, 16);
  return { r: (num >> 16) & 255, g: (num >> 8) & 255, b: num & 255 };
}

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
