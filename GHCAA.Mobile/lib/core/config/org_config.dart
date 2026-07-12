// ---------------------------------------------------------------------------
// OrgConfig Model Hierarchy — Matches OrgConfigDto.cs schema exactly
// ---------------------------------------------------------------------------

class SocialLinks {
  final String facebook;
  final String whatsapp;
  final String youtube;

  SocialLinks({
    required this.facebook,
    required this.whatsapp,
    required this.youtube,
  });

  factory SocialLinks.fromJson(Map<String, dynamic> json) {
    return SocialLinks(
      facebook: json['facebook'] ?? '#',
      whatsapp: json['whatsapp'] ?? '#',
      youtube: json['youtube'] ?? '#',
    );
  }
}

class OrgBranding {
  final String shortName;
  final String fullName;
  final String memberNickname;
  final String institutionName;
  final String institutionAcronym;
  final String membershipNumberPrefix;
  final String approvalSeal;
  final String logoUrl;
  final String primaryColor;
  final String accentColor;

  OrgBranding({
    required this.shortName,
    required this.fullName,
    required this.memberNickname,
    required this.institutionName,
    required this.institutionAcronym,
    required this.membershipNumberPrefix,
    required this.approvalSeal,
    required this.logoUrl,
    required this.primaryColor,
    required this.accentColor,
  });

  factory OrgBranding.fromJson(Map<String, dynamic> json) {
    return OrgBranding(
      shortName: json['shortName'] ?? '',
      fullName: json['fullName'] ?? '',
      memberNickname: json['memberNickname'] ?? '',
      institutionName: json['institutionName'] ?? '',
      institutionAcronym: json['institutionAcronym'] ?? '',
      membershipNumberPrefix: json['membershipNumberPrefix'] ?? '',
      approvalSeal: json['approvalSeal'] ?? '',
      logoUrl: json['logoUrl'] ?? '',
      primaryColor: json['primaryColor'] ?? '#121212',
      accentColor: json['accentColor'] ?? '#c5a059',
    );
  }
}

class OrgContact {
  final String supportEmail;
  final String importEmailBase;
  final String registeredOffice;
  final String portalBaseUrl;
  final SocialLinks socialLinks;

  OrgContact({
    required this.supportEmail,
    required this.importEmailBase,
    required this.registeredOffice,
    required this.portalBaseUrl,
    required this.socialLinks,
  });

  factory OrgContact.fromJson(Map<String, dynamic> json) {
    return OrgContact(
      supportEmail: json['supportEmail'] ?? '',
      importEmailBase: json['importEmailBase'] ?? '',
      registeredOffice: json['registeredOffice'] ?? '',
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
      allowNonMemberEventRegistration: json['allowNonMemberEventRegistration'] ?? true,
    );
  }
}

class OrgWorkflow {
  final String memberApprovalMode;
  final bool otpVerificationRequired;
  final String defaultMembershipType;
  final bool adminEmailOnNewRegistration;
  final List<String> membershipTypes;

  OrgWorkflow({
    required this.memberApprovalMode,
    required this.otpVerificationRequired,
    required this.defaultMembershipType,
    required this.adminEmailOnNewRegistration,
    required this.membershipTypes,
  });

  factory OrgWorkflow.fromJson(Map<String, dynamic> json) {
    return OrgWorkflow(
      memberApprovalMode: json['memberApprovalMode'] ?? 'ManualReview',
      otpVerificationRequired: json['otpVerificationRequired'] ?? true,
      defaultMembershipType: json['defaultMembershipType'] ?? 'General',
      adminEmailOnNewRegistration: json['adminEmailOnNewRegistration'] ?? true,
      membershipTypes: (json['membershipTypes'] as List<dynamic>?)
              ?.map((e) => e.toString())
              .toList() ??
          ['Founding', 'Executive', 'General', 'Associate', 'Honorary', 'Advisory', 'Guest'],
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

// ---------------------------------------------------------------------------
// Root OrgConfig
// ---------------------------------------------------------------------------
class OrgConfig {
  final String orgId;
  final int schemaVersion;
  final OrgBranding branding;
  final OrgContact contact;
  final OrgCurrency currency;
  final FeatureToggles features;
  final OrgWorkflow workflow;
  final Map<String, LocalePack> locales;

  OrgConfig({
    required this.orgId,
    required this.schemaVersion,
    required this.branding,
    required this.contact,
    required this.currency,
    required this.features,
    required this.workflow,
    required this.locales,
  });

  factory OrgConfig.fromJson(Map<String, dynamic> json) {
    // The API returns locales nested under "localization.locales"
    final Map<String, dynamic> localizationJson = json['localization'] ?? {};
    final Map<String, dynamic> localesJson = localizationJson['locales'] ?? json['locales'] ?? {};
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
      features: FeatureToggles.fromJson(json['features'] ?? {}),
      workflow: OrgWorkflow.fromJson(json['workflow'] ?? {}),
      locales: parsedLocales,
    );
  }

  /// Hardcoded GHCAA defaults — mirrors BuildGhcaaDefaults() in the backend.
  /// Used when both network and cache are unavailable.
  static OrgConfig get ghcaaDefaults => OrgConfig(
        orgId: 'ghcaa',
        schemaVersion: 1,
        branding: OrgBranding(
          shortName: 'GHCAA',
          fullName: 'Govt. Haraganga College Alumni Association',
          memberNickname: 'Haragangian',
          institutionName: 'Govt. Haraganga College',
          institutionAcronym: 'GHC',
          membershipNumberPrefix: 'GHC-',
          approvalSeal: 'GHC APPROVED',
          logoUrl: '/assets/logo.png',
          primaryColor: '#121212',
          accentColor: '#c5a059',
        ),
        contact: OrgContact(
          supportEmail: 'haragangian@gmail.com',
          importEmailBase: 'haragangian',
          registeredOffice: 'Govt. Haraganga College Campus, Munshiganj, Bangladesh.',
          portalBaseUrl: 'https://haragangian.com/portal',
          socialLinks: SocialLinks(facebook: '#', whatsapp: '#', youtube: '#'),
        ),
        currency: OrgCurrency(code: 'BDT', symbol: '৳', name: 'Bangladeshi Taka'),
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
        ),
        workflow: OrgWorkflow(
          memberApprovalMode: 'ManualReview',
          otpVerificationRequired: true,
          defaultMembershipType: 'General',
          adminEmailOnNewRegistration: true,
          membershipTypes: ['Founding', 'Executive', 'General', 'Associate', 'Honorary', 'Advisory', 'Guest'],
        ),
        locales: {
          'en': LocalePack(
            orgName: 'Govt. Haraganga College Alumni Association',
            tagline: 'Sharing Heritage, Aligning Lives, Integrating Networks',
            memberLabel: 'Member',
            memberPluralLabel: 'Members',
            memberNickname: 'Haragangian',
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
              'None': 'None', 'LifelongPatron': 'Lifelong Patron', 'Sponsor': 'Sponsor',
              'Advisor': 'Advisor', 'Mentor': 'Mentor', 'Recruiter': 'Recruiter',
              'Active': 'Active', 'Volunteer': 'Volunteer', 'Contributor': 'Contributor',
              'Guest': 'Guest', 'Student': 'Student',
            },
            ecRoleLabels: {},
            nav: NavLabels(
              administration: 'ADMINISTRATION',
              myAccount: 'MY ACCOUNT',
              community: 'COMMUNITY',
              mediaAndTools: 'MEDIA & TOOLS',
              adminRoleLabel: 'ADMINISTRATOR',
              memberRoleLabel: 'ALUMNI MEMBER',
              batchPrefix: 'Batch: ',
            ),
          ),
          'bn': LocalePack(
            orgName: 'সরকারি হারাগঙ্গা কলেজ প্রাক্তন ছাত্রছাত্রী সমিতি',
            tagline: 'ঐতিহ্যের বিনিময়, জীবনের সমন্বয় ও সংহতির সেতুবন্ধন',
            memberLabel: 'সদস্য',
            memberPluralLabel: 'সদস্যগণ',
            memberNickname: 'হারাগঙ্গিয়ান',
            alumniLabel: 'প্রাক্তন ছাত্রছাত্রী',
            membershipLabel: 'সদস্যপদ',
            membershipTypeLabels: {
              'Founding': 'প্রতিষ্ঠাতা সদস্য',
              'Executive': 'নির্বাহী সদস্য',
              'General': 'সাধারণ সদস্য',
              'Associate': 'সহযোগী সদস্য',
              'Honorary': 'সম্মানসূচক সদস্য',
              'Advisory': 'উপদেষ্টা সদস্য',
              'Guest': 'অতিথি সদস্য',
            },
            memberCategoryLabels: {
              'None': 'কোনোটি নয়', 'LifelongPatron': 'আজীবন পৃষ্ঠপোষক', 'Sponsor': 'স্পনসর',
              'Advisor': 'উপদেষ্টা', 'Mentor': 'মেন্টর', 'Recruiter': 'নিয়োগকর্তা',
              'Active': 'সক্রিয়', 'Volunteer': 'স্বেচ্ছাসেবক', 'Contributor': 'অবদানকারী',
              'Guest': 'অতিথি', 'Student': 'ছাত্র',
            },
            ecRoleLabels: {},
            nav: NavLabels(
              administration: 'প্রশাসন',
              myAccount: 'আমার অ্যাকাউন্ট',
              community: 'কমিউনিটি',
              mediaAndTools: 'মিডিয়া ও সরঞ্জাম',
              adminRoleLabel: 'প্রশাসক',
              memberRoleLabel: 'অ্যালামনাই সদস্য',
              batchPrefix: 'ব্যাচ: ',
            ),
          ),
        },
      );
}
