<script lang="ts">
	import { goto } from "$app/navigation";
	import { deserialize } from "$app/forms";
	import type { PageData } from "./$types";
	import type { EventDto } from "$lib/types/events";

	type Event = EventDto;

	let { data, form } = $props<{ data: PageData; form?: any }>();

	let events = $state<Event[]>(data.events || []);
	let isLoading = $state(false);
	let deleteConfirm = $state<number | null>(null);

	// Filters
	let searchName = $state("");
	let searchDate = $state("");
	let searchLocation = $state("");
	let searchStatus = $state("");

	// Pagination
	let currentPage = $state(1);
	const itemsPerPage = 10;

	// Modal states
	let selectedEvent = $state<Event | null>(null);
	let showViewModal = $state(false);

	// Duplicate modal state
	let showDuplicateModal = $state(false);
	let duplicateSource = $state<Event | null>(null);
	let duplicateStartDate = $state("");
	let duplicateEndDate = $state("");
	let duplicateError = $state("");
	let isDuplicating = $state(false);

	// Filtered Events
	const filteredEvents = $derived(
		events.filter((event) => {
			const matchName = event.title
				.toLowerCase()
				.includes(searchName.toLowerCase());

			const eventDate = getEventDate(event);
			const matchDate =
				searchDate === "" ||
				eventDate.startsWith(searchDate);

			const matchLocation = event.location
				.toLowerCase()
				.includes(searchLocation.toLowerCase());

			const matchStatus =
				searchStatus === "" ||
				event.status.toLowerCase() === searchStatus.toLowerCase();

			return (
				matchName &&
				matchDate &&
				matchLocation &&
				matchStatus
			);
		})
	);

	// Pagination
	const totalPages = $derived(
		Math.ceil(filteredEvents.length / itemsPerPage)
	);

	const paginatedEvents = $derived(
		filteredEvents.slice(
			(currentPage - 1) * itemsPerPage,
			currentPage * itemsPerPage
		)
	);

	// View Event
	function viewEvent(event: Event): void {
		selectedEvent = { ...event };
		showViewModal = true;
	}

	// Edit Event
	function editEvent(id: number): void {
		goto(`/admin/events/create?edit=${id}`);
	}

	// Manage invitations for an event
	function manageInvitations(id: number): void {
		goto(`/admin/events/invitations/${id}`);
	}

	// Manage self-registrations for an event
	function manageRegistrations(id: number): void {
		goto(`/admin/attendees?eventId=${id}`);
	}

	// Manage bulk workshop invites (plain-text, no QR) for an event
	function manageWorkshopInvites(id: number): void {
		goto(`/admin/events/workshop-invites/${id}`);
	}

	function editSelectedEvent(): void {
		if (!selectedEvent) {
			return;
		}

		showViewModal = false;
		editEvent(selectedEvent.id);
	}

	function toDateInputValue(value: string): string {
		if (!value) return "";
		return value.split("T")[0];
	}

	// Duplicate Event
	function duplicateEvent(event: Event): void {
		duplicateSource = event;
		duplicateStartDate = toDateInputValue(event.date_ad);
		duplicateEndDate = toDateInputValue(event.end_date_ad);
		duplicateError = "";
		showDuplicateModal = true;
	}

	function closeDuplicateModal(): void {
		showDuplicateModal = false;
		duplicateSource = null;
		duplicateError = "";
	}

	async function confirmDuplicate(): Promise<void> {
		if (!duplicateSource) return;

		if (!duplicateStartDate) {
			duplicateError = "Start date is required.";
			return;
		}

		if (duplicateEndDate && duplicateEndDate < duplicateStartDate) {
			duplicateError = "End date cannot be before the start date.";
			return;
		}

		isDuplicating = true;
		duplicateError = "";

		const formData = new FormData();
		formData.append("id", duplicateSource.id.toString());
		formData.append("dateAd", duplicateStartDate);
		if (duplicateEndDate) formData.append("endDateAd", duplicateEndDate);

		try {
			const response = await fetch("?/duplicate", {
				method: "POST",
				body: formData
			});

			const result = deserialize(await response.text());

			if (result.type === "success" && result.data?.event) {
				events = [...events, result.data.event as Event];
				showDuplicateModal = false;
				duplicateSource = null;
			} else if (result.type === "failure") {
				duplicateError = (result.data?.message as string) || "Failed to duplicate event.";
			} else {
				duplicateError = "Failed to duplicate event.";
			}
		} catch (error) {
			console.error(error);
			duplicateError = "Failed to duplicate event.";
		} finally {
			isDuplicating = false;
		}
	}

	// Delete Event
	async function deleteEventAction(id: number): Promise<void> {
		const confirmed = confirm(
			"Are you sure you want to delete this event?"
		);

		if (!confirmed) {
			return;
		}

		isLoading = true;
		deleteConfirm = id;

		const formData = new FormData();
		formData.append("id", id.toString());

		try {
			const response = await fetch("?/delete", {
				method: "POST",
				body: formData
			});

			if (response.ok) {
				events = events.filter((event) => event.id !== id);
				deleteConfirm = null;
			} else {
				alert("Failed to delete event");
			}
		} catch (error) {
			console.error(error);
			alert("Failed to delete event");
		} finally {
			isLoading = false;
		}
	}

	// Pagination Buttons
	function nextPage(): void {
		if (currentPage < totalPages) {
			currentPage++;
		}
	}

	function previousPage(): void {
		if (currentPage > 1) {
			currentPage--;
		}
	}

	function getStatusColor(status: string): string {
		const lower = status.toLowerCase();
		if (lower === "published") return "published";
		if (lower === "draft") return "draft";
		if (lower === "archived") return "archived";
		if (lower === "pendingapproval") return "pending";
		return "draft";
	}

	function getStatusLabel(status: string): string {
		if (status.toLowerCase() === "pendingapproval") return "Pending Approval";
		return status.charAt(0).toUpperCase() + status.slice(1);
	}

	function formatDate(dateString: string): string {
		try {
			const date = new Date(dateString);
			return date.toLocaleDateString("en-US", {
				year: "numeric",
				month: "short",
				day: "numeric"
			});
		} catch {
			return dateString;
		}
	}

	function getEventDate(event: Event): string {
		return event.date_ad?.toString() ?? "";
	}

	function handleKeydown(event: KeyboardEvent): void {
		if (event.key === "Escape") {
			showViewModal = false;
		}
	}
</script>

<div class="events-dashboard animate-fade-in" onkeydown={handleKeydown} role="main">
  <!-- Top Header Section -->
  <div class="dashboard-header">
    <div class="header-text">
      <h1>Event Management</h1>
      <p class="subtitle">Create, monitor, and manage tourism events and exhibitions across Nepal.</p>
    </div>

    {#if data.canCreate}
      <button onclick={() => goto(`/admin/events/create`)} class="add-event-btn">
        <i class="fi fi-rr-plus"></i> Add New Event
      </button>
    {/if}
  </div>

  <!-- Filters Panel -->
  <div class="filters-panel glass">
    <div class="filter-group">
      <span class="filter-icon"><i class="fi fi-rr-search"></i></span>
      <input
        type="text"
        placeholder="Search Event Name"
        bind:value={searchName}
      />
    </div>

    <div class="filter-group">
      <span class="filter-icon"><i class="fi fi-rr-calendar"></i></span>
      <input
        type="date"
        bind:value={searchDate}
      />
    </div>

    <div class="filter-group">
      <span class="filter-icon"><i class="fi fi-rr-marker"></i></span>
      <input
        type="text"
        placeholder="Search Location"
        bind:value={searchLocation}
      />
    </div>

    <div class="filter-group">
      <span class="filter-icon"><i class="fi fi-rr-filter"></i></span>
      <select bind:value={searchStatus}>
        <option value="">All Statuses</option>
        <option value="draft">Draft</option>
        <option value="published">Published</option>
        <option value="archived">Archived</option>
        <option value="pendingapproval">Pending Approval</option>
      </select>
    </div>
  </div>

  <!-- Events List Table -->
  <div class="table-container glass">
    {#if paginatedEvents.length > 0}
      <table>
        <thead>
          <tr>
            <th><i class="fi fi-rr-document"></i> Event Name</th>
            <th><i class="fi fi-rr-calendar"></i> Date</th>
            <th><i class="fi fi-rr-marker"></i> Location</th>
            <th><i class="fi fi-rr-info"></i> Status</th>
            <th class="text-right"><i class="fi fi-rr-settings-sliders"></i> Actions</th>
          </tr>
        </thead>

        <tbody>
          {#each paginatedEvents as event (event.id)}
            <tr>
              <td class="font-semibold">{event.title}</td>
              <td class="text-muted">{formatDate(getEventDate(event))}</td>
              <td>
                <span class="location-badge">
                  <i class="fi fi-rr-marker text-teal"></i> {event.location}
                </span>
              </td>

              <td>
                <span class="status-badge {getStatusColor(event.status)}">
                  <span class="dot"></span>
                  {getStatusLabel(event.status)}
                </span>
              </td>

              <td>
                <div class="actions-group">
                  <button
                    class="action-btn view"
                    onclick={() => viewEvent(event)}
                    title="View details"
                    disabled={isLoading}
                  >
                    <i class="fi fi-rr-eye"></i> View
                  </button>

                  {#if data.canUpdate}
                  <button
                    class="action-btn edit"
                    onclick={() => editEvent(event.id)}
                    title="Edit event"
                    disabled={isLoading}
                  >
                    <i class="fi fi-rr-pencil"></i> Edit
                  </button>
                  {/if}

                  {#if data.canCreate}
                  <button
                    class="action-btn duplicate"
                    onclick={() => duplicateEvent(event)}
                    title="Duplicate event"
                    disabled={isLoading}
                  >
                    <i class="fi fi-rr-copy-alt"></i> Duplicate
                  </button>
                  {/if}

                  {#if data.isSuperAdmin || event.createdById === data.currentUserId}
                  <button
                    class="action-btn invite"
                    onclick={() => manageInvitations(event.id)}
                    title="Manage invitations"
                    disabled={isLoading}
                  >
                    <i class="fi fi-rr-envelope"></i> Invite
                  </button>
                  {/if}

                  {#if event.requiresRegistration}
                  <button
                    class="action-btn invite"
                    onclick={() => manageRegistrations(event.id)}
                    title="Manage registrations"
                    disabled={isLoading}
                  >
                    <i class="fi fi-rr-users-alt"></i> Registrations
                  </button>
                  {/if}

                  {#if data.isSuperAdmin || event.createdById === data.currentUserId}
                  <button
                    class="action-btn invite"
                    onclick={() => manageWorkshopInvites(event.id)}
                    title="Bulk workshop invites (no QR)"
                    disabled={isLoading}
                  >
                    <i class="fi fi-rr-megaphone"></i> Workshop Invites
                  </button>
                  {/if}

                  {#if data.canDelete}
                  <button
                    class="action-btn delete"
                    onclick={() => deleteEventAction(event.id)}
                    title="Delete event"
                    disabled={isLoading || deleteConfirm === event.id}
                  >
                    <i class="fi fi-rr-trash"></i> Delete
                  </button>
                  {/if}
                </div>
              </td>
            </tr>
          {/each}
        </tbody>
      </table>
    {:else}
      <div class="empty-state">
        <i class="fi fi-rr-calendar-exclamation empty-icon"></i>
        <h3>No Events Found</h3>
        <p>Try adjusting your filters or search terms.</p>
      </div>
    {/if}
  </div>

  <!-- Pagination -->
  {#if totalPages > 1}
    <div class="pagination-container">
      <button
        onclick={previousPage}
        disabled={currentPage === 1}
        class="pagination-btn"
        aria-label="Previous Page"
      >
        <i class="fi fi-rr-angle-left"></i> Previous
      </button>

      <span class="pagination-info">
        Page <span class="current-number">{currentPage}</span> of {totalPages}
      </span>

      <button
        onclick={nextPage}
        disabled={currentPage === totalPages}
        class="pagination-btn"
        aria-label="Next Page"
      >
        Next <i class="fi fi-rr-angle-right"></i>
      </button>
    </div>
  {/if}

  <!-- View Modal -->
  {#if showViewModal && selectedEvent}
    <div
      class="modal-overlay"
      onclick={() => (showViewModal = false)}
      onkeydown={(e) => e.key === "Escape" && (showViewModal = false)}
      role="presentation"
      tabindex="-1"
    >
      <div
        class="modal-card animate-scale-in"
        onclick={(e) => e.stopPropagation()}
        onkeydown={(e) => e.stopPropagation()}
        role="dialog"
        aria-label="Event details"
        tabindex="0"
      >
        <div class="modal-header">
          <h2>Event Details</h2>
          <button class="modal-close-icon" onclick={() => (showViewModal = false)} aria-label="Close">
            <i class="fi fi-rr-cross"></i>
          </button>
        </div>

        <div class="modal-body">
          <div class="detail-row">
            <span class="detail-label"><i class="fi fi-rr-document"></i> Title</span>
            <div class="detail-value font-semibold">{selectedEvent.title}</div>
          </div>

          <div class="detail-row">
            <span class="detail-label"><i class="fi fi-rr-calendar"></i> Date</span>
            <div class="detail-value">{formatDate(getEventDate(selectedEvent))}</div>
          </div>

          <div class="detail-row">
            <span class="detail-label"><i class="fi fi-rr-marker"></i> Location</span>
            <div class="detail-value">{selectedEvent.location}</div>
          </div>

          <div class="detail-row">
            <span class="detail-label"><i class="fi fi-rr-info"></i> Status</span>
            <div class="detail-value">
              <span class="status-badge {getStatusColor(selectedEvent.status)}">
                <span class="dot"></span>
                {getStatusLabel(selectedEvent.status)}
              </span>
            </div>
          </div>

          <div class="detail-row">
            <span class="detail-label"><i class="fi fi-rr-quote"></i> Summary</span>
            <div class="detail-value">{selectedEvent.summary}</div>
          </div>

          <div class="detail-row">
            <span class="detail-label"><i class="fi fi-rr-user"></i> Organizer</span>
            <div class="detail-value">{selectedEvent.organizer}</div>
          </div>

          {#if selectedEvent.price > 0}
            <div class="detail-row">
              <span class="detail-label"><i class="fi fi-rr-credit"></i> Price</span>
              <div class="detail-value">Rs. {selectedEvent.price}</div>
            </div>
          {/if}
        </div>

        <div class="modal-footer">
          <button
            class="modal-btn close-btn"
            onclick={() => (showViewModal = false)}
          >
            Close
          </button>
          {#if data.canUpdate}
          <button
            class="modal-btn edit-btn"
            onclick={editSelectedEvent}
          >
            <i class="fi fi-rr-pencil"></i> Edit
          </button>
          {/if}
        </div>
      </div>
    </div>
  {/if}

  <!-- Duplicate Modal -->
  {#if showDuplicateModal && duplicateSource}
    <div
      class="modal-overlay"
      onclick={closeDuplicateModal}
      onkeydown={(e) => e.key === "Escape" && closeDuplicateModal()}
      role="presentation"
      tabindex="-1"
    >
      <div
        class="modal-card animate-scale-in"
        onclick={(e) => e.stopPropagation()}
        onkeydown={(e) => e.stopPropagation()}
        role="dialog"
        aria-label="Duplicate event"
        tabindex="0"
      >
        <div class="modal-header">
          <h2>Duplicate Event</h2>
          <button class="modal-close-icon" onclick={closeDuplicateModal} aria-label="Close">
            <i class="fi fi-rr-cross"></i>
          </button>
        </div>

        <div class="modal-body">
          <p class="duplicate-hint">
            Creating a copy of <strong>{duplicateSource.title}</strong>. Choose the start and end date for the new event.
          </p>

          <div class="form-field">
            <label for="duplicate-start-date"><i class="fi fi-rr-calendar"></i> Start Date</label>
            <input id="duplicate-start-date" type="date" bind:value={duplicateStartDate} />
          </div>

          <div class="form-field">
            <label for="duplicate-end-date"><i class="fi fi-rr-calendar"></i> End Date (optional)</label>
            <input id="duplicate-end-date" type="date" bind:value={duplicateEndDate} min={duplicateStartDate} />
          </div>

          {#if duplicateError}
            <p class="duplicate-error">{duplicateError}</p>
          {/if}
        </div>

        <div class="modal-footer">
          <button class="modal-btn cancel-btn" onclick={closeDuplicateModal} disabled={isDuplicating}>
            Cancel
          </button>
          <button class="modal-btn save-btn" onclick={confirmDuplicate} disabled={isDuplicating}>
            <i class="fi fi-rr-copy-alt"></i> {isDuplicating ? "Duplicating..." : "Duplicate"}
          </button>
        </div>
      </div>
    </div>
  {/if}

</div>

<style>
  .events-dashboard {
    max-width: 1200px;
    margin: 0 auto;
    display: flex;
    flex-direction: column;
    gap: 28px;
  }

  .dashboard-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    gap: 16px;
    flex-wrap: wrap;
  }

  .header-text h1 {
    font-size: 32px;
    margin: 0 0 6px 0;
    color: var(--color-dark);
  }

  .header-text .subtitle {
    margin: 0;
    color: var(--color-text-muted);
    font-size: 14px;
  }

  .add-event-btn {
    background: linear-gradient(135deg, var(--color-primary) 0%, #e6b910 100%);
    color: var(--color-dark);
    border: none;
    padding: 12px 22px;
    border-radius: 12px;
    cursor: pointer;
    font-weight: 600;
    font-size: 14px;
    display: inline-flex;
    align-items: center;
    gap: 8px;
    box-shadow: 0 4px 12px rgba(248, 206, 28, 0.3);
    transition: all 0.25s cubic-bezier(0.4, 0, 0.2, 1);
  }

  .add-event-btn:hover {
    transform: translateY(-2px);
    box-shadow: 0 6px 18px rgba(248, 206, 28, 0.4);
    filter: brightness(1.05);
  }

  .add-event-btn:active {
    transform: translateY(0);
  }

  /* Filters Panel */
  .filters-panel {
    display: grid;
    grid-template-columns: repeat(auto-fit, minmax(220px, 1fr));
    gap: 16px;
    padding: 20px;
    border-radius: 16px;
    background: rgba(255, 255, 255, 0.7);
    border: 1px solid rgba(28, 92, 109, 0.1);
  }

  .filter-group {
    position: relative;
    display: flex;
    align-items: center;
  }

  .filter-icon {
    position: absolute;
    left: 14px;
    color: var(--color-text-muted);
    font-size: 14px;
    pointer-events: none;
    opacity: 0.8;
  }

  .filter-group input,
  .filter-group select {
    padding: 12px 14px 12px 42px;
    border: 1px solid rgba(0, 0, 0, 0.1);
    border-radius: 10px;
    width: 100%;
    font-size: 14px;
    background-color: #ffffff;
    color: var(--color-text);
    outline: none;
    transition: all 0.2s ease;
  }

  .filter-group input:focus,
  .filter-group select:focus {
    border-color: var(--color-secondary);
    box-shadow: 0 0 0 3px rgba(28, 92, 109, 0.15);
  }

  /* Table styles */
  .table-container {
    background: #ffffff;
    border-radius: 16px;
    border: 1px solid rgba(28, 92, 109, 0.08);
    overflow: hidden;
    box-shadow: 0 4px 20px rgba(0, 0, 0, 0.02);
  }

  table {
    width: 100%;
    border-collapse: collapse;
    text-align: left;
  }

  th {
    background: rgba(28, 92, 109, 0.04);
    color: var(--color-secondary);
    padding: 18px 24px;
    font-family: 'Quicksand', sans-serif;
    font-weight: 700;
    font-size: 14px;
    border-bottom: 1.5px solid rgba(28, 92, 109, 0.1);
    letter-spacing: 0.3px;
  }

  th i {
    font-size: 12px;
    margin-right: 6px;
  }

  td {
    padding: 18px 24px;
    font-size: 14.5px;
    border-bottom: 1px solid #f1f5f9;
    color: var(--color-text);
  }

  tbody tr {
    transition: background-color 0.2s ease;
  }

  tbody tr:hover {
    background-color: rgba(28, 92, 109, 0.015);
  }

  tbody tr:last-child td {
    border-bottom: none;
  }

  .font-semibold {
    font-weight: 600;
    color: var(--color-dark);
  }

  .text-muted {
    color: var(--color-text-muted);
  }

  .location-badge {
    display: inline-flex;
    align-items: center;
    gap: 6px;
    color: var(--color-text);
    font-size: 13.5px;
  }

  .text-teal {
    color: var(--color-secondary);
  }

  /* Status Badges */
  .status-badge {
    display: inline-flex;
    align-items: center;
    gap: 6px;
    padding: 4px 10px;
    border-radius: 8px;
    font-size: 12.5px;
    font-weight: 600;
    border: 1px solid transparent;
  }

  .status-badge .dot {
    width: 6px;
    height: 6px;
    border-radius: 50%;
  }

  .status-badge.draft {
    background: rgba(107, 114, 128, 0.1);
    color: #374151;
    border-color: rgba(107, 114, 128, 0.25);
  }

  .status-badge.draft .dot {
    background-color: #6b7280;
  }

  .status-badge.published {
    background: rgba(34, 197, 94, 0.1);
    color: #16a34a;
    border-color: rgba(34, 197, 94, 0.25);
  }

  .status-badge.published .dot {
    background-color: #22c55e;
  }

  .status-badge.archived {
    background: rgba(239, 68, 68, 0.1);
    color: #dc2626;
    border-color: rgba(239, 68, 68, 0.25);
  }

  .status-badge.archived .dot {
    background-color: #ef4444;
  }

  .status-badge.pending {
    background: rgba(245, 158, 11, 0.1);
    color: #92400e;
    border-color: rgba(245, 158, 11, 0.3);
  }

  .status-badge.pending .dot {
    background-color: #f59e0b;
  }

  /* Actions Styling */
  .actions-group {
    display: flex;
    justify-content: flex-start;
    gap: 8px;
  }

  .action-btn {
    border: none;
    padding: 8px 12px;
    border-radius: 8px;
    font-size: 13px;
    font-weight: 600;
    cursor: pointer;
    display: inline-flex;
    align-items: center;
    gap: 6px;
    transition: all 0.2s cubic-bezier(0.4, 0, 0.2, 1);
  }

  .action-btn i {
    font-size: 12px;
  }

  .action-btn.view {
    background: #f1f5f9;
    color: #475569;
    border: 1px solid #e2e8f0;
  }

  .action-btn.view:hover {
    background: #e2e8f0;
    color: #1e293b;
    transform: translateY(-1px);
  }

  .action-btn.edit {
    background: rgba(248, 206, 28, 0.12);
    color: #b28600;
    border: 1px solid rgba(248, 206, 28, 0.3);
  }

  .action-btn.edit:hover {
    background: var(--color-primary);
    color: #ffffff;
    border-color: var(--color-primary);
    transform: translateY(-1px);
    box-shadow: 0 4px 10px rgba(248, 206, 28, 0.25);
  }

  .action-btn.duplicate {
    background: rgba(124, 58, 237, 0.08);
    color: #6d28d9;
    border: 1px solid rgba(124, 58, 237, 0.2);
  }

  .action-btn.duplicate:hover {
    background: #7c3aed;
    color: #ffffff;
    border-color: #7c3aed;
    transform: translateY(-1px);
    box-shadow: 0 4px 10px rgba(124, 58, 237, 0.25);
  }

  .action-btn.invite {
    background: rgba(28, 92, 109, 0.08);
    color: var(--color-secondary);
    border: 1px solid rgba(28, 92, 109, 0.2);
  }

  .action-btn.invite:hover {
    background: var(--color-secondary);
    color: #ffffff;
    border-color: var(--color-secondary);
    transform: translateY(-1px);
    box-shadow: 0 4px 10px rgba(28, 92, 109, 0.25);
  }

  .action-btn.delete {
    background: rgba(189, 36, 43, 0.08);
    color: var(--color-accent);
    border: 1px solid rgba(189, 36, 43, 0.2);
  }

  .action-btn.delete:hover {
    background: var(--color-accent);
    color: #ffffff;
    border-color: var(--color-accent);
    transform: translateY(-1px);
    box-shadow: 0 4px 10px rgba(189, 36, 43, 0.25);
  }

  /* Empty State */
  .empty-state {
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    padding: 60px 24px;
    text-align: center;
  }

  .empty-icon {
    font-size: 48px;
    color: var(--color-text-muted);
    opacity: 0.5;
    margin-bottom: 16px;
  }

  .empty-state h3 {
    margin: 0 0 8px 0;
    font-size: 18px;
    color: var(--color-dark);
  }

  .empty-state p {
    margin: 0;
    color: var(--color-text-muted);
    font-size: 14px;
  }

  /* Pagination */
  .pagination-container {
    display: flex;
    justify-content: center;
    align-items: center;
    gap: 20px;
    margin-top: 12px;
  }

  .pagination-btn {
    background: #ffffff;
    color: var(--color-dark);
    border: 1px solid rgba(0, 0, 0, 0.1);
    padding: 10px 16px;
    border-radius: 10px;
    cursor: pointer;
    font-weight: 600;
    font-size: 13.5px;
    display: inline-flex;
    align-items: center;
    gap: 8px;
    transition: all 0.2s ease;
    box-shadow: 0 2px 6px rgba(0, 0, 0, 0.02);
  }

  .pagination-btn:hover:not(:disabled) {
    background: var(--color-secondary);
    color: white;
    border-color: var(--color-secondary);
    box-shadow: 0 4px 10px rgba(28, 92, 109, 0.2);
  }

  .pagination-btn:disabled {
    opacity: 0.5;
    cursor: not-allowed;
  }

  .pagination-info {
    font-size: 14px;
    color: var(--color-text-muted);
  }

  .current-number {
    font-weight: 700;
    color: var(--color-secondary);
  }

  /* Modals Overlay */
  .modal-overlay {
    position: fixed;
    inset: 0;
    background: rgba(15, 23, 42, 0.6);
    backdrop-filter: blur(8px);
    display: flex;
    justify-content: center;
    align-items: center;
    z-index: 1100;
    padding: 20px;
    animation: fadeInModal 0.2s ease-out;
  }

  .modal-card {
    background: white;
    border-radius: 20px;
    width: 100%;
    max-width: 500px;
    box-shadow: 0 25px 50px -12px rgba(0, 0, 0, 0.15);
    border: 1px solid rgba(28, 92, 109, 0.08);
    overflow: hidden;
    display: flex;
    flex-direction: column;
  }

  .modal-header {
    background: linear-gradient(135deg, var(--color-secondary) 0%, #2a7f96 100%);
    color: white;
    padding: 20px 24px;
    display: flex;
    justify-content: space-between;
    align-items: center;
  }

  .modal-header h2 {
    margin: 0;
    font-size: 20px;
    color: white;
  }

  .modal-close-icon {
    color: white;
    background: rgba(255, 255, 255, 0.15);
    border: none;
    width: 30px;
    height: 30px;
    border-radius: 50%;
    cursor: pointer;
    display: flex;
    align-items: center;
    justify-content: center;
    font-size: 11px;
    transition: all 0.2s ease;
  }

  .modal-close-icon:hover {
    background: rgba(255, 255, 255, 0.3);
  }

  .modal-body {
    padding: 24px;
    display: flex;
    flex-direction: column;
    gap: 20px;
  }

  /* View Modal Specifics */
  .detail-row {
    display: flex;
    flex-direction: column;
    gap: 6px;
    padding-bottom: 12px;
    border-bottom: 1px solid #f1f5f9;
  }

  .detail-row:last-child {
    border-bottom: none;
    padding-bottom: 0;
  }

  .detail-label {
    font-size: 12.5px;
    color: var(--color-text-muted);
    font-weight: 600;
    display: inline-flex;
    align-items: center;
    gap: 6px;
  }

  .detail-label i {
    font-size: 13px;
  }

  .detail-value {
    font-size: 15px;
    color: var(--color-dark);
  }

  /* Duplicate Modal Specifics */
  .duplicate-hint {
    margin: 0;
    font-size: 14px;
    color: var(--color-text-muted);
    line-height: 1.5;
  }

  .duplicate-error {
    margin: 0;
    font-size: 13.5px;
    color: var(--color-accent);
    background: rgba(189, 36, 43, 0.08);
    border: 1px solid rgba(189, 36, 43, 0.2);
    border-radius: 8px;
    padding: 10px 12px;
  }

  /* Edit Modal Specifics */
  .form-field {
    display: flex;
    flex-direction: column;
    gap: 8px;
  }

  .form-field label {
    font-size: 13px;
    font-weight: 600;
    color: var(--color-dark);
    display: inline-flex;
    align-items: center;
    gap: 6px;
  }

  .form-field label i {
    color: var(--color-secondary);
  }

  .form-field input,
  .form-field select {
    padding: 12px;
    border: 1px solid rgba(0, 0, 0, 0.1);
    border-radius: 10px;
    font-size: 14px;
    outline: none;
    width: 100%;
    transition: border-color 0.2s;
  }

  .form-field input:focus,
  .form-field select:focus {
    border-color: var(--color-secondary);
    box-shadow: 0 0 0 3px rgba(28, 92, 109, 0.1);
  }

  .modal-footer {
    padding: 16px 24px;
    background: #f8fafc;
    border-top: 1px solid #e2e8f0;
    display: flex;
    justify-content: flex-end;
    gap: 12px;
  }

  .modal-btn {
    border: none;
    padding: 10px 18px;
    border-radius: 10px;
    font-size: 14px;
    font-weight: 600;
    cursor: pointer;
    transition: all 0.2s ease;
  }

  .cancel-btn,
  .close-btn {
    background: #e2e8f0;
    color: #475569;
  }

  .cancel-btn:hover,
  .close-btn:hover {
    background: #cbd5e1;
    color: #1e293b;
  }

  .edit-btn {
    background: linear-gradient(135deg, var(--color-primary) 0%, #e6b910 100%);
    color: var(--color-dark);
  }

  .edit-btn:hover {
    filter: brightness(1.05);
    transform: translateY(-1px);
  }

  .save-btn {
    background: var(--color-primary);
    color: var(--color-dark);
    box-shadow: 0 4px 10px rgba(248, 206, 28, 0.3);
    display: inline-flex;
    align-items: center;
    gap: 8px;
  }

  .save-btn:hover {
    filter: brightness(1.08);
    box-shadow: 0 6px 14px rgba(248, 206, 28, 0.4);
  }

  /* Animations */
  @keyframes fadeInModal {
    from { opacity: 0; }
    to { opacity: 1; }
  }

  .animate-scale-in {
    animation: scaleInModal 0.25s cubic-bezier(0.34, 1.56, 0.64, 1);
  }

  @keyframes scaleInModal {
    from { transform: scale(0.95); opacity: 0; }
    to { transform: scale(1); opacity: 1; }
  }
</style>
