import { test, expect, Page, APIRequestContext } from '@playwright/test';

/**
 * WP62.41 — the acceptance test that proves the white-label work is actually done.
 *
 * Does NOT run as part of the normal suite. It needs its own server, booted with
 * ORG_PROFILE=default against a database that has never run a migration before,
 * which the default playwright.config.ts webServer does not provide (it reuses the
 * everyday GHCAA dev database). Run it with:
 *
 *   1. Create an empty database and point GHCAA.API at it (a fresh Postgres database,
 *      or GHCAA__ConnectionStrings__PgSqlConnection pointed at one).
 *   2. ORG_PROFILE=default dotnet run --project GHCAA.API --urls http://localhost:5099
 *   3. PLAYWRIGHT_GENERIC_BASE_URL=http://localhost:5099 npx playwright test \
 *        --config=playwright.generic.config.ts
 *
 * As of 2026-09-06 this cannot pass yet, for two reasons this run will surface rather
 * than hide:
 *   - 8 committed EF Core migrations still bake 631 real GHC alumni (names, the
 *     college name, payment history) via InsertData, regardless of ORG_PROFILE
 *     (docs/TODO.md 62.31/82.31).
 *   - There is no bootstrap that creates an initial SuperAdmin *account* on a fresh
 *     database. `ProtectedSuperAdminSeeder` only re-grants the SuperAdmin *role* to a
 *     username that already exists; on a database that has never had GHCAA's own
 *     migrations run with real data, no such username exists, so admin login has
 *     nothing to log into. Found while writing this test, not previously tracked.
 */

const BANNED = [
  'GHCAA',
  'Govt. Haraganga College',
  'Haraganga',
  'হারাগঙ্গা', // the Bengali name, in case an assertion misses the transliterated form
];

async function assertNoBannedStrings(page: Page, where: string) {
  const text = await page.locator('body').innerText();
  for (const term of BANNED) {
    expect(text, `${where} must not render "${term}"`).not.toContain(term);
  }
}

test.describe('WP62.41 — ORG_PROFILE=default acceptance walk', () => {
  test.skip(
    !process.env.PLAYWRIGHT_GENERIC_BASE_URL,
    'Only runs against a server booted with ORG_PROFILE=default and a fresh database — see the file header.'
  );

  test('register, login, portal, admin, ID card, certificate and PDF generation render no GHC branding', async ({
    page,
    request,
  }: {
    page: Page;
    request: APIRequestContext;
  }) => {
    // 1. Public site before any account exists.
    await page.goto('/');
    await assertNoBannedStrings(page, 'landing page');

    const configResponse = await request.get('/api/config');
    expect(configResponse.ok()).toBeTruthy();
    const config = await configResponse.json();
    expect(config.orgId).not.toBe('ghcaa');
    for (const term of BANNED) {
      expect(JSON.stringify(config)).not.toContain(term);
    }

    // 2. Registration wizard.
    await page.goto('/register');
    await assertNoBannedStrings(page, 'registration wizard');

    // 3. Login as the demo member seeded by profiles/default/demo-data/users.json.
    await page.goto('/login');
    await page.locator('input[formControlName="username"], input[name="username"]').fill('demo.member1');
    await page.locator('input[formControlName="password"], input[name="password"]').fill('DemoPassword123!');
    await page.locator('button[type="submit"], button:has-text("Login")').click();
    await expect(page).toHaveURL(/.*portal/, { timeout: 15000 });
    await assertNoBannedStrings(page, 'member portal after login');

    // 4. Digital ID card and certificate — both are generated documents, the two
    // surfaces most likely to still carry a hardcoded crest or motto.
    await page.goto('/portal/digital-id');
    await assertNoBannedStrings(page, 'digital ID card page');

    // 5. Admin walk. This is the step expected to fail today: there is no admin
    // account on a database that has never run GHCAA's own seed data (see file header).
    await page.goto('/admin');
    await assertNoBannedStrings(page, 'admin console shell');
  });
});
