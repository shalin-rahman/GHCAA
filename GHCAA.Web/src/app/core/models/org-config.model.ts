export interface SocialLinks {
  facebook?: string;
  whatsapp?: string;
  youtube?: string;
  linkedin?: string;
  instagram?: string;
}

export interface OrgBranding {
  shortName: string;
  fullName: string;
  memberNickname: string;
  institutionName: string;
  institutionAcronym: string;
  membershipNumberPrefix: string;
  approvalSeal: string;
  /** Founding date as printed on letterhead — Constitution, Article I. */
  establishedOn: string;
  logoUrl: string;
  primaryColor: string;
  accentColor: string;
}

export interface OrgContact {
  supportEmail: string;
  importEmailBase: string;
  registeredOffice: string;
  campusAddress: string;
  phoneNumbers: string[];
  mapEmbedUrl: string;
  portalBaseUrl: string;
  socialLinks: SocialLinks;
}

export interface OrgCurrency {
  code: string;
  symbol: string;
  name: string;
}

export interface FeatureToggles {
  enableEvents: boolean;
  enableJobHub: boolean;
  enableGallery: boolean;
  enableForum: boolean;
  enableMentorship: boolean;
  enableFamilyLink: boolean;
  enableMagazine: boolean;
  enablePolls: boolean;
  enableGamification: boolean;
  enablePublicDirectory: boolean;
  enableDigitalIdCard: boolean;
  enableCertificates: boolean;
  enableSocialAuth: boolean;
  requirePaymentForMembership: boolean;
  requireDocumentUpload: boolean;
  allowSelfRegistration: boolean;
  allowNonMemberEventRegistration: boolean;
}

export interface OrgWorkflow {
  memberApprovalMode: string;
  otpVerificationRequired: boolean;
  defaultMembershipType: string;
  adminEmailOnNewRegistration: boolean;
  membershipTypes: string[];
}

export interface NavLabels {
  administration: string;
  myAccount: string;
  community: string;
  mediaAndTools: string;
  adminRoleLabel: string;
  memberRoleLabel: string;
  batchPrefix: string;
}

export interface LocalePack {
  orgName: string;
  tagline: string;
  memberLabel: string;
  memberPluralLabel: string;
  memberNickname: string;
  alumniLabel: string;
  membershipLabel: string;
  membershipTypeLabels: Record<string, string>;
  memberCategoryLabels: Record<string, string>;
  ecRoleLabels: Record<string, string>;
  nav: NavLabels;
}

export interface OrgLocalization {
  locales: Record<string, LocalePack>;
}

export interface OrgConfig {
  orgId: string;
  schemaVersion: number;
  branding: OrgBranding;
  contact: OrgContact;
  currency: OrgCurrency;
  features: FeatureToggles;
  workflow: OrgWorkflow;
  localization: OrgLocalization;
}
