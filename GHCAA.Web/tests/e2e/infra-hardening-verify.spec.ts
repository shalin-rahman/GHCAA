import { test, expect } from '@playwright/test';

test.describe('Infrastructure Hardening Verification', () => {
  test('Should authenticate shalin with standard BCrypt hash and load dashboard', async ({ page }) => {
    // 1. Navigate to Login
    await page.goto('http://localhost:4200/login');
    
    // 2. Perform Login
    // Credentials: shalin / Shalin@2024!
    // These were applied via authoritative programmatic seeding in Program.cs
    await page.fill('input[name="username"]', 'shalin');
    await page.fill('input[name="password"]', 'Shalin@2024!');
    await page.click('button[type="submit"]');
    
    // shalin has Admin role — lands on admin portal, not member dashboard
    await expect(page).toHaveURL(/.*admin\/(dashboard|approvals)/, { timeout: 10000 });
    await expect(page.locator('body')).toContainText('shalin');
    await expect(page.getByRole('heading', { name: 'Approvals' })).toBeVisible();
  });
});
