import { test, expect } from '@playwright/test';

test.describe('Digital ID Visual Freeze', () => {
  test.beforeEach(async ({ page }) => {
    // Setup predictable visual state
    await page.goto('/');
    await page.evaluate(() => {
      localStorage.setItem('jwt_token', 'visual_test_token');
      localStorage.setItem('user_role', 'Member');
      localStorage.setItem('user_id', '1001');
    });
  });

  test('Identity: Member Digital ID Card should match baseline', async ({ page }) => {
    await page.goto('/portal/id-card');
    
    // Wait for the card to be fully rendered (including fonts and gradients)
    const card = page.locator('.id-card-container');
    await expect(card).toBeVisible();
    
    // Slight pause to ensure animations/gradients settle
    await page.waitForTimeout(1000);

    await expect(card).toHaveScreenshot('digital-id-card.png', {
      maxDiffPixelRatio: 0.01,
      threshold: 0.1,
    });
  });
});
