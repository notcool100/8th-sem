# NTB Event Management System — Remaining Work

Audit done against the Mid-Term Defense Report (`final Project/NTB_Event_MidTerm_Defense_Report.docx`) vs. the actual codebase. The report's Chapter 3.4 describes four "AI modules" as implemented or in progress — none of them existed in the code yet. This punch-list closes that gap before the final defense.

## 1. AI Modules — [x] Implemented (2026-07-28)

### 1.1 Recommendation Engine (TF-IDF + Cosine Similarity) — [x] Done
- Corpus: title (3x weight) + summary (2x) + description + category + region + tags per event, tokenized with stop-word stripping.
- `TfIdfVectorizer` + `Bm25Ranker`/cosine similarity live in `NtbEvent.Application/Common/`.
- Service: `RecommendationService` (`NtbEvent.Application/Services/`), interface `IRecommendationService`.
- Endpoint: `GET /api/events/{id}/recommendations` on `EventsController`.
- Frontend: "You might also like" section in `EventDetailsModal.svelte`, fetched via `/api/events/{id}/recommendations`.

### 1.2 Smart Search (BM25) — [x] Done
- In-memory inverted index + BM25 (k1=1.5, b=0.75) in `Bm25Ranker`.
- Service: `SearchRankingService` / `ISearchRankingService`.
- Endpoint: `GET /api/events/search?q={query}` (admin `BuildWhereClause` ILIKE filter left untouched — this is a separate, additive public search path).
- Frontend: nav search box (`PublicNav.svelte`) submits to `/?search=`, `+page.server.ts` calls the new endpoint.

### 1.3 Automated Tag Suggestion — [x] Done
- TF-IDF keyword extraction (candidate doc scored against the published-event corpus) + Levenshtein fuzzy-match to existing tags.
- Service: `TagSuggestionService` / `ITagSuggestionService`.
- Endpoint: `POST /api/events/suggest-tags`.
- Frontend: "Suggest tags" button + Tags field added to the admin event create/edit form (previously had no UI at all despite the field existing in form state).

### 1.4 Weighted Popularity Scoring — [x] Done
- `PopularityScoreService` / `IPopularityScoreService`, formula as specified. `NormalizedAttendance` uses `ReviewsLabel` as a proxy since the domain model has no numeric attendance field (`AttendanceLabel` is free text).
- `EventDto.PopularityScore` populated on every event read; `EventFilter.SortBy = "popularity"` supported on the non-paged path.
- Featured events on the public homepage (`EventsLandingSection.svelte`) now sort by `popularityScore` descending.

## 2. Testing — [x] Done (2026-07-28)

- `backend/src/NtbEvent.Tests` (xUnit + Moq) added, referenced by no `.sln` (repo has none — built/run per-project).
- **Unit tests** (`Unit/`): UT-01–03 (auth), UT-04–06 (event CRUD), UT-07–09 (TF-IDF/cosine similarity), UT-10 (BM25), UT-11 (tag suggestion), UT-12 (popularity score).
- **System tests** (`System/EventSystemFlowTests.cs`): ST-01–08, wired against in-memory fakes (`TestSupport/`) of the repository interfaces rather than a live Postgres host — exercises real cross-service flows (EventService + RecommendationService + SearchRankingService + TagSuggestionService together). ST-06 (admin auth guard) is asserted via reflection over `EventsController`'s `[Authorize]`/`[AllowAnonymous]` attributes rather than a live HTTP redirect, since there's no `WebApplicationFactory` host in this project. ST-07 (BS calendar toggle) only checks the backend stores/returns a distinct BS date string — the actual AD↔BS conversion algorithm lives in the frontend's `dateUtils.ts` and is out of scope here.
- All 24 tests pass: `cd backend/src && dotnet test NtbEvent.Tests/NtbEvent.Tests.csproj`.
- Note: this sandbox's IPv6 route to `api.nuget.org` was broken (IPv4 fine), which hung the first `dotnet restore`. If a fresh clone hangs on "Determining projects to restore...", set `DOTNET_SYSTEM_NET_DISABLEIPV6=1` before running `dotnet build`/`dotnet test`.

## 3. Already solid — no work needed

- JWT auth + refresh token rotation, RBAC (SuperAdmin/Admin/Client)
- Event CRUD, dual AD/BS dates (real conversion logic in `dateUtils.ts`, not a stub), images, tags, highlights, lifecycle status
- Category/tag many-to-many + `Tags_jn` audit trigger
- Admin dashboard, public homepage, interactive calendar, filtering
- Dapper (list queries) + EF Core (CRUD) split, parameterized `BuildWhereClause`

## 4. Infra / deployment follow-ups (from recent session work)

- [x] `8th-sem` database schema cloned (schema-only) from dev DB to `82.180.144.91:5445` (`passwordof8th`)
- [x] Fixed 2026-07-28: the schema-only clone had all 30 EF migrations' worth of tables/columns physically present but an **empty** `__EFMigrationsHistory` table, so `dbContext.Database.MigrateAsync()` tried to replay every migration from scratch and crashed on the first non-idempotent `CREATE TABLE`. Verified the physical schema matched the final migration state (spot-checked columns from the newest migrations), then baselined all 30 migration IDs into `__EFMigrationsHistory` directly via SQL (no DDL re-run). App now starts clean against this DB.
- [x] Seeded demo data 2026-07-28: expanded `SeedData.cs` from 3 → 12 events (11 published, 6 featured, spread across Festival/Food/Adventure/Technology/Promotion/Meeting categories and Kathmandu/Pokhara/Solukhumbu regions) and added `DatabaseInitializer.SeedCategoriesAsync` (6 categories, 22 tags total via `ICategoryRepository.CreateAsync`) — previously the `Tags` table had no seeding path at all. Verified end-to-end against the live DB: search, recommendations, and tag-suggestion (typo "festivle" → matched "Festival") all return real, sensible results now.
- [x] Local `appsettings.Development.json` updated with new DB connection string and working SMTP (`events@ubucknepal.com` via `mail.ubucknepal.com`, Mailcow)
- [ ] `8th-sem-backend` Coolify app environment variables not yet fully set — still needs: Jwt__*, Smtp__*, Seed__*, SendGrid__* (or drop SendGrid if only using SMTP), ConnectionStrings__Postgres, Cors/AppUrls pointed at 8th-sem's own domain rather than copied from `ntb-event:backend`. **Needs manual action in the Coolify dashboard — not something doable from this repo/CLI.**
- [x] GitHub push to `notcool100/8th-sem` — re-verified 2026-07-28: `git fetch origin` shows local `main` and `origin/main` are identical (0 ahead / 0 behind, at commit `660d290`). The earlier push did land; no action needed.
