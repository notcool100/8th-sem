# NTB Event Management System — Remaining Work

Audit done against the Mid-Term Defense Report (`final Project/NTB_Event_MidTerm_Defense_Report.docx`) vs. the actual codebase. The report's Chapter 3.4 describes four "AI modules" as implemented or in progress — none of them exist in the code yet. This is the punch-list to close that gap before the final defense.

## 1. AI Modules (0% implemented — report claims "in progress")

### 1.1 Recommendation Engine (TF-IDF + Cosine Similarity)
- Build event corpus: concatenate title (3x weight) + summary (2x) + description + category + region + tags per event.
- Tokenize, strip stop words, compute TF-IDF vectors across the corpus.
- Compute cosine similarity between the target event and all others; return top-5 by score.
- New service: `RecommendationService` in `NtbEvent.Application`.
- New endpoint: `GET /api/events/{id}/recommendations`.
- Frontend: "You might also like" section on the event detail page.

### 1.2 Smart Search (BM25)
- Build an in-memory inverted index over title + summary + location + region.
- Implement BM25 scoring (k1=1.5, b=0.75) as described in the report.
- New service: `SearchRankingService`.
- New endpoint: `GET /api/events/search?q={query}`, replacing the current plain `ILIKE` filter used by `BuildWhereClause`.
- Frontend: point the search bar at the new endpoint.

### 1.3 Automated Tag Suggestion
- TF-IDF keyword extraction from event title + description against the existing corpus.
- Fuzzy-match extracted keywords to existing tags via Levenshtein distance.
- New service: `TagSuggestionService`.
- New endpoint: `POST /api/events/suggest-tags`.
- Frontend: call this from the admin event creation/edit form to prefill suggested tags.

### 1.4 Weighted Popularity Scoring
- Formula: `PopularityScore = 0.35*NormalizedRating + 0.25*NormalizedAttendance + 0.20*FeaturedBoost + 0.20*RecencyScore`, with `RecencyScore = e^(-0.01 * days_since_event_start)`.
- New service: `PopularityScoreService`.
- Use it to sort featured/homepage events instead of plain creation-date ordering.

## 2. Testing (missing entirely)

- No test project exists in `backend/` — only 4 `.csproj` files (Api/Application/Infrastructure/Domain), no `*.Tests`.
- Create an `NtbEvent.Tests` xUnit project.
- Implement the test cases already written up in the report but never automated:
  - **Unit tests**: UT-01–UT-03 (auth), UT-04–UT-06 (event CRUD), UT-07–UT-09 (TF-IDF/cosine similarity), UT-10 (BM25), UT-11 (tag suggestion), UT-12 (popularity score).
  - **System tests**: ST-01–ST-08 (end-to-end flows: publish→public view, smart search ranking, recommendations, tag suggestion, region filter, admin auth guard, BS calendar toggle, archive flow).

## 3. Already solid — no work needed

- JWT auth + refresh token rotation, RBAC (SuperAdmin/Admin/Client)
- Event CRUD, dual AD/BS dates (real conversion logic in `dateUtils.ts`, not a stub), images, tags, highlights, lifecycle status
- Category/tag many-to-many + `Tags_jn` audit trigger
- Admin dashboard, public homepage, interactive calendar, filtering
- Dapper (list queries) + EF Core (CRUD) split, parameterized `BuildWhereClause`

## 4. Infra / deployment follow-ups (from recent session work)

- [x] `8th-sem` database schema cloned (schema-only) from dev DB to `82.180.144.91:5445` (`passwordof8th`)
- [x] Local `appsettings.Development.json` updated with new DB connection string and working SMTP (`events@ubucknepal.com` via `mail.ubucknepal.com`, Mailcow)
- [ ] `8th-sem-backend` Coolify app environment variables not yet fully set — still needs: Jwt__*, Smtp__*, Seed__*, SendGrid__* (or drop SendGrid if only using SMTP), ConnectionStrings__Postgres, Cors/AppUrls pointed at 8th-sem's own domain rather than copied from `ntb-event:backend`
- [ ] GitHub push to `notcool100/8th-sem` — last attempt showed no refs landed on remote; needs re-verification
