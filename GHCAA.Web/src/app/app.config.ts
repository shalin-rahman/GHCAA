import { ApplicationConfig, APP_INITIALIZER, provideBrowserGlobalErrorListeners } from '@angular/core';
import { provideRouter, withInMemoryScrolling } from '@angular/router';
import { provideHttpClient, withInterceptors, withXsrfConfiguration } from '@angular/common/http';

import { routes } from './app.routes';
import { globalHttpInterceptor } from './core/interceptors/global-http.interceptor';
import { OrgConfigService } from './core/services/org-config.service';

export const appConfig: ApplicationConfig = {
  providers: [
    provideRouter(routes, withInMemoryScrolling({ scrollPositionRestoration: 'top' })),
    provideHttpClient(
      withInterceptors([globalHttpInterceptor]),
      withXsrfConfiguration({ cookieName: 'XSRF-TOKEN', headerName: 'X-XSRF-TOKEN' })
    ),
    {
      provide: APP_INITIALIZER,
      useFactory: (orgConfigService: OrgConfigService) => () => orgConfigService.loadConfig(),
      deps: [OrgConfigService],
      multi: true
    }
  ]
};
