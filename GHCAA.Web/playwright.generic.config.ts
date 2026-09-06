/// <reference types="node" />
import { defineConfig, devices } from '@playwright/test';

// WP62.41's acceptance test needs a server booted with ORG_PROFILE=default against a
// database that has never run a migration, which is not what the main dev server (see
// playwright.config.ts) points at. This config has no webServer entry on purpose — start
// that server yourself first (see generic-profile-acceptance.spec.ts's file header), then
// run: PLAYWRIGHT_GENERIC_BASE_URL=http://localhost:5099 npx playwright test --config=playwright.generic.config.ts
export default defineConfig({
  testDir: './tests/e2e',
  testMatch: 'generic-profile-acceptance.spec.ts',
  fullyParallel: false,
  retries: 0,
  workers: 1,
  reporter: 'list',
  use: {
    baseURL: process.env.PLAYWRIGHT_GENERIC_BASE_URL || 'http://localhost:5099',
    trace: 'on-first-retry',
    screenshot: 'only-on-failure',
  },
  projects: [
    {
      name: 'chromium',
      use: { ...devices['Desktop Chrome'] },
    },
  ],
});
