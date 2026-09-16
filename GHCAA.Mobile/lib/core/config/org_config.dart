// ---------------------------------------------------------------------------
// OrgConfig Model Hierarchy — Matches OrgConfigDto.cs schema exactly
// ---------------------------------------------------------------------------

class SocialLinks {
  final String facebook;
  final String whatsapp;
  final String youtube;
  final String linkedin;
  final String instagram;

  SocialLinks({
    required this.facebook,
    required this.whatsapp,
    required this.youtube,
    this.linkedin = '#',
    this.instagram = '#',
  });

  factory SocialLinks.fromJson(Map<String, dynamic> json) {
    return SocialLinks(
      facebook: json['facebook'] ?? '#',
      whatsapp: json['whatsapp'] ?? '#',
      youtube: json['youtube'] ?? '#',
      linkedin: json['linkedin'] ?? '#',
      instagram: json['instagram'] ?? '#',
    );
  }
}

class OrgBranding {
  final String appName;
  final String shortName;
  final String fullName;
  final String memberNickname;
  final String institutionName;
  final String institutionAcronym;
  final String membershipNumberPrefix;
  final String transactionPrefix;
  final String approvalSeal;
  final String establishedOn;
  final String logoUrl;
  final String constitutionPdfUrl;
  final String primaryColor;
  final String accentColor;

  OrgBranding({
    required this.appName,
    required this.shortName,
    required this.fullName,
    required this.memberNickname,
    required this.institutionName,
    required this.institutionAcronym,
    required this.membershipNumberPrefix,
    required this.transactionPrefix,
    required this.approvalSeal,
    required this.establishedOn,
    required this.logoUrl,
    required this.constitutionPdfUrl,
    required this.primaryColor,
    required this.accentColor,
  });

  factory OrgBranding.fromJson(Map<String, dynamic> json) {
    return OrgBranding(
      appName: json['appName'] ?? '',
      shortName: json['shortName'] ?? '',
      fullName: json['fullName'] ?? '',
      memberNickname: json['memberNickname'] ?? '',
      institutionName: json['institutionName'] ?? '',
      institutionAcronym: json['institutionAcronym'] ?? '',
      membershipNumberPrefix: json['membershipNumberPrefix'] ?? '',
      transactionPrefix: json['transactionPrefix'] ?? '',
      approvalSeal: json['approvalSeal'] ?? '',
      establishedOn: json['establishedOn'] ?? '',
      logoUrl: json['logoUrl'] ?? '',
      constitutionPdfUrl: json['constitutionPdfUrl'] ?? '',
      primaryColor: json['primaryColor'] ?? '#121212',
      accentColor: json['accentColor'] ?? '#c5a059',
    );
  }
}

class OrgContact {
  final String supportEmail;
  final String importEmailBase;
  final String emailDomain;
  final String registeredOffice;
  final String campusAddress;
  final List<String> phoneNumbers;
  final String mapEmbedUrl;
  final String portalBaseUrl;
  final SocialLinks socialLinks;

  OrgContact({
    required this.supportEmail,
    required this.importEmailBase,
    required this.emailDomain,
    required this.registeredOffice,
    required this.portalBaseUrl,
    required this.socialLinks,
    this.campusAddress = '',
    this.phoneNumbers = const [],
    this.mapEmbedUrl = '',
  });

  factory OrgContact.fromJson(Map<String, dynamic> json) {
    return OrgContact(
      supportEmail: json['supportEmail'] ?? '',
      importEmailBase: json['importEmailBase'] ?? '',
      emailDomain: json['emailDomain'] ?? '',
      registeredOffice: json['registeredOffice'] ?? '',
      campusAddress: json['campusAddress'] ?? '',
      phoneNumbers:
          (json['phoneNumbers'] as List?)?.map((e) => e.toString()).toList() ??
              const [],
      mapEmbedUrl: json['mapEmbedUrl'] ?? '',
      portalBaseUrl: json['portalBaseUrl'] ?? '',
      socialLinks: SocialLinks.fromJson(json['socialLinks'] ?? {}),
    );
  }
}

class OrgCurrency {
  final String code;
  final String symbol;
  final String name;

  OrgCurrency({
    required this.code,
    required this.symbol,
    required this.name,
  });

  factory OrgCurrency.fromJson(Map<String, dynamic> json) {
    return OrgCurrency(
      code: json['code'] ?? 'BDT',
      symbol: json['symbol'] ?? '৳',
      name: json['name'] ?? 'Bangladeshi Taka',
    );
  }
}

class FeatureToggles {
  final bool enableEvents;
  final bool enableJobHub;
  final bool enableGallery;
  final bool enableForum;
  final bool enableMentorship;
  final bool enableFamilyLink;
  final bool enableMagazine;
  final bool enablePolls;
  final bool enableGamification;
  final bool enablePublicDirectory;
  final bool enableDigitalIdCard;
  final bool enableCertificates;
  final bool enableSocialAuth;
  final bool requirePaymentForMembership;
  final bool requireDocumentUpload;
  final bool allowSelfRegistration;
  final bool allowNonMemberEventRegistration;
  final bool enableFundraising;

  FeatureToggles({
    required this.enableEvents,
    required this.enableJobHub,
    required this.enableGallery,
    required this.enableForum,
    required this.enableMentorship,
    required this.enableFamilyLink,
    required this.enableMagazine,
    required this.enablePolls,
    required this.enableGamification,
    required this.enablePublicDirectory,
    required this.enableDigitalIdCard,
    required this.enableCertificates,
    required this.enableSocialAuth,
    required this.requirePaymentForMembership,
    required this.requireDocumentUpload,
    required this.allowSelfRegistration,
    required this.allowNonMemberEventRegistration,
    required this.enableFundraising,
  });

  factory FeatureToggles.fromJson(Map<String, dynamic> json) {
    return FeatureToggles(
      enableEvents: json['enableEvents'] ?? true,
      enableJobHub: json['enableJobHub'] ?? true,
      enableGallery: json['enableGallery'] ?? true,
      enableForum: json['enableForum'] ?? true,
      enableMentorship: json['enableMentorship'] ?? true,
      enableFamilyLink: json['enableFamilyLink'] ?? true,
      enableMagazine: json['enableMagazine'] ?? true,
      enablePolls: json['enablePolls'] ?? true,
      enableGamification: json['enableGamification'] ?? false,
      enablePublicDirectory: json['enablePublicDirectory'] ?? true,
      enableDigitalIdCard: json['enableDigitalIdCard'] ?? true,
      enableCertificates: json['enableCertificates'] ?? true,
      enableSocialAuth: json['enableSocialAuth'] ?? false,
      requirePaymentForMembership: json['requirePaymentForMembership'] ?? true,
      requireDocumentUpload: json['requireDocumentUpload'] ?? true,
      allowSelfRegistration: json['allowSelfRegistration'] ?? true,
      allowNonMemberEventRegistration:
          json['allowNonMemberEventRegistration'] ?? true,
      enableFundraising: json['enableFundraising'] ?? true,
    );
  }
}

class OrgWorkflow {
  final String memberApprovalMode;
  final bool otpVerificationRequired;
  final String defaultMembershipType;
  final bool adminEmailOnNewRegistration;

  OrgWorkflow({
    required this.memberApprovalMode,
    required this.otpVerificationRequired,
    required this.defaultMembershipType,
    required this.adminEmailOnNewRegistration,
  });

  factory OrgWorkflow.fromJson(Map<String, dynamic> json) {
    return OrgWorkflow(
      memberApprovalMode: json['memberApprovalMode'] ?? 'ManualReview',
      otpVerificationRequired: json['otpVerificationRequired'] ?? true,
      defaultMembershipType: json['defaultMembershipType'] ?? 'General',
      adminEmailOnNewRegistration: json['adminEmailOnNewRegistration'] ?? true,
    );
  }
}

/// Governing-document registry entry (bylaws, policies, amendment PDFs).
/// Matches DocumentEntryDto.cs / org-config.model.ts's DocumentEntry.
class DocumentEntry {
  final String label;
  final String url;
  final String version;
  final String group;

  DocumentEntry({
    required this.label,
    required this.url,
    required this.version,
    required this.group,
  });

  factory DocumentEntry.fromJson(Map<String, dynamic> json) {
    return DocumentEntry(
      label: json['label'] ?? '',
      url: json['url'] ?? '',
      version: json['version'] ?? '',
      group: json['group'] ?? '',
    );
  }
}

/// Navigation labels drawn from config — used by app_drawer.dart
class NavLabels {
  final String administration;
  final String myAccount;
  final String community;
  final String mediaAndTools;
  final String adminRoleLabel;
  final String memberRoleLabel;
  final String batchPrefix;

  NavLabels({
    required this.administration,
    required this.myAccount,
    required this.community,
    required this.mediaAndTools,
    required this.adminRoleLabel,
    required this.memberRoleLabel,
    required this.batchPrefix,
  });

  factory NavLabels.fromJson(Map<String, dynamic> json) {
    return NavLabels(
      administration: json['administration'] ?? 'ADMINISTRATION',
      myAccount: json['myAccount'] ?? 'MY ACCOUNT',
      community: json['community'] ?? 'COMMUNITY',
      mediaAndTools: json['mediaAndTools'] ?? 'MEDIA & TOOLS',
      adminRoleLabel: json['adminRoleLabel'] ?? 'ADMINISTRATOR',
      memberRoleLabel: json['memberRoleLabel'] ?? 'ALUMNI MEMBER',
      batchPrefix: json['batchPrefix'] ?? 'Batch: ',
    );
  }
}

/// Full locale pack matching backend LocalePackDto + NavLabelsDto
class LocalePack {
  final String orgName;
  final String tagline;
  final String memberLabel;
  final String memberPluralLabel;
  final String memberNickname;
  final String alumniLabel;
  final String membershipLabel;
  final Map<String, String> membershipTypeLabels;
  final Map<String, String> memberCategoryLabels;
  final Map<String, String> ecRoleLabels;
  final NavLabels nav;

  LocalePack({
    required this.orgName,
    required this.tagline,
    required this.memberLabel,
    required this.memberPluralLabel,
    required this.memberNickname,
    required this.alumniLabel,
    required this.membershipLabel,
    required this.membershipTypeLabels,
    required this.memberCategoryLabels,
    required this.ecRoleLabels,
    required this.nav,
  });

  factory LocalePack.fromJson(Map<String, dynamic> json) {
    return LocalePack(
      orgName: json['orgName'] ?? '',
      tagline: json['tagline'] ?? '',
      memberLabel: json['memberLabel'] ?? 'Member',
      memberPluralLabel: json['memberPluralLabel'] ?? 'Members',
      memberNickname: json['memberNickname'] ?? '',
      alumniLabel: json['alumniLabel'] ?? 'Alumni',
      membershipLabel: json['membershipLabel'] ?? 'Membership',
      membershipTypeLabels: _parseStringMap(json['membershipTypeLabels']),
      memberCategoryLabels: _parseStringMap(json['memberCategoryLabels']),
      ecRoleLabels: _parseStringMap(json['ecRoleLabels']),
      nav: NavLabels.fromJson(json['nav'] ?? {}),
    );
  }

  // Convenience accessors matching old LocalePack interface — delegates to nav
  String get administration => nav.administration;
  String get myAccount => nav.myAccount;
  String get community => nav.community;
  String get mediaAndTools => nav.mediaAndTools;
  String get adminRoleLabel => nav.adminRoleLabel;
  String get memberRoleLabel => nav.memberRoleLabel;
  String get batchPrefix => nav.batchPrefix;

  static Map<String, String> _parseStringMap(dynamic raw) {
    if (raw is Map) {
      return raw.map((k, v) => MapEntry(k.toString(), v.toString()));
    }
    return {};
  }
}

/// Supported user-facing date formats. Wire dates remain ISO-8601.
class DateFormatConfig {
  static const String ddMmYyyy = 'dd-MM-yyyy';
  static const String mmDdYyyy = 'MM/dd/yyyy';

  final String identifier;
  final String label;
  final String regex;

  const DateFormatConfig._(this.identifier, this.label, this.regex);

  static const DateFormatConfig ddMmYyyyConfig = DateFormatConfig._(ddMmYyyy,
      'DD-MM-YYYY', r'^(0[1-9]|[12][0-9]|3[01])-(0[1-9]|1[0-2])-\d{4}$');
  static const DateFormatConfig mmDdYyyyConfig = DateFormatConfig._(mmDdYyyy,
      'MM/DD/YYYY', r'^(0[1-9]|1[0-2])/(0[1-9]|[12][0-9]|3[01])/\d{4}$');

  static DateFormatConfig fromIdentifier(dynamic value) {
    final normalized = value?.toString().trim();
    if (normalized == mmDdYyyy ||
        normalized?.toUpperCase() == 'MM_DD_YYYY' ||
        normalized?.toUpperCase() == 'MM/DD/YYYY') {
      return mmDdYyyyConfig;
    }
    return ddMmYyyyConfig;
  }
}

// ---------------------------------------------------------------------------
// Root OrgConfig
// ---------------------------------------------------------------------------
class OrgConfig {
  final String orgId;
  final int schemaVersion;
  final OrgBranding branding;
  final OrgContact contact;
  final OrgCurrency currency;
  final List<String> enabledGatewayMethods;
  final FeatureToggles features;
  final OrgWorkflow workflow;
  final Map<String, LocalePack> locales;
  final DateFormatConfig dateFormat;
  final List<DocumentEntry> documents;

  OrgConfig({
    required this.orgId,
    required this.schemaVersion,
    required this.branding,
    required this.contact,
    required this.currency,
    required this.enabledGatewayMethods,
    required this.features,
    required this.workflow,
    required this.locales,
    required this.dateFormat,
    this.documents = const [],
  });

  factory OrgConfig.fromJson(Map<String, dynamic> json) {
    // The API returns locales nested under "localization.locales"
    final Map<String, dynamic> localizationJson = json['localization'] ?? {};
    final Map<String, dynamic> localesJson =
        localizationJson['locales'] ?? json['locales'] ?? {};
    final Map<String, LocalePack> parsedLocales = {};
    localesJson.forEach((key, val) {
      if (val is Map<String, dynamic>) {
        parsedLocales[key] = LocalePack.fromJson(val);
      }
    });

    return OrgConfig(
      orgId: json['orgId'] ?? '',
      schemaVersion: json['schemaVersion'] ?? 1,
      branding: OrgBranding.fromJson(json['branding'] ?? {}),
      contact: OrgContact.fromJson(json['contact'] ?? {}),
      currency: OrgCurrency.fromJson(json['currency'] ?? {}),
      enabledGatewayMethods: (json['enabledGatewayMethods'] as List?)
              ?.map((e) => e.toString())
              .toList() ??
          const [],
      features: FeatureToggles.fromJson(json['features'] ?? {}),
      workflow: OrgWorkflow.fromJson(json['workflow'] ?? {}),
      locales: parsedLocales,
      dateFormat: DateFormatConfig.fromIdentifier(
        json['dateFormat'] ??
            localizationJson['dateFormat'] ??
            localizationJson['displayDateFormat'],
      ),
      documents: (json['documents'] as List<dynamic>?)
              ?.map((e) => DocumentEntry.fromJson(e as Map<String, dynamic>))
              .toList() ??
          const [],
    );
  }

  /// Neutral offline fallback, used before the API config has ever loaded
  /// (first launch, no cache yet) or when both network and cache fail.
  /// Carries no institution's real branding — the real values always come
  /// from the API-fetched OrgConfig once that succeeds.
  static OrgConfig get offlineDefaults => OrgConfig(
        orgId: 'default',
        schemaVersion: 1,
        branding: OrgBranding(
          appName: 'Alumni Association',
          shortName: 'Alumni Association',
          fullName: 'Alumni Association',
          memberNickname: 'Member',
          institutionName: 'Institution',
          institutionAcronym: 'AA',
          membershipNumberPrefix: 'MEM-',
          transactionPrefix: '',
          approvalSeal: 'APPROVED',
          establishedOn: '',
          logoUrl: '', // empty -> OrgLogo renders the bundled asset directly, no network hit before real config loads
          constitutionPdfUrl: '',
          primaryColor: '#121212',
          accentColor: '#2f6f4f',
        ),
        contact: OrgContact(
          supportEmail: 'support@example.org',
          importEmailBase: 'member',
          emailDomain: 'example.org',
          registeredOffice: '',
          campusAddress: '',
          phoneNumbers: const [],
          mapEmbedUrl: '',
          portalBaseUrl: '',
          socialLinks: SocialLinks(facebook: '#', whatsapp: '#', youtube: '#'),
        ),
        currency: OrgCurrency(code: 'USD', symbol: '\$', name: 'US Dollar'),
        enabledGatewayMethods: const [],
        features: FeatureToggles(
          enableEvents: true,
          enableJobHub: true,
          enableGallery: true,
          enableForum: true,
          enableMentorship: true,
          enableFamilyLink: true,
          enableMagazine: true,
          enablePolls: true,
          enableGamification: false,
          enablePublicDirectory: true,
          enableDigitalIdCard: true,
          enableCertificates: true,
          enableSocialAuth: false,
          requirePaymentForMembership: true,
          requireDocumentUpload: true,
          allowSelfRegistration: true,
          allowNonMemberEventRegistration: true,
          enableFundraising: true,
        ),
        workflow: OrgWorkflow(
          memberApprovalMode: 'ManualReview',
          otpVerificationRequired: true,
          defaultMembershipType: 'General',
          adminEmailOnNewRegistration: true,
        ),
        dateFormat: DateFormatConfig.ddMmYyyyConfig,
        documents: const [],
        locales: {
          'en': LocalePack(
            orgName: 'Alumni Association',
            tagline: '',
            memberLabel: 'Member',
            memberPluralLabel: 'Members',
            memberNickname: 'Member',
            alumniLabel: 'Alumni',
            membershipLabel: 'Membership',
            membershipTypeLabels: {
              'Founding': 'Founding Member',
              'Executive': 'Executive Member',
              'General': 'General Member',
              'Associate': 'Associate Member',
              'Honorary': 'Honorary Member',
              'Advisory': 'Advisory Member',
              'Guest': 'Guest Member',
            },
            memberCategoryLabels: {
              'None': 'None',
              'LifelongPatron': 'Lifelong Patron',
              'Sponsor': 'Sponsor',
              'Advisor': 'Advisor',
              'Mentor': 'Mentor',
              'Recruiter': 'Recruiter',
              'Active': 'Active',
              'Volunteer': 'Volunteer',
              'Contributor': 'Contributor',
              'Guest': 'Guest',
              'Student': 'Student',
            },
            ecRoleLabels: {},
            nav: NavLabels(
              administration: 'ADMINISTRATION',
              myAccount: 'MY ACCOUNT',
              community: 'COMMUNITY',
              mediaAndTools: 'MEDIA & TOOLS',
              adminRoleLabel: 'ADMINISTRATOR',
              memberRoleLabel: 'MEMBER',
              batchPrefix: 'Batch: ',
            ),
          ),
        },
      );
}
