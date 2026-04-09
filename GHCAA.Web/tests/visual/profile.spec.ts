import { test, expect, Page } from '@playwright/test';

async function loginAsMember(page: Page) {
  await page.goto('/');
  await page.evaluate(() => {
    localStorage.setItem('jwt_token', 'visual_test_token');
    localStorage.setItem('user_role', 'Member');
    localStorage.setItem('user_id', '1001');
  });
}

test.describe('Member Profile Visual Freeze', () => {
  test.beforeEach(async ({ page }) => loginAsMember(page));

  test('Profile: Member profile view should match baseline', async ({ page }) => {
    await page.goto('/portal/profile');
    await page.waitForLoadState('networkidle');
    await page.waitForSelector('.profile-container', { state: 'visible' });
    await page.waitForTimeout(800);
    await expect(page).toHaveScreenshot('member-profile-view.png', {
      fullPage: true,
      mask: [page.locator('.last-modified'), page.locator('.dynamic-date')],
    });
  });

  test('Profile: Articles page should match baseline', async ({ page }) => {
    await page.goto('/portal/articles');
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(800);
    await expect(page).toHaveScreenshot('member-articles.png', { fullPage: true });
  });
});
