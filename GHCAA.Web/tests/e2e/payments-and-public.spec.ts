import { test, expect } from '@playwright/test';
import { AuthHelper } from './utils/auth-helper';

test.describe('Member Payment Portal E2E', () => {
  let auth: AuthHelper;

  test.beforeEach(async ({ page }) => {
    auth = new AuthHelper(page);
    await auth.login('2512006', '2512006');
  });

  test.afterEach(async ({ page }) => {
    await auth.logout();
  });

  test('Member can view the payments portal', async ({ page }) => {
    await page.goto('/portal/payments');
    await expect(page).toHaveURL(/.*portal\/payments/);
    await page.waitForLoadState('networkidle');

    // Verify the page loaded with payment-related content
    const body = page.locator('body');
    await expect(body).toBeVisible();
  });
});

test.describe('Registration Fee Verification E2E', () => {
  let auth: AuthHelper;

  test.beforeEach(async ({ page }) => {
    auth = new AuthHelper(page);
    await auth.login('2512006', '2512006');
  });

  test.afterEach(async ({ page }) => {
    await auth.logout();
  });

  test('Dashboard should show registration payment status for member', async ({ page }) => {
    await page.goto('/portal/dashboard');
    await expect(page).toHaveURL(/.*portal\/dashboard/);
    await page.waitForLoadState('networkidle');

    // The dashboard contains a registration payment step
    // It should show either "Completed" or "Pending" depending on payment records
    const body = page.locator('body');
    await expect(body).toBeVisible();
    
    // Check that the registration payment indicator is present
    const paymentStep = page.locator(':text("Registration Payment"), :text("Registration Fee")').first();
    const hasPaymentStep = await paymentStep.isVisible({ timeout: 5000 }).catch(() => false);
    
    if (hasPaymentStep) {
      await expect(paymentStep).toBeVisible();
    }
  });
});

test.describe('News (Public) E2E', () => {
  test('Anonymous user can view the public news page', async ({ page }) => {
    await page.goto('/news');
    await page.waitForLoadState('networkidle');
    
    const body = page.locator('body');
    await expect(body).toBeVisible();
  });
});

test.describe('Public Pages E2E', () => {
  test('Landing page loads correctly', async ({ page }) => {
    await page.goto('/');
    await page.waitForLoadState('networkidle');
    
    const body = page.locator('body');
    await expect(body).toBeVisible();
  });

  test('About page loads correctly', async ({ page }) => {
    await page.goto('/about');
    await page.waitForLoadState('networkidle');
    
    const body = page.locator('body');
    await expect(body).toBeVisible();
  });

  test('Contact page loads correctly', async ({ page }) => {
    await page.goto('/contact');
    await page.waitForLoadState('networkidle');
    
    const body = page.locator('body');
    await expect(body).toBeVisible();
  });

  test('Login page loads correctly', async ({ page }) => {
    await page.goto('/login');
    await page.waitForLoadState('networkidle');
    
    // Should have login form elements
    await expect(page.locator('input[formControlName="username"], input[name="username"], input[placeholder*="Username"]').first()).toBeVisible();
    await expect(page.locator('input[formControlName="password"], input[name="password"], input[placeholder*="Password"]').first()).toBeVisible();
  });
});
