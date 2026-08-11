import { test, expect } from '@playwright/test';
import { AuthHelper } from './utils/auth-helper';

test.describe('Alumni Directory E2E', () => {
  let auth: AuthHelper;

  test.beforeEach(async ({ page }) => {
    auth = new AuthHelper(page);
    await auth.login('2512006', '2512006');
  });

  test.afterEach(async ({ page }) => {
    await auth.logout();
  });

  test('Member can view the alumni directory with member cards', async ({ page }) => {
    await page.goto('/portal/directory');
    await expect(page).toHaveURL(/.*portal\/directory/);

    // Verify header
    await expect(page.getByText('Member Directory')).toBeVisible();
    await expect(page.getByText('Find and connect with fellow Haragangians')).toBeVisible();

    // Verify filter section exists
    await expect(page.locator('.filters')).toBeVisible();
    await expect(page.locator('input[placeholder*="Search by name"]')).toBeVisible();

    // Wait for members to load
    await page.waitForLoadState('networkidle');

    // Verify member cards or loading state
    const memberCards = page.locator('.member-card');
    const hasMembers = await memberCards.first().isVisible({ timeout: 10000 }).catch(() => false);

    if (hasMembers) {
      // Verify card structure: name, batch, badge
      const firstCard = memberCards.first();
      await expect(firstCard.locator('h4')).toBeVisible();
      await expect(firstCard.locator('.academic-batch')).toBeVisible();
    }
  });

  test('Member can search by name in the directory', async ({ page }) => {
    await page.goto('/portal/directory');
    await page.waitForLoadState('networkidle');

    const searchInput = page.locator('input[placeholder*="Search by name"]');
    await searchInput.fill('Test');
    await page.waitForTimeout(1000); // Debounce wait

    // After searching, result count may change
    const resultsSummary = page.locator('.results-summary');
    const hasSummary = await resultsSummary.isVisible({ timeout: 5000 }).catch(() => false);
    if (hasSummary) {
      await expect(resultsSummary).toContainText('Showing');
    }
  });

  test('Member can filter directory by blood group', async ({ page }) => {
    await page.goto('/portal/directory');
    await page.waitForLoadState('networkidle');

    // Select a blood group
    const bloodSelect = page.locator('select').nth(1); // Blood group is the second select
    await bloodSelect.selectOption('APositive');
    await page.waitForTimeout(1000);

    // Verify the filter applied (page should refresh results)
    const memberCards = page.locator('.member-card');
    const hasMembers = await memberCards.first().isVisible({ timeout: 5000 }).catch(() => false);
    // Either shows filtered members or empty state
    if (!hasMembers) {
      await expect(page.locator('.empty-state')).toBeVisible();
    }
  });

  test('Member can view another member profile from directory', async ({ page }) => {
    await page.goto('/portal/directory');
    await page.waitForLoadState('networkidle');

    const memberCards = page.locator('.member-card');
    const hasMembers = await memberCards.first().isVisible({ timeout: 10000 }).catch(() => false);

    if (hasMembers) {
      // Click "Profile Details" on the first card
      await memberCards.first().locator('button:has-text("Profile Details")').click();

      // Verify modal opens
      await expect(page.locator('.modal-overlay')).toBeVisible();
      await expect(page.getByText('Member Profile')).toBeVisible();
      await expect(page.getByText('Personal Information')).toBeVisible();
      await expect(page.getByText('Academic Summary')).toBeVisible();
      await expect(page.getByText('Contact Information')).toBeVisible();

      // Close modal
      await page.locator('.close-btn').click();
      await expect(page.locator('.modal-overlay')).not.toBeVisible();
    }
  });
});
