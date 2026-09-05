namespace GHCAA.Domain
{
    public static class Constants
    {
        public static class Roles
        {
            public const string SuperAdmin = "SuperAdmin";
            public const string Admin = "Admin";
            public const string Member = "Member";
        }

        // Authorization POLICY names (registered once in ServiceExtensions.AddAppAuthorization),
        // referenced from every [Authorize(Policy = ...)] attribute. Distinct from Roles above —
        // a policy can require more than one role, so the two are not interchangeable even where
        // a policy happens to share a role's name today.
        public static class Policies
        {
            public const string AdminOnly = "AdminOnly";
            public const string SuperAdminOnly = "SuperAdminOnly";
            public const string MemberOnly = "MemberOnly";
        }

        // Output Cache policy names (registered once in Program.cs's AddOutputCache), referenced
        // from every [OutputCache(PolicyName = ...)] attribute. Only [AllowAnonymous] endpoints
        // that return the same body to every caller may use one — see the comment above
        // AddOutputCache in Program.cs for why an authenticated route must never opt in.
        // Rate-limiter policy names (registered once in Program.cs's AddRateLimiter), referenced
        // from every [EnableRateLimiting] attribute and from the "api" fallback applied to
        // MapControllers().
        public static class RateLimitPolicies
        {
            public const string Auth = "auth";
            public const string Refresh = "refresh";
            public const string Registration = "registration";
            public const string Api = "api";
        }

        public static class OutputCachePolicies
        {
            public const string PublicReference = "PublicReference";
            public const string PublicContent = "PublicContent";
        }

        public static class ConfigKeys
        {
            public const string MaxFileSizeBytes = "FileStorage:MaxFileSizeBytes";
            public const string UploadsRelativePath = "FileStorage:UploadsRelativePath";
            public const string SecureRelativePath = "FileStorage:SecureRelativePath";

            public const string ImageCompressionEnabled = "FileStorage:ImageCompression:Enabled";
            public const string ImageCompressionQuality = "FileStorage:ImageCompression:Quality";
            public const string ImageCompressionFallbackQuality = "FileStorage:ImageCompression:FallbackQuality";
            public const string ImageCompressionTargetSizeKB = "FileStorage:ImageCompression:TargetSizeKB";
            public const string ImageCompressionMaxDimensionPx = "FileStorage:ImageCompression:MaxDimensionPx";

            public const string AllowedOrigins = "AppSettings:AllowedOrigins";
            public const string ClientUrl = "AppSettings:ClientUrl";
        }

        public static class TemplateCodes
        {
            public const string Otp = "OTP_EMAIL";
            public const string Welcome = "WELCOME_EMAIL";
            public const string FeeReminder = "FEE_REMINDER";
            public const string PasswordReset = "PASSWORD_RESET";
        }

        public static class Defaults
        {
            public const long MaxFileSizeBytes = 52428800; // 50MB
            public const int ImageQuality = 85;
            public const int FallbackImageQuality = 70;
            public const int TargetImageSizeKB = 350;
            public const int ImageMaxDimensionPx = 1920; // longest side; display images never need more

            // ImportEmailBase/MembershipPrefix moved to the org-config pack (ContactDto.ImportEmailBase,
            // BrandingDto.MembershipNumberPrefix) — see MemberImportService for the new consumer.
            public const string UnknownValue = "Unknown";
            public const string ImportPrefix = "IMPORT";
        }
    }
}
