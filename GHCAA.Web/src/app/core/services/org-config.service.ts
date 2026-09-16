import { Injectable, signal, inject, effect, computed } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { API_ENDPOINTS, DEFAULT_DATE_FORMAT, normalizeDateFormat, DateFormat } from '../constants/app.constants';
import { OrgConfig } from '../models/org-config.model';
import { ORG_CONFIG_FALLBACK } from '../config/org-config-fallback.generated';
import { tap, firstValueFrom } from 'rxjs';
import { ThemeService } from './theme.service';
import { adjustLightness, darkenForContrast, hexToRgb, rgbString } from '../utils/color-math';

@Injectable({
  providedIn: 'root'
})
export class OrgConfigService {
  private http = inject(HttpClient);
  private themeService = inject(ThemeService);
  config = signal<OrgConfig | null>(null);
  readonly dateFormat = computed<DateFormat>(() =>
    normalizeDateFormat(this.config()?.localization?.dateFormat ?? DEFAULT_DATE_FORMAT)
  );

  constructor() {
    // 1b: White-labeling — push tenant branding colors, and the gold tokens derived from
    // them, into the CSS custom properties styles.scss consumes, whenever config or the
    // light/dark theme changes. --accent-gold-dark/--tier-founding/--accent-text are AA
    // text-contrast values, not plain tints, so they're darkened until they clear 4.5:1 (or
    // 4.9:1 for the tier heading, matching the floor styles.scss's static values were tuned
    // to) against a white card rather than derived by a fixed hue/lightness offset.
    effect(() => {
      const branding = this.config()?.branding;
      if (!branding) return;
      const theme = this.themeService.theme();

      const root = document.documentElement.style;
      if (branding.primaryColor) root.setProperty('--primary-color', branding.primaryColor);
      if (!branding.accentColor) return;

      const accent = branding.accentColor;
      root.setProperty('--accent-color', accent);
      const accentRgb = rgbString(hexToRgb(accent));
      root.setProperty('--accent-color-rgb', accentRgb);
      root.setProperty('--accent-rgb', accentRgb);

      const goldBright = adjustLightness(accent, 8);
      const goldGradientDark = adjustLightness(accent, -12);
      const goldTextDark = darkenForContrast(accent, '#ffffff', 4.5);
      const tierFoundingLight = darkenForContrast(accent, '#ffffff', 4.9);

      root.setProperty('--accent-gold-bright', goldBright);
      root.setProperty('--accent-gold-dark', goldTextDark);
      root.setProperty('--accent-text', theme === 'dark' ? accent : goldTextDark);
      root.setProperty('--shadow-gold', `0 4px 15px rgba(${accentRgb}, 0.25)`);
      root.setProperty('--gold-gradient', `linear-gradient(135deg, ${accent} 0%, ${goldBright} 50%, ${goldGradientDark} 100%)`);
      root.setProperty('--tier-founding', theme === 'dark' ? accent : tierFoundingLight);

      // --glass-border is accent-tinted only in light theme; dark theme's is an
      // independent translucent-white card token (styles.scss:146), not a gold tint, so
      // clear the inline override there and let that static rule apply again.
      if (theme === 'dark') {
        root.removeProperty('--glass-border');
      } else {
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
