import { test, expect } from '@playwright/test';

test.describe('Digital ID Visual Freeze', () => {
  test.beforeEach(async ({ page }) => {
    // Setup predictable visual state
    await page.goto('/');
    await page.evaluate(() => {
      localStorage.setItem('user_session', JSON.stringify({
        token: 'visual_test_token',
        role: 'Member',
        memberId: 200,
        username: 'visualtest'
      }));
    });
  });

  test('Identity: Member Digital ID Card should match baseline', async ({ page }) => {
    await page.goto('/portal/id-card');
    
    // Wait for the card to be fully rendered (including fonts and gradients)
    const card = page.locator('.credentials-container');
    await expect(card).toBeVisible();
    
    // Slight pause to ensure animations/gradients settle
    await page.waitForTimeout(1000);

    await expect(card).toHaveScreenshot('digital-id-card.png', {
      maxDiffPixelRatio: 0.01,
      threshold: 0.1,
    });
  });
});
