import { Page, expect } from '@playwright/test';

const rotatedPasswords = new Map<string, string>();

export class AuthHelper {
  constructor(private page: Page) {}

  async login(username = 'superadmin', password = 'SuperAdminPassword123!') {
    const currentPassword = rotatedPasswords.get(username) ?? password;
    await this.page.goto('/login');
    // Using placeholders as the selectors might vary (formControlName is common in Angular)
    // We try multiple selectors to be robust
    const usernameInput = this.page.locator('input[formControlName="username"], input[name="username"], input[placeholder*="Username"]');
    const passwordInput = this.page.locator('input[formControlName="password"], input[name="password"], input[placeholder*="Password"]');
    const submitButton = this.page.locator('button[type="submit"], button:has-text("Login")');

    await usernameInput.fill(username);
    await passwordInput.fill(currentPassword);
    await submitButton.click();

    if (await this.page.waitForURL(/\/portal\/change-password$/, { timeout: 5000 }).then(() => true).catch(() => false)) {
      const replacementPassword = `${currentPassword}Changed!`;
      await this.page.locator('input[name="oldPassword"]').fill(currentPassword);
      await this.page.locator('input[name="newPassword"]').fill(replacementPassword);
      await this.page.locator('input[name="confirmPassword"]').fill(replacementPassword);
      const passwordChange = this.page.waitForResponse(
        response => response.url().includes('/api/profile/change-password')
          && response.request().method() === 'POST'
          && response.ok()
      );
      await this.page.getByRole('button', { name: 'Change Password', exact: true }).click();
      await passwordChange;

      // The password-change response replaces the browser's auth cookies. Do not
      // clear them or log in through page.request, whose cookie jar is separate.
      await this.page.goto('/portal/dashboard');
      rotatedPasswords.set(username, replacementPassword);
    }

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
