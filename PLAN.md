# Plan: Stabilizing GHCAA E2E Tests

The goal is to fix intermittent 400 and 500 errors in the E2E registration workflow.

## Identified Issues
1. **500 Error on `/api/theme/active`**: Caused by `TimeZoneNotFoundException` because `Asia/Dhaka` is used on Windows (should be `Bangladesh Standard Time`).
2. **400 Error on `/api/auth/register`**: Likely a validation error (Duplicate NID/Email/Mobile or missing Academic History). We need more diagnostics.

## Proposed Steps

### 1. Fix Timezone in API (Resolve 500)
- Update `ThemeService.cs` to handle both Windows and Linux timezone IDs for Bangladesh.

### 2. Improve E2E Diagnostics (Identify 400)
- Update `full-membership-event-workflow.spec.ts` to log response bodies for errors.
- Ensure test data is truly unique and doesn't collide with seeded data.

### 3. Verify and Fix Registration (Resolve 400)
- Based on the logs from step 2, fix any validation issues in the registration payload or service.

### 4. Validation
- Run the E2E test and verify it passes consistently.

## Atomic Steps
- [ ] Edit `GHCAA.Infrastructure/Services/ThemeService.cs` to support `Bangladesh Standard Time`.
- [ ] Edit `GHCAA.Web/tests/e2e/full-membership-event-workflow.spec.ts` to log error response bodies.
- [ ] Run the E2E test to capture the 400 error body.
- [ ] (If 400 persists) Fix the root cause in payload or service.
