<script lang="ts">
  import type { CategoryFilterOption, PriceFilter } from "$lib/components/public/eventTypes";
  import { t } from "$lib/i18n";

  export interface FilterVisibility {
    category: boolean;
    dateRange: boolean;
    location: boolean;
    price: boolean;
    tags: boolean;
  }

  const DEFAULT_VISIBILITY: FilterVisibility = {
    category: true,
    dateRange: true,
    location: true,
    price: true,
    tags: true,
  };

  let {
    eventCategories = [],
    festivalCategories = [],
    tags = [],
    selectedTags = [],
    dateFrom = "",
    dateTo = "",
    locations = [],
    selectedLocation = "All Regions",
    maxPrice = 500,
    selectedMaxPrice = 500,
    priceFilter = "all",
    showAdvisory = false,
    visibleSections = DEFAULT_VISIBILITY,
    onToggleCategory = () => {},
    onReset = () => {},
    onDateFromChange = () => {},
    onDateToChange = () => {},
    onLocationChange = () => {},
    onPriceFilterChange = () => {},
    onMaxPriceChange = () => {},
    onToggleTag = () => {},
  } = $props<{
    eventCategories: CategoryFilterOption[];
    festivalCategories: CategoryFilterOption[];
    tags: string[];
    selectedTags: string[];
    dateFrom: string;
    dateTo: string;
    locations: string[];
    selectedLocation: string;
    maxPrice: number;
    selectedMaxPrice: number;
    priceFilter: PriceFilter;
    showAdvisory?: boolean;
    visibleSections?: FilterVisibility;
    onToggleCategory: (categoryId: string) => void;
    onReset: () => void;
    onDateFromChange: (value: string) => void;
    onDateToChange: (value: string) => void;
    onLocationChange: (value: string) => void;
    onPriceFilterChange: (value: PriceFilter) => void;
    onMaxPriceChange: (value: number) => void;
    onToggleTag: (tag: string) => void;
  }>();

  const isTagActive = (tag: string): boolean => selectedTags.includes(tag);
</script>

<aside class="filters-column">
  <div class="filters-card">
    <div class="filters-header">
      <h3><i class="fi fi-rr-settings-sliders"></i> {$t('filters')}</h3>
      <button type="button" class="reset-btn" onclick={onReset}>
        <i class="fi fi-rr-rotate-right"></i> {$t('resetAll')}
      </button>
    </div>

    {#if visibleSections.category}
    <div class="filter-block">
      <h4>{$t('category')} &mdash; Events</h4>
      {#each eventCategories as category}
        <label class="check-row">
          <span class="left">
            <input
              type="checkbox"
              checked={category.checked}
              onchange={() => onToggleCategory(category.id)}
            />
            {category.label}
          </span>
          <span class="count">{category.count}</span>
        </label>
      {/each}
    </div>
    <div class="filter-block">
      <h4>{$t('category')} &mdash; Festivals</h4>
      {#each festivalCategories as category}
        <label class="check-row">
          <span class="left">
            <input
              type="checkbox"
              checked={category.checked}
              onchange={() => onToggleCategory(category.id)}
            />
            {category.label}
          </span>
          <span class="count">{category.count}</span>
        </label>
      {/each}
    </div>
    {/if}

    {#if visibleSections.dateRange}
    <div class="filter-block">
      <h4>{$t('dateRange')}</h4>
      <label class="field-label" for="events-date-from">{$t('from')}</label>
      <div class="field-box">
        <i class="fi fi-rr-calendar-day"></i>
        <input
          id="events-date-from"
          class="field-input"
          type="date"
          value={dateFrom}
          onchange={(event) => onDateFromChange((event.currentTarget as HTMLInputElement).value)}
        />
      </div>

      <label class="field-label" for="events-date-to">{$t('to')}</label>
      <div class="field-box">
        <i class="fi fi-rr-calendar-day"></i>
        <input
          id="events-date-to"
          class="field-input"
          type="date"
          value={dateTo}
          onchange={(event) => onDateToChange((event.currentTarget as HTMLInputElement).value)}
        />
      </div>
    </div>
    {/if}

    {#if visibleSections.location}
    <div class="filter-block">
      <h4>{$t('location')}</h4>
      <div class="field-box">
        <i class="fi fi-rr-marker"></i>
        <select
          class="field-select"
          value={selectedLocation}
          onchange={(event) => onLocationChange((event.currentTarget as HTMLSelectElement).value)}
        >
          {#each locations as location}
            <option value={location}>{location}</option>
          {/each}
        </select>
      </div>
    </div>
    {/if}

    {#if visibleSections.price}
    <div class="filter-block">
      <h4>{$t('price')}</h4>
      <label class="check-row inline">
        <span class="left">
          <input
            type="radio"
            name="price-filter"
            checked={priceFilter === "all"}
            onchange={() => onPriceFilterChange("all")}
          />
          {$t('allPrices')}
        </span>
      </label>
      <label class="check-row inline">
        <span class="left">
          <input
            type="radio"
            name="price-filter"
            checked={priceFilter === "free"}
            onchange={() => onPriceFilterChange("free")}
          />
          {$t('freeEntry')}
        </span>
      </label>
      <label class="check-row inline">
        <span class="left">
          <input
            type="radio"
            name="price-filter"
            checked={priceFilter === "paid"}
            onchange={() => onPriceFilterChange("paid")}
          />
          {$t('paidEvents')}
        </span>
      </label>
      <div class="price-range">$0 <span>${maxPrice}+</span></div>
      <input
        type="range"
        min="0"
        max={maxPrice}
        value={selectedMaxPrice}
        class="slider"
        oninput={(event) => onMaxPriceChange(Number((event.currentTarget as HTMLInputElement).value))}
      />
      <p class="price-note">{$t('upTo')} <strong>${selectedMaxPrice}</strong></p>
    </div>
    {/if}

    {#if visibleSections.tags}
    <div class="filter-block tags-block">
      <h4>{$t('tags')}</h4>
      <div class="tags-wrap">
        {#each tags as tag}
          <button
            type="button"
            class="tag-item"
            class:active={isTagActive(tag)}
            onclick={() => onToggleTag(tag)}
          >
            {tag}
          </button>
        {/each}
      </div>
    </div>
    {/if}
  </div>

  {#if showAdvisory}
    <div class="advisory-card">
      <h4><i class="fi fi-rr-info"></i> {$t('travelAdvisory')}</h4>
      <p>
        Best time to visit Nepal is October-November and March-April for festivals and clear
        Himalayan views.
      </p>
      <button type="button">{$t('viewTravelGuide')} <i class="fi fi-rr-arrow-right"></i></button>
    </div>
  {/if}
</aside>

<style>
  .filters-column {
    display: flex;
    flex-direction: column;
    gap: 1.2rem;
  }

  .filters-card {
    background: #fff;
    border: 1px solid #e5e7eb;
    border-radius: 18px;
    padding: 1.25rem 1.2rem;
  }

  .filters-header {
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: 1rem;
    margin-bottom: 1rem;
  }

  .filters-header h3 {
    margin: 0;
    font-size: 1.75rem;
    color: #334155;
    font-weight: 700;
    display: inline-flex;
    align-items: center;
    gap: 0.45rem;
  }

  .filters-header h3 i {
    color: #bd242b;
    margin-top: 2px;
    font-size: 1rem;
  }

  .reset-btn {
    color: #bd242b;
    font-weight: 700;
    font-size: 0.88rem;
    display: inline-flex;
    align-items: center;
    gap: 0.32rem;
  }

  .filter-block {
    padding-top: 1rem;
    border-top: 1px solid #eef2f7;
  }

  .filter-block h4 {
    margin: 0 0 0.8rem;
    text-transform: uppercase;
    letter-spacing: 0.07em;
    font-size: 0.78rem;
    color: #475569;
    font-weight: 800;
  }

  .check-row {
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: 0.5rem;
    margin-bottom: 0.55rem;
    color: #334155;
    font-size: 1rem;
  }

  .check-row.inline {
    justify-content: flex-start;
    margin-bottom: 0.45rem;
  }

  .left {
    display: inline-flex;
    align-items: center;
    gap: 0.52rem;
  }

  .left input {
    accent-color: #bd242b;
  }

  .count {
    min-width: 34px;
    text-align: center;
    border-radius: 999px;
    background: #f1f5f9;
    color: #64748b;
    font-size: 0.8rem;
    font-weight: 700;
    padding: 0.18rem 0.45rem;
  }

  .field-label {
    display: block;
    font-size: 0.82rem;
    color: #6b7280;
    margin: 0 0 0.25rem;
  }

  .field-box {
    border: 1px solid #e2e8f0;
    border-radius: 10px;
    min-height: 40px;
    display: flex;
    align-items: center;
    gap: 0.55rem;
    padding: 0 0.75rem;
    color: #4b5563;
    margin-bottom: 0.65rem;
    font-size: 0.95rem;
    background: #fff;
  }

  .field-box i {
    color: #bd242b;
    font-size: 0.9rem;
  }

  .field-input,
  .field-select {
    border: none;
    outline: none;
    background: transparent;
    width: 100%;
    color: #334155;
    font-size: 0.95rem;
  }

  .field-select {
    appearance: none;
    cursor: pointer;
  }

  .price-range {
    margin-top: 0.5rem;
    display: flex;
    justify-content: space-between;
    color: #9ca3af;
    font-size: 0.82rem;
  }

  .slider {
    width: 100%;
    accent-color: #bd242b;
    margin-top: 0.3rem;
  }

  .price-note {
    text-align: center;
    color: #64748b;
    font-size: 0.9rem;
    margin-top: 0.2rem;
  }

  .tags-wrap {
    display: flex;
    flex-wrap: wrap;
    gap: 0.5rem;
  }

  .tag-item {
    background: #f1f5f9;
    color: #64748b;
    border-radius: 999px;
    font-size: 0.86rem;
    padding: 0.3rem 0.7rem;
    border: none;
  }

  .tag-item.active {
    background: #bd242b;
    color: white;
  }

  .advisory-card {
    background: linear-gradient(165deg, #1c5c6d, #263038);
    color: #e2f8f6;
    border-radius: 18px;
    padding: 1.2rem;
    border: 1px solid rgba(255, 255, 255, 0.2);
  }

  .advisory-card h4 {
    margin: 0;
    font-size: 1.05rem;
    display: inline-flex;
    align-items: center;
    gap: 0.48rem;
    color: white;
  }

  .advisory-card p {
    margin-top: 0.75rem;
    font-size: 0.95rem;
    line-height: 1.55;
    color: rgba(226, 248, 246, 0.95);
  }

  .advisory-card button {
    margin-top: 0.9rem;
    width: 100%;
    min-height: 43px;
    border-radius: 10px;
    border: 1px solid rgba(255, 255, 255, 0.35);
    color: white;
    font-weight: 700;
    background: rgba(255, 255, 255, 0.09);
  }
</style>
