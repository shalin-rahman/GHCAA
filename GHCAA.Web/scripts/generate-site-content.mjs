/**
 * Generates src/app/core/config/site-content.generated.ts from the active institution
 * profile pack (profiles/<name>/site-content.json) — the static institution prose (about
 * page founding story, register T&C institution-specific clauses, etc.) that used to be
 * hardcoded directly in component templates.
 *
 * Same ORG_PROFILE selection as scripts/generate-org-config-fallback.mjs (defaults to
 * 'ghc', falls back to 'default'). Runs as part of `npm run build`.
 */
import { readFileSync, writeFileSync, existsSync, mkdirSync } from 'node:fs';
import { join, dirname, resolve } from 'node:path';
import { fileURLToPath } from 'node:url';

const webRoot = resolve(dirname(fileURLToPath(import.meta.url)), '..');
const profilesRoot = resolve(webRoot, '..', 'profiles');
const profileName = process.env.ORG_PROFILE || 'ghc';
const outFile = join(webRoot, 'src', 'app', 'core', 'config', 'site-content.generated.ts');

function profileContentPath(name) {
    return join(profilesRoot, name, 'site-content.json');
}

let sourcePath = profileContentPath(profileName);
if (!existsSync(sourcePath)) {
    console.warn(`[generate-site-content] no site-content.json for profile '${profileName}', falling back to 'default'`);
    sourcePath = profileContentPath('default');
}
if (!existsSync(sourcePath)) {
    console.error(`[generate-site-content] source not found: ${sourcePath}`);
    process.exit(1);
}

const raw = JSON.parse(readFileSync(sourcePath, 'utf-8'));

const content = {
    aboutFoundingStory: {
        tagline: raw.AboutFoundingStory?.Tagline ?? '',
        establishedBadge: raw.AboutFoundingStory?.EstablishedBadge ?? '',
        paragraphHtml: raw.AboutFoundingStory?.ParagraphHtml ?? ''
    },
    registerTerms: {
        effectiveDate: raw.RegisterTerms?.EffectiveDate ?? '',
        preambleParagraph: raw.RegisterTerms?.PreambleParagraph ?? '',
        eligibilityParagraph: raw.RegisterTerms?.EligibilityParagraph ?? '',
        verificationParagraph: raw.RegisterTerms?.VerificationParagraph ?? '',
        ipParagraph: raw.RegisterTerms?.IpParagraph ?? ''
    },
    purposeHistoryNote: raw.PurposeHistoryNote ?? '',
    assistantName: raw.AssistantName ?? '',
    membershipTierCopy: {
        foundingShortDesc: raw.MembershipTierCopy?.FoundingShortDesc ?? '',
        associateShortDesc: raw.MembershipTierCopy?.AssociateShortDesc ?? ''
    }
};

const banner = '// GENERATED FILE — do not hand-edit.\n'
    + `// Produced by scripts/generate-site-content.mjs from profiles/${profileName}/site-content.json.\n`
    + '// Regenerate by running the build (see package.json "build" script) rather than editing this file.\n';

const body = `${banner}import { SiteContentProfile } from '../models/site-content-profile.model';\n\n`
    + `export const SITE_CONTENT: SiteContentProfile = ${JSON.stringify(content, null, 2)};\n`;

mkdirSync(dirname(outFile), { recursive: true });
writeFileSync(outFile, body, 'utf-8');
console.log(`[generate-site-content] wrote ${outFile} from profile '${profileName}'`);
