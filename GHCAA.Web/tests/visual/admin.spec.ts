import { test, expect, Page } from '@playwright/test';

async function loginAsAdmin(page: Page) {
  await page.goto('/');
  await page.evaluate(() => {
    localStorage.setItem('user_session', JSON.stringify({
      token: 'visual_admin_token',
      role: 'SuperAdmin',
      memberId: 1,
      username: 'superadmin'
    }));
  });
}

test.describe('Admin Panels Visual Freeze', () => {
  test.beforeEach(async ({ page }) => loginAsAdmin(page));

  test('Admin: Dashboard overview should match baseline', async ({ page }) => {
    await page.goto('/admin/dashboard');
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(1000);
    await expect(page).toHaveScreenshot('admin-dashboard.png', {
      fullPage: true,
      mask: [page.locator('.dynamic-date'), page.locator('.live-count')],
    });
  });

  test('Admin: Member management table should match baseline', async ({ page }) => {
    await page.goto('/admin/members');
    await page.waitForLoadState('networkidle');
    await page.waitForSelector('table tbody tr', { state: 'visible' });
    await page.waitForTimeout(800);
    await expect(page).toHaveScreenshot('admin-members-table.png', { fullPage: true });
  });

  test('Admin: Approval queue should match baseline', async ({ page }) => {
    await page.goto('/admin/approvals');
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(800);
    await expect(page).toHaveScreenshot('admin-approvals.png', { fullPage: true });
  });

  test('Admin: News moderation panel should match baseline', async ({ page }) => {
    await page.goto('/admin/news');
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(800);
    await expect(page).toHaveScreenshot('admin-news.png', { fullPage: true });
  });

  test('Admin: Events management panel should match baseline', async ({ page }) => {
    await page.goto('/admin/events');
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(800);
    await expect(page).toHaveScreenshot('admin-events.png', { fullPage: true });
  });

  test('Admin: EC Committee management should match baseline', async ({ page }) => {
    await page.goto('/admin/members/ec');
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(800);
    await expect(page).toHaveScreenshot('admin-ec-management.png', { fullPage: true });
  });

  test('Admin: Gallery management should match baseline', async ({ page }) => {
    await page.goto('/admin/gallery');
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(800);
    await expect(page).toHaveScreenshot('admin-gallery.png', { fullPage: true });
  });

  test('Admin: Article approvals should match baseline', async ({ page }) => {
    await page.goto('/admin/article-approvals');
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(800);
    await expect(page).toHaveScreenshot('admin-article-approvals.png', { fullPage: true });
  });

  test('Admin: Contact messages should match baseline', async ({ page }) => {
    await page.goto('/admin/contact-messages');
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(800);
    await expect(page).toHaveScreenshot('admin-contact-messages.png', { fullPage: true });
  });
});

test.describe('SuperAdmin Panels Visual Freeze', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/');
    await page.evaluate(() => {
      localStorage.setItem('user_session', JSON.stringify({
        token: 'visual_superadmin_token',
        role: 'SuperAdmin',
        memberId: 1,
        username: 'superadmin'
      }));
    });
  });

  test('SuperAdmin: Ledger view should match baseline', async ({ page }) => {
    await page.goto('/admin/ledger');
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(800);
    await expect(page).toHaveScreenshot('admin-ledger.png', {
      fullPage: true,
      mask: [page.locator('.transaction-date')],
    });
  });

  test('SuperAdmin: Fee configuration should match baseline', async ({ page }) => {
    await page.goto('/admin/payments/fees');
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(800);
    await expect(page).toHaveScreenshot('admin-fee-config.png', { fullPage: true });
  });

  test('SuperAdmin: Role management should match baseline', async ({ page }) => {
    test.setTimeout(60000);
    await page.goto('/admin/roles');
    await page.waitForLoadState('networkidle');
    await page.waitForSelector('.data-table', { state: 'visible' });
    await page.waitForTimeout(1000);
    await expect(page).toHaveScreenshot('admin-roles.png', { fullPage: true, timeout: 30000 });
  });

  test('SuperAdmin: Audit log should match baseline', async ({ page }) => {
    test.setTimeout(60000);
    await page.goto('/admin/audit');
    await page.waitForLoadState('networkidle');
    await page.waitForSelector('.audit-timeline', { state: 'visible' });
    await page.waitForTimeout(1000);
    await expect(page).toHaveScreenshot('admin-audit-log.png', {
      fullPage: true,
      mask: [page.locator('.log-timestamp')],
      timeout: 30000
    });
  });
});
