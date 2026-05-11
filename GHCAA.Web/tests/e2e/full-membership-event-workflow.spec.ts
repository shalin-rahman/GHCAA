/// <reference types="node" />
import { test, expect, Page } from '@playwright/test';
import * as path from 'path';
import * as fs from 'fs';
import { Buffer } from 'buffer';

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

const ADMIN_USER = 'shalin';
const ADMIN_PASS = 'Shalin@2024!';

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
  await page.goto('/login');
  await page.waitForLoadState('networkidle');

  const userInput = page.locator(
    'input[formControlName="username"], input[name="username"], input[placeholder*="Username"]'
  ).first();
  const passInput = page.locator(
    'input[formControlName="password"], input[name="password"], input[placeholder*="Password"]'
  ).first();
  const submitBtn = page.locator('button[type="submit"], button:has-text("Login")').first();

  await userInput.fill(username);
  await passInput.fill(password);
  await submitBtn.click();

  if (role === 'admin') {
    await expect(page).toHaveURL(/.*admin\/(dashboard|approvals|members|events)/, { timeout: 15000 });
  } else {
    await expect(page).toHaveURL(/.*portal\/(dashboard|profile|events)/, { timeout: 15000 });
  }
}

async function logout(page: Page) {
  await page.evaluate(() => { localStorage.clear(); sessionStorage.clear(); });
  await page.goto('/login');
}

// ─────────────────────────────────────────────────────────────────────────────
// UNIFIED E2E WORKFLOW — Optimized for speed and deterministic execution
// ─────────────────────────────────────────────────────────────────────────────
test('Comprehensive GHCAA Ecosystem Workflow', async ({ page }) => {
  test.slow();
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
  await page.locator('select[name="membershipType"]').selectOption('General');

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

  // Academic Info - Using container-based targeting for robust selection
  const academicSection = page.locator('.history-item').first();
  await academicSection.locator('.form-group', { hasText: /Degree Conferred/i }).locator('select').selectOption('HSC');
  await academicSection.locator('.form-group', { hasText: /Subject/i }).locator('select').selectOption('Science');
  await academicSection.locator('.form-group', { hasText: /Admission/i }).locator('select').selectOption('2006');
  await academicSection.locator('.form-group', { hasText: /Passing Year/i }).locator('select').selectOption('2008');

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
  
  // Explicitly select Cash / Manual Receipt
  await page.locator('.payment-card', { hasText: /Cash.*Receipt/i }).click();
  
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
  await loginAs(page, ADMIN_USER, ADMIN_PASS, 'admin');
  await page.goto('/admin/approvals');
  
  const searchInput = page.locator('input[placeholder*="Search"]').first();
  if (await searchInput.isVisible()) {
    await searchInput.fill(NEW_NID);
    await page.waitForSelector(`tr:has-text("${NEW_NID}")`, { timeout: 10000 });
  }

  const pendingRow = page.locator('tr', { hasText: NEW_NID }).first();
  await pendingRow.locator('button:has-text("Approve")').click();
  const confirmBtn = page.locator('button:has-text("Confirm"), button:has-text("Yes"), button:has-text("Approve")').last();
  if (await confirmBtn.isVisible({ timeout: 5000 })) await confirmBtn.click();

  await expect(pendingRow).not.toBeVisible({ timeout: 10000 });
  console.log('✅ Member Approved.');

  // ── STEP 3: Admin Event Creation ──────────────────────────────────────────
  console.log('--- Step 3: Admin Event Creation ---');
  await page.goto('/admin/events');
  await page.locator('button:has-text("New Event")').click();
  
  await page.locator('input[name="title"]').fill(EVENT_TITLE);
  await page.locator('textarea[name="description"]').fill('E2E Event Description');
  await page.locator('input[name="eventDate"]').fill('30-06-2026');
  await page.locator('input[name="registrationDeadline"]').fill('25-06-2026');
  await page.locator('input[name="location"]').fill('Munshiganj');
  await page.locator('input[name="fee"]').fill('500');
  await page.locator('select[name="category"]').selectOption('Reunion');
  await page.locator('select[name="status"]').selectOption('Published');
  
  await page.locator('input[type="file"]').first().setInputFiles(imgPath);
  await page.locator('button:has-text("Save Event")').click();
  await page.waitForSelector(`text=${EVENT_TITLE}`, { timeout: 15000 });
  console.log('✅ Event Created.');

  // ── STEP 4: Member Portal Interactions ───────────────────────────────────
  console.log('--- Step 4: Member Journey ---');
  await logout(page);
  // Log in as the new member (NID as password by default)
  await loginAs(page, NEW_MOBILE, NEW_NID, 'member');
  
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
  await eventCard.locator('button:has-text("Register")').click();
  await page.waitForSelector('text=Event Registration');
  await page.locator('button:has-text("Confirm Participation")').click();
  await expect(page.locator('text=Registered')).toBeVisible({ timeout: 10000 });
  console.log('✅ Member Registered for Event.');

  // ── STEP 5: Final Admin Review ───────────────────────────────────────────
  console.log('--- Step 5: Final Admin Review ---');
  await logout(page);
  await loginAs(page, ADMIN_USER, ADMIN_PASS, 'admin');
  await page.goto('/admin/events');
  
  const eventRow = page.locator(`tr:has-text("${EVENT_TITLE}")`).first();
  await eventRow.locator('button:has-text("View")').click();
  
  await page.waitForSelector('text=Participants');
  const participantRow = page.locator('tr:has-text("Pending")').first();
  if (await participantRow.isVisible()) {
    await participantRow.locator('button:has-text("Approve")').click();
    console.log('✅ Admin Approved Participation.');
  }

  console.log('✅ End-to-End Workflow Completed.');
});
