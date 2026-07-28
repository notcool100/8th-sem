<script lang="ts">
  import type { PageData } from "./$types";
  import type { ReportsSummaryDto } from "$lib/types/reports";

  let { data } = $props<{ data: PageData }>();

  const summary = $derived(data.summary as ReportsSummaryDto | null);

  // ── Date range filter ─────────────────────────────────────────────────────
  let fromDate = $state(data.from ?? "");
  let toDate   = $state(data.to   ?? "");

  function applyDateFilter() {
    const params = new URLSearchParams();
    if (fromDate) params.set("from", fromDate);
    if (toDate)   params.set("to",   toDate);
    const qs = params.toString() ? `?${params}` : "";
    window.location.href = `/admin/dashboard${qs}`;
  }

  function clearFilter() {
    window.location.href = "/admin/dashboard";
  }

  // ── Chart helpers ─────────────────────────────────────────────────────────
  function barWidth(count: number, max: number): string {
    if (max === 0) return "0%";
    return `${Math.round((count / max) * 100)}%`;
  }

  const maxCategoryCount = $derived(
    summary ? Math.max(...summary.eventsByCategory.map(c => c.count), 1) : 1
  );

  const maxMonthCount = $derived(
    summary ? Math.max(...summary.eventsByMonth.map(m => m.count), 1) : 1
  );

  function pct(part: number, total: number): string {
    if (total === 0) return "0";
    return Math.round((part / total) * 100).toString();
  }
</script>

<svelte:head><title>Dashboard — NTB Admin</title></svelte:head>

<div class="reports-page">
  <!-- ── Page header ────────────────────────────────────────────────────────── -->
  <div class="page-header">
    <div>
      <h1><i class="fi fi-rr-dashboard"></i> Dashboard</h1>
      <p>Overview of events, users, invitations and check-ins</p>
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
      <strong>Could not load dashboard data.</strong> {data.error.message}
    </div>
  {:else if !summary}
    <div class="loading-state"><i class="fi fi-rr-spinner"></i> Loading…</div>
  {:else}

  <!-- ── KPI cards ──────────────────────────────────────────────────────────── -->
  <div class="kpi-grid">
    <div class="kpi-card">
      <div class="kpi-icon events"><i class="fi fi-rr-calendar"></i></div>
      <div class="kpi-body">
        <span class="kpi-label">Total Events</span>
        <span class="kpi-value">{summary.totalEvents}</span>
        <span class="kpi-sub">{summary.publishedEvents} published</span>
      </div>
    </div>
    <div class="kpi-card">
      <div class="kpi-icon users"><i class="fi fi-rr-users"></i></div>
      <div class="kpi-body">
        <span class="kpi-label">Users</span>
        <span class="kpi-value">{summary.totalUsers}</span>
        <span class="kpi-sub">{summary.activeUsers} active</span>
      </div>
    </div>
    <div class="kpi-card">
      <div class="kpi-icon invitations"><i class="fi fi-rr-envelope"></i></div>
      <div class="kpi-body">
        <span class="kpi-label">Invitations</span>
        <span class="kpi-value">{summary.totalInvitations}</span>
        <span class="kpi-sub">{summary.verifiedInvitations} verified</span>
      </div>
    </div>
    <div class="kpi-card">
      <div class="kpi-icon checkins"><i class="fi fi-rr-qr-scan"></i></div>
      <div class="kpi-body">
        <span class="kpi-label">Check-ins</span>
        <span class="kpi-value">{summary.successfulCheckIns}</span>
        <span class="kpi-sub">{summary.totalScans} total scans</span>
      </div>
    </div>
  </div>

  <!-- ── Row 2: Event status + User breakdown ───────────────────────────────── -->
  <div class="row-2">
    <!-- Event status breakdown -->
    <div class="card">
      <h2 class="card-title"><i class="fi fi-rr-chart-pie-alt"></i> Events by Status</h2>

      <div class="status-grid">
        <div class="status-item published">
          <span class="status-count">{summary.publishedEvents}</span>
          <span class="status-label">Published</span>
          <div class="status-bar"><div style="width:{pct(summary.publishedEvents, summary.totalEvents)}%"></div></div>
          <span class="status-pct">{pct(summary.publishedEvents, summary.totalEvents)}%</span>
        </div>
        <div class="status-item draft">
          <span class="status-count">{summary.draftEvents}</span>
          <span class="status-label">Draft</span>
          <div class="status-bar"><div style="width:{pct(summary.draftEvents, summary.totalEvents)}%"></div></div>
          <span class="status-pct">{pct(summary.draftEvents, summary.totalEvents)}%</span>
        </div>
        <div class="status-item pending">
          <span class="status-count">{summary.pendingApprovalEvents}</span>
          <span class="status-label">Pending Approval</span>
          <div class="status-bar"><div style="width:{pct(summary.pendingApprovalEvents, summary.totalEvents)}%"></div></div>
          <span class="status-pct">{pct(summary.pendingApprovalEvents, summary.totalEvents)}%</span>
        </div>
        <div class="status-item archived">
          <span class="status-count">{summary.archivedEvents}</span>
          <span class="status-label">Archived</span>
          <div class="status-bar"><div style="width:{pct(summary.archivedEvents, summary.totalEvents)}%"></div></div>
          <span class="status-pct">{pct(summary.archivedEvents, summary.totalEvents)}%</span>
        </div>
      </div>

      <div class="entry-split">
        <div class="entry-pill free">
          <i class="fi fi-rr-ticket"></i>
          <span>{summary.freeEvents} Free</span>
        </div>
        <div class="entry-pill paid">
          <i class="fi fi-rr-money-bill-wave"></i>
          <span>{summary.paidEvents} Paid</span>
        </div>
      </div>
    </div>

    <!-- Users + Invitations -->
    <div class="card">
      <h2 class="card-title"><i class="fi fi-rr-users"></i> Users & Invitations</h2>

      <div class="mini-section">
        <h3>User Roles</h3>
        <div class="bar-row">
          <span class="bar-label">Admins</span>
          <div class="bar-track">
            <div class="bar-fill admin" style="width:{barWidth(summary.adminUsers, summary.totalUsers)}"></div>
          </div>
          <span class="bar-val">{summary.adminUsers}</span>
        </div>
        <div class="bar-row">
          <span class="bar-label">Clients</span>
          <div class="bar-track">
            <div class="bar-fill client" style="width:{barWidth(summary.clientUsers, summary.totalUsers)}"></div>
          </div>
          <span class="bar-val">{summary.clientUsers}</span>
        </div>
      </div>

      <div class="mini-section">
        <h3>Invitation Status</h3>
        {#each [
          { label: "Verified",   count: summary.verifiedInvitations,  cls: "verified"  },
          { label: "Sent",       count: summary.sentInvitations,       cls: "sent"      },
          { label: "Pending",    count: summary.pendingInvitations,    cls: "pending"   },
          { label: "Expired",    count: summary.expiredInvitations,    cls: "expired"   },
          { label: "Cancelled",  count: summary.cancelledInvitations,  cls: "cancelled" },
        ] as item}
          <div class="bar-row">
            <span class="bar-label">{item.label}</span>
            <div class="bar-track">
              <div class="bar-fill {item.cls}" style="width:{barWidth(item.count, summary.totalInvitations)}"></div>
            </div>
            <span class="bar-val">{item.count}</span>
          </div>
        {/each}
      </div>
    </div>
  </div>

  <!-- ── Events by category ─────────────────────────────────────────────────── -->
  <div class="card">
    <h2 class="card-title"><i class="fi fi-rr-tags"></i> Events by Category</h2>
    {#if summary.eventsByCategory.length === 0}
      <p class="empty-text">No category data available.</p>
    {:else}
      <div class="category-bars">
        {#each summary.eventsByCategory as cat}
          <div class="cat-row">
            <span class="cat-dot" style="background:{cat.color}"></span>
            <span class="cat-label">{cat.category}</span>
            <div class="bar-track wide">
              <div class="bar-fill-cat" style="width:{barWidth(cat.count, maxCategoryCount)};background:{cat.color}"></div>
            </div>
            <span class="bar-val">{cat.count}</span>
          </div>
        {/each}
      </div>
    {/if}
  </div>

  <!-- ── Events by month ────────────────────────────────────────────────────── -->
  <div class="card">
    <h2 class="card-title"><i class="fi fi-rr-calendar-lines"></i> Events Timeline</h2>
    {#if summary.eventsByMonth.length === 0}
      <p class="empty-text">No timeline data for the selected period.</p>
    {:else}
      <div class="timeline-chart">
        {#each summary.eventsByMonth as m}
          <div class="timeline-col">
            <span class="col-count">{m.count}</span>
            <div class="col-bar-wrap">
              <div class="col-bar" style="height:{barWidth(m.count, maxMonthCount)}"></div>
            </div>
            <span class="col-label">{m.label}</span>
          </div>
        {/each}
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

  /* ── KPI cards ────────────────────────────────────────────────────────────── */
  .kpi-grid {
    display: grid;
    grid-template-columns: repeat(4, 1fr);
    gap: 1rem;
  }

  .kpi-card {
    background: #fff;
    border: 1px solid #e5e7eb;
    border-radius: 16px;
    padding: 1.2rem 1.3rem;
    display: flex;
    align-items: center;
    gap: 1rem;
  }

  .kpi-icon {
    width: 50px;
    height: 50px;
    border-radius: 14px;
    display: flex;
    align-items: center;
    justify-content: center;
    font-size: 1.3rem;
    flex-shrink: 0;
  }

  .kpi-icon.events      { background: #fef3c7; color: #b45309; }
  .kpi-icon.users       { background: #dbeafe; color: #1d4ed8; }
  .kpi-icon.invitations { background: #dcfce7; color: #15803d; }
  .kpi-icon.checkins    { background: #fce7f3; color: #be185d; }

  .kpi-body {
    display: flex;
    flex-direction: column;
    gap: 0.1rem;
    min-width: 0;
  }

  .kpi-label { font-size: 0.8rem; color: #64748b; font-weight: 600; text-transform: uppercase; letter-spacing: 0.05em; }
  .kpi-value { font-size: 2rem; font-weight: 700; color: #3f515b; line-height: 1.1; font-family: 'Quicksand', sans-serif; }
  .kpi-sub   { font-size: 0.8rem; color: #94a3b8; }

  /* ── Row 2 ────────────────────────────────────────────────────────────────── */
  .row-2 {
    display: grid;
    grid-template-columns: 1fr 1fr;
    gap: 1rem;
  }

  /* ── Status grid ──────────────────────────────────────────────────────────── */
  .status-grid {
    display: grid;
    grid-template-columns: 1fr 1fr;
    gap: 1rem;
    margin-bottom: 1rem;
  }

  .status-item {
    display: flex;
    flex-direction: column;
    gap: 0.2rem;
    background: #f8fafc;
    border-radius: 12px;
    padding: 0.9rem 1rem;
  }

  .status-count { font-size: 1.6rem; font-weight: 700; color: #3f515b; font-family: 'Quicksand', sans-serif; }
  .status-label { font-size: 0.8rem; color: #64748b; font-weight: 600; }

  .status-bar {
    height: 6px;
    background: #e2e8f0;
    border-radius: 99px;
    margin: 0.4rem 0 0.1rem;
    overflow: hidden;
  }

  .status-bar div { height: 100%; border-radius: 99px; transition: width 0.6s ease; }

  .status-item.published .status-bar div { background: #16a34a; }
  .status-item.draft     .status-bar div { background: #94a3b8; }
  .status-item.pending   .status-bar div { background: #f8ce1c; }
  .status-item.archived  .status-bar div { background: #e5e7eb; }

  .status-pct { font-size: 0.78rem; color: #94a3b8; font-weight: 600; }

  .entry-split {
    display: flex;
    gap: 0.75rem;
  }

  .entry-pill {
    flex: 1;
    display: flex;
    align-items: center;
    justify-content: center;
    gap: 0.4rem;
    padding: 0.55rem;
    border-radius: 10px;
    font-weight: 700;
    font-size: 0.88rem;
  }

  .entry-pill.free { background: #dcfce7; color: #15803d; }
  .entry-pill.paid { background: #fef3c7; color: #92400e; }

  /* ── Bar rows ─────────────────────────────────────────────────────────────── */
  .mini-section { margin-bottom: 1.2rem; }
  .mini-section h3 { margin: 0 0 0.7rem; font-size: 0.78rem; text-transform: uppercase; letter-spacing: 0.07em; color: #475569; font-weight: 800; }

  .bar-row {
    display: flex;
    align-items: center;
    gap: 0.6rem;
    margin-bottom: 0.55rem;
  }

  .bar-label { min-width: 80px; font-size: 0.85rem; color: #3f515b; font-weight: 500; }
  .bar-val   { min-width: 28px; text-align: right; font-size: 0.82rem; color: #64748b; font-weight: 600; }

  .bar-track {
    flex: 1;
    height: 10px;
    background: #f1f5f9;
    border-radius: 99px;
    overflow: hidden;
  }

  .bar-track.wide { flex: 1; }

  .bar-fill { height: 100%; border-radius: 99px; transition: width 0.5s ease; }
  .bar-fill.admin    { background: #1c5c6d; }
  .bar-fill.client   { background: #3f515b; }
  .bar-fill.verified { background: #16a34a; }
  .bar-fill.sent     { background: #1c5c6d; }
  .bar-fill.pending  { background: #f8ce1c; }
  .bar-fill.expired  { background: #94a3b8; }
  .bar-fill.cancelled{ background: #fca5a5; }

  /* ── Category bars ────────────────────────────────────────────────────────── */
  .category-bars { display: flex; flex-direction: column; gap: 0.65rem; }

  .cat-row {
    display: flex;
    align-items: center;
    gap: 0.65rem;
  }

  .cat-dot {
    width: 10px;
    height: 10px;
    border-radius: 50%;
    flex-shrink: 0;
  }

  .cat-label { min-width: 120px; font-size: 0.88rem; color: #3f515b; font-weight: 500; }

  .bar-fill-cat { height: 100%; border-radius: 99px; transition: width 0.5s ease; }

  /* ── Timeline chart ───────────────────────────────────────────────────────── */
  .timeline-chart {
    display: flex;
    align-items: flex-end;
    gap: 0.5rem;
    height: 180px;
    padding-bottom: 2rem;
    overflow-x: auto;
  }

  .timeline-col {
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: 0.25rem;
    flex: 1;
    min-width: 52px;
    height: 100%;
    justify-content: flex-end;
  }

  .col-count { font-size: 0.72rem; font-weight: 700; color: #64748b; }

  .col-bar-wrap {
    width: 100%;
    flex: 1;
    display: flex;
    align-items: flex-end;
  }

  .col-bar {
    width: 100%;
    background: linear-gradient(180deg, #bd242b 0%, #f8ce1c 100%);
    border-radius: 6px 6px 0 0;
    min-height: 4px;
    transition: height 0.5s ease;
  }

  .col-label { font-size: 0.65rem; color: #94a3b8; white-space: nowrap; }

  .empty-text { color: #94a3b8; font-size: 0.9rem; padding: 1rem 0; text-align: center; }

  /* ── Responsive ───────────────────────────────────────────────────────────── */
  @media (max-width: 1100px) {
    .kpi-grid { grid-template-columns: repeat(2, 1fr); }
  }

  @media (max-width: 820px) {
    .reports-page { padding: 1rem; }
    .row-2 { grid-template-columns: 1fr; }
    .status-grid { grid-template-columns: 1fr 1fr; }
  }

  @media (max-width: 600px) {
    .kpi-grid { grid-template-columns: 1fr 1fr; }
    .kpi-value { font-size: 1.5rem; }
    .page-header { flex-direction: column; }
    .date-filter { width: 100%; }
  }
</style>
