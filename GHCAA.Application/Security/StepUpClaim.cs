namespace GHCAA.Application.Security
{
    // The custom (non-ClaimTypes.*) claim names this app issues into its JWTs, shared between
    // the issuer (TokenService, VisualTestAuthMiddleware) and every controller that reads them
    // via User.FindFirst(...), so the two can never drift apart.
    public static class AppClaimTypes
    {
        public const string MemberId = "MemberId";
    }

    // 7.13: Shared between TokenService (which issues the claim) and the API's
    // [RequireStepUp] filter (which reads it), so the two can never drift apart.
    public static class StepUpClaim
    {
        public const string Type = "step_up_verified_at";

        // Grace period since the OTP verification, not since the last access-token refresh:
        // the access token is silently refreshed roughly hourly, and refreshing must carry the
        // verification forward (see TokenService.TryGetValidStepUpEpoch) rather than resetting
        // it, or a short TTL would in practice mean "re-verify almost every request".
        //
        // SECURITY AUDIT (2026-08-29): this was previously 30 days, chosen so admins wouldn't be
        // re-prompted "on every action/login". That defeats the control's own stated purpose — a
        // stolen or left-open access-token cookie almost always already carries a still-valid
        // claim, since it rides along on every hourly silent refresh for the full 30 days. 30
        // minutes keeps the "not every single action" property (it survives several refreshes
        // within one admin session) while making a stolen cookie's window small enough to matter.
        public const int DefaultTtlMinutes = 30;

        public static bool IsValid(long verifiedAtEpochSeconds, int ttlMinutes) =>
            DateTimeOffset.UtcNow - DateTimeOffset.FromUnixTimeSeconds(verifiedAtEpochSeconds) <= TimeSpan.FromMinutes(ttlMinutes);
    }
}
