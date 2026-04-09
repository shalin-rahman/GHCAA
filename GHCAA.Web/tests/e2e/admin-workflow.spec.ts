import { test, expect } from '@playwright/test';

test.describe('Admin Workflow - Member Approval', () => {
  test('Admin should be able to approve a pending member', async ({ page }) => {
    // 1. Login as SuperAdmin
    await page.goto('/login');
    await page.fill('input[formControlName="username"]', 'superadmin');
    await page.fill('input[formControlName="password"]', 'SuperAdminPassword123!');
    await page.click('button[type="submit"]');

    // 2. Navigate to Member Approvals
    await expect(page).toHaveURL(/.*admin\/dashboard/);
    await page.click('text=Approvals');
    await expect(page).toHaveURL(/.*admin\/approvals/);

    // 3. Find and verify the pending member from Seed Data
    const pendingRow = page.locator('tr', { hasText: 'Pending Alumnus' });
    await expect(pendingRow).toBeVisible();
    await expect(pendingRow).toContainText('Pending');

    // 4. Perform Approval
    await pendingRow.locator('button.btn-approve').click();
    
    // 5. Confirm via Modal if applicable (Checking component logic)
    // Assuming a confirmation modal opens
    const confirmBtn = page.locator('button:has-text("Confirm")');
    if (await confirmBtn.isVisible()) {
        await confirmBtn.click();
    }

    // 6. Verify status update
    // Depending on UI logic, the row might disappear or show 'Active'
    await expect(pendingRow).not.toBeVisible();
    
    // 7. Verify in members list
    await page.click('text=Alumni Registry'); // Or 'Members'
    await expect(page).toHaveURL(/.*admin\/members/);
    await page.fill('input[placeholder*="Search"]', 'Pending Alumnus');
    await expect(page.locator('tr', { hasText: 'Pending Alumnus' })).toContainText('Active');
  });
});
