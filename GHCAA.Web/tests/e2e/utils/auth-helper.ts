import { Page, expect } from '@playwright/test';

export class AuthHelper {
  constructor(private page: Page) {}

  async login(username = 'superadmin', password = 'SuperAdminPassword123!') {
    await this.page.goto('/login');
    // Using placeholders as the selectors might vary (formControlName is common in Angular)
    // We try multiple selectors to be robust
    const usernameInput = this.page.locator('input[formControlName="username"], input[name="username"], input[placeholder*="Username"]');
    const passwordInput = this.page.locator('input[formControlName="password"], input[name="password"], input[placeholder*="Password"]');
    const submitButton = this.page.locator('button[type="submit"], button:has-text("Login")');

    await usernameInput.fill(username);
    await passwordInput.fill(password);
    await submitButton.click();

    await expect(this.page).toHaveURL(
      /.*(admin|portal)\/(dashboard|approvals|members|events|profile)/,
      { timeout: 15000 }
    );

    // Org config must load before feature-guarded routes (gallery, jobs)
    await this.page.waitForResponse(
      (resp) => resp.url().includes('/api/config'),
      { timeout: 15000 }
    ).catch(() => {});
  }

  async logout() {
    await this.page.evaluate(() => {
      localStorage.clear();
      sessionStorage.clear();
    });
    await this.page.goto('/login');
  }
}
