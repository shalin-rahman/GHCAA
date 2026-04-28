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
