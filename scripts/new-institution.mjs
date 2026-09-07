/**
 * Scaffolds a new profile pack under profiles/<name>/ by copying profiles/default/ as
 * the template, then prompting for the values that make the copy usable for a real
 * institution instead of the generic sample pack: names, acronym, membership-number
 * prefix, addresses, brand colors, currency, and enabled payment gateways.
 *
 * This only fills in org-config.json (see GHCAA.Application/DTOs/OrgConfigDto.cs for the
 * shape it has to match). Everything else copied from default/ — demo-data/*.json,
 * site-content.json, seo.json, assets/ — is left as the generic template. Per
 * docs/INSTITUTION_ONBOARDING.md, those are the deployer's own to edit by hand: nobody
 * else can supply an institution's real members, page copy, or logo.
 *
 * Run: node scripts/new-institution.mjs [org-id]
 * With no argument it prompts for the org id (the folder name and the profile's ORG_PROFILE
 * value) first.
 */
import { cpSync, existsSync, mkdirSync, readFileSync, writeFileSync } from 'node:fs';
import { join } from 'node:path';
import { fileURLToPath } from 'node:url';
import { createInterface } from 'node:readline/promises';

const repoRoot = join(fileURLToPath(import.meta.url), '..', '..');
const profilesRoot = join(repoRoot, 'profiles');
const templateDir = join(profilesRoot, 'default');

const rl = createInterface({ input: process.stdin, output: process.stdout });

async function ask(question, fallback = '') {
    const suffix = fallback ? ` [${fallback}]` : '';
    const answer = (await rl.question(`${question}${suffix}: `)).trim();
    return answer || fallback;
}

function slugify(value) {
    return value.toLowerCase().trim().replace(/[^a-z0-9]+/g, '-').replace(/^-+|-+$/g, '');
}

async function main() {
    if (!existsSync(templateDir)) {
        console.error(`[new-institution] template not found: ${templateDir}`);
        process.exitCode = 1;
        return;
    }

    let orgId = process.argv[2] ? slugify(process.argv[2]) : '';
    while (!orgId) {
        orgId = slugify(await ask('Profile folder name (e.g. "riverside-college")'));
        if (orgId && existsSync(join(profilesRoot, orgId))) {
            console.log(`profiles/${orgId} already exists, pick another name`);
            orgId = '';
        }
    }

    const targetDir = join(profilesRoot, orgId);

    const fullName = await ask('Institution full name', 'Sample Alumni Association');
    const shortName = await ask('Short name (used in nav/branding)', fullName);
    const acronym = await ask('Institution acronym', '');
    const institutionName = await ask('Institution name (the college/university itself)', fullName.replace(/ Alumni.*/i, ''));
    const membershipPrefix = await ask('Membership number prefix (e.g. "GHC-")', `${acronym || 'MEM'}-`);
    const transactionPrefix = await ask('Payment transaction ID prefix', membershipPrefix);
    const registeredOffice = await ask('Registered office address', '');
    const campusAddress = await ask('Campus address', registeredOffice);
    const supportEmail = await ask('Support email address', 'support@example.org');
    const emailDomain = supportEmail.includes('@') ? supportEmail.split('@')[1] : 'example.org';
    const primaryColor = await ask('Primary brand color (hex)', '#121212');
    const accentColor = await ask('Accent brand color (hex)', '#2f6f4f');
    const currencyCode = await ask('Currency code (ISO 4217, e.g. "USD")', 'USD');
    const currencySymbol = await ask('Currency symbol', '$');
    const currencyName = await ask('Currency name', 'US Dollar');
    const gatewaysAnswer = await ask(
        'Enabled payment gateways, comma-separated (matches Enums.PaymentGateway; blank = manual payment only)',
        ''
    );
    const enabledGatewayMethods = gatewaysAnswer
        ? gatewaysAnswer.split(',').map(g => g.trim()).filter(Boolean)
        : [];

    rl.close();

    mkdirSync(profilesRoot, { recursive: true });
    cpSync(templateDir, targetDir, { recursive: true });

    const configPath = join(targetDir, 'org-config.json');
    const config = JSON.parse(readFileSync(configPath, 'utf-8'));

    config.OrgId = orgId;
    config.Branding.ShortName = shortName;
    config.Branding.FullName = fullName;
    config.Branding.InstitutionName = institutionName;
    config.Branding.InstitutionAcronym = acronym;
    config.Branding.MembershipNumberPrefix = membershipPrefix;
    config.Branding.TransactionPrefix = transactionPrefix;
    config.Branding.PrimaryColor = primaryColor;
    config.Branding.AccentColor = accentColor;
    config.Contact.SupportEmail = supportEmail;
    config.Contact.EmailDomain = emailDomain;
    config.Contact.RegisteredOffice = registeredOffice;
    config.Contact.CampusAddress = campusAddress;
    config.Currency.Code = currencyCode;
    config.Currency.Symbol = currencySymbol;
    config.Currency.Name = currencyName;
    config.EnabledGatewayMethods = enabledGatewayMethods;
    if (config.Localization?.Locales?.en) {
        config.Localization.Locales.en.OrgName = fullName;
    }

    writeFileSync(configPath, JSON.stringify(config, null, 2) + '\n', 'utf-8');

    console.log(`\n[new-institution] profiles/${orgId}/ created from profiles/default/.`);
    console.log(`[new-institution] org-config.json filled in. Still to do by hand, per docs/INSTITUTION_ONBOARDING.md:`);
    console.log(`  - demo-data/*.json (leave empty until 62.31/82.31 close the seeded-migration issue)`);
    console.log(`  - site-content.json, seo.json (page copy, meta description)`);
    console.log(`  - assets/ (logo, favicons)`);
    console.log(`Set ORG_PROFILE=${orgId} to use this profile.`);
}

main();
