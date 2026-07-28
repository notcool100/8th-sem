<script lang="ts">
  import type { PageData } from "./$types";
  import type { ReportsSummaryDto } from "$lib/types/reports";
  import type { InvitationDto } from "$lib/types/invitations";
  import type { EventRegistrationDto } from "$lib/types/registrations";
  import type { WorkshopInviteDto } from "$lib/types/workshop-invites";

  let { data } = $props<{ data: PageData }>();

  const summary = $derived(data.summary as ReportsSummaryDto | null);
  const invitations = $derived((data.invitations ?? []) as InvitationDto[]);
  const registrations = $derived((data.registrations ?? []) as EventRegistrationDto[]);
  const workshopInvites = $derived((data.workshopInvites ?? []) as WorkshopInviteDto[]);

  // ── Date range filter ─────────────────────────────────────────────────────
  let fromDate = $state(data.from ?? "");
  let toDate   = $state(data.to   ?? "");

  function applyDateFilter() {
    const params = new URLSearchParams();
    if (fromDate) params.set("from", fromDate);
    if (toDate)   params.set("to",   toDate);
    const qs = params.toString() ? `?${params}` : "";
    window.location.href = `/admin/reports${qs}`;
  }

  function clearFilter() {
    window.location.href = "/admin/reports";
  }

  function formatDate(iso: string | null | undefined): string {
    if (!iso) return "—";
    const d = new Date(iso);
    return isNaN(d.getTime()) ? "—" : d.toLocaleDateString("en-US", { day: "2-digit", month: "short", year: "numeric" });
  }

  function statusClass(status: string): string {
    return `status-chip status-${status.toLowerCase()}`;
  }

  // ── Per-event invitations & registrations ─────────────────────────────────
  let eventSearch = $state("");

  const filteredEventsReport = $derived.by(() => {
    if (!summary) return [];
    const q = eventSearch.trim().toLowerCase();
    if (!q) return summary.eventsReport;
    return summary.eventsReport.filter(
      ev => ev.title.toLowerCase().includes(q) || ev.category.toLowerCase().includes(q)
    );
  });

  let expandedEventId = $state<number | null>(null);

  function toggleExpand(eventId: number) {
    expandedEventId = expandedEventId === eventId ? null : eventId;
  }

  function invitationsFor(eventId: number): InvitationDto[] {
    return invitations.filter(i => i.eventId === eventId);
  }

  function registrationsFor(eventId: number): EventRegistrationDto[] {
    return registrations.filter(r => r.eventId === eventId);
  }

  function workshopInvitesFor(eventId: number): WorkshopInviteDto[] {
    return workshopInvites.filter(w => w.eventId === eventId);
  }
</script>

<svelte:head><title>Reports & Analytics — NTB Admin</title></svelte:head>

<div class="reports-page">
  <!-- ── Page header ────────────────────────────────────────────────────────── -->
  <div class="page-header">
    <div>
      <h1><i class="fi fi-rr-stats"></i> Reports & Analytics</h1>
      <p>Per-event invitations, registrations and attendance</p>
    </div>

    <div class="date-filter">
      <div class="field-box">
        <i class="fi fi-rr-calendar-day"></i>
        <input type="date" bind:value={fromDate} placeholder="From" />
      </div>
      <span class="sep">to</span>
      <div class="field-box">
        <i class="fi fi-rr-calendar-day"></i>
        <input type="date" bind:value={toDate} placeholder="To" />
      </div>
      <button type="button" class="btn-apply" onclick={applyDateFilter}>Apply</button>
      {#if data.from || data.to}
        <button type="button" class="btn-clear" onclick={clearFilter}>Clear</button>
      {/if}
    </div>
  </div>

  {#if data.error}
    <div class="error-banner">
      <i class="fi fi-rr-exclamation"></i>
      <strong>Could not load report data.</strong> {data.error.message}
    </div>
  {:else if !summary}
    <div class="loading-state"><i class="fi fi-rr-spinner"></i> Loading…</div>
  {:else}

  <!-- ── Events: Invitations & Registrations ────────────────────────────────── -->
  <div class="card">
    <div class="events-report-head">
      <h2 class="card-title"><i class="fi fi-rr-list-check"></i> Events</h2>
      <div class="events-search">
        <i class="fi fi-rr-search"></i>
        <input type="text" placeholder="Search by event or category…" bind:value={eventSearch} />
      </div>
    </div>

    {#if summary.eventsReport.length === 0}
      <p class="empty-text">No events available.</p>
    {:else if filteredEventsReport.length === 0}
      <p class="empty-text">No events match "{eventSearch}".</p>
    {:else}
      <div class="events-table-wrap">
        <table class="events-table">
          <thead>
            <tr>
              <th></th>
              <th>Event</th>
              <th>Status</th>
              <th>Invitations</th>
              <th>Registrations</th>
              <th>Workshop Invites</th>
              <th>Check-ins</th>
            </tr>
          </thead>
          <tbody>
            {#each filteredEventsReport as ev (ev.id)}
              {@const invitees = invitationsFor(ev.id)}
              {@const registrants = registrationsFor(ev.id)}
              {@const workshopees = workshopInvitesFor(ev.id)}
              <tr class="ev-row" onclick={() => toggleExpand(ev.id)}>
                <td class="expand-cell">
                  <i class="fi fi-rr-angle-small-{expandedEventId === ev.id ? 'down' : 'right'}"></i>
                </td>
                <td>
                  <span class="ev-title">{ev.title}</span>
                  <span class="ev-meta">{ev.category} · {formatDate(ev.dateAd)}</span>
                </td>
                <td><span class={statusClass(ev.status)}>{ev.status}</span></td>
                <td>
                  {#if ev.requiresInvitation}
                    <div class="stat-chips">
                      <span class="chip total">{ev.totalInvitations} total</span>
                      <span class="chip verified">{ev.verifiedInvitations} attended</span>
                      <span class="chip sent">{ev.sentInvitations} sent</span>
                      <span class="chip pending">{ev.pendingInvitations} pending</span>
                      {#if ev.expiredInvitations || ev.cancelledInvitations}
                        <span class="chip cancelled">{ev.expiredInvitations + ev.cancelledInvitations} expired/cancelled</span>
                      {/if}
                    </div>
                  {:else}
                    <span class="na">Not enabled</span>
                  {/if}
                </td>
                <td>
                  {#if ev.requiresRegistration}
                    <div class="stat-chips">
                      <span class="chip total">{ev.totalRegistrations} total</span>
                      <span class="chip verified">{ev.approvedRegistrations} approved</span>
                      <span class="chip pending">{ev.pendingRegistrations} pending</span>
                      {#if ev.rejectedRegistrations || ev.cancelledRegistrations}
                        <span class="chip cancelled">{ev.rejectedRegistrations + ev.cancelledRegistrations} rejected/cancelled</span>
                      {/if}
                    </div>
                  {:else}
                    <span class="na">Not enabled</span>
                  {/if}
                </td>
                <td>
                  {#if ev.totalWorkshopInvites > 0}
                    <div class="stat-chips">
                      <span class="chip total">{ev.totalWorkshopInvites} total</span>
                      <span class="chip verified">{ev.verifiedWorkshopInvites} attended</span>
                      <span class="chip sent">{ev.sentWorkshopInvites} sent</span>
                      <span class="chip pending">{ev.pendingWorkshopInvites} pending</span>
                    </div>
                  {:else}
                    <span class="na">None sent</span>
                  {/if}
                </td>
                <td class="checkins-cell">{ev.successfulCheckIns}</td>
              </tr>

              {#if expandedEventId === ev.id}
                <tr class="detail-row">
                  <td colspan="7">
                    <div class="guest-detail">
                      {#if ev.requiresInvitation}
                        <h4><i class="fi fi-rr-envelope"></i> Invited guests ({invitees.length})</h4>
                        {#if invitees.length === 0}
                          <p class="empty-text small">No one has been invited yet.</p>
                        {:else}
                          <table class="guest-table">
                            <thead>
                              <tr>
                                <th>Name</th>
                                <th>Contact</th>
                                <th>Status</th>
                                <th>Sent</th>
                                <th>Attended</th>
                              </tr>
                            </thead>
                            <tbody>
                              {#each invitees as g (g.id)}
                                <tr>
                                  <td>
                                    <span class="guest-name">{g.guestName}</span>
                                    {#if g.guestOrganization}<span class="guest-meta">{g.guestOrganization}</span>{/if}
                                  </td>
                                  <td>
                                    <span class="guest-meta">{g.guestEmail}</span>
                                    {#if g.guestPhone}<span class="guest-meta">{g.guestPhone}</span>{/if}
                                  </td>
                                  <td><span class={statusClass(g.status)}>{g.status}</span></td>
                                  <td class="guest-meta">{formatDate(g.sentAtUtc)}</td>
                                  <td>
                                    {#if g.verifiedAtUtc}
                                      <span class="attended-yes"><i class="fi fi-rr-check"></i> {formatDate(g.verifiedAtUtc)}</span>
                                    {:else}
                                      <span class="attended-no">Not yet</span>
                                    {/if}
                                  </td>
                                </tr>
                              {/each}
                            </tbody>
                          </table>
                        {/if}
                      {/if}

                      {#if ev.requiresRegistration}
                        <h4><i class="fi fi-rr-user-add"></i> Registered guests ({registrants.length})</h4>
                        {#if registrants.length === 0}
                          <p class="empty-text small">No one has registered yet.</p>
                        {:else}
                          <table class="guest-table">
                            <thead>
                              <tr>
                                <th>Name</th>
                                <th>Contact</th>
                                <th>Status</th>
                                <th>Requested</th>
                                <th>Reviewed</th>
                              </tr>
                            </thead>
                            <tbody>
                              {#each registrants as g (g.id)}
                                <tr>
                                  <td>
                                    <span class="guest-name">{g.guestName}</span>
                                    {#if g.guestOrganization}<span class="guest-meta">{g.guestOrganization}</span>{/if}
                                  </td>
                                  <td>
                                    <span class="guest-meta">{g.guestEmail}</span>
                                    {#if g.guestPhone}<span class="guest-meta">{g.guestPhone}</span>{/if}
                                  </td>
                                  <td><span class={statusClass(g.status)}>{g.status}</span></td>
                                  <td class="guest-meta">{formatDate(g.requestedAtUtc)}</td>
                                  <td class="guest-meta">{formatDate(g.reviewedAtUtc)}</td>
                                </tr>
                              {/each}
                            </tbody>
                          </table>
                        {/if}
                      {/if}

                      {#if ev.totalWorkshopInvites > 0}
                        <h4><i class="fi fi-rr-megaphone"></i> Workshop invites ({workshopees.length})</h4>
                        <table class="guest-table">
                          <thead>
                            <tr>
                              <th>Name</th>
                              <th>Contact</th>
                              <th>Status</th>
                              <th>Sent</th>
                              <th>Attended</th>
                            </tr>
                          </thead>
                          <tbody>
                            {#each workshopees as g (g.id)}
                              <tr>
                                <td>
                                  <span class="guest-name">{g.fullName}</span>
                                  {#if g.organization}<span class="guest-meta">{g.organization}</span>{/if}
                                </td>
                                <td>
                                  <span class="guest-meta">{g.email}</span>
                                  {#if g.phone}<span class="guest-meta">{g.phone}</span>{/if}
                                </td>
                                <td><span class={statusClass(g.status)}>{g.status}</span></td>
                                <td class="guest-meta">{formatDate(g.sentAtUtc)}</td>
                                <td>
                                  {#if g.verifiedAtUtc}
                                    <span class="attended-yes"><i class="fi fi-rr-check"></i> {formatDate(g.verifiedAtUtc)}</span>
                                  {:else}
                                    <span class="attended-no">Not yet</span>
                                  {/if}
                                </td>
                              </tr>
                            {/each}
                          </tbody>
                        </table>
                      {/if}

                      {#if !ev.requiresInvitation && !ev.requiresRegistration && ev.totalWorkshopInvites === 0}
                        <p class="empty-text small">This event doesn't use invitations, self-registration, or workshop invites.</p>
                      {/if}
                    </div>
                  </td>
                </tr>
              {/if}
            {/each}
          </tbody>
        </table>
      </div>
    {/if}
  </div>

  {/if}
</div>

<style>
  .reports-page {
    padding: 1.5rem 2rem;
    max-width: 1300px;
    margin: 0 auto;
    display: flex;
    flex-direction: column;
    gap: 1.4rem;
  }

  /* ── Header ───────────────────────────────────────────────────────────────── */
  .page-header {
    display: flex;
    align-items: flex-start;
    justify-content: space-between;
    gap: 1.5rem;
    flex-wrap: wrap;
  }

  .page-header h1 {
    margin: 0;
    font-size: 1.55rem;
    color: #3f515b;
    display: inline-flex;
    align-items: center;
    gap: 0.5rem;
    font-family: 'Quicksand', sans-serif;
    font-weight: 700;
  }

  .page-header h1 i { color: #bd242b; font-size: 1.3rem; }

  .page-header p {
    margin: 0.2rem 0 0;
    color: #64748b;
    font-size: 0.92rem;
  }

  /* ── Date filter ──────────────────────────────────────────────────────────── */
  .date-filter {
    display: flex;
    align-items: center;
    gap: 0.5rem;
    flex-wrap: wrap;
  }

  .field-box {
    display: inline-flex;
    align-items: center;
    gap: 0.4rem;
    border: 1px solid #e2e8f0;
    border-radius: 10px;
    padding: 0 0.75rem;
    height: 40px;
    background: #fff;
    font-size: 0.9rem;
    color: #3f515b;
  }

  .field-box i { color: #bd242b; font-size: 0.85rem; }

  .field-box input {
    border: none;
    outline: none;
    background: transparent;
    color: #3f515b;
    font-size: 0.88rem;
    width: 130px;
  }

  .sep { color: #94a3b8; font-size: 0.88rem; }

  .btn-apply {
    height: 40px;
    padding: 0 1.1rem;
    background: #f8ce1c;
    color: #263038;
    border-radius: 10px;
    font-weight: 700;
    font-size: 0.9rem;
    cursor: pointer;
    transition: background 0.15s;
  }

  .btn-apply:hover { background: #e6b910; }

  .btn-clear {
    height: 40px;
    padding: 0 1rem;
    background: #f1f5f9;
    color: #64748b;
    border: 1px solid #e2e8f0;
    border-radius: 10px;
    font-weight: 600;
    font-size: 0.88rem;
    cursor: pointer;
  }

  /* ── Error / Loading ──────────────────────────────────────────────────────── */
  .error-banner {
    background: #fff1f2;
    border: 1px solid #fecdd3;
    color: #9f1239;
    border-radius: 12px;
    padding: 0.9rem 1.1rem;
    display: flex;
    align-items: center;
    gap: 0.6rem;
    font-size: 0.92rem;
  }

  .loading-state {
    padding: 3rem;
    text-align: center;
    color: #94a3b8;
    font-size: 1.1rem;
  }

  /* ── Card ─────────────────────────────────────────────────────────────────── */
  .card {
    background: #fff;
    border: 1px solid #e5e7eb;
    border-radius: 16px;
    padding: 1.4rem 1.6rem;
  }

  .card-title {
    margin: 0 0 1.2rem;
    font-size: 1.05rem;
    font-weight: 700;
    color: #3f515b;
    display: inline-flex;
    align-items: center;
    gap: 0.45rem;
    font-family: 'Quicksand', sans-serif;
  }

  .card-title i { color: #bd242b; font-size: 0.92rem; }

  .empty-text { color: #94a3b8; font-size: 0.9rem; padding: 1rem 0; text-align: center; }
  .empty-text.small { padding: 0.4rem 0 1rem; text-align: left; font-size: 0.84rem; }

  /* ── Events report table ─────────────────────────────────────────────────── */
  .events-report-head {
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: 1rem;
    flex-wrap: wrap;
    margin-bottom: 1.2rem;
  }

  .events-report-head .card-title { margin: 0; }

  .events-search {
    display: inline-flex;
    align-items: center;
    gap: 0.5rem;
    border: 1px solid #e2e8f0;
    border-radius: 10px;
    padding: 0 0.75rem;
    height: 38px;
    background: #f8fafc;
    min-width: 240px;
  }

  .events-search i { color: #94a3b8; font-size: 0.85rem; }

  .events-search input {
    border: none;
    outline: none;
    background: transparent;
    color: #3f515b;
    font-size: 0.88rem;
    width: 100%;
  }

  .events-table-wrap {
    overflow-x: auto;
  }

  .events-table {
    width: 100%;
    border-collapse: collapse;
    min-width: 980px;
  }

  .events-table th {
    text-align: left;
    font-size: 0.72rem;
    text-transform: uppercase;
    letter-spacing: 0.06em;
    color: #64748b;
    font-weight: 700;
    padding: 0.6rem 0.75rem;
    border-bottom: 1.5px solid #e5e7eb;
    white-space: nowrap;
  }

  .events-table td {
    padding: 0.75rem;
    border-bottom: 1px solid #f1f5f9;
    vertical-align: top;
  }

  .ev-row { cursor: pointer; }
  .ev-row:hover { background: #f8fafc; }

  .expand-cell { width: 28px; color: #94a3b8; padding-right: 0 !important; }

  .ev-title { display: block; font-weight: 600; color: #3f515b; font-size: 0.9rem; }
  .ev-meta  { display: block; color: #94a3b8; font-size: 0.76rem; margin-top: 0.15rem; }

  .status-chip {
    display: inline-block;
    padding: 0.2rem 0.6rem;
    border-radius: 999px;
    font-size: 0.72rem;
    font-weight: 700;
    text-transform: capitalize;
    white-space: nowrap;
  }

  .status-chip.status-published       { background: #dcfce7; color: #15803d; }
  .status-chip.status-draft           { background: #f1f5f9; color: #64748b; }
  .status-chip.status-pendingapproval { background: #fef3c7; color: #92400e; }
  .status-chip.status-archived        { background: #e5e7eb; color: #475569; }
  .status-chip.status-pending         { background: #fef3c7; color: #92400e; }
  .status-chip.status-sent            { background: #e0f2fe; color: #0369a1; }
  .status-chip.status-verified        { background: #dcfce7; color: #15803d; }
  .status-chip.status-expired         { background: #f1f5f9; color: #64748b; }
  .status-chip.status-cancelled       { background: #fee2e2; color: #b91c1c; }
  .status-chip.status-approved        { background: #dcfce7; color: #15803d; }
  .status-chip.status-rejected        { background: #fee2e2; color: #b91c1c; }

  .stat-chips {
    display: flex;
    flex-wrap: wrap;
    gap: 0.35rem;
    max-width: 320px;
  }

  .chip {
    display: inline-block;
    padding: 0.18rem 0.55rem;
    border-radius: 8px;
    font-size: 0.72rem;
    font-weight: 600;
    white-space: nowrap;
  }

  .chip.total     { background: #f1f5f9; color: #475569; }
  .chip.verified  { background: #dcfce7; color: #15803d; }
  .chip.sent      { background: #e0f2fe; color: #0369a1; }
  .chip.pending   { background: #fef3c7; color: #92400e; }
  .chip.cancelled { background: #fee2e2; color: #b91c1c; }

  .na { color: #cbd5e1; font-size: 0.82rem; font-style: italic; }

  .checkins-cell { font-weight: 700; color: #3f515b; font-size: 0.95rem; }

  /* ── Expanded guest detail ─────────────────────────────────────────────────── */
  .detail-row td {
    background: #fafbfc;
    padding: 1rem 1.25rem 1.4rem;
    border-bottom: 1.5px solid #e5e7eb;
  }

  .guest-detail h4 {
    margin: 0 0 0.6rem;
    font-size: 0.8rem;
    text-transform: uppercase;
    letter-spacing: 0.05em;
    color: #475569;
    font-weight: 800;
    display: flex;
    align-items: center;
    gap: 0.4rem;
  }

  .guest-detail h4 i { color: #bd242b; }

  .guest-detail h4:not(:first-child) { margin-top: 1.1rem; }

  .guest-table {
    width: 100%;
    border-collapse: collapse;
    margin-bottom: 0.5rem;
  }

  .guest-table th {
    text-align: left;
    font-size: 0.68rem;
    text-transform: uppercase;
    letter-spacing: 0.04em;
    color: #94a3b8;
    font-weight: 700;
    padding: 0.4rem 0.6rem;
    border-bottom: 1px solid #e5e7eb;
    white-space: nowrap;
  }

  .guest-table td {
    padding: 0.5rem 0.6rem;
    border-bottom: 1px solid #eef2f7;
    font-size: 0.82rem;
    color: #3f515b;
    vertical-align: top;
  }

  .guest-name { display: block; font-weight: 600; }
  .guest-meta { display: block; color: #94a3b8; font-size: 0.76rem; }

  .attended-yes { color: #15803d; font-weight: 700; font-size: 0.78rem; display: inline-flex; align-items: center; gap: 0.3rem; }
  .attended-no  { color: #94a3b8; font-size: 0.78rem; }

  /* ── Responsive ───────────────────────────────────────────────────────────── */
  @media (max-width: 820px) {
    .reports-page { padding: 1rem; }
    .events-search { width: 100%; min-width: 0; }
  }

  @media (max-width: 600px) {
    .page-header { flex-direction: column; }
    .date-filter { width: 100%; }
  }
</style>
