import { test, expect } from '@playwright/test';
import { AuthHelper } from './utils/auth-helper';

test.describe('Admin Workflow - Member Approval', () => {
  test('Admin should be able to approve a pending member', async ({ page }) => {
    const auth = new AuthHelper(page);

    // 1. Login as Admin
    await auth.login('shalin', 'Shalin@2024!');

    // 2. Navigate to Member Approvals
    // Note: redirect for admin is already to /admin/approvals usually
    if (!page.url().includes('admin/approvals')) {
      await page.goto('/admin/approvals');
    }
    await expect(page).toHaveURL(/.*admin\/approvals/);

    // 3. Find and verify a pending member
    // Using a more robust selector for the row
    const pendingRow = page.locator('tr, .approval-row', { hasText: /Pending/i }).first();
    await expect(pendingRow).toBeVisible({ timeout: 10000 });

    // 4. Perform Approval
    const approveBtn = pendingRow.locator('button.btn-approve, button:has-text("Approve")');
    await approveBtn.click();
    
    // 5. Confirm via Modal if applicable
    const confirmBtn = page.locator('button:has-text("Confirm"), button:has-text("Yes")');
    if (await confirmBtn.isVisible({ timeout: 2000 }).catch(() => false)) {
        await confirmBtn.click();
    }

    // 6. Verify status update (row should likely disappear from approvals)
    await expect(pendingRow).not.toBeVisible({ timeout: 10000 });
    
    // 7. Verify in members list
    await page.goto('/admin/members');
    await expect(page).toHaveURL(/.*admin\/members/);
    
    // Check for the presence of the search input and find the approved member
    // This part is specific to what member was approved. 
    // If we don't know the name, we just verify the page loads.
  });
});
