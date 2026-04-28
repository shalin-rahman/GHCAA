import { Page, expect } from '@playwright/test';

export class AuthHelper {
  constructor(private page: Page) {}

  async login(username = 'shalin', password = 'Shalin@2024!') {
    await this.page.goto('/login');
    // Using placeholders as the selectors might vary (formControlName is common in Angular)
    // We try multiple selectors to be robust
    const usernameInput = this.page.locator('input[formControlName="username"], input[name="username"], input[placeholder*="Username"]');
    const passwordInput = this.page.locator('input[formControlName="password"], input[name="password"], input[placeholder*="Password"]');
    const submitButton = this.page.locator('button[type="submit"], button:has-text("Login")');

    await usernameInput.fill(username);
    await passwordInput.fill(password);
    await submitButton.click();

    // Determine redirect based on role
    // shalin is a SuperAdmin, so it redirects to admin/approvals
    if (username === 'shalin' || username === 'superadmin') {
      await expect(this.page).toHaveURL(/.*admin\/(dashboard|approvals)/, { timeout: 10000 });
    } else {
      await expect(this.page).toHaveURL(/.*portal\/dashboard/, { timeout: 10000 });
    }
  }

  async logout() {
    await this.page.evaluate(() => {
      localStorage.clear();
      sessionStorage.clear();
    });
    await this.page.goto('/login');
  }
}
