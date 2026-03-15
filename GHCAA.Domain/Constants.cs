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

        public static class ConfigKeys
        {
            public const string MaxFileSizeBytes = "FileStorage:MaxFileSizeBytes";
            public const string UploadsRelativePath = "FileStorage:UploadsRelativePath";
            public const string SecureRelativePath = "FileStorage:SecureRelativePath";
            
            public const string ImageCompressionEnabled = "FileStorage:ImageCompression:Enabled";
            public const string ImageCompressionQuality = "FileStorage:ImageCompression:Quality";
            public const string ImageCompressionFallbackQuality = "FileStorage:ImageCompression:FallbackQuality";
            public const string ImageCompressionTargetSizeKB = "FileStorage:ImageCompression:TargetSizeKB";
            
            public const string AllowedOrigins = "AppSettings:AllowedOrigins";
            public const string ClientUrl = "AppSettings:ClientUrl";
        }

        public static class Branding
        {
            public const string AppName = "GHCAA";
            public const string OrganizationName = "Govt. Haraganga College Alumni Association";
            public const string Tagline = "Sharing Heritage, Aligning Lives, Integrating Networks";
            public const string RegisteredOffice = "Govt. Haraganga College Campus, Munshiganj, Bangladesh.";
        }

        public static class TemplateCodes
        {
            public const string Otp = "OTP_EMAIL";
            public const string Welcome = "WELCOME_EMAIL";
            public const string FeeReminder = "FEE_REMINDER";
            public const string PasswordReset = "PASSWORD_RESET";
        }

        public static class EmailSubjects
        {
            public const string PasswordReset = "GHCAA Account Password Reset";
            public const string OtpVerification = "GHCAA Verification Code";
        }

        public static class Defaults
        {
            public const long MaxFileSizeBytes = 52428800; // 50MB
            public const int ImageQuality = 85;
            public const int FallbackImageQuality = 70;
            public const int TargetImageSizeKB = 350;

            public const string ImportEmailBase = "haragangian";
            public const string MembershipPrefix = "GHC-";
            public const string UnknownValue = "Unknown";
            public const string ImportPrefix = "IMPORT";
            
            public const string SupportEmail = "haragangian@gmail.com";
            public const string LogoUrl = "https://www.haragangacollege.edu.bd/assets/logo.png";
        }
    }
}
