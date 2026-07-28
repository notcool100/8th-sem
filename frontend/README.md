# NTB Event — Frontend

SvelteKit application (Svelte 5 + TypeScript + Vite) for the Nepal Tourism Board event
platform. It renders the public site, the client area, and the admin/staff dashboard, and
talks to the .NET backend through SvelteKit server routes that act as a thin
backend-for-frontend (BFF) — the browser never holds the access token directly.

## Developing

Install dependencies and start the dev server:

```sh
npm install        # or pnpm install / yarn
npm run dev

# or open the app in a new browser tab automatically
npm run dev -- --open
```

The app runs at **http://localhost:5173** and expects the backend at
`http://localhost:5232`. Override with the `API_BASE_URL` environment variable if the
backend runs elsewhere.

## Building

```sh
npm run build
npm run preview    # preview the production build
```

> Deployment may require a SvelteKit [adapter](https://svelte.dev/docs/kit/adapters) for
> your target environment.

## Project structure

```
src/
├── lib/
│   ├── server/auth/api.ts     # Typed backend client (BFF) — attaches access tokens
│   ├── server/auth/session.ts # Cookie/session helpers
│   ├── types/                 # Shared TS types (auth, events, categories, invitations…)
│   └── components/            # Reusable UI (layout, sidebar, …)
├── routes/
│   ├── (public)/              # Public pages, incl. /invite/[token] (guest landing)
│   ├── (client)/              # Client dashboard (role: client)
│   ├── (admin)/admin/         # Admin/staff area (roles: admin, superadmin)
│   └── api/                   # SvelteKit server endpoints proxied to the backend
└── hooks.server.ts            # Session resolution, /admin & /client guards, rate limiting
```

`hooks.server.ts` resolves the session on every request (refreshing tokens when needed),
guards the `/admin` and `/client` areas by role, and rate-limits login, uploads, and public
endpoints.

## Guest invitations & QR check-in

The admin invitation flow is implemented across these routes:

- **`/admin/events/invitations/[id]`** — manage an event's invitations: invite a guest,
  resend or cancel an invitation, and view each guest's QR code. Reached via the *Invite*
  action on the events list and the *Invite guests* link on the event edit screen.
- **`/admin/check-in`** — door check-in. Scans a guest's QR with the device camera (loaded
  via the `html5-qrcode` library) or accepts a manually pasted code. A confirmation popup
  shows the guest's details; pressing *Confirm check-in* verifies the guest and expires the
  QR. Re-scanning a used QR is refused.
- **`/invite/[token]`** — public guest landing page showing the event details and the
  guest's QR code (the target of the emailed invite link).

The interactive scan/verify calls go through the server endpoints
`src/routes/api/invitations/scan/+server.ts` and `.../verify/+server.ts`, which attach the
admin's session token before calling the backend. Invitation types live in
`src/lib/types/invitations.ts` and the typed client functions in
`src/lib/server/auth/api.ts`.
