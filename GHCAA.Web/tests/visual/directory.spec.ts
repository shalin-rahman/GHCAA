import { test, expect, Page } from '@playwright/test';

async function loginAsMember(page: Page) {
  await page.goto('/');
  await page.evaluate(() => {
    localStorage.setItem('jwt_token', 'visual_test_token');
    localStorage.setItem('user_role', 'Member');
    localStorage.setItem('user_id', '1001');
  });
}

test.describe('Directory & Networking Visual Freeze', () => {
  test.beforeEach(async ({ page }) => loginAsMember(page));

  test('Directory: Member directory list should match baseline', async ({ page }) => {
    await page.goto('/portal/directory');
    await page.waitForLoadState('networkidle');
    await page.waitForSelector('.member-card', { state: 'visible' });
    await page.waitForTimeout(800);
    await expect(page).toHaveScreenshot('directory-list.png', { fullPage: true });
  });

  test('Directory: Public-facing directory should match baseline', async ({ page }) => {
    await page.goto('/directory');
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(800);
    await expect(page).toHaveScreenshot('public-directory.png', { fullPage: true });
  });
});
