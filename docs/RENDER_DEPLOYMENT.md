# GHCAA — Render Deployment Guide (API + Web App, Neon DB)

> **One Render service** builds and serves **both** the .NET API and the Angular web app
> from the same origin. The database is **Neon** Postgres.
>
> **SECURITY:** This file and `docs/deploy_conn_Info.txt` (formerly `deploy_connection.txt`, renamed
> 2026-09-07 — both names are now gitignored) contain live credentials.
> After setup, **rotate the Neon password** and remove both files from the repo
> (see [Step 7](#step-7--security-cleanup)).

---

## What was changed in the code (already done)

| File | Change |
|---|---|
| `Dockerfile` | Multi-stage: Node 22 builds Angular → copied into the API's `wwwroot`. One image serves both. |
| `GHCAA.API/Extensions/StaticFilesExtensions.cs` | SPA fallback — serves `index.html` for non-`/api` routes so deep-links work on refresh. |
| `GHCAA.Web/src/environments/environment.preprod.ts` | `apiUrl` → `/api` (relative, same-origin). The preprod URL is `https://ghcaa-ryl6.onrender.com`. The old `preprod.haragangian.com` domain does not exist. |
| `.github/workflows/neon_workflow.yml` | Creates/deletes a Neon DB branch per pull request. |

Only the dashboard and Git steps below are needed. **No further code changes required.**

---

## Prerequisites

- A **Render** account → https://dashboard.render.com
- A **Neon** account/project (already created) → https://console.neon.tech
- Push access to this GitHub repo
- The two secret values (keep them handy):
  - **Neon DATABASE_URL** (from `docs/deploy_conn_Info.txt`):
    ```
    postgresql://neondb_owner:npg_keJCzc13FsIy@ep-green-fog-ax3l40f0-pooler.c-4.us-east-2.aws.neon.tech/GhcaaDB?sslmode=require&channel_binding=require
    ```
  - **JWT signing key** (32+ chars — generate a new one or use this one):
    ```
    ahpleDW6hI1zvQ/F2x0fo8+o0Y1KX44P5tKqPFvNSgiVJ5+se8OSYwhfGlND49hf
    ```
    > To generate a fresh one in PowerShell:
    > `[Convert]::ToBase64String((1..48 | % {Get-Random -Max 256}))`

---

## Step 1 — Push the code to the `preprod` branch

The Render deploy hook fires on pushes to **`preprod`**. The last failed deploy ran an
old `master` commit, so the fix must land on `preprod`.

```bash
# from the repo root
git checkout -b preprod        # or: git checkout preprod  (if it already exists)
git merge dev                  # bring in this session's changes (or commit them directly)
git push -u origin preprod
```

---

## Step 2 — Create the Render Web Service

1. Go to https://dashboard.render.com → **New +** → **Web Service**.
2. **Connect** this GitHub repository.
3. Fill in:
   - **Name:** `ghcaa` (or any name)
   - **Branch:** `preprod`
   - **Region:** Oregon (or nearest)
   - **Runtime / Language:** **Docker**
   - **Dockerfile Path:** `./Dockerfile` (root — leave default)
   - **Instance Type:** Free or Starter
   - **Auto-Deploy:** set to **Off** — see note below.
4. Click **Create Web Service** (it will start a first build — that's fine; we set env vars next).

> ### The Auto-Deploy dropdown — pick the right one
> Render's **Settings → Auto-Deploy** has three choices. What each does, and which to use:
>
> | Option | What Render does | Use it? |
> |---|---|---|
> | **On Commit** | Deploys immediately on *every* push to `preprod` — **before/ignoring** tests. | No — skips the test gate; also double-deploys with the CI hook. |
> | **After CI Checks Pass** | Waits for the GitHub Actions checks on the commit to go **green**, then deploys itself. | Alternative — clean & native. **This option needs the deploy-hook step deleted** from the workflow (see note), else it deploys twice. No `RENDER_DEPLOY_HOOK_URL` secret needed. |
> | **Off** | Render never auto-deploys; deploys only when its **Deploy Hook** is called. | **Recommended** — our CI (`ghcaa-ci-preprod.yml`) runs tests then curls the hook. Zero workflow changes; tests always gate the deploy. |
>
> **Recommended = Off**, because the workflow is already wired to fire the hook after tests pass —
> nothing else to change. Do **Step 4** (add the deploy-hook secret).
>
> **With "After CI Checks Pass" instead:** skip the hook — remove the whole `deploy-preprod`
> job from `.github/workflows/ghcaa-ci-preprod.yml` (the `RENDER_DEPLOY_HOOK_URL` is then not needed as a
> secret). Render then deploys on its own once the CI checks are green.
>
> **Never leave a native auto-deploy (On Commit / After CI Checks Pass) on *at the same time* as the
> CI deploy-hook — that deploys twice per push.** Exactly one path should be active.

---

## Step 3 — Set the environment variables

In the service → **Environment** tab → **Add Environment Variable**, add these five. The full list, including `ORG_PROFILE`, the Gmail and SMS
settings and `DOTNET_hostBuilder__reloadConfigOnChange`, with where each value comes from, is in
[`DEPLOYMENT_CHECKLIST.md`](DEPLOYMENT_CHECKLIST.md) section 1, one-time setup. The five below are the
ones this guide depends on:

| Key | Value |
|---|---|
| `Jwt__Key` | `ahpleDW6hI1zvQ/F2x0fo8+o0Y1KX44P5tKqPFvNSgiVJ5+se8OSYwhfGlND49hf` |
| `ASPNETCORE_ENVIRONMENT` | `Preprod` |
| `AppSettings__AllowedOrigins__0` | `https://ghcaa-ryl6.onrender.com` |
| `AppSettings__ClientUrl` | `https://ghcaa-ryl6.onrender.com` |
| `DATABASE_URL` | `postgresql://neondb_owner:npg_keJCzc13FsIy@ep-green-fog-ax3l40f0-pooler.c-4.us-east-2.aws.neon.tech/GhcaaDB?sslmode=require&channel_binding=require` |

> **`AppSettings__AllowedOrigins__0` and `AppSettings__ClientUrl`.** Outside Development the app refuses
> to start when `AllowedOrigins` is empty, has a wildcard or a bad entry, or `ClientUrl` is not in the list.
> The base `appsettings.json` list is empty. For preprod set both to `https://ghcaa-ryl6.onrender.com`.
> `ClientUrl` is also the host that `www.<host>` requests are redirected to (301 for GET and HEAD, 308 otherwise).

> **Why `Jwt__Key` (double underscore)?** .NET maps `Jwt__Key` → config key `Jwt:Key`.
> Outside Development the app **refuses to start** without it (exit 139) — this was the
> cause of the earlier deploy crash. It must be **32+ characters**.

Click **Save Changes** — Render redeploys automatically.

---

## Step 4 — Get the Render deploy hook (for CI auto-deploy)

1. Service → **Settings** → scroll to **Deploy Hook** → click the eye icon to reveal, then **Copy** the URL.
   - The current hook (already in `docs/deploy_conn_Info.txt`, line 8):
     `https://api.render.com/deploy/srv-d9brj1t7vvec73cc5npg?key=QyJsiWt9fYM`
   - This URL is a **secret** — anyone with it can trigger a deploy. If it has ever been shared/committed,
     click **Regenerate hook** and use the new one (then update the GitHub secret below).
2. In GitHub → repo → **Settings** → **Secrets and variables** → **Actions** → **New repository secret**:
   - Name: `RENDER_DEPLOY_HOOK_URL`
   - Value: *(paste the deploy hook URL)*
3. (Optional) Add secret `PREPROD_DATABASE_URL` = the Neon URL above (used only by the
   CI reachability pre-check in `ghcaa-ci-preprod.yml`).

**This is the whole automation:** every push to `preprod` → CI runs tests → on success it curls
this hook → Render rebuilds the Dockerfile (API + Angular) and rolls out. Nothing manual after this.

> The deploy job (`deploy-preprod` in the workflow) only runs on a **real push to `preprod`** —
> never on pull requests — so PRs are tested but not deployed.

---

## Step 5 — Configure the Neon PR-branch workflow

`neon_workflow.yml` spins up a throwaway Neon DB branch for each pull request. It needs:

Set `NEON_PROJECT_ID` as a GitHub Actions **variable** and `NEON_API_KEY` as a **secret**. Where to
find each one in the Neon console is in [`DEPLOYMENT_CHECKLIST.md`](DEPLOYMENT_CHECKLIST.md) section 1.

> If per-PR DB branches are not needed yet, this step can be skipped — it won't affect the
> main deploy. The workflow simply won't run successfully until these are set.

---

## Step 6 — Verify the deployment

Once Render shows **Live**, run the smoke test in [`DEPLOYMENT_CHECKLIST.md`](DEPLOYMENT_CHECKLIST.md)
section 4. If the service never goes Live, match the Render log against the table in section 3 of the
same file. Recovery steps for a failed migration, lost data or lost uploads are in
[`RECOVERY_RUNBOOK.md`](RECOVERY_RUNBOOK.md).

No manual migration step is needed. On an empty database the first boot creates the schema and seed
data, and every later boot applies any new migration through `MigrationBootstrapper`
([ADR-0002](adr/0002-migrations-apply-automatically-at-startup.md), migrations apply at startup).

---

## Step 7 — Security cleanup (do this!)

**Status as of 2026-10-06:** not done. Both `docs/deploy_connection.txt` and `docs/deploy_conn_Info.txt`
are gitignored, but this file is still tracked and still holds the live Neon password, JWT key and deploy
hook. On 2026-09-18 the user closed `docs/TODO.md` 48.2 (committed secrets) without rotating them or
purging history. Rotation needs dashboard access, so only the owner can do it.

```bash
# 1. Rotate the Neon password in Neon Console → Roles → reset password,
#    then update DATABASE_URL on Render and the PREPROD_DATABASE_URL GitHub secret.

# 2. Take the live values out of this file, or stop tracking it
git rm --cached docs/RENDER_DEPLOYMENT.md
git commit -m "chore: stop tracking files containing DB credentials"
git push
```

> Removing from tracking does **not** erase them from git **history**. If these were ever
> pushed to a shared/remote repo, treat the credentials as compromised and rotate them.

---

## Scaling beyond one instance

Keep the Render service at one instance. The output cache, the `OrgConfigService` and `ThemeService`
memory caches and SignalR chat all assume a single process. What has to change before a second instance
is in [ADR-0004](adr/0004-single-instance-deployment-constraint.md), the single-instance constraint.

---

## Quick reference

Every Render variable and GitHub secret, with where to get it, is in
[`DEPLOYMENT_CHECKLIST.md`](DEPLOYMENT_CHECKLIST.md) section 1. What each variable does in the code is in
[`ENV_REVIEW.md`](ENV_REVIEW.md) section 1.
