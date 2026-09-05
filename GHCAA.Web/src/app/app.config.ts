import { ApplicationConfig, APP_INITIALIZER, ErrorHandler, provideBrowserGlobalErrorListeners } from '@angular/core';
import { provideRouter, withInMemoryScrolling, TitleStrategy } from '@angular/router';
import { provideHttpClient, withInterceptors, withXsrfConfiguration } from '@angular/common/http';

import { routes } from './app.routes';
import { globalHttpInterceptor } from './core/interceptors/global-http.interceptor';
import { OrgConfigService } from './core/services/org-config.service';
import { GlobalErrorHandler } from './core/services/global-error-handler';
import { BrandingTitleStrategy } from './core/strategies/branding-title.strategy';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    // 30.23: anchorScrolling lets routerLink [fragment] targets (e.g. dashboard profile-completion
    // steps linking into /portal/profile#section-…) scroll the matching element into view.
    provideRouter(routes, withInMemoryScrolling({ scrollPositionRestoration: 'top', anchorScrolling: 'enabled' })),
    provideHttpClient(
      withInterceptors([globalHttpInterceptor]),
      withXsrfConfiguration({ cookieName: 'XSRF-TOKEN', headerName: 'X-XSRF-TOKEN' })
    ),
    { provide: ErrorHandler, useClass: GlobalErrorHandler },
    { provide: TitleStrategy, useClass: BrandingTitleStrategy },
    {
      provide: APP_INITIALIZER,
      useFactory: (orgConfigService: OrgConfigService) => () => orgConfigService.loadConfig(),
      deps: [OrgConfigService],
      multi: true
    }
  ]
};
