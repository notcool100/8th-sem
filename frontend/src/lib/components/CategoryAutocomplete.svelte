<!-- src/lib/components/CategoryAutocomplete.svelte -->
<script lang="ts">
  import { onMount } from "svelte";
  import type { CategoryResponse } from "$lib/types/categories";

  let {
    value = $bindable(""),
    placeholder = "Select categories...",
    id = undefined,
    type = undefined,
  }: { value?: string; placeholder?: string; id?: string; type?: "event" | "festival" } = $props();

  let allCategories = $state<CategoryResponse[]>([]);
  let suggestions = $state<CategoryResponse[]>([]);
  let showDropdown = $state(false);
  let highlightedIndex = $state(-1);
  let inputValue = $state("");
  let inputEl: HTMLInputElement;

  onMount(async () => {
    try {
      const query = type ? `?type=${encodeURIComponent(type)}` : "";
      const res = await fetch(`/api/categories${query}`);
      if (!res.ok) return;
      const data = await res.json();
      allCategories = Array.isArray(data) ? data : [];
    } catch (e) {
      console.error("Failed to load categories", e);
    }
  });

  const selected = $derived.by(() => {
    if (!value) return [];
    return value
      .split(",")
      .map((name) => name.trim())
      .filter(Boolean)
      .map((name) => {
        const match = allCategories.find(
          (c) => c.name.toLowerCase() === name.toLowerCase(),
        );
        return {
          name,
          color: match?.color || "#5c9ead",
          icon: match?.icon || "📁",
        };
      });
  });

  function updateSuggestions() {
    const current = inputValue.trim().toLowerCase();
    if (!current) {
      suggestions = [];
      showDropdown = false;
      highlightedIndex = -1;
      return;
    }

    const selectedNames = selected.map((s) => s.name.toLowerCase());
    suggestions = allCategories
      .filter((c) => c.name.toLowerCase().includes(current))
      .filter((c) => !selectedNames.includes(c.name.toLowerCase()));

    showDropdown = suggestions.length > 0;
    highlightedIndex = -1;
  }

  function selectCategory(name: string) {
    const currentNames = value
      ? value
          .split(",")
          .map((n) => n.trim())
          .filter(Boolean)
      : [];

    if (!currentNames.some((n) => n.toLowerCase() === name.toLowerCase())) {
      value = [...currentNames, name].join(", ");
    }
    inputValue = "";
    showDropdown = false;
  }

  function removeCategory(index: number) {
    const currentNames = value
      ? value
          .split(",")
          .map((n) => n.trim())
          .filter(Boolean)
      : [];
    const updated = currentNames.filter((_, i) => i !== index);
    value = updated.join(", ");
  }

  function handleKeydown(event: KeyboardEvent) {
    if (showDropdown) {
      if (event.key === "ArrowDown") {
        event.preventDefault();
        highlightedIndex = (highlightedIndex + 1) % suggestions.length;
        return;
      } else if (event.key === "ArrowUp") {
        event.preventDefault();
        highlightedIndex =
          (highlightedIndex - 1 + suggestions.length) % suggestions.length;
        return;
      } else if (event.key === "Enter" || event.key === "Tab") {
        if (suggestions.length > 0) {
          event.preventDefault();
          const indexToSelect = highlightedIndex >= 0 ? highlightedIndex : 0;
          selectCategory(suggestions[indexToSelect].name);
          return;
        }
      } else if (event.key === "Escape") {
        showDropdown = false;
        return;
      }
    }

    if ((event.key === "Enter" || event.key === "Tab") && inputValue.trim()) {
      event.preventDefault();
      selectCategory(inputValue.trim());
    }
  }

  function handleClickOutside(event: MouseEvent) {
    if (showDropdown && inputEl && !inputEl.contains(event.target as Node)) {
      const dropdownEl = document.querySelector(".category-dropdown");
      if (dropdownEl && !dropdownEl.contains(event.target as Node)) {
        showDropdown = false;
      }
    }
  }
</script>

<svelte:window onclick={handleClickOutside} />

<div class="category-autocomplete-wrapper">
  <div class="selected-categories">
    {#each selected as cat, i}
      <span
        class="category-badge"
        style="border-color: {cat.color}30; background: {cat.color}15; color: {cat.color};"
      >
        <span class="badge-icon">{cat.icon}</span>
        <span class="badge-name">{cat.name}</span>
        <button
          type="button"
          class="remove-btn"
          onclick={() => removeCategory(i)}
          aria-label="Remove category"
        >
          &times;
        </button>
      </span>
    {/each}
  </div>

  <div class="input-relative-container">
    <input
      {id}
      bind:this={inputEl}
      type="text"
      placeholder={selected.length === 0 ? placeholder : "Add category..."}
      bind:value={inputValue}
      oninput={updateSuggestions}
      onfocus={() => {
        if (inputValue.trim()) showDropdown = true;
      }}
      onkeydown={handleKeydown}
      autocomplete="off"
    />

    {#if showDropdown}
      <div class="category-dropdown">
        {#each suggestions as suggestion, i}
          <button
            type="button"
            class="dropdown-item {i === highlightedIndex ? 'active' : ''}"
            onclick={() => selectCategory(suggestion.name)}
            onmouseenter={() => (highlightedIndex = i)}
          >
            <span class="suggestion-icon">{suggestion.icon}</span>
            <span class="suggestion-name">{suggestion.name}</span>
          </button>
        {/each}
      </div>
    {/if}
  </div>
</div>

<style>
  .category-autocomplete-wrapper {
    display: grid;
    gap: 8px;
    width: 100%;
  }

  .selected-categories {
    display: flex;
    flex-wrap: wrap;
    gap: 8px;
  }

  .category-badge {
    display: inline-flex;
    align-items: center;
    gap: 6px;
    padding: 6px 12px;
    border-radius: 20px;
    font-size: 13px;
    font-weight: 600;
    border: 1px solid;
    transition: all 0.2s ease;
  }

  .badge-icon {
    font-size: 14px;
  }

  .remove-btn {
    background: transparent;
    border: none;
    color: inherit;
    font-size: 16px;
    font-weight: bold;
    cursor: pointer;
    padding: 0 0 0 4px;
    line-height: 1;
    opacity: 0.6;
    transition: opacity 0.2s;
  }

  .remove-btn:hover {
    opacity: 1;
  }

  .input-relative-container {
    position: relative;
    width: 100%;
  }

  input {
    width: 100%;
    padding: 10px 12px;
    border-radius: 10px;
    border: 1px solid #c9d6e2;
    background: #f8fafc;
    color: #10243e;
    font: inherit;
  }

  input:focus {
    outline: none;
    border-color: #1c5c6d;
    box-shadow: 0 0 0 3px rgba(28, 92, 109, 0.12);
    background: #fff;
  }

  .category-dropdown {
    position: absolute;
    top: 100%;
    left: 0;
    right: 0;
    margin-top: 4px;
    max-height: 220px;
    overflow-y: auto;
    background: #fff;
    border: 1px solid #c9d6e2;
    border-radius: 10px;
    z-index: 100;
    box-shadow: 0 10px 25px rgba(15, 23, 42, 0.1);
    display: flex;
    flex-direction: column;
    padding: 4px;
  }

  .dropdown-item {
    display: flex;
    align-items: center;
    gap: 8px;
    padding: 8px 12px;
    border: none;
    background: transparent;
    width: 100%;
    text-align: left;
    font: inherit;
    font-size: 14px;
    color: #10243e;
    cursor: pointer;
    border-radius: 6px;
    transition: background-color 0.15s;
  }

  .dropdown-item:hover,
  .dropdown-item.active {
    background-color: #f1f5f9;
  }

  .suggestion-icon {
    font-size: 16px;
  }

  .suggestion-name {
    font-weight: 500;
  }

  /* Dark mode support */
  @media (prefers-color-scheme: dark) {
    input {
      /* background: #2b2b2b; */
      color: #e0e0e0;
      border-color: #444;
    }
    input:focus {
      background: #333;
    }
    .category-dropdown {
      background: #2b2b2b;
      border-color: #444;
      box-shadow: 0 10px 25px rgba(0, 0, 0, 0.3);
    }
    .dropdown-item {
      color: #e0e0e0;
    }
    .dropdown-item:hover,
    .dropdown-item.active {
      background-color: #3d3d3d;
    }
  }
</style>
