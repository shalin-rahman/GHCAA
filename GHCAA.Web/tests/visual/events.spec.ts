import { test, expect, Page } from '@playwright/test';

async function loginAsMember(page: Page) {
  await page.goto('/');
  await page.evaluate(() => {
    localStorage.setItem('jwt_token', 'visual_test_token');
    localStorage.setItem('user_role', 'Member');
    localStorage.setItem('user_id', '1001');
  });
}

test.describe('Events Visual Freeze', () => {
  test.beforeEach(async ({ page }) => loginAsMember(page));

  test('Events: Event list view should match baseline', async ({ page }) => {
    await page.goto('/portal/events');
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(800);
    await expect(page).toHaveScreenshot('events-list.png', { fullPage: true });
  });

  test('Events: Public event list should match baseline', async ({ page }) => {
    await page.goto('/events');
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(800);
    await expect(page).toHaveScreenshot('public-events-list.png', { fullPage: true });
  });
});
