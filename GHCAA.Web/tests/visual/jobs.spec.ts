import { test, expect, Page } from '@playwright/test';

async function loginAsMember(page: Page) {
  await page.goto('/');
  await page.evaluate(() => {
    localStorage.setItem('user_session', JSON.stringify({
      token: 'visual_test_token',
      role: 'Member',
      memberId: 200,
      username: 'visualtest'
    }));
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
