import { ApplicationConfig, APP_INITIALIZER, provideBrowserGlobalErrorListeners } from '@angular/core';
import { provideRouter, withInMemoryScrolling } from '@angular/router';
import { provideHttpClient, withInterceptors, HttpClient } from '@angular/common/http';

import { routes } from './app.routes';
import { globalHttpInterceptor } from './core/interceptors/global-http.interceptor';
import { APP_CONFIG } from './core/constants/app.constants';
import { firstValueFrom } from 'rxjs';
import { OrgConfigService } from './core/services/org-config.service';

export function initializeAppConfig(http: HttpClient) {
  return () => firstValueFrom(http.get('/assets/app.config.json'))
    .then((data: any) => {
      Object.assign(APP_CONFIG, data);
    })
    .catch((err) => {
      console.warn('Could not load /assets/app.config.json. Using defaults.', err);
    });
}

export const appConfig: ApplicationConfig = {
  providers: [
    provideRouter(routes, withInMemoryScrolling({ scrollPositionRestoration: 'top' })),
    provideHttpClient(withInterceptors([globalHttpInterceptor])),
    {
      provide: APP_INITIALIZER,
      useFactory: initializeAppConfig,
      deps: [HttpClient],
      multi: true
    },
    {
      provide: APP_INITIALIZER,
      useFactory: (orgConfigService: OrgConfigService) => () => orgConfigService.loadConfig(),
      deps: [OrgConfigService],
      multi: true
    }
  ]
};
