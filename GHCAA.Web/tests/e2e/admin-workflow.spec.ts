import { test, expect } from '@playwright/test';
import { AuthHelper } from './utils/auth-helper';
import { completeRegistrationPayment, getAuthToken } from './utils/api-helper';

test.describe('Admin Workflow - Member Approval', () => {
  test('Admin should be able to approve a pending member', async ({ page, request }) => {
    page.on('dialog', (dialog) => dialog.accept());

    const auth = new AuthHelper(page);
    const adminToken = await getAuthToken(request, 'superadmin', 'SuperAdminPassword123!');

    // Use the newest E2E registration (cash receipt leaves payment Pending until admin verifies)
    const membersRes = await request.get(
      'http://localhost:5087/api/admin/members?searchQuery=%40ghcaa.test&statusFilter=Applied&pageSize=1',
      { headers: { Authorization: `Bearer ${adminToken}` } }
    );
    const members = await membersRes.json();
    const targetEmail = members.items?.[0]?.email ?? members.items?.[0]?.Email;
    if (!targetEmail) {
      test.skip(true, 'No pending @ghcaa.test member — run full-membership workflow first');
    }
    await completeRegistrationPayment(request, adminToken, targetEmail);

    await auth.login('superadmin', 'SuperAdminPassword123!');

    // 2. Navigate to Member Approvals
    // Note: redirect for admin is already to /admin/approvals usually
    if (!page.url().includes('admin/approvals')) {
      await page.goto('/admin/approvals');
    }
    await expect(page).toHaveURL(/.*admin\/approvals/);

    // 3. Find and verify a pending member
    // Using a more robust selector for the row
    const pendingRow = page.locator('tbody tr', { hasText: targetEmail }).first();
    await expect(pendingRow).toBeVisible({ timeout: 10000 });

    // 4. Perform Approval (UI label is "Direct Verify")
    await pendingRow.locator('button:has-text("Direct Verify")').click();
    
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
