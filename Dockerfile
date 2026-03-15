# See https://aka.ms/customizecontainer for more info on Docker customization.

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

# --- UNIT TEST STAGE (run tests after build so all assemblies exist) ---
RUN dotnet test "GHCAA.Tests/GHCAA.Tests.csproj" -c $BUILD_CONFIGURATION --no-restore --no-build

# --- PUBLISH STAGE ---
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
WORKDIR "/src"
RUN dotnet publish "GHCAA.API/GHCAA.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
ENV ASPNETCORE_ENVIRONMENT=Production
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "GHCAA.API.dll"]
