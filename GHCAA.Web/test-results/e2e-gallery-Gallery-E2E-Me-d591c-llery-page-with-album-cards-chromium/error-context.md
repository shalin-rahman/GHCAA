# Instructions

- Following Playwright test failed.
- Explain why, be concise, respect Playwright best practices.
- Provide a snippet of code with the fix, if possible.

# Test info

- Name: e2e\gallery.spec.ts >> Gallery E2E >> Member can view the gallery page with album cards
- Location: tests\e2e\gallery.spec.ts:16:7

# Error details

```
Test timeout of 30000ms exceeded.
```

```
Error: expect(locator).toBeVisible() failed

Locator: getByRole('heading', { name: 'Legacy Archive & Moments' })
Expected: visible
Error: element(s) not found

Call log:
  - Expect "toBeVisible" with timeout 15000ms
  - waiting for getByRole('heading', { name: 'Legacy Archive & Moments' })
    - waiting for" http://localhost:4200/login" navigation to finish...
    - navigated to "http://localhost:4200/login"

```

# Test source

```ts
  1  | import { test, expect } from '@playwright/test';
  2  | import { AuthHelper } from './utils/auth-helper';
  3  | 
  4  | test.describe('Gallery E2E', () => {
  5  |   let auth: AuthHelper;
  6  | 
  7  |   test.beforeEach(async ({ page }) => {
  8  |     auth = new AuthHelper(page);
  9  |     await auth.login('2512006', '2512006');
  10 |   });
  11 | 
  12 |   test.afterEach(async ({ page }) => {
  13 |     await auth.logout();
  14 |   });
  15 | 
  16 |   test('Member can view the gallery page with album cards', async ({ page }) => {
  17 |     await page.goto('/portal/dashboard');
  18 |     await page.goto('/portal/gallery');
  19 |     await expect(page).toHaveURL(/.*portal\/gallery/);
  20 |     await page.waitForLoadState('networkidle');
  21 | 
> 22 |     await expect(page.getByRole('heading', { name: 'Legacy Archive & Moments' })).toBeVisible({ timeout: 15000 });
     |                                                                                   ^ Error: expect(locator).toBeVisible() failed
  23 |     await expect(page.getByText('Visual journey through Haragangian heritage')).toBeVisible();
  24 | 
  25 |     // Verify Share button is visible for authenticated user
  26 |     await expect(page.locator('button:has-text("Share Legacy Moment")')).toBeVisible();
  27 | 
  28 |     await page.waitForLoadState('networkidle');
  29 | 
  30 |     // Verify gallery cards or empty state
  31 |     const galleryCards = page.locator('.gallery-card');
  32 |     const emptyState = page.locator('.empty-state');
  33 |     const hasGalleries = await galleryCards.first().isVisible({ timeout: 5000 }).catch(() => false);
  34 | 
  35 |     if (hasGalleries) {
  36 |       const firstCard = galleryCards.first();
  37 |       await expect(firstCard.locator('h3')).toBeVisible();
  38 |       await expect(firstCard.locator('.photo-count')).toBeVisible();
  39 |     } else {
  40 |       await expect(emptyState).toContainText('No event galleries');
  41 |     }
  42 |   });
  43 | 
  44 |   test('Member can open the upload form and discard it', async ({ page }) => {
  45 |     await page.goto('/portal/gallery');
  46 |     await page.waitForLoadState('networkidle');
  47 | 
  48 |     // Open upload form
  49 |     await page.click('button:has-text("Share Legacy Moment")');
  50 |     await expect(page.locator('.upload-box')).toBeVisible();
  51 |     await expect(page.getByText('Share with Alumni Community')).toBeVisible();
  52 | 
  53 |     // Fill title
  54 |     await page.fill('input[name="title"]', 'E2E Test Album');
  55 |     await expect(page.locator('input[name="title"]')).toHaveValue('E2E Test Album');
  56 | 
  57 |     // Discard
  58 |     await page.click('button:has-text("Discard")');
  59 |     await expect(page.locator('.upload-box')).not.toBeVisible();
  60 |   });
  61 | 
  62 |   test('Member can drill into a gallery album when galleries exist', async ({ page }) => {
  63 |     await page.goto('/portal/gallery');
  64 |     await page.waitForLoadState('networkidle');
  65 | 
  66 |     const galleryCards = page.locator('.gallery-card');
  67 |     const hasGalleries = await galleryCards.first().isVisible({ timeout: 5000 }).catch(() => false);
  68 | 
  69 |     if (hasGalleries) {
  70 |       // Click into first gallery
  71 |       await galleryCards.first().click();
  72 | 
  73 |       // Verify detail view
  74 |       await expect(page.locator('.gallery-detail-container')).toBeVisible();
  75 |       await expect(page.locator('.photo-grid-immersive, .glass-card')).toBeVisible();
  76 | 
  77 |       // Go back
  78 |       await page.locator('button:has-text("←")').click();
  79 |       await expect(page.locator('.gallery-detail-container')).not.toBeVisible();
  80 |     }
  81 |   });
  82 | });
  83 | 
```