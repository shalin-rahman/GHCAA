/// <reference types="node" />
import { defineConfig } from '@playwright/test';

// 84.52. The election walkthrough runs alone: one worker, real waits for the polling
// window, and every actor in their own browser context. The test opens those contexts
// itself, so only the launch options here reach them.
//   WALKTHROUGH_HEADED=1     show the browser windows
//   WALKTHROUGH_SLOWMO=400   milliseconds between actions, for a live demo
//   WALKTHROUGH_SCENARIO=... another scenario file in place of scenario.json
export default defineConfig({
  testDir: './tests/e2e/election-walkthrough',
  testMatch: /.*\.walkthrough\.ts$/,
  fullyParallel: false,
  workers: 1,
  retries: 0,
  timeout: 90 * 60_000,
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
      // The https profile sets Development. Without it the API starts as Production and
      // stops on the http CORS origins.
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
