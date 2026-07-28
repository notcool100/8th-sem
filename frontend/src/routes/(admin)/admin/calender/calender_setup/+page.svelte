<script lang="ts">
	import Calendar from "$lib/components/calendar/Calendar.svelte";
	import CalendarFilters from "$lib/components/public/CalendarFilters.svelte";
	import EventDetailsModal from "$lib/components/EventDetailsModal.svelte";
	import { eventOverlapsRange, type CalendarEvent } from "$lib/components/calendar/dateUtils";
	import { goto, invalidateAll } from "$app/navigation";
	import { enhance } from "$app/forms";
	import type { PageData, ActionData } from "./$types";
	import { colorForCategory, mapEventDtoToPublicEvent, mapFestivalDtoToPublicEvent, slugifyCategory } from "$lib/utils/eventMapping";
	import type { CategoryFilterOption, PriceFilter, PublicEvent } from "$lib/components/public/eventTypes";

	let { data, form } = $props<{ data: PageData; form: ActionData }>();

	const adminEvents: PublicEvent[] = [
		...data.events.map(mapEventDtoToPublicEvent),
		...(data.festivals ?? []).map(mapFestivalDtoToPublicEvent)
	];
	const eventCategoryDefinitions = buildCategoryDefinitions(
		adminEvents.filter((event) => event.source !== "festival"),
		"event"
	);
	const festivalCategoryDefinitions = buildCategoryDefinitions(
		adminEvents.filter((event) => event.source === "festival"),
		"festival"
	);
	const categoryDefinitions = [...eventCategoryDefinitions, ...festivalCategoryDefinitions];
	const allCategoryIds = categoryDefinitions.map((category) => category.id);
	const eventDates = adminEvents.flatMap((event) => [eventDate(event).getTime(), eventEndDate(event).getTime()]).filter(Number.isFinite);
	const todayInput = formatAsInputDate(new Date());
	const defaultDateFrom = eventDates.length ? formatAsInputDate(new Date(Math.min(...eventDates))) : todayInput;
	const defaultDateTo = eventDates.length ? formatAsInputDate(new Date(Math.max(...eventDates))) : todayInput;
	const locationOptions = ["All Regions", ...Array.from(new Set(adminEvents.map((event) => event.region).filter(Boolean))).sort()];
	const tagOptions = Array.from(new Set(adminEvents.flatMap((event) => event.tags))).sort();
	const maxPrice = Math.max(500, Math.ceil(Math.max(0, ...adminEvents.map((event) => event.price)) / 50) * 50);

	let selectedCategoryIds = $state<string[]>([...allCategoryIds]);
	let dateFrom = $state(defaultDateFrom);
	let dateTo = $state(defaultDateTo);
	let selectedLocation = $state("All Regions");
	let priceFilter = $state<PriceFilter>("all");
	let selectedMaxPrice = $state(maxPrice);
	let selectedTags = $state<string[]>([]);
	let selectedEvent = $state<PublicEvent | null>(null);
	let showEventModal = $state(false);

	// ── Filter visibility settings state ────────────────────────────────────────
	let showCategory  = $state(data.filterSettings?.showCategory  ?? true);
	let showDateRange = $state(data.filterSettings?.showDateRange ?? true);
	let showLocation  = $state(data.filterSettings?.showLocation  ?? true);
	let showPrice     = $state(data.filterSettings?.showPrice     ?? true);
	let showTags      = $state(data.filterSettings?.showTags      ?? true);
	let isSundayHoliday = $state(data.filterSettings?.isSundayHoliday ?? true);
	let savingSettings = $state(false);
	let saveStatus = $state<"idle" | "saved" | "error">("idle");

	// Sync state when form action returns success
	$effect(() => {
		if (form?.success) {
			saveStatus = "saved";
			const timer = setTimeout(() => (saveStatus = "idle"), 3000);
			return () => clearTimeout(timer);
		} else if (form?.message && !form?.success) {
			saveStatus = "error";
		}
	});

	const selectedCategoriesSet = $derived(new Set(selectedCategoryIds));

	function buildCategoryDefinitions(events: PublicEvent[], idPrefix: string) {
		const categories = Array.from(new Set(events.map((event) => event.category || "Uncategorized"))).sort();
		return categories.map((category, index) => ({
			id: `${idPrefix}-${slugifyCategory(category)}`,
			label: category,
			eventCategory: category,
			color: colorForCategory(category, index)
		}));
	}

	function formatAsInputDate(date: Date): string {
		const yearValue = date.getFullYear();
		const monthValue = String(date.getMonth() + 1).padStart(2, "0");
		const dayValue = String(date.getDate()).padStart(2, "0");
		return yearValue + "-" + monthValue + "-" + dayValue;
	}

	function parseInputDate(value: string, endOfDay = false): Date | null {
		if (!value) return null;
		const [yearPart, monthPart, dayPart] = value.split("-").map(Number);
		if (!yearPart || !monthPart || !dayPart) return null;
		return new Date(yearPart, monthPart - 1, dayPart, endOfDay ? 23 : 0, endOfDay ? 59 : 0, endOfDay ? 59 : 0, endOfDay ? 999 : 0);
	}

	function eventDate(event: PublicEvent): Date {
		return new Date(event.date_ad);
	}

	function eventEndDate(event: PublicEvent): Date {
		return new Date(event.end_date_ad || event.date_ad);
	}

	function matchesDateRange(event: PublicEvent): boolean {
		const start = parseInputDate(dateFrom);
		const end = parseInputDate(dateTo, true);
		return eventOverlapsRange(event, start, end);
	}

	function matchesLocation(event: PublicEvent): boolean {
		return selectedLocation === "All Regions" || event.region === selectedLocation;
	}

	function matchesPrice(event: PublicEvent): boolean {
		if (priceFilter === "free" && event.price > 0) return false;
		if (priceFilter === "paid" && event.price <= 0) return false;
		return event.price <= selectedMaxPrice;
	}

	function matchesTag(event: PublicEvent): boolean {
		if (selectedTags.length === 0) return true;
		return selectedTags.some((tag) => event.tags.includes(tag));
	}

	function matchesCategory(event: PublicEvent): boolean {
		const defs = event.source === "festival" ? festivalCategoryDefinitions : eventCategoryDefinitions;
		const definition = defs.find((category) => category.eventCategory === event.category);
		if (!definition) return false;
		return selectedCategoriesSet.has(definition.id);
	}

	const filteredEventsWithoutCategory = $derived(adminEvents.filter((event) => matchesDateRange(event) && matchesLocation(event) && matchesPrice(event) && matchesTag(event)));
	const filteredEvents = $derived(filteredEventsWithoutCategory.filter((event) => matchesCategory(event)));

	const eventCategoryOptions = $derived<CategoryFilterOption[]>([
		{
			id: "all-event",
			label: "All Events",
			count: filteredEventsWithoutCategory.filter((event) => event.source !== "festival").length,
			color: "#c8102e",
			checked: eventCategoryDefinitions.every((d) => selectedCategoriesSet.has(d.id)),
			isAll: true
		},
		...eventCategoryDefinitions.map((definition) => ({
			id: definition.id,
			label: definition.label,
			color: definition.color,
			checked: selectedCategoriesSet.has(definition.id),
			count: filteredEventsWithoutCategory.filter(
				(event) => event.source !== "festival" && event.category === definition.eventCategory
			).length
		}))
	]);

	const festivalCategoryOptions = $derived<CategoryFilterOption[]>([
		{
			id: "all-festival",
			label: "All Festivals",
			count: filteredEventsWithoutCategory.filter((event) => event.source === "festival").length,
			color: "#c8102e",
			checked: festivalCategoryDefinitions.every((d) => selectedCategoriesSet.has(d.id)),
			isAll: true
		},
		...festivalCategoryDefinitions.map((definition) => ({
			id: definition.id,
			label: definition.label,
			color: definition.color,
			checked: selectedCategoriesSet.has(definition.id),
			count: filteredEventsWithoutCategory.filter(
				(event) => event.source === "festival" && event.category === definition.eventCategory
			).length
		}))
	]);

	const filteredEventsByCategoryLabel = $derived(categoryDefinitions.map((definition) => ({
		...definition,
		count: filteredEvents.filter((event) => event.category === definition.eventCategory).length
	})));

	function toggleCategory(categoryId: string) {
		if (categoryId === "all-event" || categoryId === "all-festival") {
			const groupIds = (categoryId === "all-event" ? eventCategoryDefinitions : festivalCategoryDefinitions).map((d) => d.id);
			const allSelected = groupIds.every((id) => selectedCategoryIds.includes(id));
			selectedCategoryIds = allSelected
				? selectedCategoryIds.filter((id) => !groupIds.includes(id))
				: Array.from(new Set([...selectedCategoryIds, ...groupIds]));
			return;
		}
		selectedCategoryIds = selectedCategoryIds.includes(categoryId) ? selectedCategoryIds.filter((item) => item !== categoryId) : [...selectedCategoryIds, categoryId];
	}

	function toggleTag(tag: string) {
		selectedTags = selectedTags.includes(tag) ? selectedTags.filter((item) => item !== tag) : [...selectedTags, tag];
	}

	function resetFilters() {
		selectedCategoryIds = [...allCategoryIds];
		dateFrom = defaultDateFrom;
		dateTo = defaultDateTo;
		selectedLocation = "All Regions";
		priceFilter = "all";
		selectedMaxPrice = maxPrice;
		selectedTags = [];
	}

	function openEventFromCalendar(event: CalendarEvent) {
		const match = adminEvents.find((item) => String(item.id) === String(event.id) && item.source === event.source);
		if (match) {
			selectedEvent = match;
			showEventModal = true;
		}
	}

	function closeEventDetails() {
		selectedEvent = null;
		showEventModal = false;
	}

	function handleKeydown(event: KeyboardEvent): void {
		if (event.key === "Escape") closeEventDetails();
	}

	// Reset visibility to all-on
	function resetVisibility() {
		showCategory = true;
		showDateRange = true;
		showLocation = true;
		showPrice = true;
		showTags = true;
		isSundayHoliday = true;
	}
</script>

<div class="calendar-setup-page" onkeydown={handleKeydown} role="main">
	<div class="page-header">
		<div class="header-info">
			<h1>Calendar Setup</h1>
			<p>Manage official events, promotions, and internal schedule planning.</p>
		</div>

		<div class="header-actions">
			<button onclick={() => goto("/admin/events/create")} class="btn btn-primary">
				<i class="fi fi-rr-plus"></i>
				<span>Add New Event</span>
			</button>
		</div>
	</div>

	<div class="setup-layout">
		<!-- Left sidebar: existing filters + new visibility panel -->
		<div class="sidebar-col">
			<CalendarFilters
				eventCategories={eventCategoryOptions}
				festivalCategories={festivalCategoryOptions}
				tags={tagOptions}
				{selectedTags}
				{dateFrom}
				{dateTo}
				locations={locationOptions}
				{selectedLocation}
				{maxPrice}
				{selectedMaxPrice}
				{priceFilter}
				showAdvisory={false}
				onToggleCategory={toggleCategory}
				onReset={resetFilters}
				onDateFromChange={(value) => (dateFrom = value)}
				onDateToChange={(value) => (dateTo = value)}
				onLocationChange={(value) => (selectedLocation = value)}
				onPriceFilterChange={(value) => (priceFilter = value)}
				onMaxPriceChange={(value) => (selectedMaxPrice = value)}
				onToggleTag={toggleTag}
			/>

			<!-- ── Public Filter Visibility panel ───────────────────────────── -->
			<div class="visibility-card">
				<div class="vc-header">
					<span class="vc-title">
						<i class="fi fi-rr-eye"></i>
						Public Filter Visibility
					</span>
					<span class="vc-subtitle">Controls which filters show on the public calendar</span>
				</div>

				<form
					method="POST"
					action="?/saveFilterSettings"
					use:enhance={() => {
						savingSettings = true;
						return async ({ update }) => {
							await update();
							savingSettings = false;
						};
					}}
				>
					<!-- Hidden inputs carry the boolean values -->
					<input type="hidden" name="showCategory"    value={String(showCategory)} />
					<input type="hidden" name="showDateRange"   value={String(showDateRange)} />
					<input type="hidden" name="showLocation"    value={String(showLocation)} />
					<input type="hidden" name="showPrice"       value={String(showPrice)} />
					<input type="hidden" name="showTags"        value={String(showTags)} />
					<input type="hidden" name="isSundayHoliday" value={String(isSundayHoliday)} />

					<div class="vc-toggles">
						<label class="vc-row">
							<div class="vc-row-left">
								<i class="fi fi-rr-apps"></i>
								<span>Categories</span>
							</div>
							<button
								type="button"
								class="toggle"
								class:on={showCategory}
								aria-pressed={showCategory}
								onclick={() => (showCategory = !showCategory)}
							>
								<span class="thumb"></span>
							</button>
						</label>

						<label class="vc-row">
							<div class="vc-row-left">
								<i class="fi fi-rr-calendar-day"></i>
								<span>Date Range</span>
							</div>
							<button
								type="button"
								class="toggle"
								class:on={showDateRange}
								aria-pressed={showDateRange}
								onclick={() => (showDateRange = !showDateRange)}
							>
								<span class="thumb"></span>
							</button>
						</label>

						<label class="vc-row">
							<div class="vc-row-left">
								<i class="fi fi-rr-marker"></i>
								<span>Location</span>
							</div>
							<button
								type="button"
								class="toggle"
								class:on={showLocation}
								aria-pressed={showLocation}
								onclick={() => (showLocation = !showLocation)}
							>
								<span class="thumb"></span>
							</button>
						</label>

						<label class="vc-row">
							<div class="vc-row-left">
								<i class="fi fi-rr-ticket"></i>
								<span>Price</span>
							</div>
							<button
								type="button"
								class="toggle"
								class:on={showPrice}
								aria-pressed={showPrice}
								onclick={() => (showPrice = !showPrice)}
							>
								<span class="thumb"></span>
							</button>
						</label>

						<label class="vc-row">
							<div class="vc-row-left">
								<i class="fi fi-rr-tags"></i>
								<span>Tags</span>
							</div>
							<button
								type="button"
								class="toggle"
								class:on={showTags}
								aria-pressed={showTags}
								onclick={() => (showTags = !showTags)}
							>
								<span class="thumb"></span>
							</button>
						</label>
					</div>

					<div class="vc-divider"></div>

					<div class="vc-toggles">
						<div class="vc-section-label">Weekly Holidays</div>

						<label class="vc-row">
							<div class="vc-row-left">
								<i class="fi fi-rr-calendar-exclamation"></i>
								<span>Saturday</span>
							</div>
							<button
								type="button"
								class="toggle on"
								aria-pressed="true"
								disabled
								title="Saturday is always a holiday"
							>
								<span class="thumb"></span>
							</button>
						</label>

						<label class="vc-row">
							<div class="vc-row-left">
								<i class="fi fi-rr-calendar-exclamation"></i>
								<span>Sunday</span>
							</div>
							<button
								type="button"
								class="toggle"
								class:on={isSundayHoliday}
								aria-pressed={isSundayHoliday}
								onclick={() => (isSundayHoliday = !isSundayHoliday)}
							>
								<span class="thumb"></span>
							</button>
						</label>
					</div>

					<div class="vc-footer">
						{#if saveStatus === "saved"}
							<span class="save-msg success">
								<i class="fi fi-rr-check"></i> Saved to public calendar
							</span>
						{:else if saveStatus === "error"}
							<span class="save-msg error">
								<i class="fi fi-rr-cross-small"></i> {form?.message ?? "Save failed"}
							</span>
						{/if}

						<div class="vc-actions">
							<button
								type="button"
								class="btn-reset-vis"
								onclick={resetVisibility}
							>Reset all on</button>
							<button
								type="submit"
								class="btn-save-vis"
								disabled={savingSettings}
							>
								{#if savingSettings}
									<i class="fi fi-rr-spinner"></i> Saving…
								{:else}
									<i class="fi fi-rr-cloud-upload"></i> Save
								{/if}
							</button>
						</div>
					</div>
				</form>
			</div>
		</div>

		<div class="main-column">
			{#if data.error}
				<div class="notice error">
					<strong>Unable to load events.</strong>
					<span>{data.error.message}</span>
				</div>
			{:else if adminEvents.length === 0}
				<div class="notice empty">
					<strong>No events scheduled yet.</strong>
					<span>Create an event to see it on the calendar.</span>
				</div>
			{/if}

			<div class="calendar-card">
				<Calendar
					events={filteredEvents}
					dateMode="AD"
					{isSundayHoliday}
					onEventClick={openEventFromCalendar}
				/>
			</div>

			<div class="legend-card">
				<h3>Calendar Legend</h3>
				<div class="legend-grid">
					{#each filteredEventsByCategoryLabel as category}
						<div class="legend-item">
							<span class="dot" style={`background:${category.color}`}></span>
							<span>{category.label}</span>
							<span class="count">{category.count}</span>
						</div>
					{/each}
				</div>
				<p class="summary">
					Showing {filteredEvents.length} of {adminEvents.length} events.
				</p>
			</div>
		</div>
	</div>
</div>

<EventDetailsModal
	open={showEventModal}
	event={selectedEvent}
	onClose={closeEventDetails}
/>

<style>
	.calendar-setup-page {
		display: flex;
		flex-direction: column;
		gap: 1.5rem;
	}

	.page-header {
		display: flex;
		justify-content: space-between;
		align-items: flex-end;
		gap: 1rem;
	}

	.header-info h1 {
		font-family: "Quicksand", sans-serif;
		font-size: 2rem;
		font-weight: 800;
		color: var(--color-dark);
		margin: 0;
	}

	.header-info p {
		margin-top: 0.35rem;
		color: #64748b;
	}

	.setup-layout {
		display: grid;
		grid-template-columns: 320px minmax(0, 1fr);
		gap: 1rem;
		align-items: start;
	}

	.sidebar-col {
		display: flex;
		flex-direction: column;
		gap: 1rem;
	}

	.main-column {
		min-width: 0;
		display: flex;
		flex-direction: column;
		gap: 1rem;
	}

	.calendar-card {
		background: white;
		padding: 1rem;
		border-radius: 16px;
		box-shadow: 0 4px 20px rgba(0, 0, 0, 0.05);
		border: 1px solid #e2e8f0;
	}

	.notice {
		display: flex;
		align-items: center;
		justify-content: space-between;
		gap: 1rem;
		border-radius: 12px;
		padding: 0.9rem 1rem;
		font-size: 0.92rem;
		border: 1px solid #dbe4ee;
		background: #f8fafc;
		color: #334155;
	}

	.notice strong { color: #10243e; }

	.notice.error {
		border-color: #fecdd3;
		background: #fff1f2;
		color: #9f1239;
	}

	.notice.error strong { color: #9f1239; }

	.legend-card {
		background: white;
		padding: 1rem;
		border-radius: 16px;
		border: 1px solid #e2e8f0;
		box-shadow: 0 2px 10px rgba(0, 0, 0, 0.02);
	}

	.legend-card h3 {
		font-size: 1rem;
		font-weight: 700;
		color: var(--color-dark);
		margin: 0 0 1rem 0;
	}

	.legend-grid {
		display: grid;
		grid-template-columns: repeat(auto-fit, minmax(220px, 1fr));
		gap: 0.75rem;
	}

	.legend-item {
		display: inline-flex;
		align-items: center;
		gap: 0.55rem;
		font-size: 0.9rem;
		color: #475569;
		font-weight: 600;
		border: 1px solid #e2e8f0;
		border-radius: 10px;
		padding: 0.5rem 0.7rem;
		background: #f8fafc;
	}

	.dot {
		width: 11px;
		height: 11px;
		border-radius: 50%;
	}

	.count {
		margin-left: auto;
		background: #e2e8f0;
		border-radius: 999px;
		padding: 0.15rem 0.45rem;
		font-size: 0.78rem;
		color: #334155;
		font-weight: 700;
	}

	.summary {
		margin-top: 0.9rem;
		color: #64748b;
		font-size: 0.9rem;
	}

	/* ── Visibility card ─────────────────────────────────────────────────────── */
	.visibility-card {
		background: white;
		border: 1px solid #e5e7eb;
		border-radius: 18px;
		overflow: hidden;
	}

	.vc-header {
		padding: 1rem 1.2rem 0.75rem;
		border-bottom: 1px solid #f1f5f9;
	}

	.vc-title {
		display: flex;
		align-items: center;
		gap: 0.5rem;
		font-size: 1rem;
		font-weight: 700;
		color: #1e293b;
	}

	.vc-title i {
		color: #c8102e;
		font-size: 0.95rem;
	}

	.vc-subtitle {
		display: block;
		margin-top: 0.2rem;
		font-size: 0.78rem;
		color: #94a3b8;
	}

	.vc-toggles {
		padding: 0.5rem 1.2rem;
		display: flex;
		flex-direction: column;
		gap: 0;
	}

	.vc-row {
		display: flex;
		align-items: center;
		justify-content: space-between;
		padding: 0.7rem 0;
		border-bottom: 1px solid #f8fafc;
		cursor: default;
	}

	.vc-row:last-child {
		border-bottom: none;
	}

	.vc-row-left {
		display: flex;
		align-items: center;
		gap: 0.6rem;
		font-size: 0.9rem;
		color: #334155;
		font-weight: 500;
	}

	.vc-row-left i {
		color: #94a3b8;
		font-size: 0.85rem;
		width: 16px;
		text-align: center;
	}

	.vc-divider {
		height: 1px;
		background: #f1f5f9;
		margin: 0 1.2rem;
	}

	.vc-section-label {
		padding: 0.75rem 0 0.35rem;
		font-size: 0.72rem;
		font-weight: 700;
		color: #94a3b8;
		text-transform: uppercase;
		letter-spacing: 0.06em;
	}

	/* Toggle switch */
	.toggle {
		width: 42px;
		height: 24px;
		border-radius: 999px;
		background: #e2e8f0;
		border: none;
		position: relative;
		cursor: pointer;
		transition: background 0.2s;
		flex-shrink: 0;
		padding: 0;
	}

	.toggle.on {
		background: #c8102e;
	}

	.toggle:disabled {
		cursor: not-allowed;
		opacity: 0.7;
	}

	.thumb {
		position: absolute;
		top: 3px;
		left: 3px;
		width: 18px;
		height: 18px;
		border-radius: 50%;
		background: white;
		box-shadow: 0 1px 4px rgba(0,0,0,0.2);
		transition: left 0.2s;
	}

	.toggle.on .thumb {
		left: 21px;
	}

	/* Footer */
	.vc-footer {
		padding: 0.85rem 1.2rem;
		border-top: 1px solid #f1f5f9;
		display: flex;
		flex-direction: column;
		gap: 0.6rem;
	}

	.save-msg {
		font-size: 0.82rem;
		font-weight: 600;
		display: flex;
		align-items: center;
		gap: 0.35rem;
	}

	.save-msg.success { color: #16a34a; }
	.save-msg.error   { color: #dc2626; }

	.vc-actions {
		display: flex;
		gap: 0.5rem;
	}

	.btn-reset-vis {
		flex: 1;
		min-height: 36px;
		background: #f8fafc;
		color: #64748b;
		border: 1px solid #e2e8f0;
		border-radius: 10px;
		font-size: 0.82rem;
		font-weight: 600;
		cursor: pointer;
		transition: all 0.15s;
	}

	.btn-reset-vis:hover {
		border-color: #94a3b8;
		color: #334155;
	}

	.btn-save-vis {
		flex: 1;
		min-height: 36px;
		background: #c8102e;
		color: white;
		border: none;
		border-radius: 10px;
		font-size: 0.85rem;
		font-weight: 700;
		cursor: pointer;
		display: inline-flex;
		align-items: center;
		justify-content: center;
		gap: 0.35rem;
		transition: background 0.15s;
	}

	.btn-save-vis:hover:not(:disabled) { background: #a50d25; }
	.btn-save-vis:disabled { opacity: 0.6; cursor: not-allowed; }

	@media (max-width: 1180px) {
		.setup-layout {
			grid-template-columns: 1fr;
		}
	}

	@media (max-width: 768px) {
		.page-header {
			flex-direction: column;
			align-items: flex-start;
		}

		.calendar-card {
			padding: 0.8rem;
		}
	}
</style>
