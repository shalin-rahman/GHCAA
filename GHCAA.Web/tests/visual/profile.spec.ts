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

test.describe('Member Profile Visual Freeze', () => {
  test.beforeEach(async ({ page }) => loginAsMember(page));

  test('Profile: Member profile view should match baseline', async ({ page }) => {
    await page.goto('/portal/profile');
    await page.waitForLoadState('networkidle');
    await page.waitForSelector('.profile-page', { state: 'visible' });
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
