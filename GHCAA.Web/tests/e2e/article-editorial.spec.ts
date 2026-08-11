import { test, expect } from '@playwright/test';
import { AuthHelper } from './utils/auth-helper';
import { approveMember, getAuthToken } from './utils/api-helper';
import { registerNewMemberViaUi, NewMember } from './utils/ui-helper';

test.describe('Article Editorial E2E (Web)', () => {
  let member: NewMember;

  // Own freshly seeded member instead of the hardcoded '2512006' shared with
  // gallery/job-hub specs, so parallel workers never race over one session.
  test.beforeAll(async ({ browser, request }) => {
    const setupPage = await browser.newPage();
    member = await registerNewMemberViaUi(setupPage);
    await setupPage.close();

    const adminToken = await getAuthToken(request, 'superadmin', 'SuperAdminPassword123!');
    await approveMember(request, adminToken, member.email);
  });

  test('Member submits article, Admin approves, it appears in News', async ({ page }) => {
    const auth = new AuthHelper(page);
    const testArticleTitle = `E2E Automated Article - ${Date.now()}`;

    // ==== STEP 1: Member submits article ====
    await auth.login(member.mobile, member.nid);

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
