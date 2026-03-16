# GHCAA - Govt. Haraganga College Alumni Association Platform

A state-of-the-art, enterprise-grade alumni management system designed for Govt. Haraganga College. This platform provides a centralized hub for membership governance, professional networking, and financial transparency.

## 💻 Powered By

| Layer              | Technology Stack                                                                 |
|--------------------|---------------------------------------------------------------------------------|
| **Backend**        | .NET 9 (Web API), Entity Framework Core                                          |
| **Database**       | PostgreSQL 16+ (Dockerized in Prod)                                             |
| **Frontend**       | Angular 18 (Signals, Standalone Components)                                    |
| **Styling**        | Vanilla SCSS (Custom Design System), Glassmorphism                             |
| **Security**       | JWT, BCrypt.Net-Next, ASP.NET Core Identity (Custom Implementation)             |
| **DevOps**         | Docker, GitHub Actions, PowerShell Automation                                   |
| **Reporting**      | ClosedXML (Excel Integration)                                                   |
| **Payment**        | SSLCommerz, Nagad (Integration Ready)                                           |

---

## ⛩️ Portal Architecture & Scopes

The platform is divided into three distinct operational scopes, each tailored for specific user interactions.

### 🌐 1. Public Portal (The Front Gate)
*Open access for alumni and the general public.*
- **🏛️ Intelligent Landing Hub**: Royal "Midnight Gold" aesthetic with Dynamic Sections (Purpose, Symbolism, and EC Highlights).
- **📊 Real-time Stats Counter**: Live counters for total Registered Members, Batches represented, and Association Events.
- **📝 Automated Onboarding**: High-integrity 5-step registration wizard with:
  - **NID Sanitization**: Automatic space removal and duplicate checking.
  - **OTP Security**: Email-based verification before submission.
  - **Academic Validation**: Specific logic to ensure at least one GHC record exists.
- **🔍 Public Alumni Directory**: High-performance "Infinite Scroll" member list with Batch and Professional filtering.
- **📰 Press & Updates**: Real-time news feed and event calendar with archival support.

### 👤 2. Member Portal (The Alumni Hub)
*Secured area for approved alumni using NID-based credentials.*
- **📊 Personal Dashboard**: Visual overview of membership status and recent association activities.
- **🤖 Haraganga AI Assistant**: Natural Language (NLP) search bot helping members find alumni by batch, sector, or professional background.
- **🛠️ Self-Service Profile**:
  - **Dynamic Privacy**: Granular toggles to hide/show contact info (Mobile/Email/Address).
  - **History Management**: Self-updateable Academic and Professional records with document re-upload capabilities.
- **💬 Networking Engine**: 
  - **Peer Chat**: Secure messaging between members without exposing private contact data.
  - **Career Hub**: Access to Job Opportunities shared within the network.
- **💰 Financial Transparency**: 
  - Real-time tracking of Membership Dues and Payment History.
  - Manual payment proof upload for life membership upgrades.
- **🎟️ Event Participation**: Managed registration for alumni-only events with payment integration.

### ⚖️ 3. Admin Command Center (Governance & Management)
*High-privilege portal for the Executive Committee and System Admins.*
- **📈 Global Analytics**: Real-time dashboard with metrics on membership growth, financial balance, and pending workflows.
- **👨‍💼 Membership Governance**: 
  - **Side-by-Side Review**: Approval/Rejection interface with document verification.
  - **Auto-Account Creation**: Approval triggers NID-based credential generation and email dispatch.
  - **Data Retention**: Soft-delete "Archive" system with one-click restoration.
- **📂 Management Suites**:
  - **EC Management**: Track Governance periods, positions, and committee transitions.
  - **📧 Communication Hub**: Mass-messaging engine with HTML templates, batch-targeting (Passing Year), and custom email campaigns.
  - **🖼️ Media Gallery CMS**: Managed photo galleries with featured image support and event-linking.
  - **Payment Config**: UI to manage bKash/Nagad/Bank instructions and gateway settings.
- **⚡ Advanced Power Tools**:
  - **Bulk Import**: Excel-to-Database bridging with complex column mapping.
  - **Premium Export**: Automated Excel reports with formatted data labels (Blood Groups, Categories).
  - **Admin Search**: High-speed lookup using Member IDs or NIDs.

---

## 🏗️ System Architecture & Security
**Enterprise-Grade Stability**
- **FR 5.1: Soft-Delete Integrity**
  - Implementation of **Global Query Filters** across the entire database. Archiving a member automatically "hides" all related history, academic records, and communications to prevent orphans.
- **FR 5.2: Role-Based Access (RBAC)**
  - Granular permissions for SuperAdmin, Admin, Member, and Guest roles.
- **FR 5.3: Authentication Model**
  - **BCrypt** password hashing with salt-per-member security.
  - **JWT (JSON Web Tokens)** for secure, stateless session management.

## 🏗️ Developer Experience & Infrastructure

To ensure rapid development and system reliability, the project includes several specialized dev-features:

### 1. Automated Data Seeding & Transformation
- **Self-Healing Seeds**: JSON-based seed data (`members.json`, `users.json`) that can be automatically synchronized with the database during migration or startup.
- **Credential Transformation Utilities**: Custom C# scripts to batch-update existing member data (e.g., transforming legacy usernames into NID-based credentials with BCrypt hashing).

### 2. Technical Infrastructure
- **Global Data Persistence**: 
  - Centralized **soft-delete architecture** using EF Core query filters. Developers don't need to manually check `IsArchived` in every query; the system handles it at the model level.
- **Clean Architecture Implementation**:
  - Clear separation of concerns between **Domain Models**, **Application DTOs**, and **Infrastructure Services**.
- **Modern Styling System**:
  - A comprehensive **SCSS Design System** with CSS variables for colors, spacing, and glassmorphism tokens, allowing for instant global UI changes.

### 3. API & Tooling
- **Swagger/OpenAPI Integration**: Auto-generated interactive API documentation for testing and integration.
- **Member Import Utility**: Dynamic Excel-to-Database bridging service with column mapping and auto-generation of missing data for legacy records.
- **File Storage Abstraction**: Pluggable storage service for handling photos and certificates, currently supporting Local Storage with an interface for Cloud (S3/Azure) expansion.

---

## 📂 Project Structure

```bash
GHCAA/
├── GHCAA.API/             # REST API Controllers & Web Host
├── GHCAA.Application/     # Logic Contracts (Interfaces) & Data Transfer Objects (DTOs)
├── GHCAA.Domain/          # Core Entities, Enums, and Shared Constants
├── GHCAA.Infrastructure/  # DB Context, Migrations, Repositories, and Services
├── GHCAA.Web/             # Angular SPA (Frontend)
├── GHCAA.Tests/           # XUnit & Integration Test Suites
├── GHCAA.Export/          # Excel/CSV Generation Utilities
├── .github/workflows/    # CI/CD (GitHub Actions)
├── Dockerfile            # Production Orchestration
└── run-app.ps1           # Developer Bootstrapper
```

---

## 🏗️ Technical Architecture

The platform is built using **Clean Architecture** (Onion Architecture) principles, ensuring that the business logic is independent of external frameworks and databases.

### Layered Structure
- **Core (Domain)**: Pure C# project containing Entities (`Member`, `User`, `AlumniEvent`), Enums, and Core Constants. No external dependencies.
- **Application**: Defines interfaces (`IMemberService`, `IAuthService`) and Data Transfer Objects (DTOs). Contains the contract for the business logic.
- **Infrastructure**: Implementation of persistent storage (PostgreSQL via Entity Framework Core), File Storage (Local/Cloud), and external services (Email, OTP).
- **API (Web)**: ASP.NET Core RESTful controllers, Middleware (Rate Limiting, Exception Handling), and JWT Authentication.
- **Web (Frontend)**: Angular 18 Single Page Application (SPA) with a modular architecture and reactive state management.

### Data Flow & Persistence
- **Repository Pattern**: Centralized data access logic.
- **Global Filters**: Every database query is automatically intercepted to filter out `IsArchived` records, providing safety against "Ghost Data".
- **Database**: PostgreSQL with complex unique indexing on NID, Mobile, and Email to ensure zero-duplicate integrity.

---

## 🧪 Testing & Quality Assurance

The project maintains a rigorous quality standard organized into three distinct layers:

### 1. Unit Testing (`GHCAA.Tests`)
- **Business Logic**: Comprehensive tests for Member Approval, Membership Number Generation, and Financial calculations.
- **Service Mocking**: Utilization of Moq to isolate services and ensure deterministic test results.

### 2. Integration Testing
- **Persistence Testing**: In-memory database tests to verify that complex EF Core Query Filters and Unique Constraints are working as expected.
- **Schema Validation**: Automated migration testing to ensure seed data (`users.json`, `members.json`) remains compatible with the current schema.

### 3. API & UI Validation
- **Swagger UI**: Interactive playground for manual API verification.
- **Frontend Interceptors**: Automated error handling and token injection validation on the Angular side.

---

## 🚀 Deployment & Operations

### Containerization & CI/CD
- **Dockerized Environment**: Multi-stage `Dockerfile` for streamlined production builds and environment consistency.
- **CI/CD Pipeline**: 
  - **GitHub Actions**: Automated build, test, and linting on every push.
  - **PowerShell Automation**: `CI-Deploy.ps1` script for automated deployment workflows.
- **Environment Management**: `.env` and `appsettings.json` driven configuration for local, staging, and production secrecy.

### System Management
- **Process Control**: Dedicated `run-app.ps1` and `stop-app.ps1` scripts for managing local/server environments.
- **Logging**: Integrated `ILogger` with tiered severity levels (Information, Warning, Error) for real-time monitoring.
- **Database Maintenance**: SQL utilities included for periodic cleanup (`delete_imported_members.sql`) and password resetting (`reset_superadmin_password.sql`).

---

## 🛠️ Getting Started

### 1. Prerequisites
- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Node.js v20+](https://nodejs.org/)
- [PostgreSQL 16+](https://www.postgresql.org/)

### 2. Database Setup
1. Create a database named `GHCAA_DB`.
2. Update the connection string in `GHCAA.API/appsettings.json`.
3. Apply migrations:
   ```bash
   dotnet ef database update --project GHCAA.Infrastructure --startup-project GHCAA.API
   ```

### 3. Running the Application
**Option A: Automated (Win/PowerShell)**
```powershell
./run-app.ps1
```

**Option B: Manual**
- **Backend**: `cd GHCAA.API && dotnet run`
- **Frontend**: `cd GHCAA.Web && npm install && npm start`

---

## 🧪 Testing Coverage
The repository targets **>85% code coverage** on core business services.
- Run all tests: `dotnet test`
- View results: `test_results.txt` (generated automatically in CI)

---

## 🎨 Design Philosophy: "Midnight Gold"
The application adheres to a premium aesthetic designed to evoke prestige and legacy:
- **Visuals**: Dark mode base with royal gold highlights and glassmorphic panels.
- **Micro-animations**: Subtle hover transitions and container entry animations.
- **Performance**: Optimized for fast LCP (Largest Contentful Paint) and smooth rendering of large data lists.
