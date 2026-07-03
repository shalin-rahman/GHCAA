import { test, expect } from '@playwright/test';

test.describe('Configuration-Driven Framework', () => {
  test('should load default configuration from API', async ({ request }) => {
    const response = await request.get('/api/config');
    expect(response.ok()).toBeTruthy();
    
    const config = await response.json();
    
    // Assert schema defaults
    expect(config.orgId).toBe('ghcaa');
    expect(config.schemaVersion).toBe(1);
    
    // Assert branding
    expect(config.branding.shortName).toBe('GHCAA');
    expect(config.branding.primaryColor).toBe('#1a237e');
    
    // Assert localization required keys
    expect(config.localization.defaultLocale).toBe('en');
    expect(config.localization.locales).toHaveProperty('en');
    expect(config.localization.locales).toHaveProperty('bn');
    
    // Assert membership type labels includes Guest
    expect(config.localization.locales.en.membershipTypeLabels).toHaveProperty('Guest');
  });

  test('admin can fetch config via UI route', async ({ page }) => {
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
