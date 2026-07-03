import { test, expect } from '@playwright/test';
import { AuthHelper } from './utils/auth-helper';

test.describe('Article Editorial E2E (Web)', () => {
  test('Member submits article, Admin approves, it appears in News', async ({ page }) => {
    const auth = new AuthHelper(page);
    const testArticleTitle = `E2E Automated Article - ${Date.now()}`;

    // ==== STEP 1: Member submits article ====
    // Using a regular member for feature testing as per latest instructions
    await auth.login('2512006', '2512006');
    
    await page.goto('/portal/articles');
    await expect(page).toHaveURL(/.*portal\/articles/);

    await page.click('button:has-text("Write New Article")');
    await page.fill('input[placeholder*="Enter a catchy title"]', testArticleTitle);
    await page.fill('textarea[placeholder*="Write your story"]',
      'This is an automated test article submitted via Playwright E2E test suite.');

    await Promise.all([
      page.waitForResponse(
        (r) => r.url().includes('/api/news/submit') && r.status() < 400,
        { timeout: 15000 }
      ),
      page.getByRole('button', { name: /Submit for Approval/i }).click(),
    ]);
    await expect(page.getByRole('heading', { name: 'Articles & Submissions' })).toBeVisible({ timeout: 10000 });

    await auth.logout();

    // ==== STEP 2: Admin approves article ====
    await auth.login('superadmin', 'SuperAdminPassword123!');
    await page.goto('/admin/article-approvals');
    await expect(page).toHaveURL(/.*admin\/article-approvals/);

    const articleRow = page.locator('tbody tr', { hasText: testArticleTitle });
    await expect(articleRow).toBeVisible({ timeout: 10000 });
    await articleRow.locator('button:has-text("Review Article")').click();
    await page.locator('button:has-text("Approve & Publish")').click();
    await expect(articleRow).not.toBeVisible({ timeout: 10000 });

    // ==== STEP 3: Verify article is published in News ====
    await page.goto('/news');
    await page.waitForLoadState('networkidle');
    await expect(page.locator('body')).toContainText(testArticleTitle);
  });
});
