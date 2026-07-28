<script lang="ts">
  import { onMount, onDestroy } from "svelte";
  import type { Map as LeafletMap, Marker } from "leaflet";

  const DEFAULT_LAT = 27.7172;
  const DEFAULT_LNG = 85.324;

  let {
    latitude = $bindable<number | null>(null),
    longitude = $bindable<number | null>(null),
    address = $bindable<string>(""),
    interactive = true,
    height = "280px",
  }: {
    latitude?: number | null;
    longitude?: number | null;
    address?: string;
    interactive?: boolean;
    height?: string;
  } = $props();

  let mapContainer: HTMLDivElement;
  let map: LeafletMap | undefined;
  let marker: Marker | undefined;
  let leafletLib: typeof import("leaflet") | undefined;
  let mapReady = $state(false);
  let isLocating = $state(false);
  let geocodeError = $state("");

  type SearchResult = {
    place_id: number;
    display_name: string;
    lat: string;
    lon: string;
  };

  let searchQuery = $state("");
  let searchResults = $state<SearchResult[]>([]);
  let isSearching = $state(false);
  let showResults = $state(false);
  let highlightedIndex = $state(-1);
  let searchDebounce: ReturnType<typeof setTimeout> | undefined;

  function handleSearchInput() {
    clearTimeout(searchDebounce);
    highlightedIndex = -1;
    const query = searchQuery.trim();
    if (query.length < 3) {
      searchResults = [];
      showResults = false;
      return;
    }
    searchDebounce = setTimeout(() => performSearch(query), 400);
  }

  // The search box lives inside the event/festival <form> — without this, Enter
  // submits the whole form instead of picking a result (and arrow keys do nothing).
  function handleSearchKeydown(event: KeyboardEvent) {
    if (event.key === "Enter") {
      event.preventDefault();
      if (showResults && searchResults.length) {
        const index = highlightedIndex >= 0 ? highlightedIndex : 0;
        selectSearchResult(searchResults[index]);
      }
      return;
    }

    if (!showResults || !searchResults.length) return;

    if (event.key === "ArrowDown") {
      event.preventDefault();
      highlightedIndex = (highlightedIndex + 1) % searchResults.length;
    } else if (event.key === "ArrowUp") {
      event.preventDefault();
      highlightedIndex = highlightedIndex <= 0 ? searchResults.length - 1 : highlightedIndex - 1;
    } else if (event.key === "Escape") {
      showResults = false;
      highlightedIndex = -1;
    }
  }

  async function performSearch(query: string) {
    isSearching = true;
    try {
      const response = await fetch(
        `https://nominatim.openstreetmap.org/search?format=jsonv2&limit=5&countrycodes=np&q=${encodeURIComponent(query)}`,
      );
      if (!response.ok) throw new Error("Search failed");
      searchResults = (await response.json()) as SearchResult[];
      showResults = searchResults.length > 0;
      highlightedIndex = -1;
    } catch {
      searchResults = [];
      showResults = false;
    } finally {
      isSearching = false;
    }
  }

  function selectSearchResult(result: SearchResult) {
    const lat = parseFloat(result.lat);
    const lng = parseFloat(result.lon);
    latitude = lat;
    longitude = lng;
    address = result.display_name;
    searchQuery = result.display_name;
    showResults = false;
    searchResults = [];
    highlightedIndex = -1;
    placeMarker(lat, lng);
    map?.setView([lat, lng], 16);
  }

  async function reverseGeocode(lat: number, lng: number) {
    if (!interactive) return;
    isLocating = true;
    geocodeError = "";
    try {
      const response = await fetch(
        `https://nominatim.openstreetmap.org/reverse?format=jsonv2&lat=${lat}&lon=${lng}`,
      );
      if (!response.ok) throw new Error("Lookup failed");
      const data = await response.json();
      if (data?.display_name) {
        address = data.display_name;
      }
    } catch {
      geocodeError = "Couldn't resolve an address for this pin — you can type one manually.";
    } finally {
      isLocating = false;
    }
  }

  function placeMarker(lat: number, lng: number) {
    if (!map || !leafletLib) return;

    if (marker) {
      marker.setLatLng([lat, lng]);
    } else {
      marker = leafletLib.marker([lat, lng], { draggable: interactive }).addTo(map);
      if (interactive) {
        marker.on("dragend", () => {
          const pos = marker!.getLatLng();
          latitude = pos.lat;
          longitude = pos.lng;
          reverseGeocode(pos.lat, pos.lng);
        });
      }
    }
  }

  onMount(() => {
    let cancelled = false;

    (async () => {
      const L = await import("leaflet");
      await import("leaflet/dist/leaflet.css");

      // Vite bundles Leaflet's default marker icons incorrectly unless pointed at explicit URLs.
      const iconRetinaUrl = (await import("leaflet/dist/images/marker-icon-2x.png?url")).default;
      const iconUrl = (await import("leaflet/dist/images/marker-icon.png?url")).default;
      const shadowUrl = (await import("leaflet/dist/images/marker-shadow.png?url")).default;
      L.Icon.Default.mergeOptions({ iconRetinaUrl, iconUrl, shadowUrl });

      if (cancelled) return;
      leafletLib = L;

      const startLat = latitude ?? DEFAULT_LAT;
      const startLng = longitude ?? DEFAULT_LNG;

      map = L.map(mapContainer, {
        center: [startLat, startLng],
        zoom: latitude != null ? 15 : 12,
      });

      L.tileLayer("https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png", {
        attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors',
        maxZoom: 19,
      }).addTo(map);

      if (interactive) {
        map.on("click", (e: any) => {
          const { lat, lng } = e.latlng;
          latitude = lat;
          longitude = lng;
          reverseGeocode(lat, lng);
        });
      }

      mapReady = true;
    })();

    return () => {
      cancelled = true;
    };
  });

  // Keep the pin in sync with latitude/longitude, whether the user just dropped it
  // or it arrived later (e.g. loading an existing event's coordinates after mount).
  $effect(() => {
    if (!mapReady || latitude == null || longitude == null) return;
    placeMarker(latitude, longitude);
    map?.setView([latitude, longitude], map.getZoom());
  });

  function locateMe() {
    if (!navigator.geolocation) return;
    isLocating = true;
    navigator.geolocation.getCurrentPosition(
      (position) => {
        latitude = position.coords.latitude;
        longitude = position.coords.longitude;
        isLocating = false;
      },
      () => {
        isLocating = false;
        geocodeError = "Couldn't get your current location.";
      },
    );
  }

  onDestroy(() => {
    clearTimeout(searchDebounce);
    map?.remove();
  });
</script>

<div class="location-picker">
  {#if interactive}
    <div class="picker-search">
      <div class="search-input-wrap">
        <i class="fi fi-rr-search"></i>
        <input
          type="text"
          placeholder="Search for a place or address…"
          bind:value={searchQuery}
          oninput={handleSearchInput}
          onkeydown={handleSearchKeydown}
          onfocus={() => { if (searchResults.length) showResults = true; }}
          onblur={() => { setTimeout(() => (showResults = false), 150); }}
        />
        {#if isSearching}<span class="spinner-icon"></span>{/if}
      </div>
      {#if showResults && searchResults.length}
        <ul class="search-results">
          {#each searchResults as result, index (result.place_id)}
            <li>
              <button
                type="button"
                class:highlighted={index === highlightedIndex}
                onclick={() => selectSearchResult(result)}
                onmouseenter={() => (highlightedIndex = index)}
              >
                <i class="fi fi-rr-marker"></i>
                <span>{result.display_name}</span>
              </button>
            </li>
          {/each}
        </ul>
      {/if}
    </div>

    <div class="picker-toolbar">
      <p class="picker-hint">
        <i class="fi fi-rr-hand-pointer"></i>
        Click the map or drag the pin to set the exact event location.
      </p>
      <button type="button" class="locate-btn" onclick={locateMe} disabled={isLocating}>
        <i class="fi fi-rr-marker"></i> Use my location
      </button>
    </div>
  {/if}

  <div bind:this={mapContainer} class="map-canvas" style={`height:${height};`}></div>

  {#if interactive && isLocating}
    <p class="picker-status"><span class="spinner-icon"></span> Looking up address...</p>
  {/if}
  {#if interactive && geocodeError}
    <p class="picker-status error">{geocodeError}</p>
  {/if}
</div>

<style>
  .location-picker {
    display: flex;
    flex-direction: column;
    gap: 8px;
  }

  .picker-search {
    position: relative;
  }

  .search-input-wrap {
    display: flex;
    align-items: center;
    gap: 8px;
    padding: 0 12px;
    border: 1px solid #cbd5e1;
    border-radius: 8px;
    background: #fff;
  }

  .search-input-wrap i {
    color: #94a3b8;
    font-size: 0.85rem;
  }

  .search-input-wrap input {
    flex: 1;
    border: none;
    outline: none;
    padding: 9px 0;
    font-size: 0.85rem;
    background: transparent;
    color: #1e293b;
  }

  .search-results {
    position: absolute;
    top: calc(100% + 4px);
    left: 0;
    right: 0;
    z-index: 1000;
    margin: 0;
    padding: 4px;
    list-style: none;
    background: #fff;
    border: 1px solid #cbd5e1;
    border-radius: 10px;
    box-shadow: 0 10px 24px rgba(15, 23, 42, 0.12);
    max-height: 220px;
    overflow-y: auto;
  }

  .search-results li button {
    display: flex;
    align-items: flex-start;
    gap: 8px;
    width: 100%;
    text-align: left;
    padding: 8px 10px;
    border: none;
    background: none;
    border-radius: 6px;
    cursor: pointer;
    font-size: 0.82rem;
    color: #334155;
  }

  .search-results li button:hover,
  .search-results li button.highlighted {
    background: #f1f5f9;
  }

  .search-results li button i {
    margin-top: 2px;
    color: #1c5c6d;
    font-size: 0.8rem;
  }

  .picker-toolbar {
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: 12px;
    flex-wrap: wrap;
  }

  .picker-hint {
    margin: 0;
    font-size: 0.85rem;
    color: #64748b;
    display: flex;
    align-items: center;
    gap: 6px;
  }

  .locate-btn {
    display: inline-flex;
    align-items: center;
    gap: 6px;
    padding: 6px 12px;
    font-size: 0.82rem;
    border: 1px solid #cbd5e1;
    background: #fff;
    border-radius: 8px;
    cursor: pointer;
    color: #334155;
  }

  .locate-btn:hover {
    background: #f1f5f9;
  }

  .locate-btn:disabled {
    opacity: 0.6;
    cursor: not-allowed;
  }

  .map-canvas {
    width: 100%;
    border: 1px solid #cbd5e1;
    border-radius: 12px;
    overflow: hidden;
  }

  .picker-status {
    margin: 0;
    font-size: 0.8rem;
    color: #64748b;
    display: flex;
    align-items: center;
    gap: 6px;
  }

  .picker-status.error {
    color: #dc2626;
  }

  .spinner-icon {
    width: 12px;
    height: 12px;
    border: 2px solid #cbd5e1;
    border-top-color: #64748b;
    border-radius: 50%;
    display: inline-block;
    animation: spin 0.7s linear infinite;
  }

  @keyframes spin {
    to {
      transform: rotate(360deg);
    }
  }
</style>
