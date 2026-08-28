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
        // it, or a 15-minute TTL would in practice mean "re-verify almost every request".
        // 30 days of continued activity; logging out (or a fresh login) starts unverified again
        // since Login/Refresh only carry the claim forward, they never fabricate one.
        public const int DefaultTtlMinutes = 30 * 24 * 60;

        public static bool IsValid(long verifiedAtEpochSeconds, int ttlMinutes) =>
            DateTimeOffset.UtcNow - DateTimeOffset.FromUnixTimeSeconds(verifiedAtEpochSeconds) <= TimeSpan.FromMinutes(ttlMinutes);
    }
}
