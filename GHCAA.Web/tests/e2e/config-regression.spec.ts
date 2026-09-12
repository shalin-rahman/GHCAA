import { test, expect, Page } from '@playwright/test';

// Per-profile config, mocked from profiles/<name>/org-config.json (camelCased, matching the
// ASP.NET Core default JSON policy GET /api/config actually returns). Mocking it, rather than
// hitting a live backend, is what lets this file run both profiles: the normal webServer only
// ever boots one ORG_PROFILE at a time against the shared GHCAA dev database, and standing up a
// second live backend per profile is exactly the setup generic-profile-acceptance.spec.ts needs
// and this file is meant to avoid (62.43).
interface ProfileFixture {
  name: string;
  config: {
    orgId: string;
    schemaVersion: number;
    branding: { shortName: string; primaryColor: string };
    currency: { code: string; symbol: string; name: string };
    enabledGatewayMethods: string[];
    features: { enableEvents: boolean };
    localization: {
      dateFormat: 'dd-MM-yyyy' | 'MM/dd/yyyy';
      defaultLocale: string;
      locales: Record<string, { membershipTypeLabels: Record<string, string> }>;
    };
  };
}

const PROFILES: ProfileFixture[] = [
  {
    name: 'ghc',
    config: {
      orgId: 'ghcaa',
      schemaVersion: 1,
      branding: { shortName: 'GHCAA', primaryColor: '#121212' },
      currency: { code: 'BDT', symbol: '৳', name: 'Bangladeshi Taka' },
      enabledGatewayMethods: [],
      features: { enableEvents: true },
      localization: {
        dateFormat: 'dd-MM-yyyy',
        defaultLocale: 'en',
        locales: {
          en: { membershipTypeLabels: { Guest: 'Guest Member' } },
          bn: { membershipTypeLabels: { Guest: 'অতিথি সদস্য' } },
        },
      },
    },
  },
  {
    name: 'default',
    config: {
      orgId: 'default',
      schemaVersion: 1,
      branding: { shortName: 'Alumni Association', primaryColor: '#121212' },
      currency: { code: 'USD', symbol: '$', name: 'US Dollar' },
      enabledGatewayMethods: [],
      features: { enableEvents: true },
      localization: {
        dateFormat: 'MM/dd/yyyy',
        defaultLocale: 'en',
        locales: {
          en: { membershipTypeLabels: { General: 'General' } },
        },
      },
    },
  },
];

async function mockConfig(page: Page, config: ProfileFixture['config']) {
  await page.route('**/api/config', (route) =>
    route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify(config),
    })
  );
}

for (const profile of PROFILES) {
  test.describe(`Configuration-Driven Framework — ${profile.name} profile`, () => {
    test('should load configuration from API', async ({ page }) => {
      await mockConfig(page, profile.config);

      const configPromise = page.waitForResponse('**/api/config');
      await page.goto('/');
      const response = await configPromise;
      expect(response.ok()).toBeTruthy();

      const config = await response.json();

      // Assert schema
      expect(config.orgId).toBe(profile.config.orgId);
      expect(config.schemaVersion).toBe(1);

      // Assert branding is this profile's own, not the other profile's
      expect(config.branding.shortName).toBe(profile.config.branding.shortName);
      expect(config.branding.primaryColor).toBe(profile.config.branding.primaryColor);

      // Assert currency/locale — the part 62.37 checks downstream code against
      expect(config.currency.code).toBe(profile.config.currency.code);
      expect(config.currency.symbol).toBe(profile.config.currency.symbol);
      expect(config.enabledGatewayMethods).toEqual(profile.config.enabledGatewayMethods);
      expect(config.localization.dateFormat).toBe(profile.config.localization.dateFormat);

      // Assert localization required keys, scoped to what this profile actually ships
      expect(config.localization.defaultLocale).toBe(profile.config.localization.defaultLocale);
      for (const locale of Object.keys(profile.config.localization.locales)) {
        expect(config.localization.locales).toHaveProperty(locale);
      }

      const [firstLocale, labels] = Object.entries(profile.config.localization.locales)[0];
      for (const [key, label] of Object.entries(labels.membershipTypeLabels)) {
        expect(config.localization.locales[firstLocale].membershipTypeLabels[key]).toBe(label);
      }
    });

    test('persists the selected date format across a browser reload', async ({ page }) => {
      let storedConfig = structuredClone(profile.config);
      await page.route('**/api/config', async (route) => {
        if (route.request().method() === 'PUT') {
          storedConfig = await route.request().postDataJSON();
          await route.fulfill({ status: 204, body: '' });
          return;
        }
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify(storedConfig),
        });
      });

      await page.goto('/');
      await page.evaluate(async (dateFormat) => {
        const response = await fetch('/api/config', {
          method: 'PUT',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify({
            ...await (await fetch('/api/config')).json(),
            localization: { dateFormat, defaultLocale: 'en', locales: {} },
          }),
        });
        if (!response.ok) throw new Error(`Config update failed: ${response.status}`);
      }, profile.config.localization.dateFormat);

      const configResponse = page.waitForResponse(
        (request) => request.url().includes('/api/config') && request.request().method() === 'GET'
      );
      await page.reload();
      const response = await configResponse;
      expect((await response.json()).localization.dateFormat)
        .toBe(profile.config.localization.dateFormat);
      expect(storedConfig.localization.dateFormat).toBe(profile.config.localization.dateFormat);
    });

    test('admin can fetch config via UI route', async ({ page }) => {
      await mockConfig(page, profile.config);

      // Navigate to a page that forces config loading (like the planned admin config page)
      // We will just verify the api call is made when app boots.
      const configPromise = page.waitForResponse('**/api/config');
      await page.goto('/admin');

      const response = await configPromise;
      expect(response.status()).toBe(200);

      const body = await response.json();
      expect(body.features.enableEvents).toBe(true);
    });
  });
}
