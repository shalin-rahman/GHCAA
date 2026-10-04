# Login authentication status flow

## Purpose

Improve the public login experience so the UI shows a short, reliable authentication progress sequence while the request is in flight, keeps the page responsive, and clears the status immediately after success or failure.

## Actors and boundaries

- A guest member enters a username and password on the public login screen.
- The Angular login component calls the existing AuthService login API.
- The same screen handles the success, failure, and timeout paths.
- The feature applies to the Web login flow only and reuses the active theme tokens and shared button styling.

## Requirements

### FR-1 Sequence behaviour

The login button shall show `Signing in` with animated dots instead of the static `Authenticating...` label while the auth request remains active. A status line under the button, read out by screen readers through `aria-live`, says what is happening in plain words.

The status line shall step through `Checking your details`, `Verifying your password`, `Loading your profile` and `Opening your portal`, one every 2 seconds. A normal login answers inside the first two. If the request is still pending after the fourth, the server is most likely waking from sleep, so the line shall repeat `Waking up the server`, `This can take up to a minute` and `Still signing you in` until the reply comes. The order shall never move backwards, the line shall never freeze on one message, and the sequence shall stop as soon as the request resolves.

(Revised 2026-10-03. The first version picked 6 or 7 random technical verbs such as `Salting` and `Ingesting`, which told the user nothing. See TODO 87.4.)

### FR-2 Visual behaviour

The active status shall use the existing theme tokens from the login page and the current button styling rather than introducing a separate ad hoc aesthetic.

Each new status message shall fade in over 0.4s with opacity and vertical offset changes. The streaming dots shall remain hardware-friendly and animate without blocking the rest of the page.

### FR-3 Safety and lifecycle

The component shall disable the submit button and social-login buttons while an auth request is running, preventing duplicate clicks.

When the component is destroyed, all active timers and pending animation state shall be cleared immediately so no stale callbacks or memory leaks remain.

The authentication flow shall enforce a 60-second safety timeout, long enough for a Render free-tier wake. If the request hangs, the system shall cancel the request, stop the progress sequence, restore the form to an idle state, and show a clear error message. A reply that arrives before the timeout shall always complete the login.

### FR-4 Completion clarity

When the API succeeds or fails, the progress indicator shall disappear immediately and the user shall see the normal login or error state again.

The login button shall return to the plain `Sign in` label after the request resolves, and the in-progress progress text shall not remain visible on the page after completion.

## Non-functional requirements

- The login flow remains non-blocking and responsive during progress updates.
- The implementation uses Angular signals and the existing login component lifecycle without adding global state or blocking loops.
- The auth safety timeout remains user-safe and graceful: the error state is surfaced without leaving the component in a stuck loading state.

## Dependencies

- Existing `AuthService.login`, `googleLogin`, and `facebookLogin` APIs.
- Current login component template, theme tokens, and button classes in `GHCAA.Web/src/app/public/login`.
- The existing Angular test setup used by `login.spec.ts`.

## Acceptance scenarios

1. Given a valid login request, when the component starts the flow, then the submit button shows `Signing in`, the status line shows `Checking your details` and the form becomes disabled.
2. Given the request stays active, when the interval advances, then the status line moves through the four normal steps in order, and after them repeats the three slow-server messages until the reply comes.
3. Given the request resolves successfully, when the response arrives, then the progress indicator disappears and the user is redirected normally.
4. Given the request fails, when the error response arrives, then the loading state ends and the error text is shown.
5. Given the request hangs for longer than 60 seconds, when the timeout fires, then the flow cancels the request, clears its timers and shows `Login timed out. Please try again.`
6. Given the component is destroyed mid-flow, when `ngOnDestroy` runs, then all timers are cleared and no state update is emitted after the component is gone.
