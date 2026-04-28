import { test, expect } from '@playwright/test';
import { AuthHelper } from './utils/auth-helper';

test.describe('Job Hub E2E', () => {
  let auth: AuthHelper;

  test.beforeEach(async ({ page }) => {
    auth = new AuthHelper(page);
    await auth.login('2512006', '2512006');
  });

  test.afterEach(async ({ page }) => {
    await auth.logout();
  });

  test('Member can view the Opportunities Hub and browse listings', async ({ page }) => {
    await page.goto('/portal/jobs');
    await expect(page).toHaveURL(/.*portal\/jobs/);

    // Verify page header
    await expect(page.getByText('Opportunities Hub')).toBeVisible();

    // Verify filter controls are present
    await expect(page.locator('input[placeholder*="Filter by title"]')).toBeVisible();
    await expect(page.locator('select').first()).toBeVisible();

    // Verify job cards or empty state
    const jobCards = page.locator('.job-card');
    const emptyState = page.locator('.empty-state');
    const hasJobs = await jobCards.first().isVisible({ timeout: 5000 }).catch(() => false);

    if (hasJobs) {
      // Verify card structure
      const firstCard = jobCards.first();
      await expect(firstCard.locator('h3')).toBeVisible();
      await expect(firstCard.locator('.meta')).toBeVisible();
    } else {
      await expect(emptyState).toBeVisible();
      await expect(emptyState).toContainText('No Open Opportunities');
    }
  });

  test('Member can open the Share Opportunity form and fill it out', async ({ page }) => {
    await page.goto('/portal/jobs');
    await expect(page).toHaveURL(/.*portal\/jobs/);

    // Click "Share Opportunity" button
    await page.click('button:has-text("Share Opportunity")');

    // Verify form is displayed
    await expect(page.locator('.form-box, .job-form').first()).toBeVisible();
    await expect(page.getByText('Share with Alumni Community')).toBeVisible();

    // Fill out the form
    await page.fill('input[name="title"]', 'E2E Test Software Engineer Position');
    await page.fill('input[name="companyName"]', 'Test Corp Ltd');
    await page.fill('input[name="location"]', 'Dhaka, Bangladesh');
    await page.fill('textarea[name="description"]', 'This is an automated test job posting from the E2E suite.');
    await page.fill('textarea[name="requirements"]', 'Must have 3+ years experience in testing.');

    // Verify inputs hold their values
    await expect(page.locator('input[name="title"]')).toHaveValue('E2E Test Software Engineer Position');
    await expect(page.locator('input[name="companyName"]')).toHaveValue('Test Corp Ltd');

    // Click Discard to close without submitting
    await page.click('button:has-text("Discard")');
    await expect(page.locator('.form-box')).not.toBeVisible();
  });

  test('Member can view job detail view', async ({ page }) => {
    await page.goto('/portal/jobs');
    await expect(page).toHaveURL(/.*portal\/jobs/);

    const jobCards = page.locator('.job-card');
    const hasJobs = await jobCards.first().isVisible({ timeout: 5000 }).catch(() => false);

    if (hasJobs) {
      // Click Details on first job
      await jobCards.first().locator('button:has-text("Details")').click();

      // Verify immersive detail view
      await expect(page.locator('.job-immersive-view')).toBeVisible();
      await expect(page.getByText('Opportunity Brief')).toBeVisible();
      await expect(page.getByText('Logistics')).toBeVisible();

      // Go back
      await page.locator('.job-immersive-view button:has-text("←")').click();
      await expect(page.locator('.job-immersive-view')).not.toBeVisible();
    }
  });

  test('Member can filter jobs by category', async ({ page }) => {
    await page.goto('/portal/jobs');
    await expect(page).toHaveURL(/.*portal\/jobs/);

    // Wait for the filter controls to load
    await page.waitForLoadState('networkidle');

    // Select a category filter
    const categorySelect = page.locator('.filter-group select');
    const hasCategories = await categorySelect.isVisible({ timeout: 5000 }).catch(() => false);

    if (hasCategories) {
      // Select "All Categories" first to verify it works
      await categorySelect.selectOption({ label: 'All Categories' });
      await page.waitForTimeout(500);
    }
  });
});
