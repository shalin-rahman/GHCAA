import { test, expect } from '@playwright/test';

test.describe('Member End-to-End Journey', () => {
  test('A member should be able to login and view their profile', async ({ page }) => {
    // 1. Visit Login Page
    await page.goto('/login');
    await expect(page).toHaveTitle(/Login/);

    // 2. Perform Login
    // Note: Using Shalin Rahman credentials from Seed Data
    await page.fill('input[formControlName="username"]', 'demo_user@test.com');
    await page.fill('input[formControlName="password"]', 'DemoPass123!');
    await page.click('button[type="submit"]');

    // 3. Verify Dashboard Redirection
    await expect(page).toHaveURL(/.*portal\/dashboard/);
    await expect(page.locator('h1')).toContainText(/Welcome/i);
    await expect(page.locator('body')).toContainText('Shalin Rahman');

    // 4. Navigate to Digital ID
    await page.click('text=Digital ID');
    await expect(page).toHaveURL(/.*portal\/id-card/);
    await expect(page.locator('.id-card-container')).toBeVisible();

    // 5. Logout
    await page.click('.profile-dropdown');
    await page.click('text=Logout');
    await expect(page).toHaveURL(/.*login/);
  });
});
