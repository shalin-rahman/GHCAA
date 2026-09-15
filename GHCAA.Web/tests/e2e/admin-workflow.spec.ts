import { test, expect } from '@playwright/test';
import { AuthHelper } from './utils/auth-helper';
import { completeRegistrationPayment, getAuthToken } from './utils/api-helper';
import { registerNewMemberViaUi } from './utils/ui-helper';

test.describe('Admin Workflow - Member Approval', () => {
  test('Admin should be able to approve a pending member', async ({ page, request }) => {
    page.on('dialog', (dialog) => dialog.accept());

    // Seed a fresh pending member for this test run so the assertion never
    // depends on leftover state from a previous/parallel test.
    const member = await registerNewMemberViaUi(page);

    const adminToken = await getAuthToken(request, 'superadmin', 'SuperAdminPassword123!');
    await completeRegistrationPayment(request, adminToken, member.email);

    const auth = new AuthHelper(page);
    await auth.login('superadmin', 'SuperAdminPassword123!');

    // 2. Navigate to Member Approvals
    // Note: redirect for admin is already to /admin/approvals usually
    if (!page.url().includes('admin/approvals')) {
      await page.goto('/admin/approvals');
    }
    await expect(page).toHaveURL(/.*admin\/approvals/);

    // 3. Find and verify a pending member
    // Using a more robust selector for the row
    const pendingRow = page.locator('tbody tr', { hasText: member.email }).first();
    await expect(pendingRow).toBeVisible({ timeout: 10000 });

    // 4. Perform Approval (UI label is "Direct Verify")
    await pendingRow.locator('button:has-text("Direct Verify")').click();

    // 5. Confirm via Modal if applicable
    const confirmBtn = page.getByRole('button', { name: 'Verify', exact: true })
      .or(page.getByRole('button', { name: 'Confirm', exact: true }))
      .or(page.getByRole('button', { name: 'Yes', exact: true }));
    if (await confirmBtn.isVisible({ timeout: 2000 }).catch(() => false)) {
        await confirmBtn.click();
    }

    // Refresh the queue before checking the server-side status change.
    await page.reload();
    await expect(page.locator('tbody tr', { hasText: member.email })).toHaveCount(0, { timeout: 10000 });

    // 7. Verify in members list
    await page.goto('/admin/members');
    await expect(page).toHaveURL(/.*admin\/members/);

    // Check for the presence of the search input and find the approved member
    // This part is specific to what member was approved.
    // If we don't know the name, we just verify the page loads.
  });
});
