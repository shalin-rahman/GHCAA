# Deployment checklist

Tick through this before, during and after every deploy of the API and web app to Render, and before
a mobile release. It covers what a person has to do by hand. The how-to for first-time setup is in
`RENDER_DEPLOYMENT.md`, every variable is explained in `ENV_REVIEW.md`, and what to do when a deploy
goes wrong is in `RECOVERY_RUNBOOK.md`.

Never paste a secret value into this file or any other tracked file. Secrets live in the Render
dashboard and in GitHub Actions secrets only.

Preprod is `https://ghcaa-ryl6.onrender.com`. Production will be `https://haragangian.com` once its DNS
is live. Wherever this list says `<origin>`, use the origin being deployed to.

## How a deploy happens

A push to the `preprod` branch runs `.github/workflows/ghcaa-ci-preprod.yml`. It runs the lint jobs,
the API, web and mobile tests, and an integrated build. Only when those pass does the
`deploy-preprod` job check that the database is reachable and call the Render deploy hook. Render then
builds the root `Dockerfile` (Angular built into the API's `wwwroot`) and starts the new container. On
boot the app applies any pending migration through `MigrationBootstrapper`.

Pull requests are tested but never deployed. Render's own Auto-Deploy must stay **Off**, or every push
deploys twice (see `RENDER_DEPLOYMENT.md` Step 2, the Auto-Deploy options).

## 1. One-time setup (check once per environment)

- [ ] Render service uses the Docker runtime, branch `preprod`, Dockerfile `./Dockerfile`. How to
      create it is in `RENDER_DEPLOYMENT.md` Step 2.
- [ ] Render Auto-Deploy is **Off**.
- [ ] The GitHub secrets and variables below are set.
- [ ] The Render environment variables below are set.

This section is the one list of every deploy value: where it comes from and where it has to go. What
each variable does inside the code, and its local default, is in `ENV_REVIEW.md` section 1, the
variable registry.

### Where to put things

- **Render variables:** dashboard.render.com, open the service, **Environment** tab, **Add
  Environment Variable**, then **Save Changes**. Saving redeploys the service.
- **GitHub secrets and variables:** the repository on GitHub, **Settings**, **Secrets and
  variables**, **Actions**. Secrets go on the **Secrets** tab and variables on the **Variables** tab.

### Render variables (the running app)

| Variable | Value | How to get it | What breaks without it |
|---|---|---|---|
| `ASPNETCORE_ENVIRONMENT` | `Preprod` (or `Production`) | Type it | Dev settings and dev keys get used |
| `ORG_PROFILE` | `ghc` | Type it. It names a folder under `profiles/` | Wrong branding and seed profile |
| `DATABASE_URL` | Postgres URL | Neon console (console.neon.tech), open the project, **Connect**, pick the database, copy the pooled connection string | No database |
| `Jwt__Key` | 32 or more random characters | Make one in PowerShell: `[Convert]::ToBase64String((1..48 \| % {Get-Random -Max 256}))`. Keep it only on Render; changing it signs everyone out | App will not start (exit 139) |
| `GmailSettings__Email` | the sending Gmail address | The Google account the association sends mail from | Email sending throws |
| `GmailSettings__AppPassword` | 16-character app password | Sign in to that Google account, turn on 2-Step Verification, then myaccount.google.com/apppasswords, create one, copy it without spaces | Email sending throws |
| `AppSettings__AllowedOrigins__0` | `<origin>` | Render shows the service URL at the top of the service page. For production use the custom domain | App will not start |
| `AppSettings__ClientUrl` | `<origin>`, same as above | Same as above | App will not start; payment returns and the `www` redirect go to the wrong host |
| `DOTNET_hostBuilder__reloadConfigOnChange` | `false` | Type it | File watchers on a read-only container |
| `SmsSettings__Token` | optional | The Greenweb SMS account dashboard | SMS is silently off |

`PORT` is set by Render itself; don't add it. AllowedOrigins rules, checked at startup outside
Development: no `*`, no empty list, each entry a bare `https://` origin with no path, query or
trailing slash, and ClientUrl must be in the list. Use `__0`, `__1` for more entries. Do not add
`www.` to the list; the API redirects `www` to the apex itself. The reasons are in `ENV_REVIEW.md`,
"Production origin".

### GitHub Actions secrets and variables (the pipeline)

| Name | Kind | How to get it | Used by |
|---|---|---|---|
| `RENDER_DEPLOY_HOOK_URL` | secret | Render service, **Settings**, **Deploy Hook**, reveal and copy. If the hook was ever shared or committed, press **Regenerate hook** and store the new one | `deploy-preprod` job; nothing deploys without it |
| `PREPROD_DATABASE_URL` | secret, optional | The same Neon connection string as `DATABASE_URL` | CI database reachability check; skipped with a warning when missing |
| `NEON_PROJECT_ID` | variable, optional | Neon console, the project, **Settings**, **Project ID** | `neon_workflow.yml`, one database branch per pull request |
| `NEON_API_KEY` | secret, optional | Neon console, **Account settings**, **API keys**, **Create API key** | `neon_workflow.yml` |
| `ANDROID_KEYSTORE_BASE64` | secret | The upload keystore file, as base64: `[Convert]::ToBase64String([IO.File]::ReadAllBytes("upload-keystore.jks"))`. A new app makes the keystore once with `keytool -genkey -v -keystore upload-keystore.jks -keyalg RSA -keysize 2048 -validity 10000 -alias upload`. Losing it means Play support has to reset the upload key | `mobile_deployment.yml`, Android build |
| `ANDROID_KEYSTORE_PASSWORD`, `ANDROID_KEY_PASSWORD` | secret | The passwords typed when the keystore was made | Android build |
| `ANDROID_KEY_ALIAS` | secret | The `-alias` used when the keystore was made (`upload` above) | Android build |
| `PLAY_STORE_SERVICE_ACCOUNT_JSON` | secret | Google Play Console, **Setup**, **API access**, link a Google Cloud service account, give it release rights, then in Google Cloud create a JSON key for it and paste the whole file | Upload to the Play internal track |
| `IOS_P12_CERTIFICATE`, `IOS_PROVISION_PROFILE` | secret | Apple Developer account: export the distribution certificate from Keychain as .p12, download the App Store provisioning profile, then base64 each file as above | `mobile_deployment.yml`, iOS build |
| `APP_STORE_ISSUER_ID`, `APP_STORE_API_KEY_ID`, `APP_STORE_API_KEY_P8` | secret | App Store Connect, **Users and Access**, **Integrations**, **App Store Connect API**, create a key. The .p8 file downloads only once | iOS upload |

### Local files that carry deploy values

| File | What to set | Notes |
|---|---|---|
| `GHCAA.Mobile/.env.preprod` | `BASE_API_URL=<origin>/api`, `ENVIRONMENT=preprod` | Tracked in git, so no secrets in it. Only `.env` is bundled into the app |
| root `.env.preprod` | the Render values, as a reference copy | Not read by the deployed app; Render variables win |

## 2. Before pushing

- [ ] All local suites pass:
      `dotnet test GHCAA.Tests/GHCAA.Tests.csproj`, `npm run test:unit -- --run` and
      `npm run type-check` in `GHCAA.Web`, `flutter test --exclude-tags golden` in `GHCAA.Mobile`.
- [ ] `dotnet format --verify-no-changes` is clean and `python docs/api/authz_catalog.py --check`
      passes. CI fails on either.
- [ ] `git diff --cached --ignore-cr-at-eol --stat` shows the expected size. A much bigger plain
      `--stat` means line endings flipped; fix that before pushing.
- [ ] If the push carries a new migration, read it for anything that drops a column or adds a unique
      index over existing rows. See `RECOVERY_RUNBOOK.md` Scenario 1, where the migrations that lose
      data on rollback are listed.
- [ ] If the push carries a migration, take a backup first. In the Neon console, create a branch
      from the main branch, which keeps a copy of the data at that moment. Nothing in this repo makes backups (see `RECOVERY_RUNBOOK.md` Scenario 2, data loss).
- [ ] If a new environment variable is needed, set it on Render **before** pushing, so the first
      boot of the new code already has it.
- [ ] Check no election is waiting for polling, polling or counting. If one is, hold any change to
      Render environment variables until it is declared. A change to AllowedOrigins during that time
      is written to the activity log with the frozen election's name, and auditors will ask why.
- [ ] Warn admins that uploads will be lost. Uploaded files sit on the container's disk, and Render
      wipes them on every redeploy and restart (see `RECOVERY_RUNBOOK.md` Scenario 3).

## 3. During the deploy

- [ ] Push to `preprod` and watch the "GHCAA Preprod CI/CD" run in GitHub Actions until
      "Deploy to Preprod (Render)" is green.
- [ ] If a test job fails, nothing deploys. Fix and push again; don't re-run the hook by hand to get
      around it.
- [ ] In the Render dashboard, watch the deploy log until the service shows **Live**.
- [ ] Search the Render log for `Migration bootstrap failed`. If it is there, the app refused to start
      because a migration failed. Stop and follow `RECOVERY_RUNBOOK.md` Scenario 1.
- [ ] If the service never goes Live, match the log against the table below.

| Log shows | Cause | Fix |
|---|---|---|
| `exit 139` at once | `Jwt__Key` missing or short | Set it, 32+ characters |
| AllowedOrigins or ClientUrl error at startup | Empty list, wildcard, bad entry, or ClientUrl not in the list | Fix the two variables in section 1 |
| `Kestrel BindAsync` or "Application is shutting down" | App not on Render's `PORT` | Make sure the deployed commit has the `PORT` binding in `Program.cs` |
| `Migration bootstrap failed; refusing to start` | A migration threw | `RECOVERY_RUNBOOK.md` Scenario 1 |
| `relation "..." does not exist` | Schema missing a table or column | `RECOVERY_RUNBOOK.md` Scenario 1 |
| Live, but `<origin>/` answers 401 | The SPA fallback lost `.AllowAnonymous()` | Roll back in Render, then fix `MapFallback` in `GHCAA.API/Extensions/StaticFilesExtensions.cs` |

## 4. After the deploy (smoke test)

- [ ] `<origin>/health` answers `Healthy`.
- [ ] `<origin>/` shows the web app, not a 401.
- [ ] Open a deep link such as `/portal/...` and press refresh. The page still loads.
- [ ] Sign in with an admin account. Step-up or OTP codes arrive by email. If not, check the
      `EmailLogs` table first; Render can block outgoing SMTP.
- [ ] Open the admin dashboard and one member list. Data loads, which proves the database link.
- [ ] Browser dev tools show no CORS errors and no `/api` 404s. API calls go to `/api` on the same
      origin.
- [ ] Hard refresh once. If old screens come back, `index.html` is being cached; see the
      Render headers before blaming the build.
- [ ] Once DNS for the domain is live:
      `curl -I https://www.<host>/x?y=1` answers 301 with `Location: https://<host>/x?y=1`.
- [ ] If an election is running, open its monitor page and confirm the phase and counts match what
      they were before the deploy.

## 5. Mobile release

The mobile app is not built by the preprod pipeline. `.github/workflows/mobile_deployment.yml` runs on
a `v*` tag or by hand.

- [ ] `GHCAA.Mobile/.env.preprod` has `BASE_API_URL=<origin>/api` and `ENVIRONMENT=preprod`. Only
      `.env` is bundled, so swap `.env.preprod` in before a local build.
- [ ] The commit being tagged has already passed the preprod CI run.
- [ ] The Android signing and Play secrets from section 1 are set, and the iOS ones if an iOS build
      is wanted.
- [ ] Bump `APP_VERSION` and the `pubspec.yaml` version, then push the tag.
- [ ] Install the internal-track build on a phone, sign in, and open one admin and one member screen.

## 6. If it goes wrong

- Migration failed, or the schema is stale: `RECOVERY_RUNBOOK.md` Scenario 1, a deploy's migration
  step failed.
- Data lost: `RECOVERY_RUNBOOK.md` Scenario 2.
- Uploads gone: `RECOVERY_RUNBOOK.md` Scenario 3. There is no restore; files must be re-uploaded.
- Bad code with no schema change: in Render, roll back to the previous deploy from the service's
  deploy list, then fix forward on `preprod`.
- Never roll back a migration without a backup. Several of them drop columns.

## 7. Do not

- Scale the Render service past one instance. Output cache, config caches and SignalR chat only work
  on a single instance (`docs/adr/0004-single-instance-deployment-constraint.md`).
- Commit `.env` files, connection strings, the JWT key or the deploy hook URL.
- Turn on Render Auto-Deploy while the CI deploy hook is in use.
- Change election rules settings to "fix" something during polling. The freeze refuses it; a
  SuperAdmin unlock is the only way, and it is logged.

## Open owner items (as of 2026-10-06)

- 7.22: preprod `AppSettings__AllowedOrigins__0` and `AppSettings__ClientUrl` on Render, and both
  `.env.preprod` files, still point at `preprod.haragangian.com`. Set them to
  `https://ghcaa-ryl6.onrender.com`. The current code will not start until this is done.
- 7.21: the `www` DNS record and the Render custom-domain redirect, then the curl check in section 4.
- Rotate the database password and deploy hook that sit in `RENDER_DEPLOYMENT.md` and its git
  history, then remove them from that file (`RENDER_DEPLOYMENT.md` Step 7, security cleanup).
