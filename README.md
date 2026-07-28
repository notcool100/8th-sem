# NTB Event Platform

A comprehensive event management platform for the Nepal Tourism Board (NTB). This project is split into a .NET 8 Clean Architecture backend and a SvelteKit frontend.

## Overview

- **Backend (`/backend`)**: A robust API built with ASP.NET Core 8 following Clean Architecture principles. It uses PostgreSQL for the database with a mix of EF Core and Dapper for data access.
- **Frontend (`/frontend`)**: A modern, performant frontend built with SvelteKit and Vite.

## Prerequisites

Before you begin, ensure you have the following installed on your machine:
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js (v18+)](https://nodejs.org/) & [npm](https://www.npmjs.com/) or [pnpm](https://pnpm.io/)
- [PostgreSQL](https://www.postgresql.org/) (running locally on default port `5432`)

## Backend Setup & Running

The backend is configured to automatically run database migrations and seed default data on startup. **You do not need to run manual EF Core migration commands.**

1. **Database Configuration**
   By default, the backend expects a local PostgreSQL instance with the following credentials (defined in `backend/src/NtbEvent.Api/appsettings.Development.json`):
   ```
   Host=localhost;Port=5432;Database=ntb_event_dev;Username=postgres;Password=postgres
   ```
   Ensure your local PostgreSQL server is running and accessible with these credentials.

2. **Run the API (and Auto-Migrate/Seed)**
   Navigate to the API project directory and run the application:
   ```bash
   cd backend/src/NtbEvent.Api
   dotnet run
   ```
   On startup, the application will automatically:
   - Create the `ntb_event_dev` database (if it doesn't exist)
   - Provision all necessary tables (`users`, `refresh_tokens`, `events`, etc.)
   - Seed sample NTB events
   - Seed the default superadmin account

3. **Default Credentials**
   Once the backend seeds the database, you can log in with the following superadmin credentials:
   - **Email:** `superadmin@ntb.gov.np`
   - **Password:** `SuperAdmin123`

## Frontend Setup & Running

The SvelteKit frontend communicates with the .NET backend.

1. **Install Dependencies**
   Navigate to the frontend directory and install the required packages:
   ```bash
   cd frontend
   npm install
   # or `pnpm install`
   ```

2. **Run the Development Server**
   Start the SvelteKit development server:
   ```bash
   npm run dev
   # or `pnpm dev`
   ```
   The frontend will typically be accessible at `http://localhost:5173`. 

## Guest Invitations & QR Check-in

The platform lets an admin invite guests to an event and verify them at the entrance with a QR code.

**The flow**

1. **Invite** — From an event (the *Invite* action on the events list, or *Invite guests* on the event edit screen) the admin enters the guest's name, email, phone, and organization. The backend stores the guest, generates a unique token + QR code, and emails the guest a personal invitation link with the QR (shown inline and attached).
2. **Guest** — The guest opens their link (`/invite/<token>`) to view the event details and their QR code.
3. **Scan** — At the venue the admin opens **Event Check-in** and scans the QR with the camera (or pastes the code). A popup shows the guest's details for confirmation — scanning does **not** consume the QR.
4. **Verify** — The admin confirms check-in. The invitation is marked `verified` and the QR **expires**; re-scanning it is refused with *"already used."*

**Data model (normalized to 3NF)** — three tables: `invitation_guests` (one row per unique person, reused across events), `event_invitations` (event ↔ guest association holding the token, status, expiry and check-in audit), and `invitation_scans` (one audit row per scan attempt). These are created automatically by the `AddEventInvitations` migration on startup.

**Email configuration** — invitation emails are sent over SMTP. Configure the `Smtp` section in `appsettings.json` (for Gmail: `Host=smtp.gmail.com`, `Port=587`, `EnableSsl=true`, `Username=<address>`, `Password=<App Password>`). If `Smtp:Host` is left empty, the email is logged instead of sent, so the flow is fully testable in development. The guest invite links are built from `AppUrls:FrontendBaseUrl`.

**Key routes**

- `/admin/events/invitations/[id]` — manage an event's invitations (invite, resend, cancel, view QR)
- `/admin/check-in` — camera + manual QR scanning and verification
- `/invite/[token]` — public guest landing page

## API Endpoints Overview

The backend exposes several key endpoints for event and user management:
- `GET /health` - Health check
- `POST /api/auth/login` - Authenticate and receive JWT
- `GET /api/events` - Retrieve a list of events
- `POST /api/events` - Create a new event

Invitation & check-in endpoints (Admin/SuperAdmin unless noted):
- `POST /api/events/{eventId}/invitations` - Invite a guest (generates QR + emails them)
- `GET /api/events/{eventId}/invitations` - List an event's invitations
- `GET /api/invitations/{id}` - Invitation detail
- `GET /api/invitations/{id}/qr` - QR code PNG
- `POST /api/invitations/{id}/resend` - Re-send the invitation email
- `POST /api/invitations/scan` - Look up an invitation by scanned token (no consume)
- `POST /api/invitations/{id}/verify` - Confirm check-in and expire the QR
- `DELETE /api/invitations/{id}` - Cancel an invitation
- `GET /api/invitations/by-token/{token}` - Public guest landing data

*See the `backend/README.md` for a complete list of endpoints and more detailed backend architecture notes.*
