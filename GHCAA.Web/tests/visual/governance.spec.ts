import { test, expect } from '@playwright/test';

test.describe('Governance Visual Freeze', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/');
    await page.evaluate(() => {
      localStorage.setItem('user_session', JSON.stringify({
        token: 'visual_test_token',
        role: 'Member',
        memberId: 200,
        username: 'visualtest'
      }));
    });
  });

  test('Governance: Executive Committee list should match baseline', async ({ page }) => {
    await page.goto('/portal/governance');
    
    // Wait for the committee members grid
    await expect(page.locator('.governance-page')).toBeVisible();
    await expect(page.locator('.committee-card').first()).toBeVisible();
    
    // Ensure skeleton loaders are gone
    await page.waitForSelector('.skeleton-loader', { state: 'detached' });
    
    await page.waitForTimeout(1000);

    await expect(page).toHaveScreenshot('governance-ec-list.png', {
      fullPage: true
    });
  });
});
