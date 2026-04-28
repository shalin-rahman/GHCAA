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
