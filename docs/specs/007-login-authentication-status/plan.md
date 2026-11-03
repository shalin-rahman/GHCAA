# Login authentication status plan

## Goal

Deliver a responsive login progress state that matches the current GHCAA login screen design and clears cleanly on all terminal paths.

## Dependencies

1. Confirm the login page is the active Web-auth experience and there is no separate API contract change required.
2. Reuse the existing Angular `AuthService` and login component lifecycle; no backend contract change is needed because status is purely client-side.
3. Validate against the existing web test suite and the current login component behavior.

## Execution plan

### 1. Capture the current state

Review the public login component, template, and styling so the status indicator follows the established theme tokens instead of adding a one-off treatment.

### 2. Implement the sequence engine

Show two real steps in order: "Checking your username and password" while the request runs, then "Signed in. Opening your portal" while the page changes. After 5, 20 and 40 seconds, show one slow-server line each. Every line shows once and the list does not loop. The last line stays until the reply or the timeout.

### 3. Wire lifecycle safety

Use one `setTimeout` per slow-server line and one for the 60-second timeout (`AUTH_TIMEOUT_MS`). Clear both on success, failure, timeout, and component destroy to prevent stale updates.

### 4. Prevent duplicate submission

Disable the form submit and social-login buttons while the request is active. This prevents repeated login requests while the current one is still resolving.

### 5. Validate and verify

Run the focused login unit tests and confirm the status disappears after the request resolves and that the timeout error appears when the request hangs.

## Delivery checkpoints

- Status text updates in order, never loops, and disappears after success or error.
- Timeout path triggers after 60 seconds and shows the graceful error message.
- Component destroy clears all timers and prevents memory leaks.
- Login buttons remain disabled during processing.
