import { test, expect } from '@playwright/test';
import { AuthHelper } from './utils/auth-helper';

test.describe('Member Profile E2E', () => {
  let auth: AuthHelper;

  test.beforeEach(async ({ page }) => {
    auth = new AuthHelper(page);
    await auth.login('2512006', '2512006');
  });

  test.afterEach(async ({ page }) => {
    await auth.logout();
  });

  test('Member can view their full profile page with membership status card', async ({ page }) => {
    await page.goto('/portal/profile');
    await expect(page).toHaveURL(/.*portal\/profile/);

    // Verify page header
    await expect(page.getByText('Member Dashboard')).toBeVisible();
    await expect(page.getByText('Manage your association identity and professional details')).toBeVisible();

    await page.waitForLoadState('networkidle');

    // Verify membership status card is displayed
    await expect(page.locator('.membership-status-card')).toBeVisible();
    
    // Verify status grid metrics
    await expect(page.getByText('Global Rank')).toBeVisible();
    await expect(page.getByText('Profile Health')).toBeVisible();
    await expect(page.getByText('Associated Batch')).toBeVisible();
    await expect(page.getByText('Member Since')).toBeVisible();
  });

  test('Member can see profile form sections with correct labels', async ({ page }) => {
    await page.goto('/portal/profile');
    await page.waitForLoadState('networkidle');

    // Verify all form sections are present
    await expect(page.getByText('Secure Identity Details')).toBeVisible();
    await expect(page.getByText('Educational Timeline')).toBeVisible();
    await expect(page.getByText('Professional Experience')).toBeVisible();
    await expect(page.getByText('Profile Photo')).toBeVisible();

    // Verify form fields exist
    await expect(page.locator('input[name="fullName"]')).toBeVisible();
    await expect(page.locator('input[name="nid"]')).toBeVisible();
    
    // NID should be readonly
    await expect(page.locator('input[name="nid"]')).toHaveAttribute('readonly', '');
  });

  test('Member can view privacy controls section', async ({ page }) => {
    await page.goto('/portal/profile');
    await page.waitForLoadState('networkidle');

    // Scroll to privacy section
    const privacySection = page.getByText('Directory Privacy Controls');
    await privacySection.scrollIntoViewIfNeeded();
    await expect(privacySection).toBeVisible();

    // Verify privacy toggles
    await expect(page.getByText('Expose Email to Alumni Community')).toBeVisible();
    await expect(page.getByText('Expose Mobile Identity to Alumni Community')).toBeVisible();
    await expect(page.getByText('Expose Residential Data to Alumni Community')).toBeVisible();
  });

  test('Member can view notification preferences section', async ({ page }) => {
    await page.goto('/portal/profile');
    await page.waitForLoadState('networkidle');

    // Scroll to notifications section
    const notifSection = page.getByText('Communication Preferences');
    await notifSection.scrollIntoViewIfNeeded();
    await expect(notifSection).toBeVisible();

    await expect(page.getByText('Receive New Event Announcements')).toBeVisible();
    await expect(page.getByText('Event Participation Approvals')).toBeVisible();
  });

  test('Member can see the Update Registry button', async ({ page }) => {
    await page.goto('/portal/profile');
    await page.waitForLoadState('networkidle');

    const updateBtn = page.locator('button:has-text("Update Registry Information")');
    await updateBtn.scrollIntoViewIfNeeded();
    await expect(updateBtn).toBeVisible();
  });
});
