# NTB Event Backend

Clean-architecture backend for the `ntb-event` project using:

- `NtbEvent.Domain`: core entities and enums
- `NtbEvent.Application`: contracts, DTOs, filters, and event service logic
- `NtbEvent.Infrastructure`: Dapper `DataRepo`, Postgres repository, and schema/bootstrap logic
- `NtbEvent.Api`: HTTP API for public/admin event access

## PostgreSQL

Set the connection string through `appsettings.json` or an environment variable:

```bash
ConnectionStrings__Postgres="Host=localhost;Port=5432;Database=ntb_event;Username=postgres;Password=postgres"
```

The API initializes the `events` table on startup and seeds sample NTB events when the table is empty.
It also provisions the auth tables (`users`, `refresh_tokens`) and seeds a default superadmin account when missing.
## Database Migrations

The project uses Entity Framework Core for database schema management.

### Prerequisites
- Ensure the EF Core tools are installed:
```bash
 dotnet tool install --global dotnet-ef
```
- The `Microsoft.EntityFrameworkCore.Design` package should be referenced in the backend project.

### Adding a Migration
1. Make changes to your entity classes or DbContext.
2. Run:
```bash
 dotnet ef migrations add <MigrationName> --project ./src/NtbEvent.Infrastructure --startup-project ./src/NtbEvent.Api
```
   Adjust the paths if your project layout differs.

### Applying Migrations
To apply pending migrations to the PostgreSQL database:
```bash
 dotnet ef database update --project ./src/NtbEvent.Infrastructure --startup-project ./src/NtbEvent.Api
```

### Updating an Existing Table
- Modify the entity/model class.
- Add a new migration as described above.
- Run `dotnet ef database update` to apply changes.

### Rolling Back
To revert to a previous migration:
```bash
 dotnet ef database update <PreviousMigrationName>
```

For more commands see `dotnet ef --help`.


Default seeded superadmin credentials:

- Email: `superadmin@ntb.gov.np`
- Password: `SuperAdmin123`

Override those values through the `Seed` section in `appsettings*.json` or environment variables.

JWT settings live under the `Jwt` section and should be overridden with a strong secret outside local development.

## Invitation email & URLs configuration

Two additional `appsettings.json` sections drive the guest invitation feature:

- **`AppUrls:FrontendBaseUrl`** — the public frontend base (e.g. `http://localhost:5173`) used to build the guest invite links that are emailed and encoded in the QR.
- **`Smtp`** — SMTP settings for sending invitation emails:

```jsonc
"Smtp": {
  "Host": "",                 // empty → emails are logged instead of sent (dev-friendly)
  "Port": 587,
  "EnableSsl": true,
  "Username": "",
  "Password": "",             // for Gmail, use a Google App Password
  "FromAddress": "no-reply@ntb.gov.np",
  "FromName": "Nepal Tourism Board"
}
```

When `Smtp:Host` is empty, `SmtpEmailService` logs the message and returns successfully, so
the full invite flow is testable without real credentials. For Gmail use
`Host=smtp.gmail.com`, `Port=587`, `EnableSsl=true`, `Username=<address>`,
`Password=<App Password>`.

## Invitations & QR Check-in

Lets an admin invite guests to an event and verify them at the door with a QR code:
invite → email link + QR → guest shows QR → admin scans (preview popup) → admin verifies →
QR is consumed/expired. The feature spans all layers — `Guest` / `Invitation` /
`InvitationScan` entities (Domain), `InvitationService` and its contracts (Application),
`GuestRepository` / `InvitationRepository` / `QrCodeService` (QRCoder) / `SmtpEmailService`
(Infrastructure), and `InvitationsController` (Api).

**Tables (normalized to 3NF), created by the `AddEventInvitations` migration:**

- `invitation_guests` — one row per unique person (by normalized email), reused across events.
- `event_invitations` — event ↔ guest association holding the token, status, expiry and check-in audit; unique `(event_id, guest_id)` and unique `token`.
- `invitation_scans` — one audit row per scan attempt.

Status lifecycle: `pending → sent → verified`, with `expired` and `cancelled` terminal. A
verified QR cannot be reused; the scan endpoint accepts either the bare token or a full
`…/invite/<token>` URL.

## API Documentation & Testing (Swagger)

When running the application in Development environment, Swagger UI is enabled to test the endpoints interactively.

- **Swagger UI URL:** `http://localhost:<PORT>/swagger` (or `https://localhost:<PORT>/swagger`)
- **JWT Authorization:** To authorize API endpoints that require authentication:
  1. Login via `POST /api/auth/login` to obtain an access token.
  2. Click the **Authorize** button on the top right of the Swagger page.
  3. Enter `Bearer <YOUR_ACCESS_TOKEN>` (including the space) and click **Authorize**.

## Main Endpoints

- `GET /health`
- `POST /api/auth/login`
- `POST /api/auth/refresh`
- `POST /api/auth/logout`
- `GET /api/auth/me`
- `GET /api/users`
- `POST /api/users`
- `GET /api/events/public`
- `GET /api/events`
- `GET /api/events/{id}`
- `POST /api/events`
- `PUT /api/events/{id}`

Invitations & check-in (Admin/SuperAdmin, except `by-token` which is public):

- `POST /api/events/{eventId}/invitations` — invite a guest (generates QR + emails them)
- `GET /api/events/{eventId}/invitations` — list an event's invitations
- `GET /api/invitations/{id}` — invitation detail
- `GET /api/invitations/{id}/qr` — QR code PNG
- `POST /api/invitations/{id}/resend` — re-send the invitation email
- `POST /api/invitations/scan` — look up by scanned token (no consume)
- `POST /api/invitations/{id}/verify` — confirm check-in and expire the QR
- `DELETE /api/invitations/{id}` — cancel an invitation
- `GET /api/invitations/by-token/{token}` — public guest landing data
