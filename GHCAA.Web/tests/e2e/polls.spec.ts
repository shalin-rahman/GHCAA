import { test, expect } from '@playwright/test';
import { AuthHelper } from './utils/auth-helper';

test.describe('Polls & Voting E2E', () => {
  let auth: AuthHelper;

  test.beforeEach(async ({ page }) => {
    auth = new AuthHelper(page);
    await auth.login('2512006', '2512006');
  });

  test.afterEach(async ({ page }) => {
    await auth.logout();
  });

  test('Member can view the polls page with active polls or empty state', async ({ page }) => {
    await page.goto('/portal/polls');
    await expect(page).toHaveURL(/.*portal\/polls/);

    // Verify page header
    await expect(page.getByText('Member Voices')).toBeVisible();
    await expect(page.getByText('Participate in shaping the future of our association')).toBeVisible();

    await page.waitForLoadState('networkidle');

    // Verify polls cards or empty state
    const pollCards = page.locator('.poll-vote-card');
    const noPollsState = page.locator('.no-polls');
    const hasPolls = await pollCards.first().isVisible({ timeout: 5000 }).catch(() => false);

    if (hasPolls) {
      // Verify card has a title and options
      const firstPoll = pollCards.first();
      await expect(firstPoll.locator('h2')).toBeVisible();
      
      // Check for voting options or results (depending on vote state)
      const votingOptions = firstPoll.locator('.voting-options');
      const voteResults = firstPoll.locator('.vote-results');
      const canVote = await votingOptions.isVisible().catch(() => false);
      const hasVoted = await voteResults.isVisible().catch(() => false);

      if (canVote) {
        // Verify option items exist
        const optionItems = votingOptions.locator('.option-item');
        expect(await optionItems.count()).toBeGreaterThan(0);

        // Verify submit button is disabled when no option selected
        await expect(firstPoll.locator('button:has-text("Submit My Vote")')).toBeDisabled();
      } else if (hasVoted) {
        // Verify results section
        await expect(voteResults.locator('.result-row').first()).toBeVisible();
        await expect(voteResults.locator('.result-footer')).toContainText('Total Participants');
      }
    } else {
      await expect(noPollsState).toBeVisible();
      await expect(noPollsState).toContainText('No Active Polls');
    }
  });

  test('Member can select a poll option when unvoted polls exist', async ({ page }) => {
    await page.goto('/portal/polls');
    await page.waitForLoadState('networkidle');

    const unvotedPoll = page.locator('.poll-vote-card:not(.voted)').first();
    const hasUnvoted = await unvotedPoll.isVisible({ timeout: 5000 }).catch(() => false);

    if (hasUnvoted) {
      // Click on first option to select it
      const firstOption = unvotedPoll.locator('.option-item').first();
      await firstOption.click();

      // Verify it's selected
      await expect(firstOption).toHaveClass(/selected/);

      // Submit button should now be enabled
      await expect(unvotedPoll.locator('button:has-text("Submit My Vote")')).toBeEnabled();
    }
  });
});
