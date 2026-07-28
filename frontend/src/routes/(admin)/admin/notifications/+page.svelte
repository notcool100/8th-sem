<script lang="ts">
	import { notifStore } from "$lib/notifications.svelte";
	import { goto } from "$app/navigation";

	let filterTab = $state<"all" | "unread">("all");

	const displayed = $derived(
		filterTab === "unread" ? notifStore.items.filter((n) => !n.isRead) : notifStore.items
	);

	function getTypeIcon(type: string) {
		if (type === "approval_request") return "fi-rr-time-check";
		if (type === "approved") return "fi-rr-check-circle";
		if (type === "rejected") return "fi-rr-cross-circle";
		return "fi-rr-bell";
	}

	function getTypeLabel(type: string) {
		if (type === "approval_request") return "Approval Request";
		if (type === "approved") return "Approved";
		if (type === "rejected") return "Rejected";
		return "Notification";
	}

	function formatDate(iso: string) {
		return new Date(iso).toLocaleDateString("en-US", {
			month: "short",
			day: "numeric",
			year: "numeric",
			hour: "2-digit",
			minute: "2-digit"
		});
	}

	function formatRelative(iso: string) {
		const diff = Date.now() - new Date(iso).getTime();
		const mins = Math.floor(diff / 60000);
		if (mins < 1) return "just now";
		if (mins < 60) return `${mins}m ago`;
		const hrs = Math.floor(mins / 60);
		if (hrs < 24) return `${hrs}h ago`;
		return `${Math.floor(hrs / 24)}d ago`;
	}

	async function handleClick(id: number, link: string | null) {
		await notifStore.markRead(id);
		if (link) goto(link);
	}

	async function handleDelete(e: MouseEvent, id: number) {
		e.stopPropagation();
		await notifStore.remove(id);
	}
</script>

<div class="page-shell">
	<div class="page-header">
		<div>
			<h1>Notifications</h1>
			<p>Stay updated on approvals, events, and system activity.</p>
		</div>
		<div class="header-actions">
			{#if notifStore.items.some((n) => !n.isRead)}
				<button class="btn-mark-all" onclick={() => notifStore.markAllRead()}>
					<i class="fi fi-rr-check-double"></i> Mark all read
				</button>
			{/if}
		</div>
	</div>

	<div class="tabs">
		<button
			class="tab"
			class:active={filterTab === "all"}
			onclick={() => (filterTab = "all")}
		>
			All
			<span class="tab-count">{notifStore.items.length}</span>
		</button>
		<button
			class="tab"
			class:active={filterTab === "unread"}
			onclick={() => (filterTab = "unread")}
		>
			Unread
			{#if notifStore.unreadCount > 0}
				<span class="tab-count unread">{notifStore.unreadCount}</span>
			{/if}
		</button>
	</div>

	{#if displayed.length === 0}
		<div class="empty-state">
			<i class="fi fi-rr-bell-slash"></i>
			<p>{filterTab === "unread" ? "No unread notifications" : "No notifications yet"}</p>
			{#if filterTab === "unread"}
				<button class="btn-switch" onclick={() => (filterTab = "all")}>View all</button>
			{/if}
		</div>
	{:else}
		<div class="notif-list">
			{#each displayed as notif (notif.id)}
				<div
					role="button"
					tabindex="0"
					class="notif-card"
					class:unread={!notif.isRead}
					onclick={() => handleClick(notif.id, notif.link)}
					onkeydown={(e) => e.key === 'Enter' || e.key === ' ' ? handleClick(notif.id, notif.link) : null}
				>
					<div class="notif-icon {notif.type}">
						<i class="fi {getTypeIcon(notif.type)}"></i>
					</div>

					<div class="notif-body">
						<div class="notif-top">
							<span class="notif-title">{notif.title}</span>
							<span class="notif-badge {notif.type}">{getTypeLabel(notif.type)}</span>
						</div>
						<p class="notif-message">{notif.message}</p>
						<div class="notif-meta">
							<span class="notif-time" title={formatDate(notif.createdAtUtc)}>
								<i class="fi fi-rr-clock"></i>
								{formatRelative(notif.createdAtUtc)}
							</span>
							{#if notif.link}
								<span class="notif-link-hint">
									<i class="fi fi-rr-arrow-right"></i> View
								</span>
							{/if}
						</div>
					</div>

					{#if !notif.isRead}
						<span class="unread-dot" title="Unread"></span>
					{/if}

					<button
						type="button"
						class="delete-btn"
						onclick={(e) => handleDelete(e, notif.id)}
						title="Delete notification"
					>
						<i class="fi fi-rr-trash"></i>
					</button>
				</div>
			{/each}
		</div>
	{/if}
</div>

<style>
	.page-shell { padding: 32px; max-width: 800px; margin: 0 auto; }

	.page-header {
		display: flex; justify-content: space-between; align-items: flex-start;
		margin-bottom: 24px; gap: 16px; flex-wrap: wrap;
	}
	.page-header h1 { margin: 0 0 4px; font-size: 1.6rem; color: #10243e; }
	.page-header p { margin: 0; color: #5a6d83; font-size: 0.92rem; }

	.header-actions { display: flex; gap: 10px; align-items: center; }

	.btn-mark-all {
		display: flex; align-items: center; gap: 6px;
		padding: 8px 16px; border-radius: 10px;
		border: 1.5px solid #c8102e; color: #c8102e;
		background: white; font-size: 0.85rem; font-weight: 700;
		cursor: pointer; transition: all 0.15s;
	}
	.btn-mark-all:hover { background: #fff0f3; }

	.tabs {
		display: flex; gap: 4px; border-bottom: 2px solid #e8f0f7;
		margin-bottom: 20px;
	}
	.tab {
		display: flex; align-items: center; gap: 6px;
		padding: 10px 16px; border: none; background: none;
		font-size: 0.9rem; font-weight: 600; color: #5a6d83;
		cursor: pointer; border-bottom: 2px solid transparent;
		margin-bottom: -2px; transition: all 0.15s;
	}
	.tab.active { color: #c8102e; border-bottom-color: #c8102e; }
	.tab:hover:not(.active) { color: #10243e; }

	.tab-count {
		display: inline-flex; align-items: center; justify-content: center;
		min-width: 20px; height: 20px; border-radius: 10px;
		background: #e8f0f7; color: #5a6d83; font-size: 0.75rem;
		font-weight: 700; padding: 0 5px;
	}
	.tab-count.unread { background: #c8102e; color: white; }

	.empty-state {
		display: flex; flex-direction: column; align-items: center;
		padding: 60px 20px; color: #5a6d83; gap: 10px;
	}
	.empty-state i { font-size: 2.5rem; color: #c8d6e5; }
	.empty-state p { margin: 0; font-size: 1rem; }
	.btn-switch {
		margin-top: 4px; background: none; border: none; color: #c8102e;
		font-size: 0.9rem; font-weight: 600; cursor: pointer;
	}

	.notif-list { display: flex; flex-direction: column; gap: 8px; }

	.notif-card {
		display: flex; align-items: flex-start; gap: 14px;
		background: white; border: 1px solid #e2eaf2;
		border-radius: 14px; padding: 16px 18px;
		cursor: pointer; transition: all 0.15s; position: relative;
		text-align: left; width: 100%;
	}
	.notif-card:hover { border-color: #c8d8e8; box-shadow: 0 4px 12px rgba(0,0,0,0.06); }
	.notif-card.unread { border-left: 3px solid #c8102e; background: rgba(200,16,46,0.015); }

	.notif-icon {
		width: 40px; height: 40px; border-radius: 12px;
		display: flex; align-items: center; justify-content: center;
		font-size: 1rem; flex-shrink: 0;
	}
	.notif-icon.approval_request { background: rgba(245,158,11,0.12); color: #d97706; }
	.notif-icon.approved { background: rgba(34,197,94,0.12); color: #16a34a; }
	.notif-icon.rejected { background: rgba(239,68,68,0.12); color: #dc2626; }

	.notif-body { flex: 1; min-width: 0; }

	.notif-top {
		display: flex; align-items: center; gap: 8px;
		margin-bottom: 4px; flex-wrap: wrap;
	}
	.notif-title { font-size: 0.9rem; font-weight: 700; color: #10243e; }

	.notif-badge {
		padding: 2px 8px; border-radius: 6px; font-size: 0.72rem;
		font-weight: 700; text-transform: uppercase; letter-spacing: 0.04em;
	}
	.notif-badge.approval_request { background: #fef3c7; color: #92400e; }
	.notif-badge.approved { background: #d1fae5; color: #065f46; }
	.notif-badge.rejected { background: #fee2e2; color: #991b1b; }

	.notif-message { margin: 0 0 8px; font-size: 0.875rem; color: #475569; line-height: 1.5; }

	.notif-meta { display: flex; align-items: center; gap: 14px; }
	.notif-time { display: flex; align-items: center; gap: 4px; font-size: 0.78rem; color: #94a3b8; }
	.notif-link-hint { display: flex; align-items: center; gap: 4px; font-size: 0.78rem; color: #c8102e; font-weight: 600; }

	.unread-dot {
		width: 8px; height: 8px; border-radius: 50%;
		background: #c8102e; flex-shrink: 0; margin-top: 6px;
	}

	.delete-btn {
		position: absolute; top: 12px; right: 12px;
		background: none; border: none; color: #c8d6e5;
		font-size: 0.85rem; cursor: pointer; padding: 4px;
		border-radius: 6px; transition: all 0.15s; opacity: 0;
	}
	.notif-card:hover .delete-btn { opacity: 1; }
	.delete-btn:hover { color: #dc2626; background: #fee2e2; }
</style>
