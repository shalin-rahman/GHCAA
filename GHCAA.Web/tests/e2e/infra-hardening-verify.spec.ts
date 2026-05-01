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
    
    // 3. Verify redirect to Dashboard
    // Dashboard route is typically /portal/dashboard
    await expect(page).toHaveURL(/.*portal\/dashboard/, { timeout: 10000 });
    
    // 4. Verify content (Checks that MemberService is activated and DI is working)
    // The dashboard title might change based on profile completion, but "Dashboard" is in breadcrumbs
    await expect(page.locator('.current-page, h1, h2')).toContainText(/Dashboard|Complete Your Profile/i);
    await expect(page.locator('body')).toContainText('Shalin Rahman');
    await expect(page.locator('body')).toContainText('GHC-0000000002');
    
    // 5. Verify Metrics (Checks that data seeding was successful)
    // We expect some non-zero stats or specific strings if seeding worked
    const stats = page.locator('.stat-card, .metric-card');
    await expect(stats.first()).toBeVisible();
  });
});
