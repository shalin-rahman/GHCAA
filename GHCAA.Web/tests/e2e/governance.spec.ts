import { test, expect } from '@playwright/test';
import { AuthHelper } from './utils/auth-helper';

test.describe('Governance E2E', () => {
  let auth: AuthHelper;

  test.beforeEach(async ({ page }) => {
    auth = new AuthHelper(page);
    await auth.login('2512006', '2512006');
  });

  test.afterEach(async ({ page }) => {
    await auth.logout();
  });

  test('Member can view the Executive Committee page', async ({ page }) => {
    await page.goto('/portal/governance');
    await expect(page).toHaveURL(/.*portal\/governance/);

    // Verify page header
    await expect(page.getByText('Executive Committee')).toBeVisible();
    await expect(page.getByText('Official leadership records of the association')).toBeVisible();

    await page.waitForLoadState('networkidle');

    // Verify committee cards or empty state
    const committeeCards = page.locator('.committee-card');
    const emptyState = page.locator('.empty-state');
    const hasCommittee = await committeeCards.first().isVisible({ timeout: 5000 }).catch(() => false);

    if (hasCommittee) {
      // Verify card structure
      const firstCard = committeeCards.first();
      await expect(firstCard.locator('h3')).toBeVisible();
      await expect(firstCard.locator('.position-badge')).toBeVisible();
    } else {
      await expect(emptyState).toContainText('No records found');
    }
  });

  test('Member can view Governance Pillars section', async ({ page }) => {
    await page.goto('/portal/governance');
    await page.waitForLoadState('networkidle');

    // Verify Governance Pillars (Constitution reference)
    await expect(page.getByText('Governance Pillars')).toBeVisible();
    await expect(page.getByText('Political Neutrality')).toBeVisible();
    await expect(page.getByText('Life-Long Connection')).toBeVisible();
    await expect(page.getByText('Transparency')).toBeVisible();
  });

  test('Member can switch EC periods when multiple exist', async ({ page }) => {
    await page.goto('/portal/governance');
    await page.waitForLoadState('networkidle');

    const periodSelector = page.locator('.period-select');
    const hasPeriods = await periodSelector.isVisible({ timeout: 5000 }).catch(() => false);

    if (hasPeriods) {
      // Get first option and verify selector works
      const options = periodSelector.locator('option');
      const optionCount = await options.count();
      expect(optionCount).toBeGreaterThan(0);
      
      // Select the first option explicitly
      await periodSelector.selectOption({ index: 0 });
      await page.waitForTimeout(500);

      // Committee section should still be visible
      await expect(page.locator('.committee-section')).toBeVisible();
    }
  });
});
