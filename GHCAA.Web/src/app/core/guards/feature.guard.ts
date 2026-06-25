import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { OrgConfigService } from '../services/org-config.service';
import { OrgConfig } from '../models/org-config.model';

export const featureGuard = (featureKey: keyof OrgConfig['features'], fallbackUrl = '/') => {
  return () => {
    const orgConfig = inject(OrgConfigService);
    const router = inject(Router);

    if (orgConfig.isFeatureEnabled(featureKey)) {
      return true;
    }

    return router.parseUrl(fallbackUrl);
  };
};
