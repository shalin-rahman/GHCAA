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
| `GHCAA.API/Program.cs` | SPA fallback — serves `index.html` for non-`/api` routes so deep-links work on refresh. |
| `GHCAA.Web/src/environments/environment.preprod.ts` | `apiUrl` → `/api` (relative, same-origin). Old `preprod.haragangian.com` domain does not exist. |
| `.github/workflows/neon_workflow.yml` | Creates/deletes a Neon DB branch per pull request. |

You only need to do the dashboard/Git steps below. **No further code changes required.**

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
  - **JWT signing key** (32+ chars — generate your own or use this one):
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

> If you want me (Claude) to commit + push for you, just say so and name the branch.

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
> | **After CI Checks Pass** | Waits for the GitHub Actions checks on the commit to go **green**, then deploys itself. | Alternative — clean & native. **If you use this, you must delete the deploy-hook step** from the workflow (see note), else it deploys twice. No `RENDER_DEPLOY_HOOK_URL` secret needed. |
> | **Off** | Render never auto-deploys; deploys only when its **Deploy Hook** is called. | **Recommended** — our CI (`ghcaa-ci-preprod.yml`) runs tests then curls the hook. Zero workflow changes; tests always gate the deploy. |
>
> **Recommended = Off**, because the workflow is already wired to fire the hook after tests pass —
> nothing else to change. Do **Step 4** (add the deploy-hook secret).
>
> **If you instead pick "After CI Checks Pass":** skip the hook — remove the whole `deploy-preprod`
> job from `.github/workflows/ghcaa-ci-preprod.yml` (and you don't need the `RENDER_DEPLOY_HOOK_URL`
> secret). Render will deploy on its own once the CI checks are green. Tell Claude and it'll make that edit.
>
> **Never leave a native auto-deploy (On Commit / After CI Checks Pass) on *at the same time* as the
> CI deploy-hook — that deploys twice per push.** Exactly one path should be active.

---

## Step 3 — Set the environment variables

In the service → **Environment** tab → **Add Environment Variable**, add these **three**:

| Key | Value |
|---|---|
| `Jwt__Key` | `ahpleDW6hI1zvQ/F2x0fo8+o0Y1KX44P5tKqPFvNSgiVJ5+se8OSYwhfGlND49hf` |
| `ASPNETCORE_ENVIRONMENT` | `Preprod` |
| `DATABASE_URL` | `postgresql://neondb_owner:npg_keJCzc13FsIy@ep-green-fog-ax3l40f0-pooler.c-4.us-east-2.aws.neon.tech/GhcaaDB?sslmode=require&channel_binding=require` |

> **Why `Jwt__Key` (double underscore)?** .NET maps `Jwt__Key` → config key `Jwt:Key`.
> Outside Development the app **refuses to start** without it (exit 139) — this was the
> cause of the earlier deploy crash. It must be **32+ characters**.

Click **Save Changes** — Render redeploys automatically.

---

## Step 4 — Get the Render deploy hook (for CI auto-deploy)

1. Service → **Settings** → scroll to **Deploy Hook** → click the eye icon to reveal, then **Copy** the URL.
   - Your current hook (already in `docs/deploy_conn_Info.txt`, line 8):
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

1. In **Neon Console** → your project → **Settings** → copy the **Project ID**.
2. Neon Console → **Account Settings** → **API Keys** → **Create API Key** → copy it.
3. In GitHub → repo **Settings** → **Secrets and variables** → **Actions**:
   - Tab **Variables** → **New repository variable**: `NEON_PROJECT_ID` = *(the Project ID)*
   - Tab **Secrets** → **New repository secret**: `NEON_API_KEY` = *(the API key)*

> If you don't need per-PR DB branches yet, you can skip this step — it won't affect the
> main deploy. The workflow simply won't run successfully until these are set.

---

## Step 6 — Verify the deployment

Once Render shows **Live**:

1. **API health:** open `https://<your-service>.onrender.com/health` → expect `Healthy`.
2. **Web app:** open `https://<your-service>.onrender.com/` → the Angular login screen loads.
3. **Deep-link refresh:** navigate into the app, then hit browser **refresh** on a route
   like `/portal/...` → it should still load (SPA fallback working).
4. **DB:** log in / load data → confirms the app reached Neon. First boot auto-creates the
   schema via `EnsureCreated()`. Every boot after that runs `MigrationBootstrapper`, which applies
   any migration added since via the real EF migrator (baselining migrations whose effect already
   exists in the schema rather than re-running them) — so later schema changes reach preprod without
   a manual migration step.

**Troubleshooting startup:**
- **`exit 139` immediately** → `Jwt__Key` is missing or under 32 chars (Step 3).
- **`Hosting failed to start` / `Kestrel BindAsync` / `TaskCanceledException`, "Application is shutting down"**
  → the app wasn't listening on Render's `PORT`. **Fixed in code**: `Program.cs` now binds to
  `http://0.0.0.0:$PORT` when the `PORT` env var is present (Render sets it automatically). Make sure
  the deployed commit includes this fix.
- **`relation "..." does not exist` in logs / login fails** → the schema wasn't created (or is missing a table/column added by a later migration). **Fixed in code**: `Program.cs` now calls `EnsureCreated()` on boot for non-Visual profiles (builds schema + seed on the empty Neon DB), and `MigrationBootstrapper` applies any migration added since (see above). Ensure the deployed commit includes both.
- **`GET /` returns 401 instead of the web app** → the SPA fallback was caught by the global `RequireAuthenticatedUser` policy. **Fixed in code**: the `MapFallback` now has `.AllowAnonymous()`.
- **Web app loads but API calls 404/CORS** → confirm same origin (the `/api` path), not an old absolute URL.

---

## Step 7 — Security cleanup (do this!)

**Status as of 2026-09-07:** half done. `docs/deploy_connection.txt` was removed from tracking, but its
replacement — `docs/deploy_conn_Info.txt` — carries the same live Render Postgres password and was left
untracked-but-not-ignored until this pass added both filenames to `.gitignore`. **Rotation has not
happened.** See `docs/TODO.md` 47.10/48.2/48.13 (`ONHOLD`, P0/P1) — the credentials already committed in
`deploy_connection.txt`'s git history are still exposed regardless of the working-tree rename, and
rotating them needs dashboard access no session has.

```bash
# 1. Rotate the Neon password in Neon Console → Roles → reset password,
#    then update DATABASE_URL on Render + the GitHub secret. STILL OUTSTANDING.

# 2. Stop tracking the secret files (docs/deploy_connection.txt done; RENDER_DEPLOYMENT.md itself not,
#    since it's the doc you're reading — untrack it once you no longer need it as a live reference)
git rm --cached docs/RENDER_DEPLOYMENT.md
git commit -m "chore: stop tracking files containing DB credentials"
git push
```

> Removing from tracking does **not** erase them from git **history**. If these were ever
> pushed to a shared/remote repo, treat the credentials as compromised and rotate them.

---

## Scaling beyond one instance

This deployment runs a single Render instance, and three mechanisms currently depend on that:

- **Output cache** — `Program.cs` registers `AddOutputCache()` with no distributed backing. A second
  instance would cache the same route independently; a public-content edit could clear one instance's
  cache and leave the other serving a stale page.
- **`OrgConfigService` / `ThemeService`** — both read `IMemoryCache` directly, in-process. A config or
  theme edit on one instance would not invalidate the other's cache.
- **SignalR (`ChatHub`)** — `AddSignalR()` has no backplane, and `ChatHub`'s connection map is a
  process-local `ConcurrentDictionary`. A client connected to one instance never receives a push
  triggered from the other.

Before adding a second instance, back the output cache and the two service caches with a distributed
store, and add a Redis (or equivalent) backplane to SignalR. See
`docs/adr/0004-single-instance-deployment-constraint.md` for the full reasoning.

---

## Quick reference — env vars at a glance

| Where | Key | Purpose |
|---|---|---|
| Render service | `Jwt__Key` | JWT signing key (32+ chars) — required or app won't boot |
| Render service | `ASPNETCORE_ENVIRONMENT` | `Preprod` |
| Render service | `DATABASE_URL` | Neon Postgres connection (parsed automatically) |
| GitHub secret | `RENDER_DEPLOY_HOOK_URL` | CI triggers Render redeploy on `preprod` push |
| GitHub secret | `PREPROD_DATABASE_URL` | (optional) CI DB reachability pre-check |
| GitHub variable | `NEON_PROJECT_ID` | Neon PR-branch workflow |
| GitHub secret | `NEON_API_KEY` | Neon PR-branch workflow |
