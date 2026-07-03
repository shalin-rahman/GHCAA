import { test, expect } from '@playwright/test';
import { AuthHelper } from './utils/auth-helper';

test.describe('Messaging Feature', () => {
  let auth: AuthHelper;

  test.beforeEach(async ({ page }) => {
    auth = new AuthHelper(page);
    // Login as one of the new test members
    await auth.login('2512006', '2512006');
  });

  test.afterEach(async ({ page }) => {
    await auth.logout();
  });

  test('should allow a member to view the messaging interface and type a message', async ({ page }) => {
    await page.goto('/portal/messages');
    await expect(page).toHaveURL(/.*\/portal\/messages/);

    // Verify UI elements
    await expect(page.getByText('Conversations', { exact: true })).toBeVisible();
    await expect(page.getByPlaceholder('Search people...')).toBeVisible();
    await expect(page.getByPlaceholder('Type a message...')).toBeVisible();

    // Type a message
    await page.getByPlaceholder('Type a message...').fill('Hello this is an E2E test message');
    
    // Check if the input contains the typed message
    await expect(page.getByPlaceholder('Type a message...')).toHaveValue('Hello this is an E2E test message');
  });
});
