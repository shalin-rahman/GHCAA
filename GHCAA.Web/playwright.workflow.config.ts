/// <reference types="node" />
import { defineConfig } from '@playwright/test';

// The membership and event walkthroughs. They reuse the election walkthrough's helpers, so the
// report folder and the borrowed-accounts state file are set here per flow, before the test
// worker loads them.
//   WALKTHROUGH_HEADED=1                show the browser windows
//   WALKTHROUGH_SLOWMO=400              milliseconds between actions, for a live demo
//   WALKTHROUGH_SCENARIO=...            another scenario file in place of the folder's scenario.json
// playwright.membership.config.ts and playwright.event.config.ts call this with their flow.
export function workflowConfig(flow: 'membership' | 'event') {
  process.env['WALKTHROUGH_REPORT'] ||= `walkthrough-report-${flow}`;
  process.env['WALKTHROUGH_STATE'] ||= `${flow}-walkthrough-state.json`;
  return defineConfig({
    testDir: `./tests/e2e/${flow}-walkthrough`,
    testMatch: /.*\.walkthrough\.ts$/,
    fullyParallel: false,
    workers: 1,
    retries: 0,
    timeout: 30 * 60_000,
    reporter: [['list']],
    use: {
      baseURL: 'http://localhost:4200',
      headless: !process.env['WALKTHROUGH_HEADED'],
      launchOptions: {
        slowMo: Number(process.env['WALKTHROUGH_SLOWMO'] || 0),
        args: process.env['WALKTHROUGH_HEADED'] ? ['--start-maximized'] : [],
      },
    },
    webServer: [
      {
        command: 'dotnet run --project ../GHCAA.API --launch-profile https',
        url: 'http://localhost:5087/health/live',
        reuseExistingServer: true,
        timeout: 120_000,
      },
      {
        command: 'npm run start',
        url: 'http://localhost:4200',
        reuseExistingServer: true,
        timeout: 120_000,
      },
    ],
  });
}
