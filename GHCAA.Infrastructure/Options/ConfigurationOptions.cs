namespace GHCAA.Infrastructure.Options
{
    // Typed binding targets for appsettings.json sections that services used to read one key at a
    // time via IConfiguration["..."] / GetValue<T>(...). Bound in DependencyInjection.cs with
    // services.Configure<T>(configuration.GetSection("...")); a service takes IOptions<T> in its
    // constructor instead of IConfiguration. A missing/mistyped section still binds to a
    // default-valued instance (same as the old raw reads' fallback strings), but the property
    // name is now checked by the compiler instead of by hand at every call site.

    public class JwtOptions
    {
        // Key is intentionally not here — JwtSigningKeyResolver resolves the signing secret from
        // several sources (config, env var, per-environment fallback) and needs the raw
        // IConfiguration plus IHostEnvironment to do that; it stays as-is.
        public string Issuer { get; set; } = "GHCAA";
        public string Audience { get; set; } = "GHCAA";
    }

    public class GmailSettingsOptions
    {
        public string? Email { get; set; }
        public string? AppPassword { get; set; }
        public string Host { get; set; } = "smtp.gmail.com";
        public int Port { get; set; } = 587;
    }

    public class SmsSettingsOptions
    {
        public string Token { get; set; } = string.Empty;
        public string BaseUrl { get; set; } = "https://api.greenweb.com.bd/api.php";
    }

    public class OtpSettingsOptions
    {
        public int ExpiryMinutes { get; set; } = 10;
        public string? HashKey { get; set; }
    }

    public class ContactUsSettingsOptions
    {
        public string[] Recipients { get; set; } = [];
    }

    public class ImageCompressionOptions
    {
        public bool Enabled { get; set; } = true;
        public int Quality { get; set; } = Domain.Constants.Defaults.ImageQuality;
        public int FallbackQuality { get; set; } = Domain.Constants.Defaults.FallbackImageQuality;
        public int TargetSizeKB { get; set; } = Domain.Constants.Defaults.TargetImageSizeKB;
        public int MaxDimensionPx { get; set; } = Domain.Constants.Defaults.ImageMaxDimensionPx;
    }

    public class FileStorageOptions
    {
        public string? BasePhysicalPath { get; set; }
        public string UploadsRelativePath { get; set; } = "uploads/members";
        public string SecureRelativePath { get; set; } = "secure_uploads/members";
        public long MaxFileSizeBytes { get; set; } = Domain.Constants.Defaults.MaxFileSizeBytes;
        public ImageCompressionOptions ImageCompression { get; set; } = new();
    }

    public class SslCommerzOptions
    {
        public string SandboxUrl { get; set; } = "https://sandbox.sslcommerz.com";
        public string ProductionUrl { get; set; } = "https://securepay.sslcommerz.com";
    }

    public class BkashOptions
    {
        public string SandboxUrl { get; set; } = "https://checkout.sandbox.bka.sh/v1.2.0-beta/checkout";
        public string ProductionUrl { get; set; } = "https://checkout.pay.bka.sh/v1.2.0-beta/checkout";
        public string SandboxPassword { get; set; } = "sandbox_pass";
        public string ProductionPassword { get; set; } = "";
        public string Username { get; set; } = "sandbox_user";
    }

    public class DGePayOptions
    {
        public string SandboxUrl { get; set; } = "https://api-uat.dgepay.net/dipon/v3";
        public string ProductionUrl { get; set; } = "https://api.dgepay.net/dipon/v3";
    }

    // Mirrors the whole "AppSettings" section so it can be bound once.
    public class AppSettingsOptions
    {
        public string[] AllowedOrigins { get; set; } = [];
        public long MaxRequestBodySize { get; set; } = 104857600;
        public bool RecreateDatabaseOnStartup { get; set; }
        public string[] ProtectedSuperAdmins { get; set; } = [];
        public string ClientUrl { get; set; } = "http://localhost:4200";
    }

    // Mirrors "GeneralSettings". FinancialService reads SystemAdminId to attribute automated,
    // non-member-initiated actions (e.g. system-triggered payment status changes) to an admin user.
    public class GeneralSettingsOptions
    {
        public int SystemAdminId { get; set; } = 1;
    }
}
