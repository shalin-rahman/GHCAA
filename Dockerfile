# See https://aka.ms/customizecontainer for more info on Docker customization.
#
# Base images below are pinned by digest (resolved via `docker buildx imagetools inspect
# <image>:<tag>` on 2026-09-04), not by floating tag, so a re-pointed tag on the registry can't
# change what actually builds. A pinned digest also means no patch releases reach the image
# automatically — refresh the digest deliberately (re-run the inspect command) rather than
# leaving it stale indefinitely; Dependabot's Docker ecosystem support can do this on a schedule.

# --- ANGULAR FRONTEND BUILD STAGE ---
# Builds the GHCAA.Web SPA and hands its static output to the API's wwwroot,
# so a single Render service serves both the API and the web app.
FROM node:22-alpine@sha256:c610fcdfb1d5b4740dd70c284ed3cb16bb857e0f7166196e36a5501df7a3aa32 AS web
# Which profiles/<name>/ drives index.html/SEO/site-content generation below (docs/TODO.md 62.17,
# 62.40). Defaults to 'ghc' so an unset --build-arg reproduces today's live GHCAA build exactly —
# a different institution's CI passes --build-arg ORG_PROFILE=<name> instead.
ARG ORG_PROFILE=ghc
ENV ORG_PROFILE=$ORG_PROFILE
WORKDIR /web
COPY ["GHCAA.Web/package.json", "GHCAA.Web/package-lock.json", "./"]
RUN npm ci
COPY GHCAA.Web/ ./
COPY profiles/ ../profiles/
# Governance documents live in docs/ (the source of truth) and are copied into
# public/assets/elections/ for the public /elections page. They are committed too, but
# regenerating here means a docs/ edit can never ship stale — see TODO 36.4.
COPY docs/Elections/ ../docs/Elections/
RUN npm run sync:docs
# gen:org-config / gen:site-content / apply-brand are the profile-driven prebuild steps `npm run
# build` normally chains (see package.json) — this Dockerfile calls `ng build` directly rather than
# through that script, so they need running explicitly. type-check is left to CI's own lint/test
# job rather than repeated here, consistent with the API side not re-running `dotnet test` in this
# image (see the restore comment below).
RUN npm run gen:org-config && npm run gen:site-content && npm run apply-brand
RUN npx ng build --configuration preprod
# @angular/build:application emits the browser bundle under dist/GHCAA.Web/browser

FROM mcr.microsoft.com/dotnet/aspnet:9.0@sha256:b1201ee0ccf9a22c06844982296c1be40d5ff9c7685dc002729f204e63fb1730 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0@sha256:f190d2dd9eef2899c91ac323caa0bd2b39334a5400ba93013e5199da39dad940 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Nothing below depends on the "web" stage's output until the final COPY --from=web near the
# bottom of this file, so without this line BuildKit schedules "web" and "build" concurrently —
# `ng build` and `dotnet restore`/`dotnet publish` peaking on the same fixed-memory Render build
# box at once. That is a strong candidate for the 8GB+ OOM on the 2026-09-07 deploy of 983c476,
# a plain refactor unlikely to have grown either build's own footprint on its own. This COPY of a
# throwaway file from "web" gives BuildKit a real dependency, forcing "web" to finish (and free
# its memory) before this stage starts. See also the two 2026-08-30 OOM fixes below (no test
# project, no separate build+publish pass) — this is the one remaining structural cause.
COPY --from=web /web/package.json /tmp/.web-stage-done

# Copy all .csproj files first (for layer caching)
COPY ["GHCAA.API/GHCAA.API.csproj", "GHCAA.API/"]
COPY ["GHCAA.Application/GHCAA.Application.csproj", "GHCAA.Application/"]
COPY ["GHCAA.Domain/GHCAA.Domain.csproj", "GHCAA.Domain/"]
COPY ["GHCAA.Infrastructure/GHCAA.Infrastructure.csproj", "GHCAA.Infrastructure/"]

# Restore the API only. GHCAA.Tests is deliberately never restored/built/tested in this image —
# `dotnet test` already runs as its own gating CI job (api-tests in ghcaa-ci-preprod.yml) on every
# push to preprod, before deploy-preprod ever triggers this Render build. Building+testing the full
# suite a second time here was pure redundant work with zero added safety, and running the NUnit
# suite's WebApplicationFactory-based integration tests (SpaStaticFileFactory boots the whole app
# per test) on Render's build machine is what pushed a preprod deploy over 8GB and OOM'd
# (2026-08-30, commit 5546a34). Removing it also makes every future deploy faster.
RUN dotnet restore "GHCAA.API/GHCAA.API.csproj"

# Copy all source code
COPY . .
WORKDIR "/src"

# --- PUBLISH STAGE ---
# `dotnet publish` already compiles the project itself, so a separate `dotnet build` step before
# it (as this Dockerfile used to have) makes the compiler — and ASP.NET Core's static web assets
# pipeline, which fingerprints every file under wwwroot — run twice over the same project for zero
# benefit once the Tests project isn't built here to justify an earlier error-surfacing pass (see
# the restore comment above). With wwwroot still carrying thousands of committed member-photo
# files at the time of the 2026-08-30 OOM investigation, doubling that pass was real, avoidable
# memory pressure on Render's build machine — publish directly instead of build-then-publish.
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
WORKDIR "/src"
RUN dotnet publish "GHCAA.API/GHCAA.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
ENV ASPNETCORE_ENVIRONMENT=Production
# Disable config-file FileSystemWatcher: Render containers hit the host inotify
# instance limit (128), crashing WebApplication.CreateBuilder at startup (exit 139).
ENV DOTNET_hostBuilder__reloadConfigOnChange=false
COPY --from=publish /app/publish .
# Copy the built Angular SPA into wwwroot so UseStaticFiles serves it at /
COPY --from=web /web/dist/GHCAA.Web/browser ./wwwroot
# Institution profile pack (docs/WHITE_LABEL_PLAN.md, docs/TODO.md Work Package 62) — data, not
# code, so it's copied as-is rather than published through the .csproj.
COPY profiles/ ./profiles/
ENTRYPOINT ["dotnet", "GHCAA.API.dll"]
