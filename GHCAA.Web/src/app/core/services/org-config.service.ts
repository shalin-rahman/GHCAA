import { Injectable, signal, inject, effect, computed } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { API_ENDPOINTS, DEFAULT_DATE_FORMAT, normalizeDateFormat, DateFormat } from '../constants/app.constants';
import { OrgConfig } from '../models/org-config.model';
import { ORG_CONFIG_FALLBACK } from '../config/org-config-fallback.generated';
import { tap, firstValueFrom } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class OrgConfigService {
  private http = inject(HttpClient);
  config = signal<OrgConfig | null>(null);
  readonly dateFormat = computed<DateFormat>(() =>
    normalizeDateFormat(this.config()?.localization?.dateFormat ?? DEFAULT_DATE_FORMAT)
  );

  constructor() {
    // 1b: White-labeling — push tenant branding colors into the CSS custom
    // properties consumed throughout styles.scss whenever config resolves/changes.
    effect(() => {
      const branding = this.config()?.branding;
      if (!branding) return;

      const root = document.documentElement.style;
      if (branding.primaryColor) root.setProperty('--primary-color', branding.primaryColor);
      if (branding.accentColor) root.setProperty('--accent-color', branding.accentColor);
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
