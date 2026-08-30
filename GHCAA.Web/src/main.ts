import { bootstrapApplication } from '@angular/platform-browser';
import { appConfig } from './app/app.config';
import { App } from './app/app';

// Clear the stale-chunk reload guard (see GlobalErrorHandler) on a successful bootstrap, so a
// later, genuinely new stale-chunk incident (after a future deploy) is still allowed one retry
// reload rather than being permanently silenced by a flag left over from a prior incident.
try {
  sessionStorage.removeItem('ghcaa-stale-chunk-reload');
} catch {
  // sessionStorage unavailable — nothing to clear.
}

bootstrapApplication(App, appConfig)
  .catch((err) => console.error(err));
