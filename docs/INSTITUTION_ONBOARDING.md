# Standing up a new institution

This is for whoever deploys this application for an institution that is not Govt. Haraganga College
Alumni Association — a checklist of what to supply and in what shape, not a tour of the code. If you
are working on the app itself, `docs/WHITE_LABEL_PLAN.md` and Work Package 62 in `docs/TODO.md` cover
the mechanism; this page only covers the deployer-facing steps.

## The one hard blocker, first

**Do not deploy this repository for a second institution until Work Package 62.31 and 82.31 are
closed.** As of this writing they are not. `docs/SEED_CLASSIFICATION.md` explains why: GHC's 631 real
alumni records, their bcrypt password hashes, and their payment history are baked as literal
`InsertData` values into eight committed EF Core migrations, not just into the seed JSON files a
profile pack can override. Applying those migrations to a fresh database — which `dotnet ef database
update` does regardless of `ORG_PROFILE` — recreates that data whether or not you have set a profile.
Setting `ORG_PROFILE` to a new institution's name changes what the *application* shows; it does not
change what the *migration chain* writes to the database. Until 62.31/82.31 land, a second
institution's database will contain a real institution's real members' real data, which is not
something a config file can fix after the fact.

## What you supply

A profile pack: `profiles/<your-name>/`, built from `profiles/default/` as a starting point (or via
`scripts/new-institution.mjs` once Work Package 62.39 exists — it does not yet).

| File | What it holds |
|---|---|
| `org-config.json` | Branding, contact details, currency, feature toggles, enabled payment gateway methods, and the bilingual locale pack (member terminology, EC role names) |
| `demo-data/*.json` | Leave these as the empty/minimal shape `profiles/default/demo-data/` ships. This is Class 3 data per `docs/SEED_CLASSIFICATION.md` — your institution's actual members, events, and history, which only your institution can supply, and which the app is deliberately seeded empty for |
| `site-content.json`, `seo.json` | Static prose (about/purpose text, T&C, meta description) — see `docs/TODO.md` 62.18 for what still needs moving here from Angular templates |
| `assets/` | Logo, favicon, launcher icons, splash art |

You do **not** supply or touch: `lookups.json`, `roles.json`, `email_templates.json` — Class 1 in
`docs/SEED_CLASSIFICATION.md`, structural for every institution and shared unchanged.

## What a fresh database looks like on day one

Assuming 62.31/82.31 are resolved by the time you read this: empty. No members, no events, no
galleries, no news, no payment history, no executive committee. Your first administrator account comes
from `appsettings.json`'s `AppSettings:ProtectedSuperAdmins` list (see `ProtectedSuperAdminSeeder`), not
from a seed file — you name your own first admin in configuration, the same way GHC's `shalin` account
is named today.

## Environment variables and settings that are yours to set

| Setting | What it controls |
|---|---|
| `ORG_PROFILE` | Which `profiles/<name>/` folder drives configuration. Unset defaults to `default`, the neutral sample pack — see the warning `OrgConfigService` logs at boot if you forget this |
| `ConnectionStrings:*` | Your own database |
| `GeneralSettings:PortalBaseUrl` (appsettings, optional) | Overrides the profile pack's `Contact.PortalBaseUrl` only if you need an environment-specific value (e.g. a staging subdomain) distinct from the pack's production URL — see `docs/TODO.md` 62.11 |
| `Jwt:Key`, `DataProtection:KeyRingPath` | Your own secrets; never copy GHC's |
| `AppSettings:ProtectedSuperAdmins` | Your first administrator's username(s) |
| `PaymentGateways:SSLCommerz`/`Bkash`/etc. sandbox/production URLs, and the `PaymentConfigurations` DB rows | Your own gateway credentials, never GHC's — see the no-gateway-keys-in-the-repo rule (Work Package 29) |

If your institution has no local payment gateway integration, set `EnabledGatewayMethods: []` in your
`org-config.json` and enable no rows in `PaymentConfigurations` — the app already supports a
manual-payment-only path.

## What still assumes Bangladesh / GHC specifically

Recorded rather than hidden, per `docs/TODO.md`'s Work Package 62 items:
- Currency/locale formatting has not been end-to-end verified against a non-BDT, non-Bengali profile
  (62.37).
- A handful of Angular templates still carry literal GHC prose or a raw `/assets/logo.png` reference
  instead of reading from the profile pack (62.18, 62.19 in progress as of this writing).
- The Docker/CI pipeline does not yet build a per-profile Web image (62.40).
- There is no automated "boot with `ORG_PROFILE=default` and confirm nothing renders GHCAA branding"
  acceptance test yet (62.41) — until it exists, treat a second institution's launch as a manual QA
  pass against `docs/TODO.md`'s Work Package 62 checklist.
