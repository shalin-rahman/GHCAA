/**
 * Generates src/app/core/config/org-config-fallback.generated.ts from the active
 * institution profile pack (profiles/<name>/org-config.json), so the boot-time fallback
 * OrgConfigService falls back to on API failure is sourced from the same profile pack
 * that drives the real API response, not hand-duplicated in TypeScript.
 *
 * The profile is chosen by ORG_PROFILE at build time (defaults to 'ghc', which is what
 * every live GHCAA build has used so far), falling back to profiles/default if the named
 * profile has no org-config.json. Runs as part of `npm run build` (see package.json).
 */
import { readFileSync, writeFileSync, existsSync, mkdirSync } from 'node:fs';
import { join, dirname, resolve } from 'node:path';
import { fileURLToPath } from 'node:url';

const webRoot = resolve(dirname(fileURLToPath(import.meta.url)), '..');
const profilesRoot = resolve(webRoot, '..', 'profiles');
const profileName = process.env.ORG_PROFILE || 'ghc';
const outFile = join(webRoot, 'src', 'app', 'core', 'config', 'org-config-fallback.generated.ts');

function profileConfigPath(name) {
    return join(profilesRoot, name, 'org-config.json');
}

let sourcePath = profileConfigPath(profileName);
if (!existsSync(sourcePath)) {
    console.warn(`[generate-org-config-fallback] no org-config.json for profile '${profileName}', falling back to 'default'`);
    sourcePath = profileConfigPath('default');
}
if (!existsSync(sourcePath)) {
    console.error(`[generate-org-config-fallback] source not found: ${sourcePath}`);
    process.exit(1);
}

const raw = JSON.parse(readFileSync(sourcePath, 'utf-8'));

// Maps the profile pack's PascalCase JSON shape onto the camelCase OrgConfig interface
// (src/app/core/models/org-config.model.ts). Written out field-by-field, rather than a
// generic PascalCase→camelCase walk, because dictionary keys like MembershipTypeLabels'
// "Founding"/"General" are enum names the app looks up verbatim and must not be recased.
function socialLinks(s) {
    return {
        facebook: s?.Facebook ?? '',
        whatsapp: s?.Whatsapp ?? '',
        youtube: s?.Youtube ?? '',
        linkedin: s?.Linkedin ?? '',
        instagram: s?.Instagram ?? ''
    };
}

function navLabels(n) {
    return {
        administration: n?.Administration ?? '',
        myAccount: n?.MyAccount ?? '',
        community: n?.Community ?? '',
        mediaAndTools: n?.MediaAndTools ?? '',
        adminRoleLabel: n?.AdminRoleLabel ?? '',
        memberRoleLabel: n?.MemberRoleLabel ?? '',
        batchPrefix: n?.BatchPrefix ?? ''
    };
}

function localePack(l) {
    return {
        orgName: l.OrgName,
        tagline: l.Tagline,
        memberLabel: l.MemberLabel,
        memberPluralLabel: l.MemberPluralLabel,
        memberNickname: l.MemberNickname,
        alumniLabel: l.AlumniLabel,
        membershipLabel: l.MembershipLabel,
        membershipTypeLabels: l.MembershipTypeLabels ?? {},
        memberCategoryLabels: l.MemberCategoryLabels ?? {},
        ecRoleLabels: l.EcRoleLabels ?? {},
        nav: navLabels(l.Nav)
    };
}

function buildFallback(cfg) {
    const locales = {};
    for (const [code, pack] of Object.entries(cfg.Localization?.Locales ?? {})) {
        locales[code] = localePack(pack);
    }

    return {
        orgId: cfg.OrgId,
        schemaVersion: cfg.SchemaVersion,
        branding: {
            appName: cfg.Branding.AppName,
            shortName: cfg.Branding.ShortName,
            fullName: cfg.Branding.FullName,
            memberNickname: cfg.Branding.MemberNickname,
            institutionName: cfg.Branding.InstitutionName,
            institutionAcronym: cfg.Branding.InstitutionAcronym,
            membershipNumberPrefix: cfg.Branding.MembershipNumberPrefix,
            transactionPrefix: cfg.Branding.TransactionPrefix,
            approvalSeal: cfg.Branding.ApprovalSeal,
            establishedOn: cfg.Branding.EstablishedOn,
            logoUrl: cfg.Branding.LogoUrl,
            constitutionPdfUrl: cfg.Branding.ConstitutionPdfUrl ?? '',
            primaryColor: cfg.Branding.PrimaryColor,
            accentColor: cfg.Branding.AccentColor
        },
        contact: {
            supportEmail: cfg.Contact.SupportEmail,
            importEmailBase: cfg.Contact.ImportEmailBase,
            emailDomain: cfg.Contact.EmailDomain ?? '',
            registeredOffice: cfg.Contact.RegisteredOffice,
            campusAddress: cfg.Contact.CampusAddress,
            phoneNumbers: cfg.Contact.PhoneNumbers ?? [],
            mapEmbedUrl: cfg.Contact.MapEmbedUrl ?? '',
            portalBaseUrl: cfg.Contact.PortalBaseUrl ?? '',
            socialLinks: socialLinks(cfg.Contact.SocialLinks)
        },
        currency: {
            code: cfg.Currency.Code,
            symbol: cfg.Currency.Symbol,
            name: cfg.Currency.Name
        },
        features: {
            enableEvents: !!cfg.Features.EnableEvents,
            enableJobHub: !!cfg.Features.EnableJobHub,
            enableGallery: !!cfg.Features.EnableGallery,
            enableForum: !!cfg.Features.EnableForum,
            enableMentorship: !!cfg.Features.EnableMentorship,
            enableFamilyLink: !!cfg.Features.EnableFamilyLink,
            enableMagazine: !!cfg.Features.EnableMagazine,
            enablePolls: !!cfg.Features.EnablePolls,
            enableGamification: !!cfg.Features.EnableGamification,
            enablePublicDirectory: !!cfg.Features.EnablePublicDirectory,
            enableDigitalIdCard: !!cfg.Features.EnableDigitalIdCard,
            enableCertificates: !!cfg.Features.EnableCertificates,
            enableSocialAuth: !!cfg.Features.EnableSocialAuth,
            requirePaymentForMembership: !!cfg.Features.RequirePaymentForMembership,
            requireDocumentUpload: !!cfg.Features.RequireDocumentUpload,
            allowSelfRegistration: !!cfg.Features.AllowSelfRegistration,
            allowNonMemberEventRegistration: !!cfg.Features.AllowNonMemberEventRegistration,
            enableFundraising: !!cfg.Features.EnableFundraising
        },
        workflow: {
            memberApprovalMode: cfg.Workflow.MemberApprovalMode,
            otpVerificationRequired: !!cfg.Workflow.OtpVerificationRequired,
            defaultMembershipType: cfg.Workflow.DefaultMembershipType,
            adminEmailOnNewRegistration: !!cfg.Workflow.AdminEmailOnNewRegistration,
            membershipTypes: cfg.Workflow.MembershipTypes ?? []
        },
        localization: {
            dateFormat: cfg.Localization?.DateFormat,
            locales
        }
    };
}

const fallback = buildFallback(raw);

const banner = '// GENERATED FILE — do not hand-edit.\n'
    + `// Produced by scripts/generate-org-config-fallback.mjs from profiles/${profileName}/org-config.json.\n`
    + '// Regenerate by running the build (see package.json "build" script) rather than editing this file.\n';

const body = `${banner}import { OrgConfig } from '../models/org-config.model';\n\n`
    + `export const ORG_CONFIG_FALLBACK: OrgConfig = ${JSON.stringify(fallback, null, 2)};\n`;

mkdirSync(dirname(outFile), { recursive: true });
writeFileSync(outFile, body, 'utf-8');
console.log(`[generate-org-config-fallback] wrote ${outFile} from profile '${profileName}'`);
