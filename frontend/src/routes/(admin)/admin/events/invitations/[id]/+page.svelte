<script lang="ts">
	import { enhance } from "$app/forms";
	import { invalidateAll } from "$app/navigation";
	import { goto } from "$app/navigation";
	import type { PageData } from "./$types";
	import type { InvitationDto } from "$lib/types/invitations";

	let { data, form } = $props<{ data: PageData; form?: any }>();

	let invitations = $derived((data.invitations ?? []) as InvitationDto[]);
	let submitting = $state(false);
	let qrPreview = $state<InvitationDto | null>(null);

	const statusCounts = $derived({
		total: invitations.length,
		verified: invitations.filter((i) => i.status === "verified").length,
		pending: invitations.filter((i) => i.status === "pending" || i.status === "sent").length
	});

	function badgeClass(status: string): string {
		switch (status) {
			case "verified":
				return "verified";
			case "sent":
				return "sent";
			case "expired":
				return "expired";
			case "cancelled":
				return "cancelled";
			default:
				return "pending";
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
				minute: "2-digit"
			});
		} catch {
			return value;
		}
	}

	async function copyLink(url: string) {
		try {
			await navigator.clipboard.writeText(url);
			alert("Invite link copied to clipboard.");
		} catch {
			alert(url);
		}
	}
</script>

<div class="invite-page">
	<div class="page-header">
		<div>
			<button class="back-link" onclick={() => goto("/admin/events")}>
				<i class="fi fi-rr-angle-left"></i> Back to events
			</button>
			<h1>Invitations</h1>
			<p class="subtitle">{data.event?.title}</p>
		</div>
		<div class="stat-pills">
			<div class="stat-pill"><strong>{statusCounts.total}</strong> total</div>
			<div class="stat-pill ok"><strong>{statusCounts.verified}</strong> checked in</div>
			<div class="stat-pill warn"><strong>{statusCounts.pending}</strong> awaiting</div>
		</div>
	</div>

	{#if form?.message}
		<div class="alert" class:ok={form?.success} class:err={!form?.success}>
			{form.message}
		</div>
	{/if}

	<div class="layout">
		<!-- Invite form -->
		<div class="card invite-card">
			<h2><i class="fi fi-rr-user-add"></i> Invite a guest</h2>
			<p class="card-hint">Enter the guest's details. They'll receive an email with a unique invitation link and QR code.</p>

			<form
				method="POST"
				action="?/invite"
				use:enhance={() => {
					submitting = true;
					return async ({ result, update }) => {
						submitting = false;
						await update();
						if (result.type === "success") {
							await invalidateAll();
						}
					};
				}}
			>
				<div class="field">
					<label for="fullName">Full name *</label>
					<input id="fullName" name="fullName" type="text" required placeholder="e.g. Ramesh Sharma" value={form?.values?.fullName ?? ""} />
				</div>
				<div class="field">
					<label for="email">Email (Gmail) *</label>
					<input id="email" name="email" type="email" required placeholder="guest@gmail.com" value={form?.values?.email ?? ""} />
				</div>
				<div class="field">
					<label for="phone">Phone</label>
					<input id="phone" name="phone" type="tel" placeholder="+977 98XXXXXXXX" value={form?.values?.phone ?? ""} />
				</div>
				<div class="field">
					<label for="organization">Organization</label>
					<input id="organization" name="organization" type="text" placeholder="Company / institution" value={form?.values?.organization ?? ""} />
				</div>
				<div class="field">
					<label for="expiresAt">QR valid until (optional)</label>
					<input id="expiresAt" name="expiresAt" type="datetime-local" value={form?.values?.expiresAt ?? ""} />
					<span class="hint">Defaults to the event end date.</span>
				</div>
				<button type="submit" class="primary-btn" disabled={submitting}>
					{#if submitting}<i class="fi fi-rr-spinner"></i> Sending…{:else}<i class="fi fi-rr-paper-plane"></i> Send invitation{/if}
				</button>
			</form>
		</div>

		<!-- Invitations list -->
		<div class="card list-card">
			<h2><i class="fi fi-rr-envelope"></i> Sent invitations ({invitations.length})</h2>

			{#if invitations.length === 0}
				<div class="empty">
					<i class="fi fi-rr-inbox"></i>
					<p>No invitations yet. Invite your first guest using the form.</p>
				</div>
			{:else}
				<div class="table-wrap">
					<table>
						<thead>
							<tr>
								<th>Guest</th>
								<th>Status</th>
								<th>Expires</th>
								<th>QR</th>
								<th class="right">Actions</th>
							</tr>
						</thead>
						<tbody>
							{#each invitations as inv (inv.id)}
								<tr>
									<td>
										<div class="guest-name">{inv.guestName}</div>
										<div class="guest-meta">{inv.guestEmail}</div>
										{#if inv.guestOrganization}<div class="guest-meta">{inv.guestOrganization}</div>{/if}
									</td>
									<td>
										<span class="badge {badgeClass(inv.status)}">{inv.status}</span>
										{#if inv.status === "verified"}<div class="guest-meta">{formatDate(inv.verifiedAtUtc)}</div>{/if}
									</td>
									<td class="muted">{formatDate(inv.expiresAtUtc)}</td>
									<td>
										{#if inv.qrDataUri}
											<button class="qr-thumb" onclick={() => (qrPreview = inv)} title="View QR">
												<img src={inv.qrDataUri} alt="QR code" />
											</button>
										{:else}—{/if}
									</td>
									<td class="right">
										<div class="row-actions">
											<button class="mini" onclick={() => copyLink(inv.inviteUrl)} title="Copy invite link">
												<i class="fi fi-rr-link"></i>
											</button>
											{#if inv.status !== "verified" && inv.status !== "cancelled"}
												<form method="POST" action="?/resend" use:enhance={() => { return async ({ update }) => { await update(); await invalidateAll(); }; }} style="display:inline">
													<input type="hidden" name="invitationId" value={inv.id} />
													<button class="mini" title="Resend email"><i class="fi fi-rr-refresh"></i></button>
												</form>
												<form method="POST" action="?/cancel" use:enhance={({ cancel }) => { if (!confirm("Cancel this invitation? The QR will stop working.")) { cancel(); return; } return async ({ update }) => { await update(); await invalidateAll(); }; }} style="display:inline">
													<input type="hidden" name="invitationId" value={inv.id} />
													<button class="mini danger" title="Cancel invitation"><i class="fi fi-rr-cross-small"></i></button>
												</form>
											{/if}
										</div>
									</td>
								</tr>
							{/each}
						</tbody>
					</table>
				</div>
			{/if}
		</div>
	</div>
</div>

{#if qrPreview}
	<div class="overlay" onclick={() => (qrPreview = null)} role="presentation">
		<div class="qr-modal" onclick={(e) => e.stopPropagation()} role="dialog" aria-label="QR code" tabindex="-1">
			<h3>{qrPreview.guestName}</h3>
			<p class="muted">{qrPreview.guestEmail}</p>
			<img src={qrPreview.qrDataUri} alt="Invitation QR code" />
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
	.stat-pill.warn strong { color: #b28600; }

	.alert { padding: 12px 16px; border-radius: 10px; font-size: 14px; }
	.alert.ok { background: rgba(34,197,94,.1); color: #16a34a; border: 1px solid rgba(34,197,94,.25); }
	.alert.err { background: rgba(239,68,68,.1); color: #dc2626; border: 1px solid rgba(239,68,68,.25); }

	.layout { display: grid; grid-template-columns: 360px 1fr; gap: 24px; align-items: start; }
	@media (max-width: 900px) { .layout { grid-template-columns: 1fr; } }

	.card { background: #fff; border: 1px solid rgba(28,92,109,.1); border-radius: 16px; padding: 22px; }
	.card h2 { margin: 0 0 6px; font-size: 17px; color: var(--color-dark); display: flex; align-items: center; gap: 8px; }
	.card-hint { margin: 0 0 16px; font-size: 13px; color: var(--color-text-muted); }

	.field { display: flex; flex-direction: column; gap: 6px; margin-bottom: 14px; }
	.field label { font-size: 13px; font-weight: 600; color: var(--color-dark); }
	.field input { padding: 11px 12px; border: 1px solid rgba(0,0,0,.12); border-radius: 10px; font-size: 14px; outline: none; }
	.field input:focus { border-color: var(--color-secondary); box-shadow: 0 0 0 3px rgba(28,92,109,.12); }
	.hint { font-size: 11.5px; color: var(--color-text-muted); }

	.primary-btn { background: linear-gradient(135deg, var(--color-primary), #e6b910); color: var(--color-dark); border: none; padding: 12px 20px; border-radius: 10px; font-weight: 600; font-size: 14px; cursor: pointer; display: inline-flex; align-items: center; gap: 8px; }
	.primary-btn:disabled { opacity: .6; cursor: not-allowed; }

	.table-wrap { overflow-x: auto; }
	table { width: 100%; border-collapse: collapse; }
	th { text-align: left; font-size: 12.5px; color: var(--color-secondary); padding: 10px 12px; border-bottom: 1.5px solid rgba(28,92,109,.1); }
	td { padding: 12px; border-bottom: 1px solid #f1f5f9; font-size: 13.5px; vertical-align: top; }
	.right { text-align: right; }
	.guest-name { font-weight: 600; color: var(--color-dark); }
	.guest-meta { font-size: 12px; color: var(--color-text-muted); }
	.muted { color: var(--color-text-muted); }

	.badge { display: inline-block; padding: 3px 10px; border-radius: 8px; font-size: 11.5px; font-weight: 600; text-transform: capitalize; }
	.badge.pending { background: rgba(107,114,128,.12); color: #374151; }
	.badge.sent { background: rgba(28,92,109,.12); color: #1c5c6d; }
	.badge.verified { background: rgba(34,197,94,.12); color: #16a34a; }
	.badge.expired { background: rgba(245,158,11,.12); color: #b28600; }
	.badge.cancelled { background: rgba(239,68,68,.12); color: #dc2626; }

	.qr-thumb { border: 1px solid #e2e8f0; border-radius: 8px; padding: 2px; background: #fff; cursor: pointer; line-height: 0; }
	.qr-thumb img { width: 44px; height: 44px; }

	.row-actions { display: inline-flex; gap: 6px; }
	.mini { width: 32px; height: 32px; border-radius: 8px; border: 1px solid #e2e8f0; background: #f8fafc; cursor: pointer; color: #475569; }
	.mini:hover { background: #e2e8f0; }
	.mini.danger { color: #dc2626; border-color: rgba(239,68,68,.3); background: rgba(239,68,68,.06); }

	.empty { text-align: center; padding: 40px 20px; color: var(--color-text-muted); }
	.empty i { font-size: 36px; opacity: .5; display: block; margin-bottom: 10px; }

	.overlay { position: fixed; inset: 0; background: rgba(15,23,42,.6); backdrop-filter: blur(6px); display: flex; align-items: center; justify-content: center; z-index: 1100; padding: 20px; }
	.qr-modal { background: #fff; border-radius: 18px; padding: 28px; text-align: center; max-width: 360px; width: 100%; }
	.qr-modal h3 { margin: 0 0 2px; }
	.qr-modal img { width: 260px; height: 260px; margin: 14px auto; display: block; }
	.token { font-family: monospace; font-size: 12px; color: var(--color-text-muted); word-break: break-all; margin: 0 0 16px; }
</style>
