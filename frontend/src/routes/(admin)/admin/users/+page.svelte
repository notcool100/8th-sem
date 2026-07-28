<script lang="ts">
	import type { ActionData, PageData } from "./$types";
	import type { ManagedUser, NavItem, UserPermission } from "$lib/types/auth";

	let { data, form } = $props<{ data: PageData; form?: ActionData }>();

	type PermRow = { canView: boolean; canCreate: boolean; canUpdate: boolean; canDelete: boolean; needsApproval: boolean };

	// ── Create form permission state ──────────────────────────────────────────
	let createPermState = $state<Record<number, PermRow>>(
		Object.fromEntries(data.navItems.map((n: NavItem) => [n.id, { canView: false, canCreate: false, canUpdate: false, canDelete: false, needsApproval: false }]))
	);

	function toggleCreateAll(field: "canView" | "canCreate" | "canUpdate" | "canDelete") {
		const anyUnchecked = data.navItems.some((n: NavItem) => !createPermState[n.id][field]);
		for (const n of data.navItems) {
			createPermState[n.id][field] = anyUnchecked;
			if (field === "canCreate" || field === "canUpdate") {
				if (!createPermState[n.id].canCreate && !createPermState[n.id].canUpdate)
					createPermState[n.id].needsApproval = false;
			}
		}
	}

	function toggleCreateRow(id: number, checked: boolean) {
		createPermState[id] = { canView: checked, canCreate: checked, canUpdate: checked, canDelete: checked, needsApproval: checked ? createPermState[id].needsApproval : false };
	}

	const allCreateView   = $derived(data.navItems.every((n: NavItem) => createPermState[n.id]?.canView));
	const allCreateCreate = $derived(data.navItems.every((n: NavItem) => createPermState[n.id]?.canCreate));
	const allCreateUpdate = $derived(data.navItems.every((n: NavItem) => createPermState[n.id]?.canUpdate));
	const allCreateDelete = $derived(data.navItems.every((n: NavItem) => createPermState[n.id]?.canDelete));

	// ── Edit drawer state ─────────────────────────────────────────────────────
	let editingUser = $state<ManagedUser | null>(null);
	let editIsActive = $state(true);
	let editPermState = $state<Record<number, PermRow>>({});

	function openEdit(user: ManagedUser) {
		editingUser = user;
		editIsActive = user.isActive;
		const base = Object.fromEntries(
			data.navItems.map((n: NavItem) => [n.id, { canView: false, canCreate: false, canUpdate: false, canDelete: false, needsApproval: false }])
		);
		for (const perm of user.permissions) {
			const navItem = data.navItems.find((n: NavItem) => n.key === perm.navItemKey);
			if (navItem) {
				base[navItem.id] = {
					canView: perm.canView,
					canCreate: perm.canCreate,
					canUpdate: perm.canUpdate,
					canDelete: perm.canDelete,
					needsApproval: perm.needsApproval
				};
			}
		}
		editPermState = base;
	}

	function closeEdit() {
		editingUser = null;
	}

	function toggleEditAll(field: "canView" | "canCreate" | "canUpdate" | "canDelete") {
		const anyUnchecked = data.navItems.some((n: NavItem) => !editPermState[n.id][field]);
		for (const n of data.navItems) {
			editPermState[n.id][field] = anyUnchecked;
			if (field === "canCreate" || field === "canUpdate") {
				if (!editPermState[n.id].canCreate && !editPermState[n.id].canUpdate)
					editPermState[n.id].needsApproval = false;
			}
		}
	}

	function toggleEditRow(id: number, checked: boolean) {
		editPermState[id] = { canView: checked, canCreate: checked, canUpdate: checked, canDelete: checked, needsApproval: checked ? editPermState[id].needsApproval : false };
	}

	const allEditView   = $derived(data.navItems.every((n: NavItem) => editPermState[n.id]?.canView));
	const allEditCreate = $derived(data.navItems.every((n: NavItem) => editPermState[n.id]?.canCreate));
	const allEditUpdate = $derived(data.navItems.every((n: NavItem) => editPermState[n.id]?.canUpdate));
	const allEditDelete = $derived(data.navItems.every((n: NavItem) => editPermState[n.id]?.canDelete));

	// ── Delete confirmation state ─────────────────────────────────────────────
	let deletingUserId = $state<number | null>(null);

	function confirmDelete(id: number) { deletingUserId = id; }
	function cancelDelete() { deletingUserId = null; }
</script>

<!-- Edit Drawer Overlay -->
{#if editingUser}
	<div class="overlay" role="presentation" onclick={closeEdit}></div>
	<aside class="edit-drawer">
		<div class="drawer-head">
			<div>
				<h2>Edit User</h2>
				<p>Update profile and module permissions.</p>
			</div>
			<button class="close-btn" onclick={closeEdit} aria-label="Close">✕</button>
		</div>

		{#if form?.message}
			<div class="notice" class:success={form.success}>{form.message}</div>
		{/if}

		<form method="POST" action="?/update" class="drawer-form">
			<input type="hidden" name="userId" value={editingUser.id} />
			<input type="hidden" name="isActive" value={String(editIsActive)} />

			<div class="form-fields">
				<label>
					<span>Full Name</span>
					<input name="fullName" type="text" value={editingUser.fullName} required />
				</label>
				<label>
					<span>Department</span>
					<input name="department" type="text" value={editingUser.department} />
				</label>
			</div>

			<label class="toggle-label">
				<span>Account Status</span>
				<button
					type="button"
					class="status-toggle"
					class:active={editIsActive}
					onclick={() => editIsActive = !editIsActive}
				>
					<span class="toggle-knob"></span>
					<span class="toggle-text">{editIsActive ? "Active" : "Inactive"}</span>
				</button>
			</label>

			{#if editingUser.role !== "superadmin"}
				<div class="perm-section">
					<div class="perm-header">
						<h3>Module Permissions</h3>
					</div>
					<div class="perm-table-wrap">
						<table class="perm-table">
							<thead>
								<tr>
									<th class="col-module">Module</th>
									<th class="col-crud">
										<button type="button" class="col-toggle" onclick={() => toggleEditAll("canView")}>
											View {allEditView ? "✓" : ""}
										</button>
									</th>
									<th class="col-crud">
										<button type="button" class="col-toggle" onclick={() => toggleEditAll("canCreate")}>
											Create {allEditCreate ? "✓" : ""}
										</button>
									</th>
									<th class="col-crud">
										<button type="button" class="col-toggle" onclick={() => toggleEditAll("canUpdate")}>
											Update {allEditUpdate ? "✓" : ""}
										</button>
									</th>
									<th class="col-crud">
										<button type="button" class="col-toggle" onclick={() => toggleEditAll("canDelete")}>
											Delete {allEditDelete ? "✓" : ""}
										</button>
									</th>
									<th class="col-approval">Needs Approval</th>
									<th class="col-all">All</th>
								</tr>
							</thead>
							<tbody>
								{#each data.navItems as item (item.id)}
									<tr>
										<td class="module-cell">
											<span class="module-section">{item.section}</span>
											<span class="module-label">{item.label}</span>
										</td>
										<td class="check-cell">
											<input type="checkbox" name="perm_{item.id}_canView" bind:checked={editPermState[item.id].canView} />
										</td>
										<td class="check-cell">
											<input type="checkbox" name="perm_{item.id}_canCreate" bind:checked={editPermState[item.id].canCreate}
												onchange={() => { if (!editPermState[item.id].canCreate && !editPermState[item.id].canUpdate) editPermState[item.id].needsApproval = false; }} />
										</td>
										<td class="check-cell">
											<input type="checkbox" name="perm_{item.id}_canUpdate" bind:checked={editPermState[item.id].canUpdate}
												onchange={() => { if (!editPermState[item.id].canCreate && !editPermState[item.id].canUpdate) editPermState[item.id].needsApproval = false; }} />
										</td>
										<td class="check-cell">
											<input type="checkbox" name="perm_{item.id}_canDelete" bind:checked={editPermState[item.id].canDelete} />
										</td>
										<td class="check-cell">
											<input
												type="checkbox"
												name="perm_{item.id}_needsApproval"
												bind:checked={editPermState[item.id].needsApproval}
												disabled={!editPermState[item.id].canCreate && !editPermState[item.id].canUpdate}
												class="approval-check"
												title={(!editPermState[item.id].canCreate && !editPermState[item.id].canUpdate) ? "Enable Create or Update first" : "Require approval for create/update actions"}
											/>
										</td>
										<td class="check-cell">
											<input
												type="checkbox"
												checked={editPermState[item.id].canView && editPermState[item.id].canCreate && editPermState[item.id].canUpdate && editPermState[item.id].canDelete}
												onchange={(e) => toggleEditRow(item.id, (e.target as HTMLInputElement).checked)}
											/>
										</td>
									</tr>
								{/each}
							</tbody>
						</table>
					</div>
				</div>
			{/if}

			<div class="drawer-actions">
				<button type="button" class="cancel-btn" onclick={closeEdit}>Cancel</button>
				<button type="submit" class="save-btn">Save Changes</button>
			</div>
		</form>
	</aside>
{/if}

<section class="users-shell">
	<div class="page-intro">
		<div>
			<p class="eyebrow">Identity & Access</p>
			<h1>User Management</h1>
			<p class="lede">Only the seeded superadmin can provision additional accounts. Public signup is disabled.</p>
		</div>
	</div>

	<div class="users-layout">
		<!-- Create User Panel -->
		<section class="panel create-panel">
			<div class="panel-head">
				<h2>Create User</h2>
				<p>Create admin accounts with granular module permissions.</p>
			</div>

			{#if form?.message && !editingUser}
				<div class="notice" class:success={form.success}>{form.message}</div>
			{/if}

			<form method="POST" action="?/create" class="user-form">
				<div class="form-fields">
					<label>
						<span>Full Name</span>
						<input name="fullName" type="text" value={form?.values?.fullName ?? ""} placeholder="Aayush Sharma" required />
					</label>
					<label>
						<span>Email Address</span>
						<input name="email" type="email" value={form?.values?.email ?? ""} placeholder="aayush@ntb.gov.np" required />
					</label>
					<label>
						<span>Department</span>
						<input name="department" type="text" value={form?.values?.department ?? ""} placeholder="Events" />
					</label>
					<label>
						<span>Temporary Password</span>
						<input name="password" type="password" value={form?.values?.password ?? ""} placeholder="StrongPass123" required />
					</label>
				</div>

				<div class="perm-section">
					<div class="perm-header">
						<h3>Module Permissions</h3>
						<p>Define what this user can do in each module.</p>
					</div>
					<div class="perm-table-wrap">
						<table class="perm-table">
							<thead>
								<tr>
									<th class="col-module">Module</th>
									<th class="col-crud">
										<button type="button" class="col-toggle" onclick={() => toggleCreateAll("canView")}>
											View {allCreateView ? "✓" : ""}
										</button>
									</th>
									<th class="col-crud">
										<button type="button" class="col-toggle" onclick={() => toggleCreateAll("canCreate")}>
											Create {allCreateCreate ? "✓" : ""}
										</button>
									</th>
									<th class="col-crud">
										<button type="button" class="col-toggle" onclick={() => toggleCreateAll("canUpdate")}>
											Update {allCreateUpdate ? "✓" : ""}
										</button>
									</th>
									<th class="col-crud">
										<button type="button" class="col-toggle" onclick={() => toggleCreateAll("canDelete")}>
											Delete {allCreateDelete ? "✓" : ""}
										</button>
									</th>
									<th class="col-approval">Needs Approval</th>
									<th class="col-all">All</th>
								</tr>
							</thead>
							<tbody>
								{#each data.navItems as item (item.id)}
									<tr>
										<td class="module-cell">
											<span class="module-section">{item.section}</span>
											<span class="module-label">{item.label}</span>
										</td>
										<td class="check-cell">
											<input type="checkbox" name="perm_{item.id}_canView" bind:checked={createPermState[item.id].canView} />
										</td>
										<td class="check-cell">
											<input type="checkbox" name="perm_{item.id}_canCreate" bind:checked={createPermState[item.id].canCreate}
												onchange={() => { if (!createPermState[item.id].canCreate && !createPermState[item.id].canUpdate) createPermState[item.id].needsApproval = false; }} />
										</td>
										<td class="check-cell">
											<input type="checkbox" name="perm_{item.id}_canUpdate" bind:checked={createPermState[item.id].canUpdate}
												onchange={() => { if (!createPermState[item.id].canCreate && !createPermState[item.id].canUpdate) createPermState[item.id].needsApproval = false; }} />
										</td>
										<td class="check-cell">
											<input type="checkbox" name="perm_{item.id}_canDelete" bind:checked={createPermState[item.id].canDelete} />
										</td>
										<td class="check-cell">
											<input
												type="checkbox"
												name="perm_{item.id}_needsApproval"
												bind:checked={createPermState[item.id].needsApproval}
												disabled={!createPermState[item.id].canCreate && !createPermState[item.id].canUpdate}
												class="approval-check"
												title={(!createPermState[item.id].canCreate && !createPermState[item.id].canUpdate) ? "Enable Create or Update first" : "Require approval for create/update actions"}
											/>
										</td>
										<td class="check-cell">
											<input
												type="checkbox"
												checked={createPermState[item.id].canView && createPermState[item.id].canCreate && createPermState[item.id].canUpdate && createPermState[item.id].canDelete}
												onchange={(e) => toggleCreateRow(item.id, (e.target as HTMLInputElement).checked)}
											/>
										</td>
									</tr>
								{/each}
							</tbody>
						</table>
					</div>
				</div>

				<button class="submit-btn" type="submit">Create User</button>
			</form>
		</section>

		<!-- Provisioned Users Table -->
		<section class="panel roster-panel">
			<div class="panel-head">
				<h2>Provisioned Users</h2>
				<p>{data.users.length} accounts currently exist in the system.</p>
			</div>

			<div class="table-wrap">
				<table>
					<thead>
						<tr>
							<th>Name</th>
							<th>Department</th>
							<th>Last Login</th>
							<th>Status</th>
							<th>Permissions</th>
							<th>Actions</th>
						</tr>
					</thead>
					<tbody>
						{#each data.users as user (user.id)}
							<tr class:deleting={deletingUserId === user.id}>
								<td>
									<div class="identity">
										<strong>{user.fullName}</strong>
										<span>{user.email}</span>
										<span class="role-badge">{user.role === "superadmin" ? "Super Admin" : "Admin"}</span>
									</div>
								</td>
								<td>{user.department || "Unassigned"}</td>
								<td>{user.lastLoginAtUtc ? new Date(user.lastLoginAtUtc).toLocaleString() : "Never"}</td>
								<td>
									<span class="status" class:active={user.isActive}>
										{user.isActive ? "Active" : "Inactive"}
									</span>
								</td>
								<td>
									{#if user.role === "superadmin"}
										<span class="full-access">Full Access</span>
									{:else}
										<span class="perm-count">{user.permissions.filter((p: UserPermission) => p.canView).length} modules</span>
									{/if}
								</td>
								<td>
									{#if deletingUserId === user.id}
										<div class="confirm-delete">
											<span>Delete?</span>
											<form method="POST" action="?/delete" style="display:contents">
												<input type="hidden" name="userId" value={user.id} />
												<button type="submit" class="action-btn danger">Yes</button>
											</form>
											<button type="button" class="action-btn neutral" onclick={cancelDelete}>No</button>
										</div>
									{:else}
										<div class="row-actions">
											<button
												type="button"
												class="action-btn edit"
												onclick={() => openEdit(user)}
												title="Edit user"
											>
												Edit
											</button>
											{#if user.role !== "superadmin"}
												<button
													type="button"
													class="action-btn danger"
													onclick={() => confirmDelete(user.id)}
													title="Delete user"
												>
													Delete
												</button>
											{/if}
										</div>
									{/if}
								</td>
							</tr>
						{/each}
					</tbody>
				</table>
			</div>
		</section>
	</div>
</section>

<style>
	.users-shell {
		display: flex;
		flex-direction: column;
		gap: 24px;
	}

	.page-intro {
		display: flex;
		justify-content: space-between;
		align-items: flex-start;
		gap: 16px;
	}

	.eyebrow {
		margin: 0 0 8px;
		font-size: 12px;
		letter-spacing: 0.12em;
		text-transform: uppercase;
		font-weight: 700;
		color: #c8102e;
	}

	h1 {
		margin: 0;
		font-size: 2rem;
		color: #10243e;
	}

	.lede {
		margin: 10px 0 0;
		max-width: 640px;
		color: #5a6d83;
	}

	.users-layout {
		display: flex;
		flex-direction: column;
		gap: 24px;
	}

	.panel {
		background: white;
		border: 1px solid #d8e2eb;
		border-radius: 22px;
		box-shadow: 0 16px 40px rgba(15, 23, 42, 0.06);
		padding: 24px;
	}

	.panel-head h2 {
		margin: 0;
		font-size: 1.2rem;
		color: #10243e;
	}

	.panel-head p {
		margin: 8px 0 0;
		color: #6a7f93;
		font-size: 0.95rem;
	}

	.notice {
		margin-top: 18px;
		padding: 12px 14px;
		border-radius: 14px;
		border: 1px solid rgba(200, 16, 46, 0.18);
		background: #fff2f3;
		color: #9f1733;
		font-size: 0.92rem;
	}

	.notice.success {
		border-color: rgba(22, 163, 74, 0.18);
		background: #effcf3;
		color: #166534;
	}

	/* ── Create form ── */
	.user-form {
		margin-top: 20px;
		display: flex;
		flex-direction: column;
		gap: 20px;
	}

	.form-fields {
		display: grid;
		grid-template-columns: 1fr 1fr;
		gap: 16px;
	}

	label {
		display: grid;
		gap: 8px;
	}

	label span {
		font-size: 0.83rem;
		font-weight: 700;
		color: #24384f;
	}

	input[type="text"],
	input[type="email"],
	input[type="password"] {
		width: 100%;
		padding: 12px 14px;
		border-radius: 12px;
		border: 1px solid #c9d6e2;
		background: #f8fafc;
		color: #10243e;
		font: inherit;
		box-sizing: border-box;
	}

	input:focus {
		outline: none;
		border-color: #c8102e;
		box-shadow: 0 0 0 3px rgba(200, 16, 46, 0.08);
		background: white;
	}

	/* ── Permission matrix ── */
	.perm-section {
		border: 1px solid #e2eaf2;
		border-radius: 16px;
		overflow: hidden;
	}

	.perm-header {
		padding: 14px 18px;
		background: #f8fafc;
		border-bottom: 1px solid #e2eaf2;
	}

	.perm-header h3 {
		margin: 0;
		font-size: 0.92rem;
		color: #10243e;
		font-weight: 700;
	}

	.perm-header p {
		margin: 4px 0 0;
		font-size: 0.82rem;
		color: #6a7f93;
	}

	.perm-table-wrap { overflow-x: auto; }

	.perm-table {
		width: 100%;
		border-collapse: collapse;
	}

	.perm-table th {
		padding: 10px 12px;
		text-align: center;
		font-size: 0.73rem;
		font-weight: 700;
		letter-spacing: 0.06em;
		text-transform: uppercase;
		color: #6a7f93;
		border-bottom: 1px solid #edf2f7;
		background: #fafbfc;
	}

	.perm-table th.col-module { text-align: left; min-width: 170px; }
	.perm-table th.col-approval { font-size: 0.68rem; color: #c8102e; font-weight: 700; text-transform: uppercase; }

	.approval-check:not(:disabled) { accent-color: #c8102e; cursor: pointer; }
	.approval-check:disabled { opacity: 0.3; cursor: not-allowed; }

	.col-toggle {
		background: none;
		border: none;
		cursor: pointer;
		font-size: 0.73rem;
		font-weight: 700;
		color: #6a7f93;
		letter-spacing: 0.06em;
		text-transform: uppercase;
		padding: 2px 4px;
		border-radius: 4px;
		transition: color 0.15s;
	}

	.col-toggle:hover { color: #c8102e; }

	.perm-table td {
		padding: 9px 12px;
		border-bottom: 1px solid #f0f4f8;
		vertical-align: middle;
	}

	.perm-table tr:last-child td { border-bottom: none; }
	.perm-table tr:hover td { background: #fafbfd; }

	.module-cell { display: flex; flex-direction: column; gap: 2px; }

	.module-section {
		font-size: 0.68rem;
		color: #94a3b8;
		font-weight: 600;
		letter-spacing: 0.08em;
		text-transform: uppercase;
	}

	.module-label { font-size: 0.86rem; color: #10243e; font-weight: 500; }

	.check-cell { text-align: center; }

	.check-cell input[type="checkbox"] {
		width: 16px;
		height: 16px;
		cursor: pointer;
		accent-color: #c8102e;
	}

	.submit-btn {
		padding: 13px 16px;
		border-radius: 14px;
		border: none;
		background: linear-gradient(135deg, #c8102e, #8f1027);
		color: white;
		font-weight: 700;
		font-size: 0.95rem;
		cursor: pointer;
		transition: opacity 0.15s;
	}

	.submit-btn:hover { opacity: 0.9; }

	/* ── Roster table ── */
	.table-wrap { margin-top: 20px; overflow-x: auto; }

	table { width: 100%; border-collapse: collapse; min-width: 640px; }

	th, td {
		padding: 13px 12px;
		text-align: left;
		border-bottom: 1px solid #edf2f7;
		vertical-align: middle;
	}

	th {
		font-size: 0.78rem;
		letter-spacing: 0.08em;
		text-transform: uppercase;
		color: #6a7f93;
	}

	td { font-size: 0.92rem; color: #24384f; }

	tr.deleting td { background: #fff8f8; }

	.identity { display: grid; gap: 3px; }
	.identity strong { color: #10243e; }
	.identity span { color: #6a7f93; font-size: 0.84rem; }

	.role-badge {
		display: inline-flex;
		align-items: center;
		padding: 2px 8px;
		border-radius: 999px;
		background: #eff6ff;
		color: #1d4ed8;
		font-size: 0.7rem;
		font-weight: 700;
		width: fit-content;
	}

	.status {
		display: inline-flex;
		align-items: center;
		padding: 5px 10px;
		border-radius: 999px;
		background: #fef2f2;
		color: #b91c1c;
		font-size: 0.77rem;
		font-weight: 700;
	}

	.status.active { background: #ecfdf3; color: #15803d; }

	.full-access {
		display: inline-flex;
		align-items: center;
		padding: 4px 10px;
		border-radius: 999px;
		background: #fef9c3;
		color: #854d0e;
		font-size: 0.77rem;
		font-weight: 700;
	}

	.perm-count { font-size: 0.85rem; color: #475569; font-weight: 600; }

	.row-actions { display: flex; gap: 8px; }

	.action-btn {
		padding: 6px 14px;
		border-radius: 8px;
		border: none;
		font-size: 0.8rem;
		font-weight: 600;
		cursor: pointer;
		transition: all 0.15s;
	}

	.action-btn.edit {
		background: #eff6ff;
		color: #1d4ed8;
	}

	.action-btn.edit:hover { background: #dbeafe; }

	.action-btn.danger {
		background: #fef2f2;
		color: #b91c1c;
	}

	.action-btn.danger:hover { background: #fee2e2; }

	.action-btn.neutral {
		background: #f1f5f9;
		color: #475569;
	}

	.action-btn.neutral:hover { background: #e2e8f0; }

	.confirm-delete {
		display: flex;
		align-items: center;
		gap: 8px;
		font-size: 0.82rem;
		color: #b91c1c;
		font-weight: 600;
	}

	/* ── Edit drawer ── */
	.overlay {
		position: fixed;
		inset: 0;
		background: rgba(0, 0, 0, 0.35);
		backdrop-filter: blur(2px);
		z-index: 1100;
	}

	.edit-drawer {
		position: fixed;
		top: 0;
		right: 0;
		bottom: 0;
		width: min(600px, 96vw);
		background: white;
		z-index: 1101;
		display: flex;
		flex-direction: column;
		overflow-y: auto;
		box-shadow: -8px 0 40px rgba(0, 0, 0, 0.18);
		animation: slideIn 0.22s cubic-bezier(0.4, 0, 0.2, 1);
	}

	@keyframes slideIn {
		from { transform: translateX(100%); opacity: 0; }
		to   { transform: translateX(0);    opacity: 1; }
	}

	.drawer-head {
		display: flex;
		justify-content: space-between;
		align-items: flex-start;
		padding: 24px 24px 16px;
		border-bottom: 1px solid #e8eef4;
		flex-shrink: 0;
	}

	.drawer-head h2 { margin: 0; font-size: 1.15rem; color: #10243e; }
	.drawer-head p  { margin: 4px 0 0; font-size: 0.88rem; color: #6a7f93; }

	.close-btn {
		width: 34px;
		height: 34px;
		border-radius: 8px;
		border: 1px solid #dde6ef;
		background: #f8fafc;
		color: #64748b;
		font-size: 14px;
		cursor: pointer;
		display: flex;
		align-items: center;
		justify-content: center;
		flex-shrink: 0;
		transition: all 0.15s;
	}

	.close-btn:hover { background: #fef2f2; color: #c8102e; border-color: #fca5a5; }

	.drawer-form {
		padding: 20px 24px;
		display: flex;
		flex-direction: column;
		gap: 20px;
		flex: 1;
	}

	/* Status toggle */
	.toggle-label { display: flex; flex-direction: column; gap: 8px; }

	.status-toggle {
		display: flex;
		align-items: center;
		gap: 10px;
		padding: 10px 16px;
		border-radius: 12px;
		border: 1px solid #c9d6e2;
		background: #f8fafc;
		cursor: pointer;
		transition: all 0.2s;
		font: inherit;
		width: fit-content;
	}

	.status-toggle.active {
		border-color: rgba(22, 163, 74, 0.4);
		background: #f0fdf4;
	}

	.toggle-knob {
		width: 36px;
		height: 20px;
		border-radius: 999px;
		background: #cbd5e1;
		position: relative;
		transition: background 0.2s;
		flex-shrink: 0;
	}

	.toggle-knob::after {
		content: "";
		position: absolute;
		width: 16px;
		height: 16px;
		border-radius: 50%;
		background: white;
		top: 2px;
		left: 2px;
		transition: transform 0.2s;
		box-shadow: 0 1px 3px rgba(0,0,0,0.2);
	}

	.status-toggle.active .toggle-knob { background: #16a34a; }
	.status-toggle.active .toggle-knob::after { transform: translateX(16px); }

	.toggle-text { font-size: 0.88rem; font-weight: 600; color: #475569; }
	.status-toggle.active .toggle-text { color: #15803d; }

	.drawer-actions {
		display: flex;
		gap: 12px;
		justify-content: flex-end;
		padding-top: 8px;
		border-top: 1px solid #edf2f7;
		margin-top: auto;
		flex-shrink: 0;
	}

	.cancel-btn {
		padding: 11px 20px;
		border-radius: 12px;
		border: 1px solid #c9d6e2;
		background: white;
		color: #475569;
		font: inherit;
		font-weight: 600;
		font-size: 0.92rem;
		cursor: pointer;
		transition: all 0.15s;
	}

	.cancel-btn:hover { background: #f1f5f9; }

	.save-btn {
		padding: 11px 24px;
		border-radius: 12px;
		border: none;
		background: linear-gradient(135deg, #c8102e, #8f1027);
		color: white;
		font: inherit;
		font-weight: 700;
		font-size: 0.92rem;
		cursor: pointer;
		transition: opacity 0.15s;
	}

	.save-btn:hover { opacity: 0.9; }

	@media (max-width: 640px) {
		.panel { padding: 18px; border-radius: 18px; }
		h1 { font-size: 1.6rem; }
		.form-fields { grid-template-columns: 1fr; }
		.edit-drawer { width: 100vw; }
	}
</style>
