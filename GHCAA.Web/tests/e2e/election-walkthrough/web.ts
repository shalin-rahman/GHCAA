import { Browser, BrowserContext, Locator, Page, expect } from '@playwright/test';
import path from 'node:path';
import { PASSWORD, Person, noCodeError, waitForCode } from './local';
import { Kind, Report, SHOTS_DIR } from './report';

/**
 * True once the locator is visible, false after the timeout. Locator.isVisible() answers at
 * once and ignores its timeout, and it does not run the step-up handler, so a page still
 * behind the "Confirm it's you" dialog reads as missing the button.
 */
export async function shows(locator: Locator, timeout = 10_000): Promise<boolean> {
  return expect(locator).toBeVisible({ timeout }).then(() => true, () => false);
}

// A headed window opens maximized and the page takes the window's size. A fixed viewport
// made the window taller than the screen at 125% scaling, so the taskbar hid the sidebar's
// last items. Headless runs keep one size so the screenshots match.
export const WINDOW_VIEWPORT = process.env['WALKTHROUGH_HEADED'] ? null : { width: 1366, height: 768 };

// One browser context per person, so each one keeps their own cookies and
// the screenshots show what that person actually sees.
export class Actor {
  private constructor(
    readonly person: Person,
    readonly role: string,
    readonly context: BrowserContext,
    readonly page: Page,
    private readonly report: Report,
  ) {}

  static async open(browser: Browser, person: Person, role: string, report: Report): Promise<Actor> {
    const context = await browser.newContext({ viewport: WINDOW_VIEWPORT });
    // Without this a selector that matches nothing waits until the 90-minute test timeout.
    context.setDefaultTimeout(60_000);
    const page = await context.newPage();
    const actor = new Actor(person, role, context, page, report);
    page.on('dialog', d => d.accept());
    await actor.answerStepUp();
    await actor.login();
    return actor;
  }

  get label(): string {
    return `${this.person.name} (${this.role})`;
  }

  async login(): Promise<void> {
    // Feature-guarded routes need the org config before they will open. The app can fetch it
    // while the login page loads, so listen from before the page opens. Listening only after
    // the login used to sit out the whole 10 seconds for every person.
    const config = this.page.waitForResponse(r => r.url().includes('/api/config'), { timeout: 10_000 }).catch(() => {});
    await this.page.goto('/login');
    await this.page.locator('#login-username').fill(this.person.username);
    await this.page.locator('#login-password').fill(PASSWORD);
    await this.page.locator('button.login-submit').click();
    await expect(this.page).not.toHaveURL(/\/login/, { timeout: 20_000 });
    await config;
  }

  /** Drops the session and logs in again, so a new login picks up roles added since. */
  async relogin(): Promise<void> {
    await this.context.clearCookies();
    await this.page.evaluate(() => { sessionStorage.clear(); localStorage.removeItem('user_session'); });
    await this.login();
  }

  /**
   * The step-up dialog can open in the middle of any action. Opening it sends a code,
   * so the handler reads the newest code sent after the dialog appeared and enters it.
   */
  private async answerStepUp(): Promise<void> {
    const dialog = this.page.locator('.modal-overlay[role="dialog"]', { hasText: "Confirm it's you" });
    await this.page.addLocatorHandler(dialog, async () => {
      let since = new Date(Date.now() - 3_000);
      const emailInstead = dialog.getByRole('button', { name: 'Email me a code instead' });
      if (await emailInstead.isVisible()) {
        since = new Date(Date.now() - 1_000);
        await emailInstead.click();
      }
      const code = await waitForCode(this.person.userId, since);
      if (!code) throw noCodeError(this.person, since);
      await dialog.locator('#stepUpCode').fill(code);
      await dialog.getByRole('button', { name: 'Verify and continue' }).click();
      await expect(dialog).toBeHidden({ timeout: 15_000 });
    });
  }

  async goto(route: string): Promise<void> {
    await this.page.goto(route);
    await this.page.waitForLoadState('networkidle').catch(() => {});
  }

  /** Takes a full-page screenshot and adds it to the report under this person's name. */
  async shot(title: string, detail?: string, kind: Kind = 'web', ok = true): Promise<void> {
    // A quiet network takes at least 500 ms to confirm. Over a hundred shots that is about a
    // minute, so a demo run skips it and relies on each step having waited for its own result.
    if (!process.env['WALKTHROUGH_DEMO']) await this.page.waitForLoadState('networkidle').catch(() => {});
    // The step-up handler answers the dialog, but only when an assertion runs, so give it one.
    await expect(this.page.locator('.modal-overlay[role="dialog"]', { hasText: "Confirm it's you" })).toBeHidden({ timeout: 60_000 }).catch(() => {});
    const name = this.report.nextShotName();
    await this.page.screenshot({ path: path.join(SHOTS_DIR, name), fullPage: true });
    this.report.add({ kind, actor: this.label, title, detail, shot: name, ok });
  }

  /** Answers the app's own confirm dialog. Its first button is the confirm one. */
  async confirm(title?: string): Promise<void> {
    const box = this.page.locator('.modal-overlay[role="dialog"] .confirm-dialog-box');
    await expect(box).toBeVisible({ timeout: 10_000 });
    if (title) await this.shot(title);
    await box.locator('.confirm-dialog-actions button').first().click();
    await expect(box).toBeHidden({ timeout: 15_000 });
  }

  /** Runs the click and saves the file the page hands over. */
  async download(click: () => Promise<void>, saveTo: string): Promise<string> {
    const [file] = await Promise.all([this.page.waitForEvent('download', { timeout: 30_000 }), click()]);
    await file.saveAs(saveTo);
    return saveTo;
  }

  async close(): Promise<void> {
    await this.context.close();
  }
}
