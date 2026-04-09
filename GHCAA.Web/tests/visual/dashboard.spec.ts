import { test, expect } from '@playwright/test';

test.describe('Portal Dashboard Visual Freeze', () => {
  test.beforeEach(async ({ page }) => {
    // Setup predictable visual state for Member
    await page.goto('/');
    await page.evaluate(() => {
      localStorage.setItem('jwt_token', 'visual_test_token');
      localStorage.setItem('user_role', 'Member');
      localStorage.setItem('user_id', '1001');
    });
  });

  test('Dashboard: Member Home should match baseline', async ({ page }) => {
    await page.goto('/portal/dashboard');
    
    // Wait for the dashboard grid and banners
    await expect(page.locator('.dashboard-container')).toBeVisible();
    await expect(page.locator('.stats-grid')).toBeVisible();
    
    // Ensure skeleton loaders are gone
    await page.waitForSelector('.skeleton-loader', { state: 'detached' });
    
    await page.waitForTimeout(1000);

    await expect(page).toHaveScreenshot('member-dashboard.png', {
      fullPage: true,
      mask: [page.locator('.dynamic-date')], // Mask parts that might still be dynamic
    });
  });
});
