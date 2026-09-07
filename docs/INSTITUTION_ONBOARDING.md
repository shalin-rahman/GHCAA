# Standing up a new institution

This is for whoever deploys this app for an institution that is not Govt. Haraganga College Alumni
Association. It is a checklist of what to supply and in what shape, not a tour of the code. If you
are working on the app itself, read `docs/WHITE_LABEL_PLAN.md` and Work Package 62 in
`docs/TODO.md` instead.

## Read this first

Do not deploy this repository for a second institution until Work Package 62.31 and 82.31 are
closed. They are not closed yet. `docs/SEED_CLASSIFICATION.md` explains why: GHC's 631 real alumni
records, their password hashes, and their payment history are written as literal values into eight
committed EF Core migrations, not just into the seed JSON files a profile pack can override.
Running `dotnet ef database update` recreates that data no matter what `ORG_PROFILE` is set to.
Setting `ORG_PROFILE` changes what the app shows. It does not change what the migrations write to
the database. Until 62.31 and 82.31 are done, a second institution's database still ends up holding
GHC's real members' real data, and no config file can undo that after the fact.

## What you supply

A profile pack: a new folder `profiles/<your-name>/`, built from `profiles/default/` as a starting
point. `scripts/new-institution.mjs` is meant to scaffold this automatically, but it has not been
built yet (Work Package 62.39), so for now you copy the folder by hand.

| File | What it holds |
|---|---|
| `org-config.json` | Branding, contact details, currency, feature toggles, which payment gateways are enabled, and the bilingual locale pack (member terminology, EC role names) |
| `demo-data/*.json` | Leave these empty, the same shape `profiles/default/demo-data/` ships. This is your institution's own members, events, and history. Nobody else can supply it, so the app starts you with nothing here on purpose |
| `site-content.json`, `seo.json` | Static page text: about/purpose copy, terms and conditions, meta description |
| `assets/` | Logo, favicon, launcher icons, splash art |

You do not touch `lookups.json`, `roles.json`, or `email_templates.json`. Those are shared,
structural files every institution uses unchanged.

## What a fresh database looks like on day one

Once 62.31 and 82.31 are done: empty. No members, no events, no galleries, no news, no payment
history, no executive committee. Your first administrator account comes from
`appsettings.json`'s `AppSettings:ProtectedSuperAdmins` list, the same way GHC's `shalin` account is
set up today. It is not seeded from a file, so you name your own first admin in configuration.

## Settings that are yours to set

| Setting | What it controls |
|---|---|
| `ORG_PROFILE` | Which `profiles/<name>/` folder drives configuration. Leave it unset and the app uses `default`, the neutral sample pack, and logs a warning at boot to remind you |
| `ConnectionStrings:*` | Your own database |
| `GeneralSettings:PortalBaseUrl` (appsettings, optional) | Only needed if one environment needs a different portal URL than the profile pack's — a staging subdomain, say |
| `Jwt:Key` | Your own secret. Never copy GHC's |
| `AppSettings:ProtectedSuperAdmins` | Your first administrator's username(s) |
| Payment gateway sandbox/production URLs and the `PaymentConfigurations` database rows | Your own gateway credentials, never GHC's. No gateway keys belong in this repository |

If your institution has no local payment gateway, set `EnabledGatewayMethods: []` in your
`org-config.json` and leave every row in `PaymentConfigurations` disabled. The app already runs
fine on manual payment alone.

## What still assumes Bangladesh or GHC

Recorded here instead of hidden, since Work Package 62 in `docs/TODO.md` has the details:
- Currency and locale formatting have not been checked end to end against a non-BDT, non-Bengali
  profile (62.37).
- A few Angular templates still write GHC-specific text directly instead of reading it from the
  profile pack (62.18, 62.19 — most of this is fixed, a handful of spots remain).
- There is no automated test yet that boots the app with `ORG_PROFILE=default` and checks that
  nothing renders GHCAA branding (62.41). Until that exists, treat a second institution's launch as
  a manual pass through the Work Package 62 checklist in `docs/TODO.md`.
