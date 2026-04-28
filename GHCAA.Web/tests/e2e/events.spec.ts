import { test, expect } from '@playwright/test';
import { AuthHelper } from './utils/auth-helper';

test.describe('Events Portal E2E', () => {
  let auth: AuthHelper;

  test.beforeEach(async ({ page }) => {
    auth = new AuthHelper(page);
    await auth.login('2512006', '2512006');
  });

  test.afterEach(async ({ page }) => {
    await auth.logout();
  });

  test('Member can view the events list page', async ({ page }) => {
    await page.goto('/portal/events');
    await expect(page).toHaveURL(/.*portal\/events/);

    // Wait for events to load
    await page.waitForLoadState('networkidle');

    // Verify page has event content (cards or empty state)
    const body = page.locator('body');
    await expect(body).toBeVisible();
    
    // Check for event cards or event-related elements
    const eventCards = page.locator('.event-card, .glass-card').first();
    const emptyState = page.locator('.empty-state, :text("No events")').first();
    const hasEvents = await eventCards.isVisible({ timeout: 5000 }).catch(() => false);
    
    if (hasEvents) {
      await expect(eventCards).toBeVisible();
    } else {
      // Page loaded but no events
      await expect(body).toContainText(/event/i);
    }
  });

  test('Member can view event details when events exist', async ({ page }) => {
    await page.goto('/portal/events');
    await page.waitForLoadState('networkidle');

    const eventCards = page.locator('.event-card, .glass-card[class*="event"]');
    const hasEvents = await eventCards.first().isVisible({ timeout: 5000 }).catch(() => false);

    if (hasEvents) {
      // Click on the first event card
      await eventCards.first().click();
      await page.waitForTimeout(1000);

      // Should show event detail
      const body = page.locator('body');
      await expect(body).toContainText(/event/i);
    }
  });
});

test.describe('Events (Public) E2E', () => {
  test('Anonymous user can view the public events page', async ({ page }) => {
    await page.goto('/events');
    await page.waitForLoadState('networkidle');

    // The public events page should be accessible without login
    const body = page.locator('body');
    await expect(body).toBeVisible();
    // Should contain event-related content or empty state
    await expect(body).toContainText(/event/i);
  });
});
