import { test, expect, Page } from '@playwright/test';

// Reusable auth helper
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

test.describe('Content & News Visual Freeze', () => {
  test.beforeEach(async ({ page }) => loginAsMember(page));

  test('Content: News hub list view should match baseline', async ({ page }) => {
    await page.goto('/portal/news');
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(800);
    await expect(page).toHaveScreenshot('news-list.png', { fullPage: true });
  });

  test('Content: Magazine page should match baseline', async ({ page }) => {
    await page.goto('/magazine');
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(800);
    await expect(page).toHaveScreenshot('magazine-page.png', { fullPage: true });
  });

  test('Content: Gallery grid should match baseline', async ({ page }) => {
    await page.goto('/portal/gallery');
    await page.waitForLoadState('networkidle');
    // Wait for either gallery layout to render.
    await page.waitForSelector('.gallery-card, .data-table tbody tr', { state: 'visible' });
    await page.waitForTimeout(1000);
    await expect(page).toHaveScreenshot('gallery-grid.png', { fullPage: true });
  });
});
