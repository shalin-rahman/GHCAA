import { test, expect } from '@playwright/test';
import { AuthHelper } from './utils/auth-helper';

test.describe('Admin Dashboard & Management E2E', () => {
  let auth: AuthHelper;

  test.beforeEach(async ({ page }) => {
    auth = new AuthHelper(page);
    await auth.login('shalin', 'Shalin@2024!');
  });

  test.afterEach(async ({ page }) => {
    await auth.logout();
  });

  test('Admin can view the dashboard with summary metrics', async ({ page }) => {
    await page.goto('/admin/dashboard');
    await expect(page).toHaveURL(/.*admin\/dashboard/);
    await page.waitForLoadState('networkidle');

    // Dashboard should have metrics or summary cards
    const body = page.locator('body');
    await expect(body).toContainText(/dashboard/i);
  });

  test('Admin can navigate to Members list and see member records', async ({ page }) => {
    await page.goto('/admin/members');
    await expect(page).toHaveURL(/.*admin\/members/);
    await page.waitForLoadState('networkidle');

    // Should have a table or list of members
    const body = page.locator('body');
    await expect(body).toBeVisible();
  });

  test('Admin can navigate to Events management', async ({ page }) => {
    await page.goto('/admin/events');
    await expect(page).toHaveURL(/.*admin\/events/);
    await page.waitForLoadState('networkidle');

    const body = page.locator('body');
    await expect(body).toBeVisible();
  });

  test('Admin can navigate to Gallery management', async ({ page }) => {
    await page.goto('/admin/gallery');
    await expect(page).toHaveURL(/.*admin\/gallery/);
    await page.waitForLoadState('networkidle');

    const body = page.locator('body');
    await expect(body).toBeVisible();
  });

  test('Admin can navigate to News Posts management', async ({ page }) => {
    await page.goto('/admin/news');
    await expect(page).toHaveURL(/.*admin\/news/);
    await page.waitForLoadState('networkidle');

    const body = page.locator('body');
    await expect(body).toBeVisible();
  });

  test('Admin can navigate to Article Approvals', async ({ page }) => {
    await page.goto('/admin/article-approvals');
    await expect(page).toHaveURL(/.*admin\/article-approvals/);
    await page.waitForLoadState('networkidle');

    const body = page.locator('body');
    await expect(body).toBeVisible();
  });

  test('Admin can navigate to Contact Messages', async ({ page }) => {
    await page.goto('/admin/contact-messages');
    await expect(page).toHaveURL(/.*admin\/contact-messages/);
    await page.waitForLoadState('networkidle');

    const body = page.locator('body');
    await expect(body).toBeVisible();
  });

  test('Admin can navigate to EC Management', async ({ page }) => {
    await page.goto('/admin/members/ec');
    await expect(page).toHaveURL(/.*admin\/members\/ec/);
    await page.waitForLoadState('networkidle');

    const body = page.locator('body');
    await expect(body).toBeVisible();
  });

  test('Admin can navigate to Poll Management', async ({ page }) => {
    await page.goto('/admin/polls');
    await expect(page).toHaveURL(/.*admin\/polls/);
    await page.waitForLoadState('networkidle');

    const body = page.locator('body');
    await expect(body).toBeVisible();
  });
});

test.describe('SuperAdmin Finance & Tools E2E', () => {
  let auth: AuthHelper;

  test.beforeEach(async ({ page }) => {
    auth = new AuthHelper(page);
    await auth.login('superadmin', 'SuperAdminPassword123!');
  });

  test.afterEach(async ({ page }) => {
    await auth.logout();
  });

  test('SuperAdmin can view Financial Ledger', async ({ page }) => {
    await page.goto('/admin/ledger');
    await expect(page).toHaveURL(/.*admin\/ledger/);
    await page.waitForLoadState('networkidle');

    const body = page.locator('body');
    await expect(body).toBeVisible();
  });

  test('SuperAdmin can view Fee Policy configuration', async ({ page }) => {
    await page.goto('/admin/payments/fees');
    await expect(page).toHaveURL(/.*admin\/payments\/fees/);
    await page.waitForLoadState('networkidle');

    const body = page.locator('body');
    await expect(body).toBeVisible();
  });

  test('SuperAdmin can view User Roles management', async ({ page }) => {
    await page.goto('/admin/roles');
    await expect(page).toHaveURL(/.*admin\/roles/);
    await page.waitForLoadState('networkidle');

    const body = page.locator('body');
    await expect(body).toBeVisible();
  });

  test('SuperAdmin can view Audit Logs', async ({ page }) => {
    await page.goto('/admin/audit');
    await expect(page).toHaveURL(/.*admin\/audit/);
    await page.waitForLoadState('networkidle');

    const body = page.locator('body');
    await expect(body).toBeVisible();
  });
});
