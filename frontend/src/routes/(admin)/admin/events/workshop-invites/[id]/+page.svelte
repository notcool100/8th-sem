<script lang="ts">
	import { enhance } from "$app/forms";
	import { invalidateAll } from "$app/navigation";
	import { goto } from "$app/navigation";
	import type { PageData } from "./$types";
	import type { ImportWorkshopInviteeRow, WorkshopInviteDto } from "$lib/types/workshop-invites";

	let { data, form } = $props<{ data: PageData; form?: any }>();

	let invites = $derived((data.invites ?? []) as WorkshopInviteDto[]);
	let search = $state("");
	let workshopDate = $state((data.event?.date_ad ?? "").slice(0, 10));
	let sendingId = $state<number | null>(null);
	let importing = $state(false);
	let importRowsJson = $state("");
	let importForm: HTMLFormElement | undefined;
	let fileInput: HTMLInputElement | undefined;
	let qrPreview = $state<WorkshopInviteDto | null>(null);

	function pickFirst(row: Record<string, unknown>, keys: string[]): string {
		for (const key of keys) {
			const value = row[key];
			if (value !== undefined && value !== null && String(value).trim() !== "") {
				return String(value).trim();
			}
		}
		return "";
	}

	async function handleFileSelected(event: Event) {
		const input = event.currentTarget as HTMLInputElement;
		const file = input.files?.[0];
		if (!file) return;

		const XLSX = await import("xlsx");
		const buffer = await file.arrayBuffer();
		const workbook = XLSX.read(buffer, { type: "array" });
		const sheet = workbook.Sheets[workbook.SheetNames[0]];
		const rawRows = XLSX.utils.sheet_to_json<Record<string, unknown>>(sheet, { defval: "" });

		const rows: ImportWorkshopInviteeRow[] = rawRows
			.map((row) => ({
				fullName: pickFirst(row, ["Full Name", "Name", "fullName"]),
				email: pickFirst(row, ["Email", "email"]),
				phone: pickFirst(row, ["Mobile", "Phone", "phone"]) || undefined,
				organization: pickFirst(row, ["Organisation", "Organization", "organization"]) || undefined
			}))
			.filter((row) => row.fullName && row.email);

		if (rows.length === 0) {
			alert("No valid rows found (need at least a Name and Email column).");
			input.value = "";
			return;
		}

		importRowsJson = JSON.stringify(rows);
		importing = true;
		importForm?.requestSubmit();
	}

	let filtered = $derived(
		invites.filter((invite) => {
			const q = search.trim().toLowerCase();
			if (!q) return true;
			return invite.fullName.toLowerCase().includes(q) || invite.email.toLowerCase().includes(q);
		})
	);

	const statusCounts = $derived({
		total: invites.length,
		sent: invites.filter((i) => i.status === "sent").length,
		pending: invites.filter((i) => i.status === "pending").length,
		verified: invites.filter((i) => i.status === "verified").length
	});

	function formatDate(value?: string | null): string {
		if (!value) return "—";
		try {
			return new Date(value).toLocaleString("en-US", {
				year: "numeric",
				month: "short",
				day: "numeric",
				hour: "2-digit",
				minute: "2-digit"
			});
		} catch {
			return value;
		}
	}
</script>

<div class="invite-page">
	<div class="page-header">
		<div>
			<button class="back-link" onclick={() => goto("/admin/events")}>
				<i class="fi fi-rr-angle-left"></i> Back to events
			</button>
			<h1>Workshop Invites</h1>
			<p class="subtitle">{data.event?.title}</p>
		</div>
		<div class="stat-pills">
			<div class="stat-pill"><strong>{statusCounts.total}</strong> total</div>
			<div class="stat-pill ok"><strong>{statusCounts.verified}</strong> checked in</div>
			<div class="stat-pill sent"><strong>{statusCounts.sent}</strong> sent</div>
			<div class="stat-pill warn"><strong>{statusCounts.pending}</strong> not sent</div>
		</div>
	</div>

	{#if form?.message}
		<div class="alert" class:ok={form?.success} class:err={!form?.success}>
			{form.message}
		</div>
	{/if}

	<div class="card">
		<div class="toolbar">
			<div class="field date-field">
				<label for="workshopDate">Workshop date</label>
				<input id="workshopDate" type="date" bind:value={workshopDate} />
				<span class="hint">Used in the email and the invitation page as this guest's scheduled date.</span>
			</div>
			<div class="field search-field">
				<label for="search">Search</label>
				<input id="search" type="text" placeholder="Search by name or email…" bind:value={search} />
			</div>
			<div class="field">
				<span class="field-label-spacer">&nbsp;</span>
				<button
					type="button"
					class="mini-btn"
					disabled={importing}
					onclick={() => fileInput?.click()}
				>
					{#if importing}<i class="fi fi-rr-spinner"></i> Importing…{:else}<i class="fi fi-rr-upload"></i> Import from Excel{/if}
				</button>
				<input
					bind:this={fileInput}
					type="file"
					accept=".xlsx,.xls"
					style="display:none"
					onchange={handleFileSelected}
				/>
			</div>
		</div>

		<form
			bind:this={importForm}
			method="POST"
			action="?/import"
			style="display:none"
			use:enhance={() => {
				return async ({ update }) => {
					importing = false;
					if (fileInput) fileInput.value = "";
					await update();
					await invalidateAll();
				};
			}}
		>
			<input type="hidden" name="rows" value={importRowsJson} />
		</form>

		{#if invites.length === 0}
			<div class="empty">
				<i class="fi fi-rr-inbox"></i>
				<p>No recipients imported for this event yet.</p>
			</div>
		{:else}
			<div class="table-wrap">
				<table>
					<thead>
						<tr>
							<th>Name</th>
							<th>Email</th>
							<th>Organization</th>
							<th>Status</th>
							<th>QR</th>
							<th class="right">Action</th>
						</tr>
					</thead>
					<tbody>
						{#each filtered as invite (invite.id)}
							<tr>
								<td>
									<div class="guest-name">{invite.fullName}</div>
									{#if invite.phone}<div class="guest-meta">{invite.phone}</div>{/if}
								</td>
								<td class="muted">{invite.email}</td>
								<td class="muted">{invite.organization || "—"}</td>
								<td>
									<span class="badge {invite.status}">{invite.status}</span>
									{#if invite.status === "sent"}<div class="guest-meta">{formatDate(invite.sentAtUtc)}</div>{/if}
									{#if invite.status === "verified"}<div class="guest-meta">{formatDate(invite.verifiedAtUtc)}</div>{/if}
								</td>
								<td>
									{#if invite.qrDataUri}
										<button class="qr-thumb" onclick={() => (qrPreview = invite)} title="View QR">
											<img src={invite.qrDataUri} alt="QR code" />
										</button>
									{:else}—{/if}
								</td>
								<td class="right">
									<form
										method="POST"
										action="?/send"
										use:enhance={() => {
											sendingId = invite.id;
											return async ({ update }) => {
												sendingId = null;
												await update();
												await invalidateAll();
											};
										}}
										style="display:inline"
									>
										<input type="hidden" name="inviteId" value={invite.id} />
										<input type="hidden" name="workshopDate" value={workshopDate} />
										<button class="mini-btn" disabled={sendingId === invite.id || !workshopDate || invite.status === "verified"} title={invite.status === "verified" ? "Already checked in" : "Send invitation"}>
											{#if sendingId === invite.id}
												<i class="fi fi-rr-spinner"></i> Sending…
											{:else if invite.status === "sent"}
												<i class="fi fi-rr-refresh"></i> Resend
											{:else if invite.status === "verified"}
												<i class="fi fi-rr-check"></i> Checked in
											{:else}
												<i class="fi fi-rr-paper-plane"></i> Send
											{/if}
										</button>
									</form>
								</td>
							</tr>
						{/each}
					</tbody>
				</table>
			</div>
		{/if}
	</div>
</div>

{#if qrPreview}
	<div class="overlay" onclick={() => (qrPreview = null)} role="presentation">
		<div class="qr-modal" onclick={(e) => e.stopPropagation()} role="dialog" aria-label="QR code" tabindex="-1">
			<h3>{qrPreview.fullName}</h3>
			<p class="muted">{qrPreview.email}</p>
			<img src={qrPreview.qrDataUri} alt="Workshop invite QR code" />
			<p class="token">Code: {qrPreview.token}</p>
			<button class="primary-btn" onclick={() => (qrPreview = null)}>Close</button>
		</div>
	</div>
{/if}

<style>
	.invite-page { max-width: 1200px; margin: 0 auto; display: flex; flex-direction: column; gap: 24px; }
	.page-header { display: flex; justify-content: space-between; align-items: flex-end; flex-wrap: wrap; gap: 16px; }
	.back-link { background: none; border: none; color: var(--color-secondary); cursor: pointer; font-size: 13px; padding: 0 0 8px; display: inline-flex; align-items: center; gap: 4px; }
	.page-header h1 { margin: 0; font-size: 28px; color: var(--color-dark); }
	.subtitle { margin: 4px 0 0; color: var(--color-text-muted); }
	.stat-pills { display: flex; gap: 10px; }
	.stat-pill { background: #fff; border: 1px solid rgba(0,0,0,.08); border-radius: 10px; padding: 8px 14px; font-size: 13px; color: var(--color-text-muted); }
	.stat-pill strong { color: var(--color-dark); font-size: 16px; margin-right: 4px; }
	.stat-pill.ok strong { color: #16a34a; }
	.stat-pill.sent strong { color: var(--color-secondary); }
	.stat-pill.warn strong { color: #b28600; }

	.alert { padding: 12px 16px; border-radius: 10px; font-size: 14px; }
	.alert.ok { background: rgba(34,197,94,.1); color: #16a34a; border: 1px solid rgba(34,197,94,.25); }
	.alert.err { background: rgba(239,68,68,.1); color: #dc2626; border: 1px solid rgba(239,68,68,.25); }

	.card { background: #fff; border: 1px solid rgba(28,92,109,.1); border-radius: 16px; padding: 22px; }

	.toolbar { display: flex; gap: 20px; flex-wrap: wrap; margin-bottom: 18px; }
	.field { display: flex; flex-direction: column; gap: 6px; }
	.field label { font-size: 13px; font-weight: 600; color: var(--color-dark); }
	.field input { padding: 11px 12px; border: 1px solid rgba(0,0,0,.12); border-radius: 10px; font-size: 14px; outline: none; }
	.field input:focus { border-color: var(--color-secondary); box-shadow: 0 0 0 3px rgba(28,92,109,.12); }
	.hint { font-size: 11.5px; color: var(--color-text-muted); }
	.date-field input { width: 180px; }
	.search-field { flex: 1; min-width: 220px; }
	.search-field input { width: 100%; }
	.field-label-spacer { font-size: 13px; line-height: 1; visibility: hidden; }

	.table-wrap { overflow-x: auto; max-height: 70vh; overflow-y: auto; }
	table { width: 100%; border-collapse: collapse; }
	th { text-align: left; font-size: 12.5px; color: var(--color-secondary); padding: 10px 12px; border-bottom: 1.5px solid rgba(28,92,109,.1); position: sticky; top: 0; background: #fff; }
	td { padding: 12px; border-bottom: 1px solid #f1f5f9; font-size: 13.5px; vertical-align: top; }
	.right { text-align: right; }
	.guest-name { font-weight: 600; color: var(--color-dark); }
	.guest-meta { font-size: 12px; color: var(--color-text-muted); }
	.muted { color: var(--color-text-muted); }

	.badge { display: inline-block; padding: 3px 10px; border-radius: 8px; font-size: 11.5px; font-weight: 600; text-transform: capitalize; }
	.badge.pending { background: rgba(107,114,128,.12); color: #374151; }
	.badge.sent { background: rgba(28,92,109,.12); color: #1c5c6d; }
	.badge.verified { background: rgba(34,197,94,.12); color: #16a34a; }

	.mini-btn { background: linear-gradient(135deg, var(--color-primary), #e6b910); color: var(--color-dark); border: none; padding: 8px 14px; border-radius: 8px; font-weight: 600; font-size: 12.5px; cursor: pointer; display: inline-flex; align-items: center; gap: 6px; }
	.mini-btn:disabled { opacity: .6; cursor: not-allowed; }

	.qr-thumb { border: 1px solid #e2e8f0; border-radius: 8px; padding: 2px; background: #fff; cursor: pointer; line-height: 0; }
	.qr-thumb img { width: 44px; height: 44px; }

	.overlay { position: fixed; inset: 0; background: rgba(15,23,42,.6); backdrop-filter: blur(6px); display: flex; align-items: center; justify-content: center; z-index: 1100; padding: 20px; }
	.qr-modal { background: #fff; border-radius: 18px; padding: 28px; text-align: center; max-width: 360px; width: 100%; }
	.qr-modal h3 { margin: 0 0 2px; }
	.qr-modal img { width: 260px; height: 260px; margin: 14px auto; display: block; }
	.token { font-family: monospace; font-size: 12px; color: var(--color-text-muted); word-break: break-all; margin: 0 0 16px; }
	.primary-btn { background: linear-gradient(135deg, var(--color-primary), #e6b910); color: var(--color-dark); border: none; padding: 12px 20px; border-radius: 10px; font-weight: 600; font-size: 14px; cursor: pointer; display: inline-flex; align-items: center; gap: 8px; }

	.empty { text-align: center; padding: 40px 20px; color: var(--color-text-muted); }
	.empty i { font-size: 36px; opacity: .5; display: block; margin-bottom: 10px; }
</style>
