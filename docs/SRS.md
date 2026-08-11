# Software Requirements Specification (SRS) - GHCAA Platform

**Version:** 2.6
**Date:** 2026-04-24
**Status:** Comprehensive System Definition

---

## 1. Project Introduction

### 1.1 Purpose

The GHCAA (Govt. Haraganga College Alumni Association) Platform is an enterprise-grade digital ecosystem designed to unify the alumni network. It serves as the authoritative system for membership management, financial transparency, event orchestration, and professional networking.

### 1.2 Scope

The system encompasses a high-performance **REST API (ASP.NET Core)**, a feature-rich **Web Portal (Angular)**, and a native-feel **Mobile Application (Flutter)**. It manages the entire member lifecycle from registration to career-long engagement.

### 1.3 Vision

To create a "Midnight Gold" branded experience that evokes prestige and legacy, providing alumni with secure, intelligent, and real-time tools to connect and contribute to their alma mater.

---

## 2. System Architecture & Tech Stack

The platform follows **Clean Architecture** principles to ensure maintainability and scalability.

| Layer               | Technology            | Role                                         |
| :------------------ | :-------------------- | :------------------------------------------- |
| **Backend**   | ASP.NET Core 9.0 (C#) | RESTful API, RBAC, SignalR Hubs              |
| **Database**  | PostgreSQL 16+        | Relational storage with Global Query Filters |
| **Frontend**  | Angular 21 (Signals)  | Admin Command Center & Public Portal         |
| **Mobile**    | Flutter (Dart)        | Member Hub (iOS/Android)                     |
| **Assistant** | Rule-based engine (C#) | Intent/keyword classifier over internal data |
| **Real-time** | SignalR               | Instant Notifications & Peer Messaging       |
| **Auth**      | JWT + BCrypt          | Stateless Authentication & Secure Hashing    |

---

## 3. Functional Requirements (FR)

### 3.1 Membership & Identity Lifecycle

The core of the system is a high-integrity registration and verification workflow.

* **3.1.1 Registration Wizard**: A 3-step guided process (Personal, Academic, Professional, Attachments, Verification).
  * *Constraint*: Registration requires at least one record from "Govt. Haraganga College".
* **3.1.2 Approval Workflow**: Admins review "Applied" users. Upon approval:
  * A unique **Membership Number** is generated (e.g., `GHC-[Year]-[Serial]`).
  * System auto-provisions a **User Account** where the Membership Number becomes the Login ID.
* **3.1.3 Digital ID & Certificates**: Automatic generation of secure, QR-coded SVG/PDF ID cards and membership certificates for "Active" members.
* **3.1.4 Profile Management**: Granular control over personal data with member-defined **Privacy Toggles** (Masking NID, Email, Mobile, Address).
* **3.1.5 Social Authentication**: Integration with **Google** and **Facebook** for simplified login. The system automatically links social IDs to existing accounts via email or prompts for profile completion for new users.
* **3.1.6 Onboarding Workflow**: "Applied" members who login via social auth are restricted to a Profile Wizard. Full system access is gated until:
  1. **Profile Completion**: 100% of mandatory fields provided. The 13 required fields are:
     * Personal: `FullName`, `Email`, `MobileNo`, `DateOfBirth`, `Gender`, `NID`, `FatherName`, `MotherName`, `PermanentAddress`, `BloodGroup`.
     * Visual: `PhotoPath`.
     * Academic: At least one record from "Govt. Haraganga College" (`AcademicHistory`).
     * Professional: At least one professional experience record (`ProfessionalHistory`).
  2. **Financial Commitment**: Successful payment of registration/membership fees.
  3. **Admin Approval**: Formal verification by the association secretariat.

### 3.2 Event Management & Participation

A centralized hub for physical and virtual gathering orchestration.

* **3.2.1 Intelligent Catalog**: Discovery hub with capacity capping, deadlines, and guest entry policies.
* **3.2.2 Registration & Waitlisting**: Automated handling of participant limits and waitlist transitions.
* **3.2.3 QR Attendance Tracking**: Coordinators use the Mobile App to scan Member ID QR codes for real-time attendance logging.
* **3.2.4 Administrative Controls**: Management of event tasks, budgets, expenses, and gallery documentation.

### 3.3 Financial Governance & Payments

Ensuring absolute transparency and automated revenue collection.

* **3.3.1 Immutable Ledger**: Tracking all income (Dues, Donations, Fees) and expenses with audit-ready records.
* **3.3.2 Automated Dues**: Generation and notification of annual membership fees based on configurable types (General, Life, etc.).
* **3.3.3 Manual, Admin-Configurable Payments**: The platform operates without live payment-gateway credentials. Administrators configure wallet, bank, and mobile-financial-service (**bKash**, **Nagad**, bank transfer) instructions; members pay through the displayed channel and upload proof of payment, which an administrator verifies and approves. Automated gateway callback handling (**SSLCommerz**, **bKash**, **Nagad** webhooks) exists in the codebase and can be enabled later by supplying gateway keys, but is not required for operation.
* **3.3.4 Digital Receipts**: Instant generation of PDF tax receipts for all successful contributions.

### 3.4 Networking & Social Ecosystem

Fostering professional and personal growth within the network.

* **3.4.1 Smart Directory**: High-performance "Infinite Scroll" directory with batch, department, and professional filtering.
* **3.4.2 Peer-to-Peer Messaging**: Real-time SignalR-powered chat between members without exposing private contact info.
* **3.4.3 Job & Mentorship Hub**: Internal boards for sharing career opportunities and requesting formal mentorship.
* **3.4.4 Gamification**: "Contribution Points" and "Badges" (Legend, Elite, Active) awarded for engagement and attendance.

### 3.5 In-App Assistant (Haraganga Assistant)

* **3.5.1 Guided Search**: A rule-based intent/keyword classifier that maps member questions to internal lookups (e.g., finding alumni by batch or profession) over existing directory data — no external LLM dependency.
* **3.5.2 Policy Support**: Answers association rules and portal-navigation questions from a curated internal knowledge base. (The service is structured so a generative provider could be added later without changing the API surface.)

### 3.6 Governance & CMS

* **3.6.1 EC Management**: Management of Executive Committee terms, roles (President, GS, etc.), and historical governance records.
* **3.6.2 Constitution Hub**: Version-controlled governing documents with member voting capabilities.
* **3.6.3 Communication Hub**: Mass email/SMS engine with HTML templates and targeted segmenting (e.g., by batch or type).
* **3.6.4 Dynamic Themes**: UI transformations for special days (e.g., National Days) controlled via Admin settings.
* **3.6.5 Site Content Blocks**: Admin-editable, keyed content blocks (title, rich-text body, display order, active flag) that drive the public About Us page and the Contact page intro, so institutional copy is changed from the admin panel rather than by redeploy. Block bodies are sanitized server-side on every write; the public page retains a static fallback so it can never render empty.
* **3.6.6 News & Notice Board**: News and official notices share one entity discriminated by `PostType`, managed from a single admin screen and surfaced on one tab-filtered public/portal feed. Notices may carry a PDF attachment. **Notices may be posted by administrators only** — the member article-submission path rejects notice-typed posts from non-admin callers, while member *news* submissions continue to follow the existing approval workflow.
* **3.6.7 Contact Details Configuration**: On-campus address, phone numbers, support email, social links, and an optional map embed URL are held in organisation configuration and edited by SuperAdmin, giving web and mobile a single source of truth. The map URL is admin-supplied and therefore allow-list validated before being embedded.
* **3.6.8 Member Polls & Voting**: Admin-managed polling system for sentiment analysis and formal association decisions. Supports single/multiple choice, expiry dates, and real-time result visualization for members.

---

## 4. Non-Functional Requirements (NFR)

### 4.1 Security & Data Integrity

* **Zero "Ghost Data" Architecture**: All records use **Soft-Delete (IsArchived)**. Global Query Filters in EF Core ensure archived data never leaks to UI but remains available for audit.
* **Strict Deduplication**: Unique database indexes on NID, Mobile, and Email to prevent record collision.
* **Stateless Auth**: JWT-based session management with real-time **Security Stamp** validation to invalidate sessions if a user is terminated.
* **Rate Limiting**: Tiered limits (Auth: 5/min, Registration: 10/5min, API: 100/min) to prevent brute-force and DDoS.

### 4.2 Performance & UI/UX

* **Server-Side Optimization**: Automatic image compression (< 350KB) and SVG-first document generation.
* **Midnight Gold Aesthetic**: A premium design system with Glassmorphism, smooth micro-animations, and fast LCP (Largest Contentful Paint).
* **Resiliency**: Frontend interceptors to handle partial API failures gracefully without crashing user sessions.

---

## 5. System State Logic

| Status                      | Permissions                 | Directory Visibility                         |
| :-------------------------- | :-------------------------- | :------------------------------------------- |
| **Applied (Partial)** | Restricted Onboarding View  | Hidden                                       |
| **Applied (Paid)**    | Restricted "Pending Review" | Hidden                                       |
| **Active**            | Full Access                 | Full Visibility (respecting privacy toggles) |
| **Inactive**          | View Only / Access Denied   | Hidden                                       |
| **Terminated**        | Account Locked              | Hidden                                       |
| **IsArchived**        | System Hidden               | System Hidden (SuperAdmin Audit Only)        |

---

## 6. Infrastructure & Deployment

* **Storage**: Local file storage abstraction with Cloud (S3/Azure) interface readiness.
* **Automation**: Multi-stage **Sequential CI Pipeline** (Analysis -> API Tests -> UI Tests -> Mobile Tests -> Build).
* **Observability**: Tiered `ILogger` implementation with SignalR-based real-time admin analytics.

---

> [!IMPORTANT]
> This SRS is the definitive reference for the GHCAA Platform development. Any divergence from these requirements must be documented in the **[TODO.md](TODO.md)** backlog.
