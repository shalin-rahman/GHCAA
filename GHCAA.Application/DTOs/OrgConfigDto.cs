namespace GHCAA.Application.DTOs
{
    public record OrgConfigDto
    {
        public string OrgId { get; init; } = "default";
        public int SchemaVersion { get; init; } = 1;
        public BrandingDto Branding { get; init; } = new();
        public ContactDto Contact { get; init; } = new();
        public CurrencyDto Currency { get; init; } = new();
        public FeatureToggleDto Features { get; init; } = new();
        public WorkflowDto Workflow { get; init; } = new();
        public LocalizationDto Localization { get; init; } = new();
    }

    public record BrandingDto
    {
        public string ShortName { get; init; } = string.Empty;
        public string FullName { get; init; } = string.Empty;
        public string MemberNickname { get; init; } = string.Empty;
        public string InstitutionName { get; init; } = string.Empty;
        public string InstitutionAcronym { get; init; } = string.Empty;
        public string MembershipNumberPrefix { get; init; } = string.Empty;
        public string ApprovalSeal { get; init; } = string.Empty;
        public string LogoUrl { get; init; } = string.Empty;
        public string PrimaryColor { get; init; } = "#121212";
        public string AccentColor { get; init; } = "#c5a059";
    }

    public record ContactDto
    {
        public string SupportEmail { get; init; } = string.Empty;
        public string ImportEmailBase { get; init; } = string.Empty;
        public string RegisteredOffice { get; init; } = string.Empty;
        public string PortalBaseUrl { get; init; } = string.Empty;
        public SocialLinksDto SocialLinks { get; init; } = new();
    }

    public record SocialLinksDto
    {
        public string Facebook { get; init; } = "#";
        public string Whatsapp { get; init; } = "#";
        public string Youtube { get; init; } = "#";
    }

    public record CurrencyDto
    {
        public string Code { get; init; } = "BDT";
        public string Symbol { get; init; } = "৳";
        public string Name { get; init; } = "Bangladeshi Taka";
    }

    public record FeatureToggleDto
    {
        public bool EnableEvents { get; init; } = true;
        public bool EnableJobHub { get; init; } = true;
        public bool EnableGallery { get; init; } = true;
        public bool EnableForum { get; init; } = true;
        public bool EnableMentorship { get; init; } = true;
        public bool EnableFamilyLink { get; init; } = true;
        public bool EnableMagazine { get; init; } = true;
        public bool EnablePolls { get; init; } = true;
        public bool EnableGamification { get; init; } = false;
        public bool EnablePublicDirectory { get; init; } = true;
        public bool EnableDigitalIdCard { get; init; } = true;
        public bool EnableCertificates { get; init; } = true;
        public bool EnableSocialAuth { get; init; } = false;
        public bool RequirePaymentForMembership { get; init; } = true;
        public bool RequireDocumentUpload { get; init; } = true;
        public bool AllowSelfRegistration { get; init; } = true;
        public bool AllowNonMemberEventRegistration { get; init; } = true;
    }

    public record WorkflowDto
    {
        public string MemberApprovalMode { get; init; } = "ManualReview";
        public bool OtpVerificationRequired { get; init; } = true;
        public string DefaultMembershipType { get; init; } = "General";
        public bool AdminEmailOnNewRegistration { get; init; } = true;
        public List<string> MembershipTypes { get; init; } = [];
    }

    public record LocalizationDto
    {
        public string DefaultLocale { get; init; } = "en";
        public List<string> SupportedLocales { get; init; } = ["en"];
        public Dictionary<string, LocalePackDto> Locales { get; init; } = [];
    }

    public record LocalePackDto
    {
        public string OrgName { get; init; } = string.Empty;
        public string Tagline { get; init; } = string.Empty;
        public string MemberLabel { get; init; } = string.Empty;
        public string MemberPluralLabel { get; init; } = string.Empty;
        public string MemberNickname { get; init; } = string.Empty;
        public string AlumniLabel { get; init; } = string.Empty;
        public string MembershipLabel { get; init; } = string.Empty;
        public Dictionary<string, string> MembershipTypeLabels { get; init; } = [];
        public Dictionary<string, string> MemberCategoryLabels { get; init; } = [];
        public Dictionary<string, string> EcRoleLabels { get; init; } = [];
        public NavLabelsDto Nav { get; init; } = new();
        public EmailSubjectsDto EmailSubjects { get; init; } = new();
    }

    public record NavLabelsDto
    {
        public string Administration { get; init; } = string.Empty;
        public string MyAccount { get; init; } = string.Empty;
        public string Community { get; init; } = string.Empty;
        public string MediaAndTools { get; init; } = string.Empty;
        public string AdminRoleLabel { get; init; } = string.Empty;
        public string MemberRoleLabel { get; init; } = string.Empty;
        public string BatchPrefix { get; init; } = "Batch: ";
    }

    public record EmailSubjectsDto
    {
        public string PasswordReset { get; init; } = string.Empty;
        public string OtpVerification { get; init; } = string.Empty;
        public string Welcome { get; init; } = string.Empty;
        public string FeeReminder { get; init; } = string.Empty;
    }
}
