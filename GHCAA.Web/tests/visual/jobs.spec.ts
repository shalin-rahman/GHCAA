import { test, expect, Page } from '@playwright/test';

async function loginAsMember(page: Page) {
  await page.goto('/');
  await page.evaluate(() => {
    localStorage.setItem('jwt_token', 'visual_test_token');
    localStorage.setItem('user_role', 'Member');
    localStorage.setItem('user_id', '1001');
  });
}

test.describe('Career & Jobs Visual Freeze', () => {
  test.beforeEach(async ({ page }) => loginAsMember(page));

  test('Jobs: Job listings page should match baseline', async ({ page }) => {
    await page.goto('/portal/jobs');
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(800);
    await expect(page).toHaveScreenshot('jobs-list.png', { fullPage: true });
  });

  test('Jobs: Public jobs page should match baseline', async ({ page }) => {
    await page.goto('/jobs');
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(800);
    await expect(page).toHaveScreenshot('public-jobs-list.png', { fullPage: true });
  });
});
