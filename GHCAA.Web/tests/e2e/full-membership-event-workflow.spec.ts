/// <reference types="node" />
import { test, expect, Page } from '@playwright/test';
import * as path from 'path';
import * as fs from 'fs';
import { Buffer } from 'buffer';
import { approveMember, completeRegistrationPayment, getAuthToken } from './utils/api-helper';
import { AuthHelper } from './utils/auth-helper';

declare const __dirname: string;

// ─────────────────────────────────────────────────────────────────────────────
// Test data – unique per run so DB collisions are avoided
// ─────────────────────────────────────────────────────────────────────────────
const RUN_ID   = Date.now();
const NEW_NID  = `1990${RUN_ID.toString().slice(-13)}`; // 17-digit NID
const NEW_EMAIL = `e2e.${RUN_ID}@ghcaa.test`;
const NEW_MOBILE = `017${RUN_ID.toString().slice(-8)}`; // 11-digit Mobile
const NEW_NAME  = `E2E Member ${RUN_ID}`;
const EVENT_TITLE = `E2E Event ${RUN_ID}`;

const ADMIN_USER = 'superadmin';
const ADMIN_PASS = 'SuperAdminPassword123!';

function toDateTimeLocal(value: Date): string {
  const pad = (part: number) => String(part).padStart(2, '0');
  return `${value.getFullYear()}-${pad(value.getMonth() + 1)}-${pad(value.getDate())}T${pad(value.getHours())}:${pad(value.getMinutes())}`;
}

// ─────────────────────────────────────────────────────────────────────────────
// Tiny stub image – created in-memory so no external asset is needed
// ─────────────────────────────────────────────────────────────────────────────
function stubImagePath(): string {
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

// ─────────────────────────────────────────────────────────────────────────────
// Helpers
// ─────────────────────────────────────────────────────────────────────────────
async function loginAs(page: Page, username: string, password: string, role: 'admin' | 'member') {
  await new AuthHelper(page).login(username, password);
}

async function logout(page: Page) {
  await new AuthHelper(page).logout();
}

// ─────────────────────────────────────────────────────────────────────────────
// UNIFIED E2E WORKFLOW — Optimized for speed and deterministic execution
// ─────────────────────────────────────────────────────────────────────────────
test('Comprehensive GHCAA Ecosystem Workflow', async ({ page, request }) => {
  test.slow();
  page.on('dialog', (dialog) => dialog.accept());
  const imgPath = stubImagePath();
  page.on('console', msg => console.log(`BROWSER [${msg.type()}]: ${msg.text()}`));
  page.on('pageerror', err => console.log(`BROWSER ERROR: ${err.message}`));
  page.on('requestfailed', request => console.log(`BROWSER REQ FAILED: ${request.url()} - ${request.failure()?.errorText}`));
  page.on('response', async response => {
    if (response.status() >= 400) {
      const body = await response.text().catch(() => 'No body');
      console.log(`BROWSER REQ ERROR: ${response.url()} -> ${response.status()}\nBody: ${body}`);
    }
  });

  // ── STEP 1: New Member Registration ───────────────────────────────────────
  console.log('--- Step 1: Member Registration ---');
  await page.goto('/register');
  await page.waitForLoadState('networkidle');

  await page.locator('input[name="fullName"]').fill(NEW_NAME);
  await page.locator('input[name="fatherName"]').fill('E2E Father');
  await page.locator('input[name="motherName"]').fill('E2E Mother');
  await page.locator('input[name="dateOfBirth"]').fill('15-06-1990'); // Standard format
  await page.locator('input[name="dateOfBirth"]').blur();
  
  await page.locator('input[name="nid"]').fill(NEW_NID);
  await page.locator('select[name="gender"]').selectOption('Male');
  await page.locator('select[name="bloodGroup"]').selectOption('APositive');
  await page.locator('select[name="tShirtSize"]').selectOption('L');

  // Dynamic fee check - using innerText match as fallback for visual consistency
  await page.waitForFunction(() => document.body.innerText.match(/1[,.]?000|500/), { timeout: 10000 });

  await page.locator('input[name="email"]').fill(NEW_EMAIL);
  await page.locator('input[name="mobileNo"]').fill(NEW_MOBILE);
  await page.locator('textarea[name="presentAddress"]').fill('123 E2E Street, Dhaka');
  await page.locator('textarea[name="permanentAddress"]').fill('456 Home Village');

  // Verify no validation errors before continuing
  const step1Errors = page.locator('.text-red-500, .invalid-feedback').filter({ visible: true });
  if (await step1Errors.count() > 0) {
    throw new Error(`Step 1 Validation Error: ${await step1Errors.first().innerText()}`);
  }
  await page.locator('button:has-text("Continue Assessment")').click();
  await page.waitForSelector('text=Background & Milestones', { timeout: 15000 });

  // Academic Info — first record is pre-seeded with Haraganga College
  const academicSelects = page.locator('select').first();
  await expect(academicSelects).toBeVisible({ timeout: 15000 });
  await academicSelects.selectOption('HSC');
  await page.locator('select').nth(1).selectOption('Science');
  await page.locator('select').nth(2).selectOption('2006');
  await page.locator('select').nth(3).selectOption('2008');

  // Professional History (Standard date format)
  await page.getByPlaceholder(/Employer Title/i).first().fill('GHCAA Test Corp');
  await page.getByPlaceholder(/Chief Technologist/i).first().fill('Software Engineer');
  await page.locator('select').filter({ hasText: /Select Sector/ }).selectOption({ index: 1 });
  await page.locator('input[placeholder="dd-mm-yyyy"]').first().fill('01-01-2010'); 
  
  const isCurrent = page.locator('label').filter({ hasText: /I currently serve/i }).locator('input[type="checkbox"]');
  if (await isCurrent.isVisible() && !(await isCurrent.isChecked())) await isCurrent.check();

  await page.locator('input[name*="emergencyContactName"]').fill('Emergency Contact');
  await page.locator('input[name*="emergencyContactRelation"]').fill('Sibling');
  await page.locator('input[name*="emergencyContactPhone"]').fill('01898765432');

  await page.locator('button:has-text("Continue Assessment")').click();
  // Wait for the actual content of Step 3, not the stepper
  await page.waitForSelector('h2:has-text("Registry Filing")', { timeout: 15000 });
  
  const cashPayment = page.locator('.payment-card', { hasText: /Cash.*Manual Receipt/i }).first();
  await cashPayment.waitFor({ state: 'visible', timeout: 30000 });
  await cashPayment.click();
  
  // Targeted file uploads using container labels for precision
  const photoContainer = page.locator('.form-group', { hasText: /Profile Photo/i });
  await photoContainer.locator('input[type="file"]').setInputFiles(imgPath);

  const certContainer = page.locator('.form-group', { hasText: /Academic Certificate/i });
  await certContainer.locator('input[type="file"]').setInputFiles(imgPath);

  // Payment portal file drop for receipt
  const receiptDrop = page.locator('app-payment-portal .file-drop');
  if (await receiptDrop.count() > 0) {
    await receiptDrop.locator('input[type="file"]').setInputFiles(imgPath);
  }

  // Accept all terms explicitly
  await page.locator('label', { hasText: /I have read and understood/i }).locator('input').check();
  await page.locator('label', { hasText: /I explicitly consent/i }).locator('input').check();
  await page.locator('label', { hasText: /I solemnly affirm/i }).locator('input').check();

  // Finalize Registry
  await page.locator('button[type="submit"]:has-text("Finalize Registry")').click();
  
  // Wait for success states: OTP screen, success message, or redirect to login
  await Promise.race([
    page.waitForSelector('text=Security Authentication', { timeout: 60000 }),
    page.waitForSelector('text=submitted successfully', { timeout: 60000 }),
    page.waitForURL(/.*login/, { timeout: 60000 })
  ]);
  console.log('✅ Registration Submitted.');

  // ── STEP 2: Admin Approval ────────────────────────────────────────────────
  console.log('--- Step 2: Admin Approval ---');
  const adminToken = await getAuthToken(request, ADMIN_USER, ADMIN_PASS);
  await completeRegistrationPayment(request, adminToken, NEW_EMAIL);
  await loginAs(page, ADMIN_USER, ADMIN_PASS, 'admin');
  await page.goto('/admin/approvals');

  const searchInput = page.locator('input[placeholder*="Search"]').first();
  if (await searchInput.isVisible()) {
    await searchInput.fill(NEW_EMAIL);
    await page.waitForSelector(`tr:has-text("${NEW_EMAIL}")`, { timeout: 10000 });
  }

  await expect(page.locator('tr', { hasText: NEW_EMAIL })).toBeVisible({ timeout: 10000 });
  await approveMember(request, adminToken, NEW_EMAIL);

  await page.reload();
  await page.waitForLoadState('networkidle');
  await expect(page.locator('tr', { hasText: NEW_EMAIL })).toHaveCount(0, { timeout: 15000 });
  console.log('✅ Member Approved.');

  // ── STEP 3: Admin Event Creation ──────────────────────────────────────────
  console.log('--- Step 3: Admin Event Creation ---');
  await page.goto('/admin/events');
  await page.getByRole('button', { name: /Launch New Event/ }).click();
  
  await page.locator('input[formControlName="title"]').fill(EVENT_TITLE);
  await page.locator('textarea[formControlName="description"]').fill('E2E Event Description');
  const eventStart = new Date(Date.now() + 2 * 24 * 60 * 60 * 1000);
  const eventEnd = new Date(eventStart.getTime() + 8 * 60 * 60 * 1000);
  const registrationStart = new Date(Date.now() - 60 * 60 * 1000);
  const registrationEnd = new Date(eventStart.getTime() - 60 * 60 * 1000);
  await page.locator('input[formControlName="startDate"]').fill(toDateTimeLocal(eventStart));
  await page.locator('input[formControlName="endDate"]').fill(toDateTimeLocal(eventEnd));
  await page.locator('input[formControlName="registrationStartDate"]').fill(toDateTimeLocal(registrationStart));
  await page.locator('input[formControlName="registrationEndDate"]').fill(toDateTimeLocal(registrationEnd));
  await page.locator('input[formControlName="location"]').fill('Munshiganj');
  await page.locator('input[formControlName="registrationFee"]').fill('500');
  
  await page.getByRole('button', { name: 'Save', exact: true }).click();
  await page.waitForSelector(`text=${EVENT_TITLE}`, { timeout: 15000 });
  console.log('✅ Event Created.');

  // ── STEP 4: Member Portal Interactions ───────────────────────────────────
  console.log('--- Step 4: Member Journey ---');
  await logout(page);
  // Log in as the new member; the shared auth helper completes first-login rotation.
  await loginAs(page, NEW_NID, NEW_NID, 'member');
  
  // Update Profile
  await page.goto('/portal/profile');
  const bioInput = page.locator('textarea[name="bio"], textarea[name="about"]').first();
  if (await bioInput.isVisible()) {
    await bioInput.fill('Updated Bio via E2E');
    await page.locator('button:has-text("Update Registry")').click();
    await page.waitForSelector('text=success', { timeout: 5000 }).catch(() => {});
  }

  // Register for Event
  await page.goto('/portal/events');
  await page.waitForSelector(`text=${EVENT_TITLE}`);
  const eventCard = page.locator('.event-card', { hasText: EVENT_TITLE }).first();
  await eventCard.locator('button', { hasText: /Secure Pass|Join Event/ }).click();
  await page.waitForSelector('text=Event Registration');
  const eventCashPayment = page.locator('.payment-card', { hasText: /Cash.*Manual Receipt/i }).last();
  await eventCashPayment.click();
  const [registrationResponse] = await Promise.all([
    page.waitForResponse(response => response.request().method() === 'POST' && response.url().includes('/api/events/register')),
    page.locator('button:has-text("Confirm My Spot")').click()
  ]);
  expect(registrationResponse.ok()).toBeTruthy();
  await page.getByRole('button', { name: 'My Participations' }).click();
  await expect(page.locator('.reg-card', { hasText: EVENT_TITLE })).toBeVisible({ timeout: 10000 });
  console.log('✅ Member Registered for Event.');

  // ── STEP 5: Final Admin Review ───────────────────────────────────────────
  console.log('--- Step 5: Final Admin Review ---');
  await logout(page);
  await loginAs(page, ADMIN_USER, ADMIN_PASS, 'admin');
  await page.goto('/admin/events');
  
  await page.getByRole('button', { name: /Participation Approvals/ }).click();
  const participantRow = page.locator('tr', { hasText: EVENT_TITLE }).first();
  await expect(participantRow).toBeVisible({ timeout: 15000 });
  await participantRow.getByRole('button', { name: /Approve/ }).click();
  console.log('✅ Admin Approved Participation.');

  console.log('✅ End-to-End Workflow Completed.');
});
