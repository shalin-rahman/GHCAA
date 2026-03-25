# Software Requirements Specification (SRS) - Alumni Association Web Portal (GHCAA)

This Software Requirements Specification (SRS) consolidates all fields, administrative logic, and association-specific features into a professional framework for the Alumni Association Web Portal.

---

## 1. Project Overview
The objective is a centralized web platform for the Alumni Association of Government Haraganga College (GHC). The system manages the entire member lifecycle—from initial application and payment verification to active networking and long-term archiving.

---

## 2. Functional Requirements (FR)

### 2.1 User Registration & Profile Management
The system provides a structured registration form with the following data sections:

#### A. Personal & Identity Information
- **Full Name, Father’s Name, Mother’s Name**: Required text fields.
- **Date of Birth**: Date picker (Validated; must match NID/Certificate; stored as UTC).
- **Gender & Blood Group**: Enum-driven dropdowns (Male/Female/Other; A+, B+, etc.).
- **NID No**: Numeric field (10, 13, or 17 digits; sanitized on input).
- **Mobile No**: 11-digit validated text field (Cleaned of formatting).
- **Email**: Unique text field (Verified via OTP/Link).
- **Present & Permanent Address**: Multi-line text.
- **Emergency Contact**: Multi-line text (Name, Relation, Phone Number).

#### B. Academic Information 
To maintain a lifetime academic record, the system uses a normalized history collection. Each record tracks:
- **Institution Name**: Required (e.g., Govt. Haraganga College).
- **Degree & Subject**: Dropdowns (HSC, Bachelor, Masters, etc. / Science, Humanities, etc.).
- **Admission & Passing Year**: Sequential validation (Passing ≥ Admission).
- **Result & Certificate Path**: Optional fields for verification documents.
- **Institutional Affiliation**: A Boolean flag (`IsGHC`) to track students who joined GHC. *Registration requires at least one GHC record.*

#### C. Professional Information
Professional history is tracked as a collection of roles to monitor career progression:
- **Organization Name & Designation**: Required fields for each role.
- **Professional Sector**: Categorized dropdown (Teaching, Business, IT, Medical, etc.).
- **Employment Period**: Start and End dates (Null if current).
- **Current Role**: A flag to identify the member's active occupation for directory search.

#### D. Attachments (Max 10MB each)
- **Passport Photo**: JPEG/PNG (Used for ID Card & Directory).
- **Academic Documents**: Linked directly to specific `AcademicRecord` entries.
- **Payment Proof**: Linked to `PaymentHistory` for registration/fee verification.

### 2.2 Administrative & Workflow Logic
The core of the portal is the transition from **Applicant** to **Official Member**.

1.  **Submission**: User submits the form. **Status = Applied**. `AppliedDate` is recorded.
2.  **Approval**: Admin verifies documents and payment. Upon "Approve" action:
    - `ApprovedDate` and `ApprovedBy` (Admin ID) are stamped.
    - **Membership Number** is auto-generated (e.g., `GHC-[PassingYear]-[Serial]`).
    - **Membership Status** changes to **Active**.
3.  **Login Swap**: Once approved, the Membership Number becomes the Login ID (Username).
4.  **Archiving**: The `IsArchived` flag (Default: False) allows for soft-deletion and access revocation.

---

## 3. Advanced Portal Modules

### 3.1 Financial & History Tracking
- **Payment History**: Multi-column table tracking `TrxID`, `Amount`, `Date`, and `Status`.
- **Membership History**: A log of changes in membership types (e.g., General to Life) or EC positions.
- **Annual Dues**: System tracks and notifies members of upcoming yearly fees based on `MembershipFeeConfig`.

### 3.2 Networking & Engagement
- **Member Directory**: Searchable database (Filter by Batch, Blood Group, or Profession) with PII privacy toggles.
- **Digital ID Card/Certificate**: System-generated printable documents using dynamic SVG templates.
- **Job & Mentorship Hub**: Section for alumni-posted jobs and career guidance.
- **Communication**: Admin tools for bulk Email/SMS alerts (Targeting by Batch, Membership Type, or custom lists).

---

## 4. Non-Functional Requirements (NFR)

| Category | Requirement |
| :--- | :--- |
| **Security** | Data encryption (BCrypt for passwords); Unique NID/Email/Mobile globally enforced; JWT stateless auth. |
| **Integrity** | `Passing Year` must be ≥ `Admission Year`; Mandatory degree check for GHC affiliation. |
| **Performance** | Server-side image optimization (350KB target); Infinite-scroll pagination for directory. |
| **Privacy** | Member-controlled visibility flags (Mask Phone, Address, NID, or Email). |
| **Accessibility** | Mobile-responsive UI; Optimized for low-bandwidth mobile connections. |

---

## 5. System State Logic & Lifecycle

| Status | Behavior / Permissions |
| :--- | :--- |
| **Applied** | (Default) User can view their application status in a restricted "Pending" view. Cannot access community. |
| **Active** | Full portal access. Login ID = Membership No. Visible in public/member directories. |
| **Inactive** | Profile hidden from directory. Login disabled (Dues unpaid/Resigned). |
| **Terminated**| Account locked permanently; records kept for history but inaccessible for user. |
| **IsArchived** | (Boolean) True → Record removed from all front-end views and searches; preserved in DB for SuperAdmin only. |

---

## 6. Project & Application State Persistence

### 1. Account & Authentication (Post-Approval)
- **Login ID**: Auto-populated with **Membership Number** (Post-approval).
- **Password**: Min 8 characters, Alpha-numeric, BCrypt hashed.
- **Role Assignment**: Automatically assigned **Member** role; Admin/SuperAdmin assigned via EC Management.

### 2. Administrative Audit Trail
- **Applied Date**: System Generated (Timestamp of initial submission).
- **Approved Date**: System Generated (Timestamp of status change to Active).
- **Approved By**: Admin ID trace.
- **Audit Logs**: Comprehensive tracking for all administrative actions (Approve, Reject, Archive).

### 3. Communication Workflow
- **Application Phase**: User registers using **Email** as a temporary identifier.
- **Approval Notification**: Automated email: *"Your application is approved. Your Membership Number is [No]. You can now log in using this number and your chosen password."*
- **Broadcasts**: Template-based messaging system for standardized communication.
