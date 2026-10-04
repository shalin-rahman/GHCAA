import { test, expect } from '@playwright/test';
import { AuthHelper } from './utils/auth-helper';
import { approveMember, getAuthToken } from './utils/api-helper';
import { registerNewMemberViaUi, NewMember } from './utils/ui-helper';

// Below 768px the portal sidebar is an off-canvas drawer. Before this it was display:none,
// so pages missing from the bottom bar (Governance, Messaging, Polls...) had no link at all.
test.describe('Portal menu on a phone-width browser', () => {
  test.use({ viewport: { width: 390, height: 844 } });

  let member: NewMember;

  test.beforeAll(async ({ browser, request }) => {
    const setupPage = await browser.newPage();
    member = await registerNewMemberViaUi(setupPage);
    await setupPage.close();

    const adminToken = await getAuthToken(request, 'superadmin', 'SuperAdminPassword123!');
    await approveMember(request, adminToken, member.email);
  });

  test('Menu button opens a drawer with every portal page, and it closes after navigating', async ({ page }) => {
    await new AuthHelper(page).login(member.nid, member.nid);
    await page.goto('/portal/dashboard');

    const drawer = page.locator('aside.sidebar');
    await expect(drawer).not.toBeInViewport();

    await page.locator('.mobile-nav').getByRole('button', { name: 'Open menu' }).click();
    await expect(drawer).toBeInViewport();
    for (const label of ['Governance', 'Messaging', 'Polls', 'My Communications', 'My Requests', 'Payments']) {
      await expect(drawer.getByRole('link', { name: label })).toBeVisible();
    }

    await drawer.getByRole('link', { name: 'Governance' }).click();
    await expect(page).toHaveURL(/\/portal\/governance$/);
    await expect(drawer).not.toBeInViewport();
  });

  test('Tapping the backdrop closes the drawer', async ({ page }) => {
    await new AuthHelper(page).login(member.nid, member.nid);
    await page.goto('/portal/dashboard');

    await page.locator('.mobile-nav').getByRole('button', { name: 'Open menu' }).click();
    const drawer = page.locator('aside.sidebar');
    await expect(drawer).toBeInViewport();

    await page.locator('.sidebar-overlay').click({ position: { x: 370, y: 400 } });
    await expect(drawer).not.toBeInViewport();
  });
});
