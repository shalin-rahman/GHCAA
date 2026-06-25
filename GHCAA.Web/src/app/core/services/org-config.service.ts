import { Injectable, signal, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { API_ENDPOINTS } from '../constants/app.constants';
import { OrgConfig } from '../models/org-config.model';
import { tap, firstValueFrom } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class OrgConfigService {
  private http = inject(HttpClient);
  config = signal<OrgConfig | null>(null);

  loadConfig(): Promise<void> {
    return firstValueFrom(
      this.http.get<OrgConfig>(API_ENDPOINTS.CONFIG).pipe(
        tap(cfg => this.config.set(cfg))
      )
    )
    .then(() => {})
    .catch(err => {
      console.error('Failed to load organization configuration:', err);
      // Fallback/Default configuration in case of load failure to prevent app breakdown
      this.config.set({
        orgId: 'GHCAA',
        features: {
          enableEBook: true,
          enableAlumniGallery: true,
          enableDiscussionForums: true,
          enableJobBoard: true,
          enableAcademicRecords: true,
          enableProfessionalRecords: true
        },
        contact: {
          email: 'haragangian@gmail.com',
          phone: '',
          address: 'Govt. Haraganga College Campus, Munshiganj, Bangladesh.',
          socialLinks: {}
        }
      });
    });
  }

  isFeatureEnabled(featureName: keyof OrgConfig['features']): boolean {
    const cfg = this.config();
    return cfg ? !!cfg.features[featureName] : false;
  }

  updateConfig(config: OrgConfig): Promise<void> {
    return firstValueFrom(
      this.http.put<void>(API_ENDPOINTS.CONFIG, config)
    ).then(() => {
      this.config.set(config);
    });
  }
}
