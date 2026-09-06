/** Static institution prose from profiles/<name>/site-content.json, baked in at build time. */
export interface AboutFoundingStoryContent {
  tagline: string;
  establishedBadge: string;
  /** May contain `{branding.x}` placeholders — run through interpolateOrgTemplate before display. */
  paragraphHtml: string;
}

export interface RegisterTermsContent {
  effectiveDate: string;
  eligibilityParagraph: string;
  ipParagraph: string;
}

export interface MembershipTierCopyContent {
  /** May contain a `{branding.x}` placeholder — run through interpolateOrgTemplate before display. */
  foundingShortDesc: string;
  associateShortDesc: string;
}

export interface SiteContentProfile {
  aboutFoundingStory: AboutFoundingStoryContent;
  registerTerms: RegisterTermsContent;
  purposeHistoryNote: string;
  /** May contain a `{branding.x}` placeholder — run through interpolateOrgTemplate before display. */
  assistantName: string;
  membershipTierCopy: MembershipTierCopyContent;
}
