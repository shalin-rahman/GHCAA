import { test, expect } from '@playwright/test';
import { AuthHelper } from './utils/auth-helper';

test.describe('Gallery E2E', () => {
  let auth: AuthHelper;

  test.beforeEach(async ({ page }) => {
    auth = new AuthHelper(page);
    await auth.login('2512006', '2512006');
  });

  test.afterEach(async ({ page }) => {
    await auth.logout();
  });

  test('Member can view the gallery page with album cards', async ({ page }) => {
    await page.goto('/portal/dashboard');
    await page.goto('/portal/gallery');
    await expect(page).toHaveURL(/.*portal\/gallery/);
    await page.waitForLoadState('networkidle');

    await expect(page.getByRole('heading', { name: 'Legacy Archive & Moments' })).toBeVisible({ timeout: 15000 });
    await expect(page.getByText('Visual journey through Haragangian heritage')).toBeVisible();

    // Verify Share button is visible for authenticated user
    await expect(page.locator('button:has-text("Share Legacy Moment")')).toBeVisible();

    await page.waitForLoadState('networkidle');

    // Verify gallery cards or empty state
    const galleryCards = page.locator('.gallery-card');
    const emptyState = page.locator('.empty-state');
    const hasGalleries = await galleryCards.first().isVisible({ timeout: 5000 }).catch(() => false);

    if (hasGalleries) {
      const firstCard = galleryCards.first();
      await expect(firstCard.locator('h3')).toBeVisible();
      await expect(firstCard.locator('.photo-count')).toBeVisible();
    } else {
      await expect(emptyState).toContainText('No event galleries');
    }
  });

  test('Member can open the upload form and discard it', async ({ page }) => {
    await page.goto('/portal/gallery');
    await page.waitForLoadState('networkidle');

    // Open upload form
    await page.click('button:has-text("Share Legacy Moment")');
    await expect(page.locator('.upload-box')).toBeVisible();
    await expect(page.getByText('Share with Alumni Community')).toBeVisible();

    // Fill title
    await page.fill('input[name="title"]', 'E2E Test Album');
    await expect(page.locator('input[name="title"]')).toHaveValue('E2E Test Album');

    // Discard
    await page.click('button:has-text("Discard")');
    await expect(page.locator('.upload-box')).not.toBeVisible();
  });

  test('Member can drill into a gallery album when galleries exist', async ({ page }) => {
    await page.goto('/portal/gallery');
    await page.waitForLoadState('networkidle');

    const galleryCards = page.locator('.gallery-card');
    const hasGalleries = await galleryCards.first().isVisible({ timeout: 5000 }).catch(() => false);

    if (hasGalleries) {
      // Click into first gallery
      await galleryCards.first().click();

      // Verify detail view
      await expect(page.locator('.gallery-detail-container')).toBeVisible();
      await expect(page.locator('.photo-grid-immersive, .glass-card')).toBeVisible();

      // Go back
      await page.locator('button:has-text("←")').click();
      await expect(page.locator('.gallery-detail-container')).not.toBeVisible();
    }
  });
});
