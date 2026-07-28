// src/lib/components/TagAutocomplete.svelte
<script lang="ts">
  import { onMount, tick } from 'svelte';

  let {
    value = $bindable(''),
    placeholder = '',
    id = undefined
  }: { value?: string; placeholder?: string; id?: string } = $props();

  let allTags = $state<string[]>([]);
let suggestions = $state<string[]>([]);
let showDropdown = $state(false);
let highlightedIndex = $state(-1);
let inputEl: HTMLInputElement;

// New state for selected tags and current input
let selectedTags = $state<string[]>([]);
let inputValue = $state<string>('');

  // Load tags from API on component mount
  onMount(async () => {
    try {
      const res = await fetch('/api/tags');
      if (!res.ok) return;

      const data = await res.json();
      allTags = Array.isArray(data) ? data : (data?.data ?? []);
    } catch (e) {
      console.error('Failed to load tags', e);
    }
  });

  // Initialize selectedTags from bound value when component mounts or value changes
  $effect(() => {
    if (value && selectedTags.length === 0) {
      selectedTags = value.split(',').map(t => t.trim()).filter(t => t.length > 0);
    }
  });

  // Keep bound value in sync with selectedTags array
$effect(() => {
  value = selectedTags.join(', ');
});

// Reactive input handling
function updateSuggestions() {
  const current = inputValue.trim();
  if (current.length === 0) {
    suggestions = [];
    showDropdown = false;
    highlightedIndex = -1;
    return;
  }
  const used = selectedTags;
  suggestions = allTags
    .filter(t => t.toLowerCase().startsWith(current.toLowerCase()))
    .filter(t => !used.includes(t));
  showDropdown = suggestions.length > 0;
  highlightedIndex = -1;
}

  function selectSuggestion(tag: string) {
  if (!selectedTags.includes(tag)) {
    selectedTags = [...selectedTags, tag];
  }
  inputValue = '';
  showDropdown = false;
}

function removeTag(index: number) {
  selectedTags = selectedTags.filter((_, i) => i !== index);
}

  function handleKeydown(event: KeyboardEvent) {
    // If dropdown is visible, navigate suggestions
    if (showDropdown) {
      if (event.key === 'ArrowDown') {
        event.preventDefault();
        highlightedIndex = (highlightedIndex + 1) % suggestions.length;
        return;
      } else if (event.key === 'ArrowUp') {
        event.preventDefault();
        highlightedIndex = (highlightedIndex - 1 + suggestions.length) % suggestions.length;
        return;
      } else if (event.key === 'Enter' || event.key === 'Tab') {
        if (suggestions.length > 0) {
          event.preventDefault();
          const indexToSelect = highlightedIndex >= 0 ? highlightedIndex : 0;
          selectSuggestion(suggestions[indexToSelect]);
          return;
        }
      } else if (event.key === 'Escape') {
        showDropdown = false;
        return;
      }
    }

    // When dropdown is not shown or no suggestion selected, handle Enter to add custom tag
    if ((event.key === 'Enter' || event.key === 'Tab') && inputValue.trim().length > 0) {
      event.preventDefault();
      selectSuggestion(inputValue.trim());
    }
  }
</script>

<style>
  .autocomplete-wrapper {
    position: relative;
    width: 100%;
  }
  input {
    width: 100%;
    padding: 12px 14px;
    border: 1px solid rgba(0,0,0,0.1);
    border-radius: 10px;
    font-size: 14px;
  }
  .dropdown {
    position: absolute;
    top: 100%;
    left: 0;
    right: 0;
    max-height: 200px;
    overflow-y: auto;
    background: rgba(255,255,255,0.9);
    backdrop-filter: blur(8px);
    border: 1px solid rgba(0,0,0,0.1);
    border-top: none;
    border-radius: 0 0 8px 8px;
    z-index: 10;
    box-shadow: 0 4px 12px rgba(0,0,0,0.08);
  }
  .item {
    padding: 8px 12px;
    cursor: pointer;
  }
  .item:hover,
  .item.active {
    background: rgba(28,92,109,0.1);
  }
  .selected-tags {
    display: flex;
    flex-wrap: wrap;
    gap: 6px;
    margin-bottom: 4px;
  }
  .tag-badge {
    background: rgba(28,92,109,0.2);
    color: var(--color-dark);
    padding: 4px 8px;
    border-radius: 6px;
    font-size: 13px;
    display: inline-flex;
    align-items: center;
    gap: 4px;
  }
  .remove-tag {
    background: transparent;
    border: none;
    color: inherit;
    font-weight: bold;
    cursor: pointer;
    padding: 0 2px;
    line-height: 1;
  }
</style>

<div class="autocomplete-wrapper">
  <div class="selected-tags">
  {#each selectedTags as tag, i}
    <span class="tag-badge">
      {tag}
      <button type="button" class="remove-tag" onclick={() => removeTag(i)} aria-label="Remove tag">×</button>
    </span>
  {/each}
</div>

<input
  {id}
  bind:this={inputEl}
  type="text"
  placeholder={placeholder}
  bind:value={inputValue}
  oninput={updateSuggestions}
  onkeydown={handleKeydown}
  autocomplete="off"
/>
  {#if showDropdown}
    <div class="dropdown">
      {#each suggestions as suggestion, i}
        <div
          class="item {i === highlightedIndex ? 'active' : ''}"
          onclick={() => selectSuggestion(suggestion)}
          onkeydown={(event) => {
            if (event.key === 'Enter' || event.key === ' ') {
              event.preventDefault();
              selectSuggestion(suggestion);
            }
          }}
          onmouseenter={() => (highlightedIndex = i)}
          role="option"
          aria-selected={i === highlightedIndex}
          tabindex="0"
        >{suggestion}</div>
      {/each}
    </div>
  {/if}
</div>
