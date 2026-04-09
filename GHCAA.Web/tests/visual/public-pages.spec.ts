import { test, expect } from '@playwright/test';

// Public pages – no auth needed
test.describe('Public Pages Visual Freeze', () => {
  test('Public: Landing page should match baseline', async ({ page }) => {
    await page.goto('/');
    await expect(page.locator('body')).toBeVisible();
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(1000);
    await expect(page).toHaveScreenshot('public-landing.png', { fullPage: true });
  });

  test('Public: About page should match baseline', async ({ page }) => {
    await page.goto('/about');
    await page.waitForLoadState('networkidle');
    await expect(page).toHaveScreenshot('public-about.png', { fullPage: true });
  });

  test('Public: Contact page should match baseline', async ({ page }) => {
    await page.goto('/contact');
    await page.waitForLoadState('networkidle');
    await expect(page).toHaveScreenshot('public-contact.png', { fullPage: true });
  });

  test('Public: Login page should match baseline', async ({ page }) => {
    await page.goto('/login');
    await page.waitForLoadState('networkidle');
    await expect(page).toHaveScreenshot('public-login.png', { fullPage: true });
  });

  test('Public: Registration page should match baseline', async ({ page }) => {
    await page.goto('/register');
    await page.waitForLoadState('networkidle');
    await expect(page).toHaveScreenshot('public-register.png', { fullPage: true });
  });
});
