<script lang="ts">
  // @ts-ignore - emoji-picker-element is a web component without types
  import { onMount } from "svelte";
  import TagAutocomplete from "$lib/components/TagAutocomplete.svelte";
  import type { PageData } from "./$types";
  import { enhance } from "$app/forms";

  onMount(async () => {
    // @ts-ignore
    await import("emoji-picker-element");
  });

  interface Category {
    id: number;
    title: string;
    description: string;
    tags: string;
    color: string;
    emoji: string;
    isHoliday: boolean;
    type: "event" | "festival";
  }

  let { data }: { data: PageData } = $props();

  function normalizeTags(tags: unknown): string {
    if (Array.isArray(tags)) return tags.join(", ");
    if (typeof tags === "string") return tags;
    return "";
  }

  function normalizeText(value: unknown): string {
    return typeof value === "string" ? value : "";
  }
  
  let categories = $state<Category[]>([]);
  
  $effect(() => {
    categories = (data.categories ?? []).map((c) => ({
      id: c.id,
      title: normalizeText(c.name),
      description: normalizeText(c.description),
      tags: normalizeTags(c.tag),
      color: normalizeText(c.color) || "#d90766",
      emoji: normalizeText(c.icon) || "😊",
      isHoliday: c.isHoliday ?? false,
      type: c.type === "festival" ? "festival" : "event"
    }));
  });

  // Filters
  let searchTitle = $state("");
  let searchTags = $state("");

  // Pagination
  let currentPage = $state(1);
  const itemsPerPage = 5;

  // Modal states
  let selectedCategory = $state<Category | null>(null);
  let showViewModal = $state(false);
  let showEditModal = $state(false);
  let showAddModal = $state(false);

  // Add form fields
  let newTitle = $state("");
  let newDescription = $state("");
  let newTags = $state("");
  let newColor = $state("#d90766");
  let newEmoji = $state("😊");
  let newType = $state<"event" | "festival">("event");

  // Emoji picker visibility states
  let showNewEmojiPicker = $state(false);
  let showEditEmojiPicker = $state(false);

  // Click outside custom action for closing pickers
  function clickOutside(node: HTMLElement, callback: () => void) {
    const handleClick = (event: MouseEvent) => {
      if (node && !node.contains(event.target as Node) && !event.defaultPrevented) {
        callback();
      }
    };

    document.addEventListener("click", handleClick, true);

    return {
      destroy() {
        document.removeEventListener("click", handleClick, true);
      }
    };
  }

  // Filtered Categories
  const filteredCategories = $derived(
    categories.filter((cat) => {
      const matchTitle = normalizeText(cat.title)
        .toLowerCase()
        .includes(searchTitle.toLowerCase());
      const matchTags = normalizeText(cat.tags)
        .toLowerCase()
        .includes(searchTags.toLowerCase());
      return matchTitle && matchTags;
    }),
  );

  // Pagination
  const totalPages = $derived(
    Math.ceil(filteredCategories.length / itemsPerPage),
  );
  const paginatedCategories = $derived(
    filteredCategories.slice(
      (currentPage - 1) * itemsPerPage,
      currentPage * itemsPerPage,
    ),
  );

  // View Category
  function viewCategory(cat: Category): void {
    selectedCategory = { ...cat };
    showViewModal = true;
  }

  // Edit Category
  function editCategory(cat: Category): void {
    selectedCategory = { ...cat };
    showEditModal = true;
    showEditEmojiPicker = false;
  }

  // Form submission enhance handlers
  function handleAddSubmit() {
    return async ({ result }: { result: any }) => {
      if (result.type === "success") {
        newTitle = "";
        newDescription = "";
        newTags = "";
        newColor = "#d90766";
        newEmoji = "😊";
        newType = "event";
        showAddModal = false;
        showNewEmojiPicker = false;
      } else if (result.type === "failure") {
        alert(result.data?.message || "Failed to create category");
      }
    };
  }

  function handleEditSubmit() {
    return async ({ result }: { result: any }) => {
      if (result.type === "success") {
        showEditModal = false;
        showEditEmojiPicker = false;
        selectedCategory = null;
      } else if (result.type === "failure") {
        alert(result.data?.message || "Failed to update category");
      }
    };
  }

  // Delete Category
  async function deleteCategory(id: number): Promise<void> {
    const confirmDelete = confirm(
      "Are you sure you want to delete this category?",
    );
    if (!confirmDelete) return;

    try {
      const formData = new FormData();
      formData.append("id", id.toString());
      
      const response = await fetch("?/delete", {
        method: "POST",
        body: formData
      });

      if (response.ok) {
        const { invalidateAll } = await import("$app/navigation");
        await invalidateAll();
      } else {
        alert("Failed to delete category");
      }
    } catch (e) {
      console.error(e);
      alert("Failed to delete category");
    }
  }

  // Emoji handlers
  function handleNewEmoji(event: any): void {
    newEmoji = event.detail.unicode;
  }
  function handleEditEmoji(event: any): void {
    if (selectedCategory) selectedCategory.emoji = event.detail.unicode;
  }

  // Pagination Buttons
  function nextPage(): void {
    if (currentPage < totalPages) currentPage++;
  }
  function previousPage(): void {
    if (currentPage > 1) currentPage--;
  }
  function handleKeydown(event: KeyboardEvent): void {
    if (event.key === "Escape") {
      showViewModal = false;
      showEditModal = false;
      showAddModal = false;
      showNewEmojiPicker = false;
      showEditEmojiPicker = false;
    }
  }
</script>

<svelte:window onkeydown={handleKeydown} />

<div class="categories-dashboard animate-fade-in" role="main">
  <!-- Top Header Section -->
  <div class="dashboard-header">
    <div class="header-text">
      <h1>Category Management</h1>
      <p class="subtitle">
        Organise event types, assign emojis, colors, and tags for quick
        classification.
      </p>
    </div>

    {#if data.canCreate}
    <button onclick={() => (showAddModal = true)} class="add-category-btn">
      <i class="fi fi-rr-add"></i> Add New Category
    </button>
    {/if}
  </div>

  <!-- Filters Panel -->
  <div class="filters-panel glass">
    <div class="filter-group">
      <span class="filter-icon"><i class="fi fi-rr-search"></i></span>
      <input
        type="text"
        placeholder="Search Category Name"
        bind:value={searchTitle}
      />
    </div>

    <div class="filter-group">
      <span class="filter-icon"><i class="fi fi-rr-tags"></i></span>
      <input type="text" placeholder="Search Tags" bind:value={searchTags} />
    </div>
  </div>

  <!-- Categories Table -->
  <div class="table-container glass">
    {#if paginatedCategories.length > 0}
      <table>
        <thead>
          <tr>
            <th><i class="fi fi-rr-smile"></i> Emoji</th>
            <th><i class="fi fi-rr-document"></i> Title</th>
            <th><i class="fi fi-rr-align-left"></i> Description</th>
            <th><i class="fi fi-rr-tags"></i> Tags</th>
            <th><i class="fi fi-rr-palette"></i> Color</th>
            <th><i class="fi fi-rr-list-check"></i> Applies To</th>
            <th class="text-right"
              ><i class="fi fi-rr-settings-sliders"></i> Actions</th
            >
          </tr>
        </thead>

        <tbody>
          {#each paginatedCategories as cat}
            <tr>
              <td class="emoji-cell">{cat.emoji}</td>

              <td class="font-semibold">{cat.title}</td>

              <td class="text-muted description-cell">{cat.description}</td>

              <td>
                <div class="tags-group">
                  {#each cat.tags.split(",") as tag}
                    <span class="tag-badge">{tag.trim()}</span>
                  {/each}
                </div>
              </td>

              <td>
                <div class="color-preview">
                  <span class="color-swatch" style="background:{cat.color}"
                  ></span>
                  <span class="color-hex text-muted">{cat.color}</span>
                </div>
              </td>

              <td>
                <span class="type-badge {cat.type}">{cat.type === "festival" ? "Festival" : "Event"}</span>
              </td>

              <td>
                <div class="actions-group">
                  <button
                    class="action-btn view"
                    onclick={() => viewCategory(cat)}
                    title="View details"
                  >
                    <i class="fi fi-rr-eye"></i> View
                  </button>

                  {#if data.canUpdate}
                  <button
                    class="action-btn edit"
                    onclick={() => editCategory(cat)}
                    title="Edit category"
                  >
                    <i class="fi fi-rr-pencil"></i> Edit
                  </button>
                  {/if}

                  {#if data.canDelete}
                  <button
                    class="action-btn delete"
                    onclick={() => deleteCategory(cat.id)}
                    title="Delete category"
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
        <i class="fi fi-rr-tags empty-icon"></i>
        <h3>No Categories Found</h3>
        <p>Try adjusting your filters or add a new category.</p>
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
  {#if showViewModal && selectedCategory}
    <div
      class="modal-overlay"
      onclick={() => (showViewModal = false)}
      onkeydown={(e) => e.key === "Escape" && (showViewModal = false)}
      role="button"
      tabindex="0"
    >
      <div
        class="modal-card animate-scale-in"
        onclick={(e) => e.stopPropagation()}
        onkeydown={(e) => e.stopPropagation()}
        role="dialog"
        aria-label="Category details"
        tabindex="0"
      >
        <div class="modal-header">
          <h2>View Category</h2>
          <button
            class="modal-close-icon"
            onclick={() => (showViewModal = false)}
            aria-label="Close"
          >
            <i class="fi fi-rr-cross"></i>
          </button>
        </div>

        <div class="modal-body">
          <div class="detail-row">
            <span class="detail-label"
              ><i class="fi fi-rr-smile"></i> Emoji</span
            >
            <div class="detail-value emoji-large">{selectedCategory.emoji}</div>
          </div>

          <div class="detail-row">
            <span class="detail-label"
              ><i class="fi fi-rr-list-check"></i> Applies To</span
            >
            <div class="detail-value">
              <span class="type-badge {selectedCategory.type}">
                {selectedCategory.type === "festival" ? "Festival" : "Event"}
              </span>
            </div>
          </div>

          <div class="detail-row">
            <span class="detail-label"
              ><i class="fi fi-rr-document"></i> Title</span
            >
            <div class="detail-value font-semibold">
              {selectedCategory.title}
            </div>
          </div>

          <div class="detail-row">
            <span class="detail-label"
              ><i class="fi fi-rr-align-left"></i> Description</span
            >
            <div class="detail-value">{selectedCategory.description}</div>
          </div>

          <div class="detail-row">
            <span class="detail-label"><i class="fi fi-rr-tags"></i> Tags</span>
            <div class="detail-value">
              <div class="tags-group">
                {#each selectedCategory.tags.split(",") as tag}
                  <span class="tag-badge">{tag.trim()}</span>
                {/each}
              </div>
            </div>
          </div>

          <div class="detail-row">
            <span class="detail-label"
              ><i class="fi fi-rr-palette"></i> Color</span
            >
            <div class="detail-value">
              <div class="color-preview">
                <span
                  class="color-swatch"
                  style="background:{selectedCategory.color}"
                ></span>
                <span>{selectedCategory.color}</span>
              </div>
            </div>
          </div>

        </div>

        <div class="modal-footer">
          <button
            class="modal-btn close-btn"
            onclick={() => (showViewModal = false)}>Close</button
          >
        </div>
      </div>
    </div>
  {/if}

  <!-- Edit Modal -->
  {#if showEditModal && selectedCategory}
    <div
      class="modal-overlay"
      onclick={() => (showEditModal = false)}
      onkeydown={(e) => e.key === "Escape" && (showEditModal = false)}
      role="button"
      tabindex="0"
    >
      <form
        class="modal-card animate-scale-in"
        method="POST"
        action="?/update"
        use:enhance={handleEditSubmit}
        onclick={(e) => e.stopPropagation()}
        onkeydown={(e) => e.stopPropagation()}
        role="dialog"
        aria-label="Edit category"
        tabindex="0"
      >
        <input type="hidden" name="id" value={selectedCategory.id} />
        <input type="hidden" name="color" value={selectedCategory.color} />
        <input type="hidden" name="icon" value={selectedCategory.emoji} />
        <input type="hidden" name="tags" value={selectedCategory.tags} />
        <input type="hidden" name="isHoliday" value={String(selectedCategory.isHoliday)} />

        <div class="modal-header">
          <h2>Edit Category</h2>
          <button
            type="button"
            class="modal-close-icon"
            onclick={() => (showEditModal = false)}
            aria-label="Close"
          >
            <i class="fi fi-rr-cross"></i>
          </button>
        </div>

        <div class="modal-body scrollable">
          <div class="modal-form-grid">
            <div class="form-field full-width">
              <label for="edit-title"
                ><i class="fi fi-rr-document"></i> Title</label
              >
              <input
                id="edit-title"
                name="name"
                type="text"
                bind:value={selectedCategory.title}
                placeholder="Category Title"
              />
            </div>

            <div class="form-field full-width">
              <label for="edit-desc"
                ><i class="fi fi-rr-align-left"></i> Description</label
              >
              <textarea
                id="edit-desc"
                name="description"
                bind:value={selectedCategory.description}
                placeholder="Describe this category..."
              ></textarea>
            </div>

            <div class="form-field full-width">
              <span class="field-label"><i class="fi fi-rr-list-check"></i> Applies To</span>
              <div class="type-toggle-group">
                <label class="type-toggle-option">
                  <input
                    type="radio"
                    name="type"
                    value="event"
                    bind:group={selectedCategory.type}
                  />
                  Event
                </label>
                <label class="type-toggle-option">
                  <input
                    type="radio"
                    name="type"
                    value="festival"
                    bind:group={selectedCategory.type}
                  />
                  Festival
                </label>
              </div>
            </div>

            <div class="form-grid-two-columns">
              <div class="form-field">
                <label for="edit-tags">
                  <i class="fi fi-rr-tags"></i> Tags (comma-separated)
                </label>
                <TagAutocomplete id="edit-tags" bind:value={selectedCategory.tags} placeholder="e.g. AI, Coding" />
              </div>

              <div class="form-grid-sub-two">
                <div class="form-field emoji-field" use:clickOutside={() => (showEditEmojiPicker = false)}>
                  <span id="edit-emoji-label" class="field-label"><i class="fi fi-rr-smile"></i> Emoji</span>
                  <div class="emoji-picker-container">
                    <button type="button" class="emoji-trigger-btn" onclick={() => (showEditEmojiPicker = !showEditEmojiPicker)}>
                      <span class="emoji-trigger-val">{selectedCategory.emoji}</span>
                      <span class="emoji-trigger-text">Emoji</span>
                    </button>
                    {#if showEditEmojiPicker}
                      <div class="emoji-popover">
                        <emoji-picker onemoji-click={handleEditEmoji} aria-labelledby="edit-emoji-label"></emoji-picker>
                      </div>
                    {/if}
                  </div>
                </div>

                <div class="form-field">
                  <label for="edit-color"
                    ><i class="fi fi-rr-palette"></i> Color</label
                  >
                  <div class="color-picker-wrapper">
                    <div class="color-swatch-large" style="background: {selectedCategory.color}">
                      <input
                        id="edit-color"
                        type="color"
                        bind:value={selectedCategory.color}
                        class="real-color-input"
                      />
                    </div>
                    <span class="color-hex-label">{selectedCategory.color}</span>
                  </div>
                </div>
              </div>
            </div>

          </div>
        </div>

        <div class="modal-footer">
          <button
            type="button"
            class="modal-btn cancel-btn"
            onclick={() => (showEditModal = false)}>Cancel</button
          >
          <button class="modal-btn save-btn" type="submit">
            <i class="fi fi-rr-check"></i> Save Changes
          </button>
        </div>
      </form>
    </div>
  {/if}

  <!-- Add Modal -->
  {#if showAddModal}
    <div
      class="modal-overlay"
      onclick={() => (showAddModal = false)}
      onkeydown={(e) => e.key === "Escape" && (showAddModal = false)}
      role="button"
      tabindex="0"
    >
      <form
        class="modal-card animate-scale-in"
        method="POST"
        action="?/create"
        use:enhance={handleAddSubmit}
        onclick={(e) => e.stopPropagation()}
        onkeydown={(e) => e.stopPropagation()}
        role="dialog"
        aria-label="Add new category"
        tabindex="0"
      >
        <input type="hidden" name="color" value={newColor} />
        <input type="hidden" name="icon" value={newEmoji} />
        <input type="hidden" name="tags" value={newTags} />
        <input type="hidden" name="isHoliday" value="false" />

        <div class="modal-header">
          <h2>Add New Category</h2>
          <button
            type="button"
            class="modal-close-icon"
            onclick={() => (showAddModal = false)}
            aria-label="Close"
          >
            <i class="fi fi-rr-cross"></i>
          </button>
        </div>

        <div class="modal-body scrollable">
          <div class="modal-form-grid">
            <div class="form-field full-width">
              <label for="new-title"
                ><i class="fi fi-rr-document"></i> Title</label
              >
              <input
                id="new-title"
                name="name"
                type="text"
                bind:value={newTitle}
                placeholder="Category Title"
              />
            </div>

            <div class="form-field full-width">
              <label for="new-desc"
                ><i class="fi fi-rr-align-left"></i> Description</label
              >
              <textarea
                id="new-desc"
                name="description"
                bind:value={newDescription}
                placeholder="Describe this category..."
              ></textarea>
            </div>

            <div class="form-field full-width">
              <span class="field-label"><i class="fi fi-rr-list-check"></i> Applies To</span>
              <div class="type-toggle-group">
                <label class="type-toggle-option">
                  <input type="radio" name="type" value="event" bind:group={newType} />
                  Event
                </label>
                <label class="type-toggle-option">
                  <input type="radio" name="type" value="festival" bind:group={newType} />
                  Festival
                </label>
              </div>
            </div>

            <div class="form-grid-two-columns">
              <div class="form-field">
                <label for="new-tags">
                  <i class="fi fi-rr-tags"></i> Tags (comma-separated)
                </label>
                <TagAutocomplete id="new-tags" bind:value={newTags} placeholder="e.g. AI, Coding" />
              </div>

              <div class="form-grid-sub-two">
                <div class="form-field emoji-field" use:clickOutside={() => (showNewEmojiPicker = false)}>
                  <span id="new-emoji-label" class="field-label"><i class="fi fi-rr-smile"></i> Emoji</span>
                  <div class="emoji-picker-container">
                    <button type="button" class="emoji-trigger-btn" onclick={() => (showNewEmojiPicker = !showNewEmojiPicker)}>
                      <span class="emoji-trigger-val">{newEmoji}</span>
                      <span class="emoji-trigger-text">Emoji</span>
                    </button>
                    {#if showNewEmojiPicker}
                      <div class="emoji-popover">
                        <emoji-picker onemoji-click={handleNewEmoji} aria-labelledby="new-emoji-label"></emoji-picker>
                      </div>
                    {/if}
                  </div>
                </div>

                <div class="form-field">
                  <label for="new-color"
                    ><i class="fi fi-rr-palette"></i> Color</label
                  >
                  <div class="color-picker-wrapper">
                    <div class="color-swatch-large" style="background: {newColor}">
                      <input
                        id="new-color"
                        type="color"
                        bind:value={newColor}
                        class="real-color-input"
                      />
                    </div>
                    <span class="color-hex-label">{newColor}</span>
                  </div>
                </div>
              </div>
            </div>

          </div>
        </div>

        <div class="modal-footer">
          <button
            type="button"
            class="modal-btn cancel-btn"
            onclick={() => (showAddModal = false)}>Cancel</button
          >
          <button class="modal-btn save-btn" type="submit">
            <i class="fi fi-rr-check"></i> Add Category
          </button>
        </div>
      </form>
    </div>
  {/if}
</div>

<style>
  .categories-dashboard {
    max-width: 1200px;
    margin: 0 auto;
    display: flex;
    flex-direction: column;
    gap: 28px;
  }

  /* Header */
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

  .add-category-btn {
    background: linear-gradient(
      135deg,
      var(--color-primary) 0%,
      #e6b910 100%
    );
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

  .add-category-btn:hover {
    transform: translateY(-2px);
    box-shadow: 0 6px 18px rgba(248, 206, 28, 0.4);
    filter: brightness(1.05);
  }

  .add-category-btn:active {
    transform: translateY(0);
  }

  /* Filters */
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

  .filter-group input {
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

  .filter-group input:focus {
    border-color: var(--color-secondary);
    box-shadow: 0 0 0 3px rgba(28, 92, 109, 0.15);
  }

  /* Table */
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
    font-family: "Quicksand", sans-serif;
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
    vertical-align: middle;
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

  .emoji-cell {
    font-size: 26px;
    line-height: 1;
  }

  .description-cell {
    max-width: 240px;
    white-space: nowrap;
    overflow: hidden;
    text-overflow: ellipsis;
    font-size: 13.5px;
  }

  /* Tags */
  .tags-group {
    display: flex;
    flex-wrap: wrap;
    gap: 6px;
  }

  .tag-badge {
    background: rgba(28, 92, 109, 0.08);
    color: var(--color-secondary);
    border: 1px solid rgba(28, 92, 109, 0.2);
    padding: 3px 10px;
    border-radius: 6px;
    font-size: 12px;
    font-weight: 600;
    white-space: nowrap;
  }

  /* Color preview */
  .color-preview {
    display: inline-flex;
    align-items: center;
    gap: 8px;
  }

  .color-swatch {
    display: inline-block;
    width: 22px;
    height: 22px;
    border-radius: 6px;
    border: 1px solid rgba(0, 0, 0, 0.1);
    flex-shrink: 0;
  }

  .color-hex {
    font-size: 13px;
    font-family: monospace;
  }

  /* Actions */
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

  /* Modal Overlay */
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
    max-width: 560px;
    box-shadow: 0 25px 50px -12px rgba(0, 0, 0, 0.15);
    border: 1px solid rgba(28, 92, 109, 0.08);
    overflow: hidden;
    display: flex;
    flex-direction: column;
    max-height: 88vh;
    min-height: auto;
  }

  .modal-header {
    background: linear-gradient(
      135deg,
      var(--color-secondary) 0%,
      #2a7f96 100%
    );
    color: white;
    padding: 20px 24px;
    display: flex;
    justify-content: space-between;
    align-items: center;
    flex-shrink: 0;
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
    padding: 28px 24px;
    display: flex;
    flex-direction: column;
    gap: 20px;
    flex: 1;
    overflow-y: auto;
    overflow-x: hidden;
  }

  .modal-body.scrollable {
    overflow-y: auto;
    scrollbar-width: thin;
    scrollbar-color: rgba(28, 92, 109, 0.3) rgba(28, 92, 109, 0.1);
  }

  .modal-body.scrollable::-webkit-scrollbar {
    width: 6px;
  }

  .modal-body.scrollable::-webkit-scrollbar-track {
    background: rgba(28, 92, 109, 0.05);
  }

  .modal-body.scrollable::-webkit-scrollbar-thumb {
    background: rgba(28, 92, 109, 0.3);
    border-radius: 3px;
  }

  .modal-body.scrollable::-webkit-scrollbar-thumb:hover {
    background: rgba(28, 92, 109, 0.5);
  }

  /* View Modal */
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

  .emoji-large {
    font-size: 36px;
    line-height: 1;
  }

  /* Edit / Add form */
  .form-field {
    display: flex;
    flex-direction: column;
    gap: 10px;
  }

  .form-field label,
  .form-field .field-label {
    font-size: 13px;
    font-weight: 700;
    color: #1e293b;
    display: inline-flex;
    align-items: center;
    gap: 8px;
    text-transform: uppercase;
    letter-spacing: 0.5px;
  }

  .form-field label i,
  .form-field .field-label i {
    color: var(--color-secondary);
    font-size: 14px;
  }

  .type-toggle-group {
    display: flex;
    gap: 16px;
  }

  .type-toggle-option {
    display: inline-flex;
    align-items: center;
    gap: 6px;
    font-size: 14px;
    font-weight: 600;
    color: #334155;
    text-transform: none;
    letter-spacing: normal;
    cursor: pointer;
  }

  .type-toggle-option input[type="radio"] {
    accent-color: var(--color-secondary, #1c5c6d);
  }

  .type-badge {
    display: inline-flex;
    align-items: center;
    padding: 3px 10px;
    border-radius: 999px;
    font-size: 11px;
    font-weight: 700;
    text-transform: uppercase;
    letter-spacing: 0.4px;
  }

  .type-badge.event {
    background: rgba(3, 105, 161, 0.12);
    color: #0369a1;
  }

  .type-badge.festival {
    background: rgba(217, 119, 6, 0.12);
    color: #d97706;
  }

  .form-field input,
  .form-field textarea {
    padding: 12px 14px;
    border: 2px solid #e2e8f0;
    border-radius: 12px;
    font-size: 14px;
    outline: none;
    width: 100%;
    transition: all 0.2s ease;
    font-family: inherit;
    box-sizing: border-box;
    background: #f8fafc;
    color: #1e293b;
  }

  .form-field input::placeholder,
  .form-field textarea::placeholder {
    color: #94a3b8;
  }

  .form-field textarea {
    min-height: 100px;
    resize: none;
    font-family: inherit;
  }

  .form-field input:focus,
  .form-field textarea:focus {
    border-color: var(--color-secondary);
    background: white;
    box-shadow: 0 0 0 3px rgba(28, 92, 109, 0.1);
  }

  /* Modal Form Layout */
  .modal-form-grid {
    display: flex;
    flex-direction: column;
    gap: 18px;
    width: 100%;
  }

  .form-grid-two-columns {
    display: grid;
    grid-template-columns: 1.2fr 0.8fr;
    gap: 20px;
    align-items: flex-start;
  }

  @media (max-width: 500px) {
    .form-grid-two-columns {
      grid-template-columns: 1fr;
      gap: 16px;
    }
  }

  .form-grid-sub-two {
    display: grid;
    grid-template-columns: 1fr 1fr;
    gap: 12px;
    width: 100%;
  }

  /* Emoji Picker Popover */
  .emoji-picker-container {
    position: relative;
    width: 100%;
  }

  .emoji-trigger-btn {
    display: flex;
    align-items: center;
    justify-content: center;
    gap: 8px;
    background: #f8fafc;
    border: 2px solid #e2e8f0;
    border-radius: 12px;
    padding: 10px 14px;
    cursor: pointer;
    transition: all 0.2s ease;
    width: 100%;
    height: 48px;
    box-sizing: border-box;
  }

  .emoji-trigger-btn:hover {
    border-color: var(--color-secondary);
    background: #ffffff;
    box-shadow: 0 2px 8px rgba(28, 92, 109, 0.05);
  }

  .emoji-trigger-val {
    font-size: 20px;
    line-height: 1;
  }

  .emoji-trigger-text {
    font-size: 13px;
    color: #475569;
    font-weight: 700;
    text-transform: uppercase;
    letter-spacing: 0.3px;
  }

  .emoji-popover {
    position: absolute;
    bottom: calc(100% + 8px);
    right: 0;
    z-index: 1200;
    box-shadow: 0 12px 30px rgba(15, 23, 42, 0.15);
    border-radius: 14px;
    border: 1px solid #e2e8f0;
    overflow: hidden;
    animation: slideUpPopover 0.2s cubic-bezier(0.16, 1, 0.3, 1);
  }

  @keyframes slideUpPopover {
    from {
      transform: translateY(8px);
      opacity: 0;
    }
    to {
      transform: translateY(0);
      opacity: 1;
    }
  }

  emoji-picker {
    max-height: 280px;
    overflow-y: auto;
    border: none;
    background: white;
  }

  /* Premium Styled Color Picker */
  .full-width {
    width: 100%;
  }

  /* Ensure modal form grid fills container */
  .modal-form-grid {
    width: 100%;
  }

  /* Adjustments for emoji picker and color picker to align correctly */
  .emoji-trigger-btn,
  .color-picker-wrapper {
    min-height: 48px;
  }


  .color-picker-wrapper:hover {
    border-color: var(--color-secondary);
    background: #ffffff;
    box-shadow: 0 2px 8px rgba(28, 92, 109, 0.05);
  }

  .color-swatch-large {
    position: relative;
    width: 28px;
    height: 28px;
    border-radius: 8px;
    border: 1px solid rgba(0, 0, 0, 0.1);
    cursor: pointer;
    overflow: hidden;
    flex-shrink: 0;
    box-shadow: 0 2px 4px rgba(0, 0, 0, 0.05);
  }

  .real-color-input {
    position: absolute;
    top: -10px;
    left: -10px;
    width: 48px;
    height: 48px;
    padding: 0;
    border: none;
    cursor: pointer;
    opacity: 0;
  }

  .color-hex-label {
    font-size: 12.5px;
    font-family: monospace;
    font-weight: 700;
    color: #475569;
    text-transform: uppercase;
    letter-spacing: 0.3px;
  }

  /* Modal footer */
  .modal-footer {
    padding: 18px 24px;
    background: linear-gradient(to top, #f8fafc, #fafbfc);
    border-top: 1px solid #e2e8f0;
    display: flex;
    justify-content: flex-end;
    gap: 12px;
    flex-shrink: 0;
  }

  .modal-btn {
    border: none;
    padding: 11px 20px;
    border-radius: 10px;
    font-size: 14px;
    font-weight: 600;
    cursor: pointer;
    transition: all 0.2s ease;
    display: inline-flex;
    align-items: center;
    gap: 8px;
    text-transform: uppercase;
    letter-spacing: 0.3px;
  }

  .cancel-btn,
  .close-btn {
    background: #e2e8f0;
    color: #475569;
    border: 1px solid #cbd5e1;
  }

  .cancel-btn:hover,
  .close-btn:hover {
    background: #cbd5e1;
    color: #1e293b;
    border-color: #94a3b8;
  }

  .save-btn {
    background: linear-gradient(135deg, var(--color-primary) 0%, #e6b910 100%);
    color: var(--color-dark);
    border: none;
    box-shadow: 0 4px 12px rgba(248, 206, 28, 0.35);
    min-width: 120px;
    justify-content: center;
  }

  .save-btn:hover {
    transform: translateY(-2px);
    box-shadow: 0 6px 16px rgba(28, 92, 109, 0.35);
  }

  .save-btn:active {
    transform: translateY(0);
  }

  /* Holiday badge */
  .holiday-badge {
    display: inline-flex;
    align-items: center;
    gap: 5px;
    background: #fee2e2;
    color: #b91c1c;
    border: 1px solid #fca5a5;
    border-radius: 8px;
    font-size: 12px;
    font-weight: 700;
    padding: 3px 10px;
    white-space: nowrap;
  }

  /* Holiday toggle field */
  .holiday-toggle-field {
    background: #fafafa;
    border: 1px solid #e2e8f0;
    border-radius: 12px;
    padding: 14px 16px;
  }

  .toggle-switch {
    display: flex;
    align-items: center;
    gap: 12px;
    cursor: pointer;
    user-select: none;
  }

  .toggle-switch input[type="checkbox"] {
    position: absolute;
    opacity: 0;
    width: 0;
    height: 0;
  }

  .toggle-track {
    position: relative;
    width: 44px;
    height: 24px;
    background: #d1d5db;
    border-radius: 999px;
    flex-shrink: 0;
    transition: background 0.2s;
  }

  .toggle-switch input:checked ~ .toggle-track {
    background: #dc2626;
  }

  .toggle-thumb {
    position: absolute;
    top: 3px;
    left: 3px;
    width: 18px;
    height: 18px;
    background: white;
    border-radius: 50%;
    box-shadow: 0 1px 3px rgba(0,0,0,0.2);
    transition: transform 0.2s;
  }

  .toggle-switch input:checked ~ .toggle-track .toggle-thumb {
    transform: translateX(20px);
  }

  .toggle-label {
    font-size: 13px;
    color: #475569;
    font-weight: 500;
  }

  /* Animations */
  @keyframes fadeInModal {
    from {
      opacity: 0;
    }
    to {
      opacity: 1;
    }
  }

  .animate-scale-in {
    animation: scaleInModal 0.25s cubic-bezier(0.34, 1.56, 0.64, 1);
  }

  @keyframes scaleInModal {
    from {
      transform: scale(0.95);
      opacity: 0;
    }
    to {
      transform: scale(1);
      opacity: 1;
    }
  }
</style>
