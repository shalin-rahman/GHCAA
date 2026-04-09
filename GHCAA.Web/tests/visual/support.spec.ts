import { test, expect, Page } from '@playwright/test';

async function loginAsMember(page: Page) {
  await page.goto('/');
  await page.evaluate(() => {
    localStorage.setItem('jwt_token', 'visual_test_token');
    localStorage.setItem('user_role', 'Member');
    localStorage.setItem('user_id', '1001');
  });
}

test.describe('Support & Communication Visual Freeze', () => {
  test.beforeEach(async ({ page }) => loginAsMember(page));

  test('Support: AI Assistant chat interface should match baseline', async ({ page }) => {
    await page.goto('/portal/assistant');
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(800);
    await expect(page).toHaveScreenshot('ai-assistant.png', { fullPage: true });
  });

  test('Support: Member messages inbox should match baseline', async ({ page }) => {
    await page.goto('/portal/messages');
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(800);
    await expect(page).toHaveScreenshot('messages-inbox.png', { fullPage: true });
  });
});
