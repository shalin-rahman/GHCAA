import { test, expect } from '@playwright/test';
import { AuthHelper } from './utils/auth-helper';

test.describe('Member End-to-End Journey', () => {
  test('A member should be able to login and view their profile', async ({ page }) => {
    const auth = new AuthHelper(page);
    
    // 1. Perform Login with a non-admin member
    // Using NID for both username and password as per latest instructions
    await auth.login('2512006', '2512006');

    // 2. Verify redirect to Dashboard
    await expect(page).toHaveURL(/.*portal\/dashboard/);
    
    // 3. Verify Dashboard content & Metrics
    // User is identified by NID/MembershipNumber if FullName is missing in seed
    await expect(page.locator('body')).toContainText('2512006');
    
    // Check for metrics cards (observed values for 2512006)
    await expect(page.locator('.stat-card', { hasText: /Profile Complete/i })).toContainText('84.62%');
    await expect(page.locator('.stat-card', { hasText: /Upcoming Events/i })).toContainText('2');

    // 4. Navigate to Digital ID
    await page.click('text=Digital ID');
    await expect(page).toHaveURL(/.*portal\/id-card/);
    const idCard = page.locator('.id-card-container, .digital-id-card, app-digital-id');
    await expect(idCard.first()).toBeVisible();

    // 5. Logout
    await auth.logout();
    await expect(page).toHaveURL(/.*login/);
  });
});
