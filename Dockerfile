# See https://aka.ms/customizecontainer for more info on Docker customization.

# --- ANGULAR FRONTEND BUILD STAGE ---
# Builds the GHCAA.Web SPA and hands its static output to the API's wwwroot,
# so a single Render service serves both the API and the web app.
FROM node:22-alpine AS web
WORKDIR /web
COPY ["GHCAA.Web/package.json", "GHCAA.Web/package-lock.json", "./"]
RUN npm ci
COPY GHCAA.Web/ ./
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
COPY ["GHCAA.Tests/GHCAA.Tests.csproj", "GHCAA.Tests/"]

# Restore all projects
RUN dotnet restore "GHCAA.API/GHCAA.API.csproj"
RUN dotnet restore "GHCAA.Tests/GHCAA.Tests.csproj"

# Copy all source code
COPY . .

# --- BUILD ENTIRE SOLUTION first (so all project references are compiled) ---
WORKDIR "/src"
RUN dotnet build "GHCAA.API/GHCAA.API.csproj" -c $BUILD_CONFIGURATION --no-restore
RUN dotnet build "GHCAA.Tests/GHCAA.Tests.csproj" -c $BUILD_CONFIGURATION --no-restore

# --- UNIT TEST STAGE (excludes filesystem-dependent tests incompatible with Linux container) ---
RUN dotnet test "GHCAA.Tests/GHCAA.Tests.csproj" -c $BUILD_CONFIGURATION --no-restore --no-build \
    --filter "FullyQualifiedName!~LocalFileStorageServiceTests"

# --- PUBLISH STAGE ---
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
ENTRYPOINT ["dotnet", "GHCAA.API.dll"]
