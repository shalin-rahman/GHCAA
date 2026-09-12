import { Page, expect } from '@playwright/test';
import * as path from 'path';
import * as fs from 'fs';
import { Buffer } from 'buffer';

declare const __dirname: string;

export interface NewMember {
  name: string;
  email: string;
  mobile: string;
  nid: string;
}

/** Tiny stub image created in-memory so no external asset is needed. */
export function stubImagePath(): string {
  const tmpDir = path.join(process.cwd(), 'test-results', 'stubs');
  fs.mkdirSync(tmpDir, { recursive: true });
  const imgPath = path.join(tmpDir, 'stub.jpg');
  if (!fs.existsSync(imgPath)) {
    const jpegBytes = Buffer.from(
      '/9j/4AAQSkZJRgABAQAAAQABAAD/2wBDAAgGBgcGBQgHBwcJCQgKDBQNDAsLDBkSEw8U' +
      'HRofHh0aHBwgJC4nICIsIxwcKDcpLDAxNDQ0Hyc5PTgyPC4zNDL/2wBDAQkJCQwLDBgN' +
      'DRgyIRwhMjIyMjIyMjIyMjIyMjIyMjIyMjIyMjIyMjIyMjIyMjIyMjIyMjIyMjIyMjIy' +
      'MjL/wAARCAABAAEDASIAAhEBAxEB/8QAFAABAAAAAAAAAAAAAAAAAAAACf/EABQQAQAAAAAA' +
      'AAAAAAAAAAAAAP/EABQBAQAAAAAAAAAAAAAAAAAAAAD/xAAUEQEAAAAAAAAAAAAAAAAAAAAA' +
      '/9oADAMBAAIRAxEAPwCwABmX/9k=',
      'base64'
    );
    fs.writeFileSync(imgPath, jpegBytes);
  }
  return imgPath;
}

/**
 * Drives the public /register wizard through submission, leaving a fresh
 * member in "Applied" status with a Pending cash-receipt payment. Each call
 * generates its own unique NID/email/mobile so it's safe to run concurrently
 * and self-contained (no dependency on leftover DB state from other runs).
 */
export async function registerNewMemberViaUi(page: Page): Promise<NewMember> {
  const runId = `${Date.now()}${Math.floor(Math.random() * 1000)}`;
  const member: NewMember = {
    name: `E2E Member ${runId}`,
    email: `e2e.${runId}@ghcaa.test`,
    mobile: `017${runId.slice(-8)}`,
    nid: `1990${runId.slice(-13)}`,
  };
  const imgPath = stubImagePath();

  await page.goto('/register');
  await page.waitForLoadState('networkidle');

  await page.locator('input[name="fullName"]').fill(member.name);
  await page.locator('input[name="fatherName"]').fill('E2E Father');
  await page.locator('input[name="motherName"]').fill('E2E Mother');
  await page.locator('input[name="dateOfBirth"]').fill('15-06-1990');
  await page.locator('input[name="dateOfBirth"]').blur();

  await page.locator('input[name="nid"]').fill(member.nid);
  await page.locator('select[name="gender"]').selectOption('Male');
  await page.locator('select[name="bloodGroup"]').selectOption('APositive');
  await page.locator('select[name="tShirtSize"]').selectOption('L');

  await page.waitForFunction(() => document.body.innerText.match(/1[,.]?000|500/), { timeout: 10000 });

  await page.locator('input[name="email"]').fill(member.email);
  await page.locator('input[name="mobileNo"]').fill(member.mobile);
  await page.locator('textarea[name="presentAddress"]').fill('123 E2E Street, Dhaka');
  await page.locator('textarea[name="permanentAddress"]').fill('456 Home Village');

  const step1Errors = page.locator('.text-red-500, .invalid-feedback').filter({ visible: true });
  if (await step1Errors.count() > 0) {
    throw new Error(`Step 1 Validation Error: ${await step1Errors.first().innerText()}`);
  }
  await page.locator('button:has-text("Continue Assessment")').click();
  await page.waitForSelector('text=Background & Milestones', { timeout: 15000 });

  const backgroundStep = page.locator('.step-content').filter({ hasText: 'Background & Milestones' });
  const academicSelects = backgroundStep.locator('select').nth(0);
  await expect(academicSelects).toBeVisible({ timeout: 15000 });
  await backgroundStep.locator('select').nth(0).selectOption('HSC');
  await backgroundStep.locator('select').nth(1).selectOption('Science');
  await backgroundStep.locator('select').nth(2).selectOption('2006');
  await backgroundStep.locator('select').nth(3).selectOption('2008');

  await page.getByPlaceholder(/Employer Title/i).first().fill('GHCAA Test Corp');
  await page.getByPlaceholder(/Chief Technologist/i).first().fill('Software Engineer');
  await backgroundStep.locator('select').nth(4).selectOption({ index: 1 });
  await page.locator('input[placeholder="dd-mm-yyyy"]').first().fill('01-01-2010');

  const isCurrent = backgroundStep.locator('input[type="checkbox"]').last();
  if (await isCurrent.isVisible() && !(await isCurrent.isChecked())) await isCurrent.check();

  await page.locator('input[name*="emergencyContactName"]').fill('Emergency Contact');
  await page.locator('input[name*="emergencyContactRelation"]').fill('Sibling');
  await page.locator('input[name*="emergencyContactPhone"]').fill('01898765432');

  await page.locator('button:has-text("Continue Assessment")').click();
  await page.waitForSelector('h2:has-text("Registry Filing")', { timeout: 15000 });

  const cashPayment = page.locator('.payment-card', { hasText: /Cash.*Manual Receipt/i }).first();
  await cashPayment.waitFor({ state: 'visible', timeout: 30000 });
  await cashPayment.click();

  const photoContainer = page.locator('.form-group', { hasText: /Profile Photo/i });
  await photoContainer.locator('input[type="file"]').setInputFiles(imgPath);

  const certContainer = page.locator('.form-group', { hasText: /Academic Certificate/i });
  await certContainer.locator('input[type="file"]').setInputFiles(imgPath);

  const receiptDrop = page.locator('app-payment-portal .file-drop');
  if (await receiptDrop.count() > 0) {
    await receiptDrop.locator('input[type="file"]').setInputFiles(imgPath);
  }

  await page.locator('label', { hasText: /I have read and understood/i }).locator('input').check();
  await page.locator('label', { hasText: /I explicitly consent/i }).locator('input').check();
  await page.locator('label', { hasText: /I solemnly affirm/i }).locator('input').check();

  await page.locator('button[type="submit"]:has-text("Finalize Registry")').click();

  await expect(page.getByRole('heading', { name: 'Security Authentication' })).toBeVisible({ timeout: 60000 });

  return member;
}
