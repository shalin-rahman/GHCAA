# GHCAA

Alumni Association Management System for Government Haraganga College.

## Features

- **Member Management**: Registration with document verification, ID card generation, and alumni directory.
- **Messaging**: Real-time private chat threads between alumni using SignalR.
- **Notifications**: System alerts for application status, news, and events.
- **Finance**: Membership dues tracking, ledger management, and CSV export.
- **Job Hub**: Portal for alumni to browse and post employment opportunities.
- **Event Management**: Configure upcoming alumni events with fee details, member participation tracking, payment reference/receipt verification, and automated email confirmations.

## Tech Stack

- **Backend**: .NET 8, EF Core, PostgreSQL, SignalR (WebSockets)
- **Frontend**: Angular 21, SCSS, Signals-based state management
- **Auth**: JWT-based authentication with role-based access control (RBAC)

## Getting Started

### Prerequisites

- .NET 8 SDK
- Node.js (v20 or higher)
- PostgreSQL

### Local Development

1. Configure the PostgreSQL connection string in `GHCAA.API/appsettings.json`.
2. Run the platform using the provided setup script:
   ```powershell
   ./run-app.ps1
   ```
   This launches both the .NET API and the Angular development server.

### Build and Deployment

A single script is used to prepare a production build:
```powershell
./CI-Deploy.ps1
```
The script performs the following tasks:
- Cleans existing build artifacts.
- Publishes the .NET project to the `publish/` folder.
- Runs database migrations via EF Core.
- Builds the Angular production bundle.
- Copies the frontend assets to the API's `wwwroot` for unified hosting.

## Development Scripts

- `run-app.ps1`: Starts both backend and frontend for local testing.
- `CI-Deploy.ps1`: Generates a production-ready deployment package in the `publish/` directory.
