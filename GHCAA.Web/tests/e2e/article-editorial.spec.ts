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

    // Open article form
    await page.click('button[aria-label*="New"], button:has-text("Write Article"), button:has-text("New")');
    await page.fill('input[formControlName="title"]', testArticleTitle);
    await page.fill('textarea[formControlName="body"], [formControlName="content"], .editor-content',
      'This is an automated test article submitted via Playwright E2E test suite.');

    await page.click('button[type="submit"], button:has-text("Submit")');
    
    // Wait for success toast or redirection
    await expect(page.locator('.success-toast, .alert-success, text=/success/i').first()).toBeVisible({ timeout: 10000 });

    await auth.logout();

    // ==== STEP 2: Admin approves article ====
    await auth.login('shalin', 'Shalin@2024!');
    await page.goto('/admin/article-approvals');
    await expect(page).toHaveURL(/.*admin\/article-approvals/);

    const articleRow = page.locator('tr, .article-card, .approval-item', { hasText: testArticleTitle });
    await expect(articleRow).toBeVisible({ timeout: 10000 });
    
    await articleRow.locator('button.btn-approve, button:has-text("Approve")').click();
    
    // Confirm modal if present
    const confirmBtn = page.locator('button:has-text("Confirm"), button:has-text("Yes")');
    if (await confirmBtn.isVisible({ timeout: 2000 }).catch(() => false)) {
      await confirmBtn.click();
    }
    
    await expect(articleRow).not.toBeVisible({ timeout: 10000 });

    // ==== STEP 3: Verify article is published in News ====
    await page.goto('/news');
    await page.waitForLoadState('networkidle');
    await expect(page.locator('body')).toContainText(testArticleTitle);
  });
});
