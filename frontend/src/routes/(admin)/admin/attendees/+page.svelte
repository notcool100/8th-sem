<script lang="ts">
	import { enhance } from "$app/forms";
	import { invalidateAll } from "$app/navigation";
	import type { PageData } from "./$types";
	import type { EventRegistrationDto } from "$lib/types/registrations";
	import type { InvitationDto } from "$lib/types/invitations";
	import type { WorkshopInviteDto } from "$lib/types/workshop-invites";
	import { getFieldLabel } from "$lib/config/selfRegistrationFields";

	let { data, form } = $props<{ data: PageData; form?: any }>();

	type TabKey = "registrations" | "invitations" | "workshop";

	let registrations = $derived((data.registrations ?? []) as EventRegistrationDto[]);
	let invitations = $derived((data.invitations ?? []) as InvitationDto[]);
	let workshopInvites = $derived((data.workshopInvites ?? []) as WorkshopInviteDto[]);

	let activeTab = $state<TabKey>("registrations");
	let selectedEventId = $state<number | null>(data.initialEventId ?? null);
	let eventSearch = $state("");
	let statusFilter = $state("all");
	let guestSearch = $state("");

	function switchTab(tab: TabKey) {
		activeTab = tab;
		selectedEventId = null;
		eventSearch = "";
		statusFilter = "all";
		guestSearch = "";
	}

	type EventGroup = {
		eventId: number;
		eventTitle: string;
		total: number;
		counts: Record<string, number>;
	};

	function buildGroups(items: { eventId: number; eventTitle: string; status: string }[]): EventGroup[] {
		const map = new Map<number, EventGroup>();
		for (const item of items) {
			let group = map.get(item.eventId);
			if (!group) {
				group = { eventId: item.eventId, eventTitle: item.eventTitle, total: 0, counts: {} };
				map.set(item.eventId, group);
			}
			group.total += 1;
			group.counts[item.status] = (group.counts[item.status] ?? 0) + 1;
		}
		return Array.from(map.values()).sort((a, b) => b.total - a.total);
	}

	const registrationGroups = $derived(buildGroups(registrations));
	const invitationGroups = $derived(buildGroups(invitations));
	const workshopGroups = $derived(buildGroups(workshopInvites));

	const activeGroups = $derived(
		activeTab === "registrations" ? registrationGroups
		: activeTab === "invitations" ? invitationGroups
		: workshopGroups,
	);

	const filteredGroups = $derived(
		activeGroups.filter((g) => {
			const term = eventSearch.trim().toLowerCase();
			return !term || g.eventTitle.toLowerCase().includes(term);
		}),
	);

	const selectedEventTitle = $derived(
		registrations.find((r) => r.eventId === selectedEventId)?.eventTitle ?? "",
	);

	const selectedEventRegistrations = $derived(
		registrations.filter((r) => r.eventId === selectedEventId),
	);

	const filteredRegistrations = $derived(
		selectedEventRegistrations.filter((r) => {
			const matchesStatus = statusFilter === "all" || r.status === statusFilter;
			const term = guestSearch.trim().toLowerCase();
			const matchesSearch =
				!term || r.guestName.toLowerCase().includes(term) || r.guestEmail.toLowerCase().includes(term);
			return matchesStatus && matchesSearch;
		}),
	);

	const registrationStats = $derived({
		total: registrations.length,
		pending: registrations.filter((r) => r.status === "pending").length,
		approved: registrations.filter((r) => r.status === "approved").length,
		rejected: registrations.filter((r) => r.status === "rejected").length,
	});

	const invitationStats = $derived({
		total: invitations.length,
		pending: invitations.filter((i) => i.status === "pending" || i.status === "sent").length,
		verified: invitations.filter((i) => i.status === "verified").length,
		cancelled: invitations.filter((i) => i.status === "cancelled" || i.status === "expired").length,
	});

	const workshopStats = $derived({
		total: workshopInvites.length,
		pending: workshopInvites.filter((w) => w.status === "pending" || w.status === "sent").length,
		verified: workshopInvites.filter((w) => w.status === "verified").length,
	});

	function badgeClass(status: string): string {
		switch (status) {
			case "approved":
			case "verified":
				return "approved";
			case "rejected":
			case "cancelled":
			case "expired":
				return "rejected";
			case "sent":
				return "sent";
			default:
				return "pending";
		}
	}

	async function downloadExport(mouseEvent: MouseEvent, kind: TabKey, eventId: number, eventTitle: string) {
		mouseEvent.preventDefault();
		mouseEvent.stopPropagation();

		const path =
			kind === "registrations"
				? `/api/registrations/${eventId}/export`
				: kind === "invitations"
					? `/api/invitations/${eventId}/export`
					: `/api/workshop-invites/${eventId}/export`;

		try {
			const response = await fetch(path);
			if (!response.ok) {
				alert("Failed to download the Excel file.");
				return;
			}
			const blob = await response.blob();
			const slug = eventTitle
				.trim()
				.toLowerCase()
				.replace(/[^a-z0-9]+/g, "-")
				.replace(/(^-|-$)/g, "") || "event";
			const url = URL.createObjectURL(blob);
			const anchor = document.createElement("a");
			anchor.href = url;
			anchor.download = `${slug}-${kind}.xlsx`;
			document.body.appendChild(anchor);
			anchor.click();
			anchor.remove();
			URL.revokeObjectURL(url);
		} catch {
			alert("Failed to download the Excel file.");
		}
	}

	function formatDate(value?: string | null): string {
		if (!value) return "—";
		try {
			return new Date(value).toLocaleString("en-US", {
				year: "numeric",
				month: "short",
				day: "numeric",
				hour: "2-digit",
				minute: "2-digit",
			});
		} catch {
			return value;
		}
	}
</script>

<div class="attendees-page">
	<div class="page-header">
		<div>
			<h1>Attendees</h1>
			<p class="subtitle">Browse registrations, invitations, and workshop invites — grouped by event.</p>
		</div>
		{#if activeTab === "registrations"}
			<div class="stat-pills">
				<div class="stat-pill"><strong>{registrationStats.total}</strong> total</div>
				<div class="stat-pill ok"><strong>{registrationStats.approved}</strong> approved</div>
				<div class="stat-pill warn"><strong>{registrationStats.pending}</strong> awaiting review</div>
				<div class="stat-pill err"><strong>{registrationStats.rejected}</strong> rejected</div>
			</div>
		{:else if activeTab === "invitations"}
			<div class="stat-pills">
				<div class="stat-pill"><strong>{invitationStats.total}</strong> total</div>
				<div class="stat-pill ok"><strong>{invitationStats.verified}</strong> checked in</div>
				<div class="stat-pill warn"><strong>{invitationStats.pending}</strong> awaiting check-in</div>
				<div class="stat-pill err"><strong>{invitationStats.cancelled}</strong> cancelled/expired</div>
			</div>
		{:else}
			<div class="stat-pills">
				<div class="stat-pill"><strong>{workshopStats.total}</strong> total</div>
				<div class="stat-pill ok"><strong>{workshopStats.verified}</strong> checked in</div>
				<div class="stat-pill warn"><strong>{workshopStats.pending}</strong> awaiting check-in</div>
			</div>
		{/if}
	</div>

	{#if data.errors?.registrations}
		<div class="alert err"><strong>Unable to load registrations.</strong> {data.errors.registrations.message}</div>
	{/if}
	{#if data.errors?.invitations}
		<div class="alert err"><strong>Unable to load invitations.</strong> {data.errors.invitations.message}</div>
	{/if}
	{#if data.errors?.workshopInvites}
		<div class="alert err"><strong>Unable to load workshop invites.</strong> {data.errors.workshopInvites.message}</div>
	{/if}

	{#if form?.message}
		<div class="alert" class:ok={form?.success} class:err={!form?.success}>
			{form.message}
		</div>
	{/if}

	<div class="tabs">
		<button type="button" class="tab" class:active={activeTab === "registrations"} onclick={() => switchTab("registrations")}>
			<i class="fi fi-rr-form"></i> Registrations <span class="tab-count">{registrations.length}</span>
		</button>
		<button type="button" class="tab" class:active={activeTab === "invitations"} onclick={() => switchTab("invitations")}>
			<i class="fi fi-rr-envelope"></i> Invitations <span class="tab-count">{invitations.length}</span>
		</button>
		<button type="button" class="tab" class:active={activeTab === "workshop"} onclick={() => switchTab("workshop")}>
			<i class="fi fi-rr-users-alt"></i> Workshop Invites <span class="tab-count">{workshopInvites.length}</span>
		</button>
	</div>

	<div class="card">
		{#if activeTab === "registrations" && selectedEventId !== null}
			<!-- ── Level 2: registrations for one event ─────────────────────────── -->
			<button type="button" class="back-link" onclick={() => (selectedEventId = null)}>
				<i class="fi fi-rr-arrow-small-left"></i> All events
			</button>
			<h2 class="event-detail-title">{selectedEventTitle}</h2>

			<div class="filters-row">
				<input class="search" type="search" placeholder="Search by guest or email…" bind:value={guestSearch} />
				<select bind:value={statusFilter}>
					<option value="all">All statuses</option>
					<option value="pending">Pending</option>
					<option value="approved">Approved</option>
					<option value="rejected">Rejected</option>
					<option value="cancelled">Cancelled</option>
				</select>
			</div>

			{#if filteredRegistrations.length === 0}
				<div class="empty">
					<i class="fi fi-rr-inbox"></i>
					<p>No registrations match your filters.</p>
				</div>
			{:else}
				<div class="table-wrap">
					<table>
						<thead>
							<tr>
								<th>Guest</th>
								<th>Status</th>
								<th>Requested</th>
								<th class="right">Actions</th>
							</tr>
						</thead>
						<tbody>
							{#each filteredRegistrations as reg (reg.id)}
								<tr>
									<td>
										<div class="guest-name">{reg.guestName}</div>
										<div class="guest-meta">{reg.guestEmail}</div>
										{#if reg.guestPhone}<div class="guest-meta">{reg.guestPhone}</div>{/if}
										{#if reg.guestOrganization}<div class="guest-meta">{reg.guestOrganization}</div>{/if}
										{#if reg.additionalFields && Object.keys(reg.additionalFields).length}
											<details class="extra-fields">
												<summary>More details</summary>
												<dl>
													{#each Object.entries(reg.additionalFields) as [key, value] (key)}
														<dt>{getFieldLabel(key)}</dt>
														<dd>{value}</dd>
													{/each}
												</dl>
											</details>
										{/if}
									</td>
									<td>
										<span class="badge {badgeClass(reg.status)}">{reg.status}</span>
										{#if reg.reviewedAtUtc}<div class="guest-meta">{formatDate(reg.reviewedAtUtc)}</div>{/if}
									</td>
									<td class="muted">{formatDate(reg.requestedAtUtc)}</td>
									<td class="right">
										{#if reg.status === "pending" && data.canUpdate}
											<div class="row-actions">
												<form method="POST" action="?/approve" use:enhance={() => { return async ({ update }) => { await update(); await invalidateAll(); }; }} style="display:inline">
													<input type="hidden" name="registrationId" value={reg.id} />
													<button class="mini ok" title="Approve"><i class="fi fi-rr-check"></i> Approve</button>
												</form>
												<form method="POST" action="?/reject" use:enhance={({ cancel }) => { if (!confirm("Reject this registration?")) { cancel(); return; } return async ({ update }) => { await update(); await invalidateAll(); }; }} style="display:inline">
													<input type="hidden" name="registrationId" value={reg.id} />
													<button class="mini danger" title="Reject"><i class="fi fi-rr-cross-small"></i> Reject</button>
												</form>
											</div>
										{:else}
											<span class="muted">—</span>
										{/if}
									</td>
								</tr>
							{/each}
						</tbody>
					</table>
				</div>
			{/if}
		{:else}
			<!-- ── Level 1: event picker for the active tab ─────────────────────── -->
			<div class="filters-row">
				<input class="search" type="search" placeholder="Search events…" bind:value={eventSearch} />
			</div>

			{#if filteredGroups.length === 0}
				<div class="empty">
					<i class="fi fi-rr-calendar-day"></i>
					<p>No events with {activeTab === "registrations" ? "registrations" : activeTab === "invitations" ? "invitations" : "workshop invites"} match your search.</p>
				</div>
			{:else}
				<div class="event-grid">
					{#each filteredGroups as group (group.eventId)}
						<div class="event-card-wrap">
							{#if activeTab === "registrations"}
								<button type="button" class="event-card" onclick={() => (selectedEventId = group.eventId)}>
									<div class="event-card-title">{group.eventTitle}</div>
									<div class="event-card-total">{group.total} registration{group.total !== 1 ? "s" : ""}</div>
									<div class="event-card-badges">
										{#if group.counts.pending}<span class="badge pending">{group.counts.pending} pending</span>{/if}
										{#if group.counts.approved}<span class="badge approved">{group.counts.approved} approved</span>{/if}
										{#if group.counts.rejected}<span class="badge rejected">{group.counts.rejected} rejected</span>{/if}
									</div>
								</button>
							{:else if activeTab === "invitations"}
								<a class="event-card" href="/admin/events/invitations/{group.eventId}">
									<div class="event-card-title">{group.eventTitle}</div>
									<div class="event-card-total">{group.total} invitation{group.total !== 1 ? "s" : ""}</div>
									<div class="event-card-badges">
										{#if group.counts.pending}<span class="badge pending">{group.counts.pending} pending</span>{/if}
										{#if group.counts.sent}<span class="badge sent">{group.counts.sent} sent</span>{/if}
										{#if group.counts.verified}<span class="badge approved">{group.counts.verified} checked in</span>{/if}
										{#if group.counts.cancelled}<span class="badge rejected">{group.counts.cancelled} cancelled</span>{/if}
									</div>
									<div class="event-card-cta">Manage invitations <i class="fi fi-rr-arrow-small-right"></i></div>
								</a>
							{:else}
								<a class="event-card" href="/admin/events/workshop-invites/{group.eventId}">
									<div class="event-card-title">{group.eventTitle}</div>
									<div class="event-card-total">{group.total} recipient{group.total !== 1 ? "s" : ""}</div>
									<div class="event-card-badges">
										{#if group.counts.pending}<span class="badge pending">{group.counts.pending} pending</span>{/if}
										{#if group.counts.sent}<span class="badge sent">{group.counts.sent} sent</span>{/if}
										{#if group.counts.verified}<span class="badge approved">{group.counts.verified} checked in</span>{/if}
									</div>
									<div class="event-card-cta">Manage workshop invites <i class="fi fi-rr-arrow-small-right"></i></div>
								</a>
							{/if}
							<button
								type="button"
								class="event-card-download"
								title="Download {group.eventTitle} attendee list (Excel)"
								onclick={(e) => downloadExport(e, activeTab, group.eventId, group.eventTitle)}
							>
								<i class="fi fi-rr-file-download"></i>
							</button>
						</div>
					{/each}
				</div>
			{/if}
		{/if}
	</div>
</div>

<style>
	.attendees-page { max-width: 1280px; margin: 0 auto; display: flex; flex-direction: column; gap: 20px; }
	.page-header { display: flex; justify-content: space-between; align-items: flex-end; flex-wrap: wrap; gap: 16px; }
	.page-header h1 { margin: 0; font-size: 28px; color: var(--color-dark); }
	.subtitle { margin: 4px 0 0; color: var(--color-text-muted); }
	.stat-pills { display: flex; gap: 10px; flex-wrap: wrap; }
	.stat-pill { background: #fff; border: 1px solid rgba(0,0,0,.08); border-radius: 10px; padding: 8px 14px; font-size: 13px; color: var(--color-text-muted); }
	.stat-pill strong { color: var(--color-dark); font-size: 16px; margin-right: 4px; }
	.stat-pill.ok strong { color: #16a34a; }
	.stat-pill.warn strong { color: #b28600; }
	.stat-pill.err strong { color: #dc2626; }

	.alert { padding: 12px 16px; border-radius: 10px; font-size: 14px; }
	.alert.ok { background: rgba(34,197,94,.1); color: #16a34a; border: 1px solid rgba(34,197,94,.25); }
	.alert.err { background: rgba(239,68,68,.1); color: #dc2626; border: 1px solid rgba(239,68,68,.25); }

	/* ── Tabs ─────────────────────────────────────────────────────────────── */
	.tabs { display: flex; gap: 6px; border-bottom: 1.5px solid rgba(28,92,109,.1); }
	.tab {
		display: inline-flex; align-items: center; gap: 8px;
		padding: 10px 16px; border: none; background: none; cursor: pointer;
		font-size: 14px; font-weight: 600; color: var(--color-text-muted);
		border-bottom: 2.5px solid transparent; margin-bottom: -1.5px;
	}
	.tab:hover { color: var(--color-dark); }
	.tab.active { color: #1c5c6d; border-bottom-color: #1c5c6d; }
	.tab-count { background: #f1f5f9; color: #475569; border-radius: 10px; padding: 1px 8px; font-size: 12px; font-weight: 700; }
	.tab.active .tab-count { background: rgba(28,92,109,.12); color: #1c5c6d; }

	.card { background: #fff; border: 1px solid rgba(28,92,109,.1); border-radius: 16px; padding: 22px; }

	.filters-row { display: flex; gap: 10px; margin-bottom: 18px; flex-wrap: wrap; }
	.filters-row .search { flex: 1; min-width: 220px; }
	.filters-row input, .filters-row select { padding: 10px 12px; border: 1px solid #d7dee7; border-radius: 10px; font-size: 13.5px; outline: none; }
	.filters-row input:focus, .filters-row select:focus { border-color: #1c5c6d; box-shadow: 0 0 0 3px rgba(28,92,109,.12); }

	/* ── Event picker grid (level 1) ──────────────────────────────────────── */
	.event-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(260px, 1fr)); gap: 14px; }
	.event-card-wrap { position: relative; }
	.event-card {
		display: flex; flex-direction: column; gap: 8px; text-align: left;
		background: #f8fafc; border: 1px solid #e2e8f0; border-radius: 14px;
		padding: 16px; width: 100%; cursor: pointer; text-decoration: none; color: inherit;
		transition: border-color .12s, box-shadow .12s;
	}
	.event-card:hover { border-color: #1c5c6d; box-shadow: 0 4px 14px rgba(28,92,109,.1); }
	.event-card-title { font-weight: 700; color: var(--color-dark); font-size: 15px; padding-right: 30px; }
	.event-card-total { font-size: 12.5px; color: var(--color-text-muted); }
	.event-card-badges { display: flex; flex-wrap: wrap; gap: 6px; }
	.event-card-cta { margin-top: 4px; font-size: 12.5px; font-weight: 700; color: #1c5c6d; display: flex; align-items: center; gap: 4px; }

	.event-card-download {
		position: absolute; top: 14px; right: 14px; z-index: 2;
		display: inline-flex; align-items: center; justify-content: center;
		width: 28px; height: 28px; border-radius: 8px;
		border: 1px solid #e2e8f0; background: #fff; color: #1c5c6d;
		cursor: pointer; transition: background .12s, color .12s, border-color .12s;
	}
	.event-card-download:hover { background: #1c5c6d; color: #fff; border-color: #1c5c6d; }
	.event-card-download i { font-size: 12.5px; }

	/* ── Back link / detail header (level 2) ─────────────────────────────── */
	.back-link {
		display: inline-flex; align-items: center; gap: 6px; border: none; background: none;
		cursor: pointer; color: #1c5c6d; font-weight: 600; font-size: 13.5px; padding: 0; margin-bottom: 12px;
	}
	.back-link:hover { text-decoration: underline; }
	.event-detail-title { margin: 0 0 16px; font-size: 20px; color: var(--color-dark); }

	.table-wrap { overflow-x: auto; }
	table { width: 100%; border-collapse: collapse; }
	th { text-align: left; font-size: 12.5px; color: var(--color-secondary); padding: 10px 12px; border-bottom: 1.5px solid rgba(28,92,109,.1); }
	td { padding: 12px; border-bottom: 1px solid #f1f5f9; font-size: 13.5px; vertical-align: top; }
	.right { text-align: right; }
	.guest-name { font-weight: 600; color: var(--color-dark); }
	.guest-meta { font-size: 12px; color: var(--color-text-muted); }
	.muted { color: var(--color-text-muted); }

	.extra-fields { margin-top: 6px; }
	.extra-fields summary { font-size: 11.5px; color: var(--color-primary, #1c5c6d); cursor: pointer; font-weight: 600; }
	.extra-fields dl { margin: 8px 0 0; display: grid; grid-template-columns: minmax(0, 40%) minmax(0, 60%); gap: 4px 10px; }
	.extra-fields dt { font-size: 11px; font-weight: 600; color: var(--color-text-muted); }
	.extra-fields dd { margin: 0; font-size: 12px; color: var(--color-dark); word-break: break-word; }

	.badge { display: inline-block; padding: 3px 10px; border-radius: 8px; font-size: 11.5px; font-weight: 600; text-transform: capitalize; }
	.badge.pending { background: rgba(107,114,128,.12); color: #374151; }
	.badge.approved { background: rgba(34,197,94,.12); color: #16a34a; }
	.badge.rejected { background: rgba(239,68,68,.12); color: #dc2626; }
	.badge.sent { background: rgba(28,92,109,.12); color: #1c5c6d; }

	.row-actions { display: inline-flex; gap: 6px; justify-content: flex-end; }
	.mini { display: inline-flex; align-items: center; gap: 4px; height: 32px; padding: 0 10px; border-radius: 8px; border: 1px solid #e2e8f0; background: #f8fafc; cursor: pointer; color: #475569; font-size: 12.5px; font-weight: 600; }
	.mini:hover { background: #e2e8f0; }
	.mini.ok { color: #16a34a; border-color: rgba(34,197,94,.3); background: rgba(34,197,94,.06); }
	.mini.danger { color: #dc2626; border-color: rgba(239,68,68,.3); background: rgba(239,68,68,.06); }

	.empty { text-align: center; padding: 40px 20px; color: var(--color-text-muted); }
	.empty i { font-size: 36px; opacity: .5; display: block; margin-bottom: 10px; }
</style>
