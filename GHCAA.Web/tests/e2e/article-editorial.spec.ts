import { test, expect } from '@playwright/test';

test.describe('Article Editorial E2E (Web)', () => {
  test('Member submits article, Admin approves, it appears in News', async ({ page }) => {
    // ==== STEP 1: Member submits article ====
    await page.goto('/login');
    await page.fill('input[formControlName="username"]', 'demo_user@test.com');
    await page.fill('input[formControlName="password"]', 'DemoPass123!');
    await page.click('button[type="submit"]');
    await expect(page).toHaveURL(/.*portal\/dashboard/);

    await page.goto('/portal/articles');
    await expect(page.locator('body')).toBeVisible();

    // Open article form
    await page.click('button[aria-label="New Article"], button:has-text("Write Article"), button:has-text("New")');
    await page.fill('input[formControlName="title"]', 'E2E Automated Article');
    await page.fill('textarea[formControlName="body"], [formControlName="content"]',
      'This is an automated test article submitted via Playwright E2E test suite.');

    await page.click('button[type="submit"], button:has-text("Submit")');
    await expect(page.locator('.success-toast, .alert-success')).toBeVisible({ timeout: 5000 });

    // ==== STEP 2: Logout & login as Admin ====
    await page.evaluate(() => localStorage.clear());
    await page.goto('/login');
    await page.fill('input[formControlName="username"]', 'superadmin');
    await page.fill('input[formControlName="password"]', 'SuperAdminPassword123!');
    await page.click('button[type="submit"]');
    await expect(page).toHaveURL(/.*admin\/dashboard/);

    // ==== STEP 3: Admin approves article ====
    await page.goto('/admin/article-approvals');
    const articleRow = page.locator('tr, .article-card', { hasText: 'E2E Automated Article' });
    await expect(articleRow).toBeVisible({ timeout: 5000 });
    await articleRow.locator('button.btn-approve, button:has-text("Approve")').click();
    // Confirm modal if present
    const confirmBtn = page.locator('button:has-text("Confirm"), button:has-text("Yes")');
    if (await confirmBtn.isVisible({ timeout: 1000 }).catch(() => false)) {
      await confirmBtn.click();
    }
    await expect(articleRow).not.toBeVisible({ timeout: 5000 });

    // ==== STEP 4: Verify article is published in News ====
    await page.goto('/news');
    await page.waitForLoadState('networkidle');
    await expect(page.locator('body')).toContainText('E2E Automated Article');
  });
});
