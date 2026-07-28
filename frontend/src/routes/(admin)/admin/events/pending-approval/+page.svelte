<script lang="ts">
	import { enhance } from "$app/forms";
	import type { PageData, ActionData } from "./$types";
	import type { ApprovalRequest } from "$lib/types/auth";

	let { data, form }: { data: PageData; form: ActionData | null } = $props();

	let approvals: ApprovalRequest[] = $derived(data.approvals ?? []);
	let pending = $derived(approvals.filter(a => a.status === "pending"));
	let reviewed = $derived(approvals.filter(a => a.status !== "pending"));

	let rejectingId = $state<number | null>(null);
	let rejectReason = $state("");
	let processingId = $state<number | null>(null);

	function openReject(id: number) {
		rejectingId = id;
		rejectReason = "";
	}

	function closeReject() {
		rejectingId = null;
		rejectReason = "";
	}

	function formatDate(iso: string | null) {
		if (!iso) return "—";
		return new Date(iso).toLocaleDateString("en-US", {
			year: "numeric", month: "short", day: "numeric",
			hour: "2-digit", minute: "2-digit"
		});
	}
</script>

<div class="page-shell">
	<div class="page-header">
		<div>
			<h1>Event Approvals</h1>
			<p>Review and approve or reject event submissions from team members.</p>
		</div>
		<div class="badge-summary">
			<span class="badge pending">{pending.length} Pending</span>
			<span class="badge reviewed">{reviewed.length} Reviewed</span>
		</div>
	</div>

	{#if form?.message}
		<div class="toast {form.success ? 'toast-success' : 'toast-error'}">
			<i class="fi fi-rr-{form.success ? 'check' : 'cross-circle'}"></i>
			{form.message}
		</div>
	{/if}

	<!-- Pending approvals -->
	{#if pending.length > 0}
		<section class="card-section">
			<h2 class="section-title"><i class="fi fi-rr-clock"></i> Pending Review</h2>
			<div class="approval-list">
				{#each pending as req (req.id)}
					<div class="approval-card pending-card">
						<div class="approval-meta">
							<div class="action-badge {req.action}">
								{req.action === "create" ? "New Event" : "Update"}
							</div>
							<span class="event-title">{req.eventTitle}</span>
						</div>
						<div class="approval-info">
							<span><i class="fi fi-rr-user"></i> {req.requestedByName}</span>
							<span><i class="fi fi-rr-calendar"></i> {formatDate(req.requestedAtUtc)}</span>
						</div>

						{#if data.canUpdate}
							<div class="approval-actions">
								<form method="POST" action="?/approve" use:enhance={() => {
									processingId = req.id;
									return async ({ update }) => {
										processingId = null;
										await update();
									};
								}}>
									<input type="hidden" name="approvalId" value={req.id} />
									<button type="submit" class="btn-approve" disabled={processingId === req.id}>
										{#if processingId === req.id}
											<i class="fi fi-rr-spinner"></i> Processing…
										{:else}
											<i class="fi fi-rr-check"></i> Approve
										{/if}
									</button>
								</form>
								<button type="button" class="btn-reject" onclick={() => openReject(req.id)}>
									<i class="fi fi-rr-cross"></i> Reject
								</button>
							</div>
						{/if}
					</div>
				{/each}
			</div>
		</section>
	{:else}
		<div class="empty-state">
			<i class="fi fi-rr-check-circle"></i>
			<p>No pending approvals — all caught up!</p>
		</div>
	{/if}

	<!-- Reviewed history -->
	{#if reviewed.length > 0}
		<section class="card-section">
			<h2 class="section-title"><i class="fi fi-rr-history"></i> Review History</h2>
			<table class="history-table">
				<thead>
					<tr>
						<th>Event</th>
						<th>Action</th>
						<th>Requested By</th>
						<th>Requested At</th>
						<th>Status</th>
						<th>Reviewed By</th>
						<th>Rejection Reason</th>
					</tr>
				</thead>
				<tbody>
					{#each reviewed as req (req.id)}
						<tr>
							<td class="event-name">{req.eventTitle}</td>
							<td><span class="action-badge {req.action}">{req.action}</span></td>
							<td>{req.requestedByName}</td>
							<td>{formatDate(req.requestedAtUtc)}</td>
							<td>
								<span class="status-chip {req.status}">
									{req.status}
								</span>
							</td>
							<td>{req.reviewedByName ?? "—"}</td>
							<td class="rejection">{req.rejectionReason ?? "—"}</td>
						</tr>
					{/each}
				</tbody>
			</table>
		</section>
	{/if}
</div>

<!-- Reject modal -->
{#if rejectingId !== null}
	<div class="modal-overlay" onclick={closeReject} role="dialog" aria-modal="true">
		<div class="modal-card" onclick={(e) => e.stopPropagation()}>
			<div class="modal-header">
				<h3><i class="fi fi-rr-cross-circle"></i> Reject Event</h3>
				<button class="modal-close" onclick={closeReject}><i class="fi fi-rr-cross"></i></button>
			</div>
			<p class="modal-sub">Provide a reason so the submitter knows what to fix.</p>
			<form method="POST" action="?/reject" use:enhance={() => {
				return async ({ update }) => {
					closeReject();
					await update();
				};
			}}>
				<input type="hidden" name="approvalId" value={rejectingId} />
				<div class="field">
					<label for="reason">Rejection Reason <span class="req">*</span></label>
					<textarea
						id="reason"
						name="reason"
						rows="4"
						bind:value={rejectReason}
						placeholder="Explain why this event is being rejected..."
						required
					></textarea>
				</div>
				<div class="modal-footer">
					<button type="button" class="btn-cancel" onclick={closeReject}>Cancel</button>
					<button type="submit" class="btn-reject-submit" disabled={!rejectReason.trim()}>
						<i class="fi fi-rr-cross-circle"></i> Confirm Rejection
					</button>
				</div>
			</form>
		</div>
	</div>
{/if}

<style>
	.page-shell { padding: 32px; max-width: 1200px; margin: 0 auto; }

	.page-header {
		display: flex;
		align-items: flex-start;
		justify-content: space-between;
		margin-bottom: 28px;
		gap: 16px;
		flex-wrap: wrap;
	}
	.page-header h1 { margin: 0 0 4px; font-size: 1.6rem; color: #10243e; }
	.page-header p { margin: 0; color: #5a6d83; font-size: 0.92rem; }

	.badge-summary { display: flex; gap: 10px; align-items: center; }
	.badge {
		padding: 6px 14px;
		border-radius: 20px;
		font-size: 0.82rem;
		font-weight: 700;
	}
	.badge.pending { background: #fef3c7; color: #92400e; }
	.badge.reviewed { background: #d1fae5; color: #065f46; }

	.toast {
		display: flex; align-items: center; gap: 10px;
		padding: 14px 18px; border-radius: 12px;
		margin-bottom: 20px; font-size: 0.92rem; font-weight: 600;
	}
	.toast-success { background: #d1fae5; color: #065f46; }
	.toast-error { background: #fee2e2; color: #991b1b; }

	.section-title {
		display: flex; align-items: center; gap: 8px;
		font-size: 1rem; font-weight: 700; color: #10243e;
		margin: 0 0 14px;
	}

	.card-section { margin-bottom: 36px; }

	.approval-list { display: flex; flex-direction: column; gap: 12px; }

	.approval-card {
		background: white;
		border: 1px solid #d8e2eb;
		border-radius: 14px;
		padding: 18px 20px;
		display: flex;
		align-items: center;
		gap: 16px;
		flex-wrap: wrap;
	}
	.pending-card { border-left: 4px solid #f59e0b; }

	.approval-meta { display: flex; align-items: center; gap: 10px; flex: 1; min-width: 0; }
	.event-title { font-weight: 700; color: #10243e; font-size: 0.98rem; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }

	.approval-info {
		display: flex; gap: 18px; color: #5a6d83;
		font-size: 0.85rem; align-items: center; flex-shrink: 0;
	}
	.approval-info span { display: flex; align-items: center; gap: 5px; }

	.approval-actions { display: flex; gap: 10px; align-items: center; }

	.action-badge {
		padding: 3px 10px; border-radius: 6px;
		font-size: 0.74rem; font-weight: 700; text-transform: uppercase;
		letter-spacing: 0.04em; white-space: nowrap;
	}
	.action-badge.create { background: #dbeafe; color: #1d4ed8; }
	.action-badge.update { background: #ede9fe; color: #6d28d9; }

	.btn-approve {
		display: flex; align-items: center; gap: 6px;
		padding: 8px 18px; border-radius: 10px;
		background: linear-gradient(135deg, #059669, #047857);
		color: white; border: none; font-weight: 700;
		font-size: 0.88rem; cursor: pointer; transition: opacity 0.15s;
	}
	.btn-approve:hover { opacity: 0.88; }
	.btn-approve:disabled { opacity: 0.5; cursor: not-allowed; }

	.btn-reject {
		display: flex; align-items: center; gap: 6px;
		padding: 8px 18px; border-radius: 10px;
		background: white; border: 2px solid #c8102e;
		color: #c8102e; font-weight: 700; font-size: 0.88rem;
		cursor: pointer; transition: all 0.15s;
	}
	.btn-reject:hover { background: #fee2e2; }

	.empty-state {
		display: flex; flex-direction: column; align-items: center;
		justify-content: center; padding: 60px 20px;
		color: #5a6d83; gap: 12px; font-size: 1rem;
	}
	.empty-state i { font-size: 2.5rem; color: #059669; }

	.history-table {
		width: 100%; border-collapse: collapse;
		background: white; border-radius: 14px;
		overflow: hidden; border: 1px solid #d8e2eb;
		font-size: 0.88rem;
	}
	.history-table th {
		background: #f5f8fb; padding: 12px 14px;
		text-align: left; font-weight: 700; color: #5a6d83;
		font-size: 0.78rem; text-transform: uppercase; letter-spacing: 0.04em;
		border-bottom: 1px solid #d8e2eb;
	}
	.history-table td {
		padding: 12px 14px; border-bottom: 1px solid #eef2f7;
		color: #334155; vertical-align: middle;
	}
	.history-table tr:last-child td { border-bottom: none; }
	.event-name { font-weight: 600; color: #10243e; max-width: 200px; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
	.rejection { max-width: 200px; color: #c8102e; font-size: 0.82rem; }

	.status-chip {
		padding: 3px 10px; border-radius: 20px;
		font-size: 0.74rem; font-weight: 700; text-transform: capitalize;
	}
	.status-chip.approved { background: #d1fae5; color: #065f46; }
	.status-chip.rejected { background: #fee2e2; color: #991b1b; }
	.status-chip.pending  { background: #fef3c7; color: #92400e; }

	/* Modal */
	.modal-overlay {
		position: fixed; inset: 0; background: rgba(15,36,62,0.45);
		display: flex; align-items: center; justify-content: center; z-index: 999;
	}
	.modal-card {
		background: white; border-radius: 20px; padding: 28px;
		width: 100%; max-width: 480px; box-shadow: 0 24px 64px rgba(0,0,0,0.2);
	}
	.modal-header { display: flex; align-items: center; justify-content: space-between; margin-bottom: 8px; }
	.modal-header h3 { margin: 0; font-size: 1.1rem; color: #10243e; display: flex; align-items: center; gap: 8px; }
	.modal-close { background: none; border: none; cursor: pointer; font-size: 1rem; color: #5a6d83; padding: 4px; }
	.modal-sub { color: #5a6d83; font-size: 0.88rem; margin: 0 0 18px; }

	.field label { display: block; font-size: 0.85rem; font-weight: 700; color: #334155; margin-bottom: 6px; }
	.field textarea {
		width: 100%; padding: 10px 12px;
		border: 1.5px solid #d8e2eb; border-radius: 10px;
		font-size: 0.9rem; resize: vertical; font-family: inherit;
		transition: border-color 0.15s; box-sizing: border-box;
	}
	.field textarea:focus { outline: none; border-color: #c8102e; }
	.req { color: #c8102e; }

	.modal-footer {
		display: flex; justify-content: flex-end; gap: 10px; margin-top: 20px;
	}
	.btn-cancel {
		padding: 10px 20px; border-radius: 10px; border: 1.5px solid #d8e2eb;
		background: white; color: #5a6d83; font-weight: 600; cursor: pointer;
	}
	.btn-reject-submit {
		display: flex; align-items: center; gap: 6px;
		padding: 10px 20px; border-radius: 10px;
		background: linear-gradient(135deg, #c8102e, #8f1027);
		color: white; border: none; font-weight: 700; cursor: pointer;
		transition: opacity 0.15s;
	}
	.btn-reject-submit:disabled { opacity: 0.5; cursor: not-allowed; }
	.btn-reject-submit:not(:disabled):hover { opacity: 0.88; }
</style>
