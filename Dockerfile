# See https://aka.ms/customizecontainer for more info on Docker customization.

# --- ANGULAR FRONTEND BUILD STAGE ---
# Builds the GHCAA.Web SPA and hands its static output to the API's wwwroot,
# so a single Render service serves both the API and the web app.
FROM node:22-alpine AS web
WORKDIR /web
COPY ["GHCAA.Web/package.json", "GHCAA.Web/package-lock.json", "./"]
RUN npm ci
COPY GHCAA.Web/ ./
# Governance documents live in docs/ (the source of truth) and are copied into
# public/assets/elections/ for the public /elections page. They are committed too, but
# regenerating here means a docs/ edit can never ship stale — see TODO 36.4.
COPY docs/Elections/ ../docs/Elections/
RUN npm run sync:docs
RUN npx ng build --configuration preprod
# @angular/build:application emits the browser bundle under dist/GHCAA.Web/browser

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

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
