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

test.describe('Financial Portal Visual Freeze', () => {
  test.beforeEach(async ({ page }) => loginAsMember(page));

  test('Finance: Member payment portal should match baseline', async ({ page }) => {
    await page.goto('/portal/payments');
    await page.waitForLoadState('networkidle');
    // Mask dynamic timestamps in payment history
    await page.waitForTimeout(1000);
    await expect(page).toHaveScreenshot('member-payments.png', {
      fullPage: true,
      mask: [page.locator('.payment-date')],
    });
  });
});
