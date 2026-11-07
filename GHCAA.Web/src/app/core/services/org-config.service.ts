import { Injectable, signal, inject, effect, computed } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { API_ENDPOINTS, DEFAULT_DATE_FORMAT, normalizeDateFormat, DateFormat, setOrgLabelMaps } from '../constants/app.constants';
import { OrgConfig } from '../models/org-config.model';
import { ORG_CONFIG_FALLBACK } from '../config/org-config-fallback.generated';
import { tap, firstValueFrom } from 'rxjs';
import { ThemeService } from './theme.service';
import {
  ACCENT_GRADIENT_SPREAD, accentGradient, adjustLightness, darkenForContrast, DARK_LABEL, DARK_PRIMARY_LIFT, DARK_TEXT_SURFACE, hexToRgb,
  lightenForContrast, MIN_TEXT_CONTRAST, parseHexColor, readableTextOn, rgbString
} from '../utils/color-math';

@Injectable({
  providedIn: 'root'
})
export class OrgConfigService {
  // Every custom property the branding effect writes. It clears these on each run.
  private static readonly ROOT_TOKENS = [
    '--accent-color', '--accent-color-rgb', '--accent-rgb', '--accent-gold-bright', '--accent-gold-dark',
    '--shadow-gold', '--gold-gradient', '--tier-founding', '--glass-border', '--select-chevron-gold'
  ];
  private static readonly BODY_TOKENS = [
    '--primary-color', '--primary-rgb', '--text-on-primary', '--accent-text', '--accent-hover-text',
    '--focus-ring-color', '--text-on-accent', '--shadow-accent-hover', '--accent-on-dark-text',
    '--accent-on-dark-hover-text'
  ];

  private http = inject(HttpClient);
  private themeService = inject(ThemeService);
  config = signal<OrgConfig | null>(null);
  readonly dateFormat = computed<DateFormat>(() =>
    normalizeDateFormat(this.config()?.localization?.dateFormat ?? DEFAULT_DATE_FORMAT)
  );

  constructor() {
    effect(() => setOrgLabelMaps(this.localePack()));

    // 1b: White-labeling — push tenant branding colors, and the gold tokens derived from
    // them, into the CSS custom properties styles.scss consumes, whenever config or the
    // light/dark theme changes. --accent-gold-dark/--tier-founding/--accent-text are AA
    // text-contrast values, not plain tints, so they're darkened until they clear 4.5:1 (or
    // 4.9:1 for the tier heading, matching the floor styles.scss's static values were tuned
    // to) against a white card rather than derived by a fixed hue/lightness offset.
    effect(() => {
      const branding = this.config()?.branding;
      const root = document.documentElement.style;
      const body = document.body.style;
      // Clear everything first, so a color removed from branding falls back to the theme
      // stylesheet instead of keeping the last value written here.
      OrgConfigService.ROOT_TOKENS.forEach(prop => root.removeProperty(prop));
      OrgConfigService.BODY_TOKENS.forEach(prop => body.removeProperty(prop));
      if (!branding) return;
      const theme = this.themeService.theme();

      // Anything that is not a 3 or 6 digit hex is treated as unset, since the derived
      // tokens below need real channel values.
      const validHex = (value?: string | null) => (value && parseHexColor(value) ? value.trim() : null);
      let primary: string | null = null;
      const configuredPrimary = validHex(branding.primaryColor);
      if (configuredPrimary) {
        // body.light-theme and body.dark-theme both declare the primary, so it goes inline on
        // <body>. Dark mode lifts it 3 points, which turns the default #121212 into the
        // #1a1a1a the dark theme always used.
        primary = theme === 'dark'
          ? adjustLightness(configuredPrimary, DARK_PRIMARY_LIFT)
          : configuredPrimary;
        body.setProperty('--primary-color', primary);
        body.setProperty('--primary-rgb', rgbString(hexToRgb(primary)));
        body.setProperty('--text-on-primary', readableTextOn(primary));
      }
      const accent = validHex(branding.accentColor);
      if (!accent) return;

      root.setProperty('--accent-color', accent);
      const accentRgb = rgbString(hexToRgb(accent));
      root.setProperty('--accent-color-rgb', accentRgb);
      root.setProperty('--accent-rgb', accentRgb);

      const goldBright = adjustLightness(accent, ACCENT_GRADIENT_SPREAD);
      const goldTextDark = darkenForContrast(accent, '#ffffff', MIN_TEXT_CONTRAST);
      const tierFoundingLight = darkenForContrast(accent, '#ffffff', 4.9);

      root.setProperty('--accent-gold-bright', goldBright);
      root.setProperty('--accent-gold-dark', goldTextDark);
      root.setProperty('--shadow-gold', `0 4px 15px rgba(${accentRgb}, 0.25)`);

      // These tokens are also declared on body.dark-theme, which would hide a value set on
      // <html>, so they go inline on <body>. Accent text keeps 4.5:1 on the lightest surface
      // of each theme; the label on a filled accent is whichever of dark or white reads better.
      const accentTextDark = lightenForContrast(accent, DARK_TEXT_SURFACE, MIN_TEXT_CONTRAST);
      const accentText = theme === 'dark' ? accentTextDark : goldTextDark;
      const accentHoverText = theme === 'dark'
        ? adjustLightness(accentTextDark, 8)
        : adjustLightness(goldTextDark, -4);
      body.setProperty('--accent-text', accentText);
      body.setProperty('--accent-hover-text', accentHoverText);
      body.setProperty('--focus-ring-color', theme === 'dark' ? accentHoverText : goldTextDark);
      // .btn-accent paints --gold-gradient, so its label must read on every stop. The
      // stops narrow for accents where the full spread leaves no readable label.
      const gradient = accentGradient(accent);
      body.setProperty('--text-on-accent', gradient.label);
      if (primary) {
        // .on-dark islands, the public nav and the footer sit on the primary, not on a theme
        // surface. A custom accent can be too close to a custom primary to read, so move it
        // away: lighter on a dark primary, darker on a light one.
        const primaryIsLight = readableTextOn(primary) === DARK_LABEL;
        const onDarkText = primaryIsLight
          ? darkenForContrast(accent, primary, MIN_TEXT_CONTRAST)
          : lightenForContrast(accent, primary, MIN_TEXT_CONTRAST);
        body.setProperty('--accent-on-dark-text', onDarkText);
        body.setProperty('--accent-on-dark-hover-text', adjustLightness(onDarkText, primaryIsLight ? -8 : 8));
      }
      body.setProperty('--shadow-accent-hover', `0 8px 25px rgba(${accentRgb}, 0.4)`);
      const [gradStart, gradMid, gradEnd] = gradient.stops;
      root.setProperty('--gold-gradient', `linear-gradient(135deg, ${gradStart} 0%, ${gradMid} 50%, ${gradEnd} 100%)`);
      root.setProperty('--tier-founding', theme === 'dark' ? accentTextDark : tierFoundingLight);

      // --glass-border is accent-tinted only in light theme; dark theme's is an
      // independent translucent-white card token (styles/themes/_dark.scss), not a gold tint,
      // so it stays cleared there and that static rule applies.
      if (theme !== 'dark') {
        root.setProperty('--glass-border', `rgba(${accentRgb}, 0.12)`);
      }

      // admin-payment-config.scss's select chevron: a data-URI's stroke color can't
      // reference var(--accent-color) directly, so the whole url() value is computed here.
      const accentHexNoHash = accent.replace('#', '');
      root.setProperty(
        '--select-chevron-gold',
        `url("data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' fill='none' viewBox='0 0 24 24' stroke='%23${accentHexNoHash}'%3E%3Cpath stroke-linecap='round' stroke-linejoin='round' stroke-width='2' d='M19 9l-7 7-7-7'%3E%3C/path%3E%3C/svg%3E")`
      );
    });
  }

  loadConfig(): Promise<void> {
    return firstValueFrom(
      this.http.get<OrgConfig>(API_ENDPOINTS.CONFIG).pipe(
        tap(cfg => this.config.set(cfg))
      )
    )
    .then(() => {})
    .catch(err => {
      console.error('Failed to load organization configuration:', err);
      // Fallback used before the real API response arrives and if it never does.
      // ORG_CONFIG_FALLBACK is generated at build time from the active institution
      // profile pack (see scripts/generate-org-config-fallback.mjs) so this isn't a
      // second hand-maintained copy of the branding values.
      this.config.set(ORG_CONFIG_FALLBACK);
    });
  }

  isFeatureEnabled(featureName: keyof OrgConfig['features']): boolean {
    const cfg = this.config();
    return cfg ? !!cfg.features[featureName] : false;
  }

  localePack(locale: string = 'en') {
    return this.config()?.localization?.locales?.[locale];
  }

  updateConfig(config: OrgConfig): Promise<void> {
    return firstValueFrom(
      this.http.put<void>(API_ENDPOINTS.CONFIG, config)
    ).then(() => {
      this.config.set(config);
    });
  }
}
