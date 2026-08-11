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

test.describe('Admin Event Forms Visual Quality', () => {
  test.beforeEach(async ({ page }) => loginAsAdmin(page));

  test('Event Creation: Empty form state', async ({ page }) => {
    await page.goto('/admin/events');
    await page.waitForLoadState('networkidle');
    
    // Click 'Launch New Event'
    await page.click('button:has-text("Launch New Event")');
    
    // Wait for form to reveal
    await page.waitForSelector('.events-form', { state: 'visible' });
    await page.waitForTimeout(500);
    
    await expect(page).toHaveScreenshot('admin-event-create-empty.png', { fullPage: true });
  });

  test('Event Creation: Showing validation errors', async ({ page }) => {
    await page.goto('/admin/events');
    await page.waitForLoadState('networkidle');
    await page.click('button:has-text("Launch New Event")');
    await page.waitForSelector('.events-form', { state: 'visible' });

    // Try to submit empty
    await page.click('button[type="submit"]');
    
    await page.waitForTimeout(300);
    await expect(page).toHaveScreenshot('admin-event-create-errors.png', { fullPage: true });
  });

  test('Event Creation: Cross-field date validation error', async ({ page }) => {
    await page.goto('/admin/events');
    await page.waitForLoadState('networkidle');
    await page.click('button:has-text("Launch New Event")');
    await page.waitForSelector('.events-form', { state: 'visible' });

    // Fill start date
    await page.fill('input[formControlName="startDate"]', '2026-05-20T10:00');
    // Fill end date EARLIER than start date
    await page.fill('input[formControlName="endDate"]', '2026-05-19T10:00');
    
    // Trigger touched
    await page.focus('input[formControlName="title"]');
    await page.click('body'); 

    await page.waitForTimeout(500);
    await expect(page).toHaveScreenshot('admin-event-date-validation-error.png', { fullPage: true });
  });
  
  test('Event Creation: Multi-step participation registration form', async ({ page }) => {
      // This is for the public/member registration for an event
      await page.goto('/events');
      await page.waitForLoadState('networkidle');
      
      // Click 'Register' on the first available event card if any
      const registerBtn = page.locator('button:has-text("Register")').first();
      if (await registerBtn.isVisible()) {
          await registerBtn.click();
          await page.waitForSelector('.registration-form-modal', { state: 'visible' });
          await page.waitForTimeout(500);
          await expect(page).toHaveScreenshot('event-registration-modal.png');
      }
  });
});
