# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy project files indicating architecture dependencies
COPY ["GHCAA.API/GHCAA.API.csproj", "GHCAA.API/"]
COPY ["GHCAA.Application/GHCAA.Application.csproj", "GHCAA.Application/"]
COPY ["GHCAA.Domain/GHCAA.Domain.csproj", "GHCAA.Domain/"]
COPY ["GHCAA.Infrastructure/GHCAA.Infrastructure.csproj", "GHCAA.Infrastructure/"]
COPY ["GHCAA.Tests/GHCAA.Tests.csproj", "GHCAA.Tests/"]

# Restore everything
RUN dotnet restore "GHCAA.API/GHCAA.API.csproj"
RUN dotnet restore "GHCAA.Tests/GHCAA.Tests.csproj"

# Copy all the source code
COPY . .

# --- UNIT TEST STAGE ---
# Run tests first, breaking the build early if tests fail.
WORKDIR "/src/GHCAA.Tests"
RUN dotnet test "GHCAA.Tests.csproj" -c $BUILD_CONFIGURATION --no-restore

# --- BUILD STAGE ---
WORKDIR "/src/GHCAA.API"
RUN dotnet build "GHCAA.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "GHCAA.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
ENV ASPNETCORE_ENVIRONMENT=Production
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "GHCAA.API.dll"]
