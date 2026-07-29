<script lang="ts">
	import EventDetailsModal from "$lib/components/EventDetailsModal.svelte";
	import RichTextEditor from "$lib/components/RichTextEditor.svelte";
	import CategoryAutocomplete from "$lib/components/CategoryAutocomplete.svelte";
	import LocationPickerMap from "$lib/components/LocationPickerMap.svelte";
	import { adToBS, bsToAD } from "$lib/components/calendar/dateUtils";
	import { shellTitleTemplate, wrapEmailShell } from "$lib/utils/email-shell";
	import type { PublicEvent } from "$lib/components/public/eventTypes";
	import type { EventType } from "$lib/types/events";
	import type { ActionData, PageData } from "./$types";
	import { SELF_REGISTRATION_FIELD_SECTIONS, ALL_TOGGLEABLE_FIELD_KEYS } from "$lib/config/selfRegistrationFields";

	const INVITATION_EMAIL_PLACEHOLDERS = [
		"FullName", "EventTitle", "EventSummary", "EventDate", "EventLocation", "InviteUrl", "ExpiryLine", "QrCodeImage",
	] as const;

	const FALLBACK_HERO_IMAGE =
		"https://images.unsplash.com/photo-1516450360452-9312f5463805?auto=format&fit=crop&w=1400&q=80";
	const FALLBACK_MAP_IMAGE =
		"https://staticmap.openstreetmap.de/staticmap.php?center=27.7172,85.3240&zoom=13&size=1200x500&markers=27.7172,85.3240,red-pushpin";

	const STATUS_OPTIONS_FULL = [
		{ value: "draft", label: "Draft" },
		{ value: "published", label: "Published" },
		{ value: "archived", label: "Archived" },
	] as const;

	const STATUS_OPTIONS_APPROVAL = [
		{ value: "draft", label: "Save as Draft" },
		{ value: "published", label: "Request to Approve" },
	] as const;

	const ENTRY_OPTIONS = ["Free Entry", "Paid Entry"] as const;

	const TONE_OPTIONS = ["orange", "blue", "purple", "green", "red"] as const;

	type HighlightInput = {
		icon: string;
		title: string;
		description: string;
		tone: string;
	};

	type FormValues = PageData["defaults"] & { endDateBs?: string };

	let { data, form } = $props<{ data: PageData; form?: ActionData }>();
	const isEditing = $derived(Boolean(data.isEditing));
	const formAction = $derived(
		isEditing && data.eventId
			? `?/update&edit=${data.eventId}`
			: "?/create",
	);

	const initialValues: FormValues = $derived({
		...data.defaults,
		...(form?.values ?? {}),
	});
	const initialHighlights = $derived(
		initialValues.highlights?.length
			? initialValues.highlights
			: data.defaults.highlights,
	);

	let values = $state<FormValues>({
		...initialValues,
	});
	let highlights = $state<HighlightInput[]>(
		initialHighlights.map((highlight: HighlightInput) => ({
			icon: highlight.icon,
			title: highlight.title,
			description: highlight.description,
			tone: highlight.tone || "orange",
		})),
	);
	let showPreview = $state(false);

	// Date helper functions
	function convertAdToBs(adDateStr: string): string {
		if (!adDateStr) return "";
		try {
			const d = new Date(adDateStr);
			if (Number.isNaN(d.getTime())) return "";
			const bs = adToBS(d);
			return `${bs.year}-${String(bs.month).padStart(2, "0")}-${String(bs.day).padStart(2, "0")}`;
		} catch (e) {
			console.error(e);
			return "";
		}
	}

	function convertBsToAd(bsDateStr: string): string {
		if (!bsDateStr) return "";
		try {
			const parts = bsDateStr.split("-");
			if (parts.length !== 3) return "";
			const year = parseInt(parts[0], 10);
			const month = parseInt(parts[1], 10); // 1-indexed
			const day = parseInt(parts[2], 10);
			if (Number.isNaN(year) || Number.isNaN(month) || Number.isNaN(day))
				return "";

			const jsDate = bsToAD(year, month, day);
			if (Number.isNaN(jsDate.getTime())) return "";

			const y = jsDate.getFullYear();
			const m = String(jsDate.getMonth() + 1).padStart(2, "0");
			const d = String(jsDate.getDate()).padStart(2, "0");
			return `${y}-${m}-${d}`;
		} catch (e) {
			console.error(e);
			return "";
		}
	}

	// Bidirectional date synchronization
	$effect(() => {
		if (values.dateAd) {
			const calculatedBs = convertAdToBs(values.dateAd);
			if (calculatedBs && values.dateBs !== calculatedBs) {
				values.dateBs = calculatedBs;
			}
		}
	});

	$effect(() => {
		if (values.dateBs) {
			const calculatedAd = convertBsToAd(values.dateBs);
			if (calculatedAd && values.dateAd !== calculatedAd) {
				values.dateAd = calculatedAd;
			}
		}
	});

	$effect(() => {
		if (values.endDateAd) {
			const calculatedBs = convertAdToBs(values.endDateAd);
			if (calculatedBs && values.endDateBs !== calculatedBs) {
				values.endDateBs = calculatedBs;
			}
		} else {
			values.endDateBs = "";
		}
	});

	$effect(() => {
		if (values.endDateBs) {
			const calculatedAd = convertBsToAd(values.endDateBs);
			if (calculatedAd && values.endDateAd !== calculatedAd) {
				values.endDateAd = calculatedAd;
			}
		} else {
			values.endDateAd = "";
		}
	});

	// Image uploads state
	let uploadedImages = $state<string[]>([]);
	let isUploading = $state(false);
	let uploadError = $state("");

	$effect(() => {
		if (values.imageUrls && uploadedImages.length === 0) {
			uploadedImages = values.imageUrls
				.split(/\r?\n|,/)
				.map((s: string) => s.trim())
				.filter(Boolean);
		}
	});

	$effect(() => {
		values.imageUrls = uploadedImages.join("\n");
	});

	async function handleFileChange(event: Event) {
		const input = event.target as HTMLInputElement;
		if (!input.files || input.files.length === 0) return;

		isUploading = true;
		uploadError = "";

		const files = Array.from(input.files);
		for (const file of files) {
			const formData = new FormData();
			formData.append("file", file);

			try {
				const response = await fetch("/api/upload", {
					method: "POST",
					body: formData,
				});

				if (!response.ok) {
					const errData = await response.json();
					throw new Error(errData.message || "Upload failed");
				}

				const result = await response.json();
				if (result.isSuccess && result.data) {
					uploadedImages = [...uploadedImages, result.data];
				}
			} catch (err: any) {
				console.error(err);
				uploadError = `Failed to upload: ${err.message}`;
			}
		}

		isUploading = false;
		input.value = "";
	}

	function removeUploadedImage(index: number) {
		uploadedImages = uploadedImages.filter((_, i) => i !== index);
	}

	// Organizer image (logo/photo) upload state
	let isUploadingOrganizerImage = $state(false);
	let organizerImageError = $state("");

	async function handleOrganizerImageChange(event: Event) {
		const input = event.target as HTMLInputElement;
		if (!input.files || input.files.length === 0) return;

		isUploadingOrganizerImage = true;
		organizerImageError = "";

		const formData = new FormData();
		formData.append("file", input.files[0]);

		try {
			const response = await fetch("/api/upload", {
				method: "POST",
				body: formData,
			});

			if (!response.ok) {
				const errData = await response.json();
				throw new Error(errData.message || "Upload failed");
			}

			const result = await response.json();
			if (result.isSuccess && result.data) {
				values.organizerImageUrl = result.data;
			}
		} catch (err: any) {
			console.error(err);
			organizerImageError = `Failed to upload: ${err.message}`;
		}

		isUploadingOrganizerImage = false;
		input.value = "";
	}

	function removeOrganizerImage() {
		values.organizerImageUrl = "";
	}

	function addHighlight() {
		highlights = [
			...highlights,
			{ icon: "", title: "", description: "", tone: "orange" },
		];
	}

	function removeHighlight(index: number) {
		if (highlights.length <= 1) {
			highlights = [
				{ icon: "", title: "", description: "", tone: "orange" },
			];
			return;
		}

		highlights = highlights.filter(
			(_, currentIndex) => currentIndex !== index,
		);
	}

	const registrationLink = $derived(
		values.requiresRegistration && values.slug && typeof window !== "undefined"
			? `${window.location.origin}/register/${values.slug}`
			: "",
	);

	let linkCopied = $state(false);

	function copyRegistrationLink() {
		if (!registrationLink) return;
		navigator.clipboard.writeText(registrationLink).then(() => {
			linkCopied = true;
			setTimeout(() => (linkCopied = false), 2000);
		});
	}

	function setRequiresRegistration(checked: boolean) {
		values.requiresRegistration = checked;
		if (checked) values.requiresInvitation = false;
	}

	function setRequiresInvitation(checked: boolean) {
		values.requiresInvitation = checked;
		if (checked) values.requiresRegistration = false;
	}

	// ── Self-registration field picker ───────────────────────────────────────
	function toggleField(key: string, checked: boolean) {
		const set = new Set(values.selfRegistrationFields);
		if (checked) set.add(key);
		else set.delete(key);
		values.selfRegistrationFields = Array.from(set);
	}

	function isSectionEnabled(section: (typeof SELF_REGISTRATION_FIELD_SECTIONS)[number]): boolean {
		const keys = section.fields.filter((f) => !f.locked).map((f) => f.key);
		return keys.length > 0 && keys.every((key) => values.selfRegistrationFields.includes(key));
	}

	function toggleSection(section: (typeof SELF_REGISTRATION_FIELD_SECTIONS)[number], checked: boolean) {
		const keys = section.fields.filter((f) => !f.locked).map((f) => f.key);
		const set = new Set(values.selfRegistrationFields);
		for (const key of keys) {
			if (checked) set.add(key);
			else set.delete(key);
		}
		values.selfRegistrationFields = Array.from(set);
	}

	const allFieldsEnabled = $derived(
		ALL_TOGGLEABLE_FIELD_KEYS.every((key) => values.selfRegistrationFields.includes(key)),
	);

	function toggleAllFields(checked: boolean) {
		values.selfRegistrationFields = checked ? [...ALL_TOGGLEABLE_FIELD_KEYS] : [];
	}

	// ── Per-event custom invitation email preview ────────────────────────────
	const invitationPreviewSample = $derived<Record<string, string>>({
		FullName: "Jane Doe",
		EventTitle: values.title || "Your Event Title",
		EventSummary: values.summary || "Event summary goes here.",
		EventDate: values.dateAd
			? new Date(values.dateAd).toLocaleDateString("en-US", { weekday: "long", year: "numeric", month: "long", day: "numeric" })
			: "Saturday, July 12, 2026",
		EventLocation: values.region || "Kathmandu",
		InviteUrl: "https://example.com/invite/abc123",
		ExpiryLine: "",
		QrCodeImage:
			'<div style="width:220px;height:220px;margin:0 auto;border-radius:8px;background:#eef2f7;display:flex;align-items:center;justify-content:center;color:#94a3b8;font-size:12px;font-family:sans-serif;">QR CODE</div>',
	});

	function substituteInvitationPreview(html: string): string {
		let result = html;
		for (const [key, value] of Object.entries(invitationPreviewSample)) {
			result = result.split(`{{${key}}}`).join(value);
		}
		return result.replace(/{{\s*[A-Za-z]+\s*}}/g, "");
	}

	const invitationPreviewHtml = $derived(
		wrapEmailShell(
			"EventInvitation",
			substituteInvitationPreview(shellTitleTemplate("EventInvitation")),
			substituteInvitationPreview(values.invitationEmailBodyHtml ?? ""),
		),
	);

	let copiedInvitationToken = $state("");

	function copyInvitationPlaceholder(token: string) {
		navigator.clipboard.writeText(`{{${token}}}`).then(() => {
			copiedInvitationToken = token;
			setTimeout(() => (copiedInvitationToken = ""), 1500);
		});
	}

	function parseList(input: string): string[] {
		if (!input) return [];

		return input
			.split(/\r?\n|,/)
			.map((value) => value.trim())
			.filter(Boolean);
	}

	let suggestingTags = $state(false);
	let tagSuggestError = $state("");

	async function suggestTagsForEvent() {
		tagSuggestError = "";
		suggestingTags = true;
		try {
			const res = await fetch("/api/events/suggest-tags", {
				method: "POST",
				headers: { "Content-Type": "application/json" },
				body: JSON.stringify({ title: values.title, description: values.longDescription })
			});
			if (!res.ok) {
				tagSuggestError = "Couldn't fetch tag suggestions.";
				return;
			}
			const suggestions: { tag: string }[] = await res.json();
			const existing = parseList(values.tags);
			const merged = [...existing];
			for (const { tag } of suggestions) {
				if (!merged.some((t) => t.toLowerCase() === tag.toLowerCase())) {
					merged.push(tag);
				}
			}
			values.tags = merged.join("\n");
		} catch {
			tagSuggestError = "Couldn't fetch tag suggestions.";
		} finally {
			suggestingTags = false;
		}
	}

	function parseNumber(value: string, fallback = 0): number {
		const numeric = Number(value);
		return Number.isFinite(numeric) ? numeric : fallback;
	}

	function slugify(source: string, customSlug: string): string {
		const raw = customSlug.trim() || source.trim();
		if (!raw) {
			return "event-preview";
		}

		return raw
			.toLowerCase()
			.replace(/[^a-z0-9]+/g, "-")
			.replace(/^-+|-+$/g, "");
	}

	function formatDateRangeLabel(
		startDateRaw: string,
		endDateRaw: string,
	): string {
		if (!startDateRaw) {
			return "Date to be announced";
		}

		const startDate = new Date(`${startDateRaw}T00:00:00`);
		if (Number.isNaN(startDate.getTime())) {
			return "Date to be announced";
		}

		const endDate = endDateRaw
			? new Date(`${endDateRaw}T00:00:00`)
			: startDate;
		if (Number.isNaN(endDate.getTime())) {
			return startDate.toLocaleDateString("en-US", {
				month: "short",
				day: "numeric",
				year: "numeric",
			});
		}

		const monthFormatter = new Intl.DateTimeFormat("en-US", {
			month: "short",
		});
		if (startDate.toDateString() === endDate.toDateString()) {
			return `${monthFormatter.format(startDate)} ${startDate.getDate()}, ${startDate.getFullYear()}`;
		}

		if (
			startDate.getFullYear() === endDate.getFullYear() &&
			startDate.getMonth() === endDate.getMonth()
		) {
			return `${monthFormatter.format(startDate)} ${startDate.getDate()} - ${endDate.getDate()}, ${startDate.getFullYear()}`;
		}

		if (startDate.getFullYear() === endDate.getFullYear()) {
			return `${monthFormatter.format(startDate)} ${startDate.getDate()} - ${monthFormatter.format(endDate)} ${endDate.getDate()}, ${startDate.getFullYear()}`;
		}

		return `${startDate.toLocaleDateString("en-US", {
			month: "short",
			day: "numeric",
			year: "numeric",
		})} - ${endDate.toLocaleDateString("en-US", {
			month: "short",
			day: "numeric",
			year: "numeric",
		})}`;
	}

	function buildDurationLabel(
		startDateRaw: string,
		endDateRaw: string,
	): string {
		if (!startDateRaw) {
			return "Duration to be announced";
		}

		const start = new Date(`${startDateRaw}T00:00:00`);
		const end = endDateRaw ? new Date(`${endDateRaw}T00:00:00`) : start;
		if (Number.isNaN(start.getTime()) || Number.isNaN(end.getTime())) {
			return "Duration to be announced";
		}

		const days = Math.max(
			1,
			Math.floor(
				(end.getTime() - start.getTime()) / (1000 * 60 * 60 * 24),
			) + 1,
		);
		if (days === 1) {
			return "1 day event";
		}

		return `${days} day program`;
	}

	function buildReadTimeLabel(content: string): string {
		const words = content
			.split(/\s+/)
			.map((value) => value.trim())
			.filter(Boolean).length;
		const minutes = Math.max(1, Math.ceil(words / 220));
		return `${minutes} min read`;
	}

	$effect(() => {
		values.durationLabel = buildDurationLabel(
			values.dateAd,
			values.endDateAd,
		);
	});

	$effect(() => {
		values.dateRangeLabel = formatDateRangeLabel(
			values.dateAd,
			values.endDateAd,
		);
	});

	const previewEvent = $derived<PublicEvent>({
		id: 0,
		slug: slugify(values.title, values.slug),
		title: values.title || "Event title preview",
		date_ad: values.dateAd || new Date().toISOString().slice(0, 10),
		date_bs: values.dateBs || "2083-01-01",
		category: values.category || "Festival",
		type: values.type,
		color: values.color || "#0f766e",
		summary: values.summary || "Short event summary will appear here.",
		longDescription:
			values.longDescription ||
			"Add a full event description to preview the rich content shown in the event details modal.",
		location: values.region || "Location to be announced",
		region: values.region || "Region",
		address: values.address || "Address to be announced",
		dateRangeLabel:
			values.dateRangeLabel ||
			formatDateRangeLabel(values.dateAd, values.endDateAd),
		durationLabel:
			values.durationLabel ||
			buildDurationLabel(values.dateAd, values.endDateAd),
		attendanceLabel: values.attendanceLabel || "Attendance details",
		attendanceNote: values.attendanceNote || "Additional attendance notes",
		entryType: values.entryType,
		showEntryType: values.showEntryType,
		price: parseNumber(values.price),
		rating: 0,
		reviewsLabel: "0",
		tags: parseList(values.tags),
		image: (() => {
			const parsed = parseList(values.imageUrls);
			return parsed.length ? parsed : [FALLBACK_HERO_IMAGE];
		})(),
		mapImage: values.mapImage || FALLBACK_MAP_IMAGE,
		organizer: values.organizer || "Organizer",
		organizerSubtitle: values.organizerSubtitle || "Organizer subtitle",
		organizerVerified: values.organizerVerified,
		organizerImageUrl: values.organizerImageUrl || undefined,
		highlights:
			highlights
				.filter((highlight) => highlight.title || highlight.description)
				.map((highlight) => ({
					icon: highlight.icon || "fi fi-rr-star",
					title: highlight.title || "Highlight title",
					description:
						highlight.description || "Highlight description",
					tone: highlight.tone as
						| "orange"
						| "blue"
						| "purple"
						| "green"
						| "red",
				})) || [],
		featured: values.featured,
		readTime: values.readTime || buildReadTimeLabel(values.longDescription),
	});
</script>

<section class="create-shell animate-fade-in">
	<div class="page-intro">
		<div>
			<p class="eyebrow">Events Management</p>
			<h1>{isEditing ? "Edit Event" : "Create Event"}</h1>
			<p class="lede">
				{isEditing
					? "Update this event and confirm the public details in the live preview before saving."
					: "Fill every field used by the public event model. The preview uses the same event details modal, so you can confirm content exactly before publishing."}
			</p>
		</div>
		<div class="intro-actions">
			{#if isEditing && data.eventId}
				<a class="invite-link" href={`/admin/events/invitations/${data.eventId}`}>
					<i class="fi fi-rr-envelope"></i> Invite guests
				</a>
			{/if}
			<button
				class="preview-toggle"
				type="button"
				onclick={() => (showPreview = !showPreview)}
			>
				{showPreview ? "Hide Preview" : "Show Live Preview"}
			</button>
		</div>
	</div>

	{#if form?.message}
		<div class="notice" class:notice-success={form?.success} class:notice-approval={form?.requiresApproval}>
			{#if form?.requiresApproval}<i class="fi fi-rr-clock"></i>{/if}
			{form.message}
		</div>
	{/if}

	<div class="content-grid">
		<form method="POST" action={formAction} class="event-form">
			<section class="panel">
				<h2>Core Details</h2>
				<div class="field-grid two-up">
					<label>
						<span>Title *</span>
						<input
							name="title"
							bind:value={values.title}
							required
						/>
					</label>
					<label>
						<span>Slug (optional)</span>
						<input
							name="slug"
							bind:value={values.slug}
							placeholder="dashain-begins"
						/>
					</label>
				</div>

				<label>
					<span>Summary *</span>
					<textarea
						name="summary"
						bind:value={values.summary}
						rows="2"
						required
					></textarea>
				</label>

				<label>
					<span>Long Description *</span>
					<input
						type="hidden"
						name="longDescription"
						bind:value={values.longDescription}
					/>
					<RichTextEditor
						bind:value={values.longDescription}
						placeholder="Describe the event details..."
					/>
				</label>
			</section>

			<section class="panel">
				<h2>Classification & Timeline</h2>
				<div class="field-grid two-up">
					<label>
						<span>Category *</span>
						<input
							type="hidden"
							name="category"
							bind:value={values.category}
						/>
						<CategoryAutocomplete
							bind:value={values.category}
							placeholder="Select categories..."
							type="event"
						/>
					</label>
					<label>
						<span>Status</span>
						<select name="status" bind:value={values.status}>
							{#each (data.needsApproval ? STATUS_OPTIONS_APPROVAL : STATUS_OPTIONS_FULL) as option}
								<option value={option.value}>{option.label}</option>
							{/each}
						</select>
						{#if data.needsApproval && values.status === "published"}
							<span class="approval-hint">
								<i class="fi fi-rr-info"></i> This event will be sent to an approver before going live.
							</span>
						{/if}
					</label>
				</div>

				<div class="field-grid">
					<label>
						<span>Tags</span>
						<textarea
							name="tags"
							bind:value={values.tags}
							rows="2"
							placeholder="One tag per line, or comma-separated"
						></textarea>
						<button
							type="button"
							class="suggest-tags-btn"
							onclick={suggestTagsForEvent}
							disabled={suggestingTags || (!values.title && !values.longDescription)}
						>
							<i class="fi fi-rr-sparkles"></i>
							{suggestingTags ? "Suggesting…" : "Suggest tags"}
						</button>
						{#if tagSuggestError}
							<span class="field-error">{tagSuggestError}</span>
						{/if}
					</label>
				</div>

				<div class="field-grid two-up">
					<label>
						<span>Start Date (AD) *</span>
						<input
							type="date"
							name="dateAd"
							bind:value={values.dateAd}
							required
						/>
					</label>
					<label>
						<span>Start Date (BS) *</span>
						<input
							name="dateBs"
							bind:value={values.dateBs}
							required
							placeholder="2083-02-20"
						/>
					</label>
				</div>

				<div class="field-grid two-up">
					<label>
						<span>End Date (AD)</span>
						<input
							type="date"
							name="endDateAd"
							bind:value={values.endDateAd}
							min={values.dateAd || undefined}
						/>
						{#if values.endDateAd && values.dateAd && values.endDateAd < values.dateAd}
							<span class="field-error">End date cannot be earlier than the start date.</span>
						{/if}
					</label>
					<label>
						<span>End Date (BS)</span>
						<input
							name="endDateBs"
							bind:value={values.endDateBs}
							placeholder="2083-02-21"
						/>
					</label>
				</div>

				<div class="field-grid two-up">
					<label>
						<span>Date Range Label (Auto)</span>
						<input
							name="dateRangeLabel"
							bind:value={values.dateRangeLabel}
							readonly
							style="background-color: #f1f5f9; cursor: not-allowed;"
							placeholder="Calculated automatically"
						/>
					</label>
				</div>

				<input type="hidden" name="type" bind:value={values.type} />
				<div class="holiday-toggle-field">
					<span class="holiday-field-label"><i class="fi fi-rr-calendar-star"></i> Public Holiday</span>
					<label class="holiday-toggle-switch">
						<input
							type="checkbox"
							checked={values.type === "holiday"}
							onchange={(e) => { values.type = e.currentTarget.checked ? "holiday" : "event"; }}
						/>
						<span class="holiday-toggle-track">
							<span class="holiday-toggle-thumb"></span>
						</span>
						<span class="holiday-toggle-label">
							{values.type === "holiday" ? "Yes — shown in red on calendar" : "No — normal calendar display"}
						</span>
					</label>
				</div>
			</section>

			<section class="panel">
				<h2>Registration & Invitation</h2>
				<p class="panel-hint">
					Choose at most one: let visitors self-register via a shareable link, or
					restrict entry to admin-issued invitations. Leave both unchecked for a
					purely informational event.
				</p>
				<div class="toggle-group">
					<label class="checkbox">
						<input
							name="requiresRegistration"
							type="checkbox"
							checked={values.requiresRegistration}
							onchange={(e) => setRequiresRegistration(e.currentTarget.checked)}
						/>
						<span>Registration required (self-register)</span>
					</label>
					<label class="checkbox">
						<input
							name="requiresInvitation"
							type="checkbox"
							checked={values.requiresInvitation}
							onchange={(e) => setRequiresInvitation(e.currentTarget.checked)}
						/>
						<span>Invitation required (admin invites only)</span>
					</label>
				</div>

				{#if values.requiresRegistration}
					<div class="share-link-box">
						{#if registrationLink}
							<span>Share this link so visitors can register:</span>
							<div class="share-link-row">
								<input readonly value={registrationLink} onclick={(e) => e.currentTarget.select()} />
								<button type="button" class="secondary-btn" onclick={copyRegistrationLink}>
									{linkCopied ? "Copied!" : "Copy link"}
								</button>
							</div>
						{:else}
							<span>Save the event with a slug to get its shareable registration link.</span>
						{/if}
					</div>

					<div class="self-reg-fields">
						<div class="self-reg-fields-header">
							<h3>Self-Registration Fields</h3>
							<label class="checkbox enable-all">
								<input
									type="checkbox"
									checked={allFieldsEnabled}
									onchange={(e) => toggleAllFields(e.currentTarget.checked)}
								/>
								<span>Enable all fields</span>
							</label>
						</div>
						<p class="panel-hint">
							Choose which fields visitors fill out on this event's public registration
							form. Full Name and Email are always collected.
						</p>

						{#each SELF_REGISTRATION_FIELD_SECTIONS as section (section.id)}
							<div class="self-reg-section">
								<div class="self-reg-section-header">
									<strong>{section.title}</strong>
									{#if section.fields.some((f) => !f.locked)}
										<label class="checkbox small">
											<input
												type="checkbox"
												checked={isSectionEnabled(section)}
												onchange={(e) => toggleSection(section, e.currentTarget.checked)}
											/>
											<span>Enable all</span>
										</label>
									{/if}
								</div>
								<div class="self-reg-field-grid">
									{#each section.fields as field (field.key)}
										<label class="checkbox field-toggle" class:locked={field.locked}>
											<input
												type="checkbox"
												name="selfRegistrationFields"
												value={field.key}
												checked={field.locked || values.selfRegistrationFields.includes(field.key)}
												disabled={field.locked}
												onchange={(e) => toggleField(field.key, e.currentTarget.checked)}
											/>
											<span>{field.label}{field.required ? " *" : ""}{field.locked ? " (always included)" : ""}</span>
										</label>
									{/each}
								</div>
							</div>
						{/each}
					</div>
				{/if}
			</section>

			{#if values.requiresInvitation}
				<section class="panel">
					<h2>Custom Invitation Email</h2>
					<p class="panel-hint">
						Optional. Leave blank to use the global "Event Invitation" template
						(edit it under <a href="/admin/email-templates" target="_blank">Email Templates</a>).
						Fill these in to send a subject/body unique to this event instead.
					</p>

					<label>
						<span>Subject override</span>
						<input
							name="invitationEmailSubject"
							bind:value={values.invitationEmailSubject}
							placeholder="You're invited: {values.title || 'Event Title'}"
						/>
					</label>

					<label class="invitation-body-label">
						<span>Body override</span>
						<input type="hidden" name="invitationEmailBodyHtml" bind:value={values.invitationEmailBodyHtml} />
						<RichTextEditor
							bind:value={values.invitationEmailBodyHtml}
							placeholder="Leave blank to use the global default invitation email..."
						/>
					</label>

					<div class="placeholder-chips">
						{#each INVITATION_EMAIL_PLACEHOLDERS as token}
							<button type="button" class="chip" onclick={() => copyInvitationPlaceholder(token)}>
								{`{{${token}}}`}
								{#if copiedInvitationToken === token}<span class="copied">Copied!</span>{/if}
							</button>
						{/each}
					</div>

					{#if values.invitationEmailBodyHtml?.trim()}
						<div class="invitation-preview-label">
							<span>Preview (sample data)</span>
							<div class="invitation-preview-frame">
								<iframe title="Invitation email preview" srcdoc={invitationPreviewHtml}></iframe>
							</div>
						</div>
					{/if}
				</section>
			{/if}

			<section class="panel">
				<h2>Location & Attendance</h2>
				<div class="field-grid two-up">
					<label>
						<span>Region *</span>
						<input
							name="region"
							bind:value={values.region}
							required
							placeholder="Kathmandu Valley"
						/>
					</label>
					<label>
						<span>Address</span>
						<input
							name="address"
							bind:value={values.address}
							placeholder="Basantapur, Kathmandu"
						/>
					</label>
				</div>

				<div class="field-grid two-up">
					<label>
						<span>Duration Label (Auto)</span>
						<input
							name="durationLabel"
							bind:value={values.durationLabel}
							readonly
							style="background-color: #f1f5f9; cursor: not-allowed;"
							placeholder="Calculated automatically"
						/>
					</label>
					<label>
						<span>Attendance Label</span>
						<input
							name="attendanceLabel"
							bind:value={values.attendanceLabel}
							placeholder="Thousands attend"
						/>
					</label>
				</div>

				<label>
					<span>Attendance Note</span>
					<input
						type="hidden"
						name="attendanceNote"
						bind:value={values.attendanceNote}
					/>
					<RichTextEditor
						bind:value={values.attendanceNote}
						placeholder="Describe attendance notes..."
					/>
				</label>
			</section>

			<section class="panel">
				<h2>Pricing & Metrics</h2>
				<div class="toggle-group" style="margin-bottom: 4px;">
					<label class="checkbox">
						<input
							name="showEntryType"
							type="checkbox"
							checked={values.showEntryType}
							onchange={(e) => { values.showEntryType = e.currentTarget.checked; }}
						/>
						<span>Show entry type publicly (Free / Paid badge)</span>
					</label>
				</div>
				<div class="field-grid four-up">
					<label>
						<span>Entry Type</span>
						<select name="entryType" bind:value={values.entryType} disabled={!values.showEntryType}>
							{#each ENTRY_OPTIONS as option}
								<option value={option}>{option}</option>
							{/each}
						</select>
					</label>
					<label>
						<span>Price</span>
						<input
							name="price"
							type="number"
							step="0.01"
							min="0"
							bind:value={values.price}
						/>
					</label>
				</div>
			</section>

			<section class="panel">
				<h2>Media & Organizer</h2>

				<div class="image-upload-wrapper">
					<input
						type="hidden"
						name="imageUrls"
						bind:value={values.imageUrls}
					/>

					<div class="uploader-header">
						<span>Upload Event Images *</span>
						<label class="upload-trigger-btn">
							<i class="fi fi-rr-upload"></i> Upload Images
							<input
								type="file"
								multiple
								accept="image/*"
								onchange={handleFileChange}
								style="display: none;"
							/>
						</label>
					</div>

					{#if isUploading}
						<div class="upload-progress-info">
							<span class="spinner-icon"></span> Uploading...
						</div>
					{/if}

					{#if uploadError}
						<div class="upload-error-message">{uploadError}</div>
					{/if}

					<div class="uploaded-image-preview-grid">
						{#each uploadedImages as img, idx}
							<div class="uploaded-image-preview-card">
								<img src={img} alt="Event preview" />
								<button
									type="button"
									class="remove-uploaded-image-btn"
									onclick={() => removeUploadedImage(idx)}
									aria-label="Remove image"
								>
									&times;
								</button>
							</div>
						{/each}
					</div>
				</div>

				<div class="map-field-wrapper">
					<input type="hidden" name="latitude" value={values.latitude ?? ""} />
					<input type="hidden" name="longitude" value={values.longitude ?? ""} />
					<span>Event Location Pin</span>
					<LocationPickerMap
						bind:latitude={values.latitude}
						bind:longitude={values.longitude}
						bind:address={values.address}
					/>
				</div>

				<div class="field-grid two-up" style="margin-top: 12px;">
					<label>
						<span>Organizer *</span>
						<input
							name="organizer"
							bind:value={values.organizer}
							required
						/>
					</label>
					<label>
						<span>Organizer Subtitle</span>
						<input
							name="organizerSubtitle"
							bind:value={values.organizerSubtitle}
						/>
					</label>
				</div>

				<div class="organizer-image-wrapper">
					<input
						type="hidden"
						name="organizerImageUrl"
						bind:value={values.organizerImageUrl}
					/>

					<div class="uploader-header">
						<span>Organizer Image / Logo</span>
						<label class="upload-trigger-btn">
							<i class="fi fi-rr-upload"></i> Upload Image
							<input
								type="file"
								accept="image/*"
								onchange={handleOrganizerImageChange}
								style="display: none;"
							/>
						</label>
					</div>

					{#if isUploadingOrganizerImage}
						<div class="upload-progress-info">
							<span class="spinner-icon"></span> Uploading...
						</div>
					{/if}

					{#if organizerImageError}
						<div class="upload-error-message">{organizerImageError}</div>
					{/if}

					{#if values.organizerImageUrl}
						<div class="uploaded-image-preview-grid">
							<div class="uploaded-image-preview-card organizer-logo-preview">
								<img src={values.organizerImageUrl} alt="Organizer" />
								<button
									type="button"
									class="remove-uploaded-image-btn"
									onclick={removeOrganizerImage}
									aria-label="Remove organizer image"
								>
									&times;
								</button>
							</div>
						</div>
					{/if}
				</div>

				<div class="field-grid two-up">
					<label>
						<span>Read Time Label</span>
						<input
							name="readTime"
							bind:value={values.readTime}
							placeholder="4 min read"
						/>
					</label>
					<div class="toggle-group">
						<label class="checkbox">
							<input
								name="organizerVerified"
								type="checkbox"
								bind:checked={values.organizerVerified}
							/>
							<span>Organizer verified</span>
						</label>
						<label class="checkbox">
							<input
								name="featured"
								type="checkbox"
								bind:checked={values.featured}
							/>
							<span>Featured event</span>
						</label>
					</div>
				</div>
			</section>

			<section class="panel">
				<div class="highlights-head">
					<h2>Highlights</h2>
					<button
						type="button"
						class="secondary-btn"
						onclick={addHighlight}>Add highlight</button
					>
				</div>

				<div class="highlights-list">
					{#each highlights as highlight, index}
						<article class="highlight-row">
							<div class="field-grid four-up">
								<label>
									<span>Icon</span>
									<input
										name="highlightIcon"
										bind:value={highlight.icon}
										placeholder="fi fi-rr-star"
									/>
								</label>
								<label>
									<span>Tone</span>
									<select
										name="highlightTone"
										bind:value={highlight.tone}
									>
										{#each TONE_OPTIONS as tone}
											<option value={tone}>{tone}</option>
										{/each}
									</select>
								</label>
								<label class="grow-2">
									<span>Title</span>
									<input
										name="highlightTitle"
										bind:value={highlight.title}
										placeholder="Ceremony"
									/>
								</label>
								<button
									type="button"
									class="danger-btn"
									onclick={() => removeHighlight(index)}
								>
									Remove
								</button>
							</div>

							<label>
								<span>Description</span>
								<textarea
									name="highlightDescription"
									bind:value={highlight.description}
									rows="2"
									placeholder="Describe this highlight"
								></textarea>
							</label>
						</article>
					{/each}
				</div>
			</section>

			<div class="actions">
				<button class="submit-btn" type="submit"
					>{isEditing ? "Update Event" : "Create Event"}</button
				>
			</div>
		</form>

	</div>

	<EventDetailsModal
		open={showPreview}
		event={previewEvent}
		onClose={() => (showPreview = false)}
	/>
</section>

<style>
	.create-shell {
		display: grid;
		gap: 20px;
	}

	.page-intro {
		display: flex;
		justify-content: space-between;
		gap: 16px;
		align-items: flex-start;
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
		max-width: 760px;
		color: #5a6d83;
	}

	.notice {
		padding: 12px 14px;
		border-radius: 14px;
		border: 1px solid rgba(200, 16, 46, 0.18);
		background: #fff2f3;
		color: #9f1733;
		font-size: 0.92rem;
		display: flex;
		align-items: center;
		gap: 8px;
	}
	.notice.notice-success {
		background: #d1fae5;
		color: #065f46;
		border-color: rgba(6, 95, 70, 0.2);
	}
	.notice.notice-approval {
		background: #fffbeb;
		color: #92400e;
		border-color: rgba(146, 64, 14, 0.2);
	}

	.preview-toggle {
		padding: 10px 14px;
		border-radius: 12px;
		background: #fff;
		border: 1px solid #cbd5e1;
		font-weight: 600;
		color: #1f3b57;
	}

	.intro-actions {
		display: inline-flex;
		gap: 10px;
		align-items: center;
	}

	.invite-link {
		padding: 10px 14px;
		border-radius: 12px;
		background: linear-gradient(135deg, var(--color-secondary), #2a7f96);
		color: #fff;
		font-weight: 600;
		text-decoration: none;
		display: inline-flex;
		align-items: center;
		gap: 6px;
		font-size: 14px;
	}

	.content-grid {
		display: grid;
		grid-template-columns: 1fr;
		gap: 20px;
	}

	.event-form {
		display: grid;
		gap: 16px;
	}

	.panel {
		background: #fff;
		border: 1px solid #d8e2eb;
		border-radius: 18px;
		box-shadow: 0 10px 26px rgba(15, 23, 42, 0.05);
		padding: 18px;
		display: grid;
		gap: 14px;
	}

	.panel h2 {
		margin: 0;
		font-size: 1.1rem;
		color: #10243e;
	}

	.field-grid {
		display: grid;
		gap: 12px;
	}

	.field-grid.two-up {
		grid-template-columns: repeat(2, minmax(0, 1fr));
	}

	.field-grid.four-up {
		grid-template-columns: repeat(4, minmax(0, 1fr));
	}

	label {
		display: grid;
		gap: 8px;
	}

	label span {
		font-size: 0.82rem;
		font-weight: 700;
		color: #24384f;
	}

	input,
	select,
	textarea {
		width: 100%;
		padding: 10px 12px;
		border-radius: 10px;
		border: 1px solid #c9d6e2;
		background: #f8fafc;
		color: #10243e;
		font: inherit;
		resize: vertical;
	}

	input:focus,
	select:focus,
	textarea:focus {
		outline: none;
		border-color: #1c5c6d;
		box-shadow: 0 0 0 3px rgba(28, 92, 109, 0.12);
		background: #fff;
	}

	.approval-hint {
		display: flex;
		align-items: center;
		gap: 5px;
		margin-top: 5px;
		font-size: 0.78rem;
		color: #c8102e;
		font-weight: 600;
	}

	.field-error {
		display: block;
		margin-top: 5px;
		font-size: 0.78rem;
		color: #c8102e;
		font-weight: 600;
	}

	.toggle-group {
		display: grid;
		gap: 10px;
		align-content: end;
	}

	.panel-hint {
		margin: -4px 0 14px;
		font-size: 0.82rem;
		color: #64748b;
	}

	.panel-hint a {
		color: #1c5c6d;
		font-weight: 600;
	}

	.invitation-body-label,
	.invitation-preview-label {
		display: grid;
		gap: 8px;
		margin-top: 14px;
	}

	.invitation-preview-label span {
		font-size: 0.82rem;
		font-weight: 700;
		color: #24384f;
	}

	.placeholder-chips {
		display: flex;
		flex-wrap: wrap;
		gap: 8px;
		margin-top: 14px;
	}

	.chip {
		padding: 6px 10px;
		border-radius: 999px;
		background: #ecf4f8;
		border: 1px solid #c7dce6;
		color: #1f3b57;
		font-family: monospace;
		font-size: 0.8rem;
		display: inline-flex;
		align-items: center;
		gap: 6px;
	}

	.copied {
		font-family: inherit;
		font-weight: 700;
		color: #16a34a;
		font-size: 0.72rem;
	}

	.invitation-preview-frame {
		border: 1px solid #d8e2eb;
		border-radius: 12px;
		overflow: hidden;
		background: #f8fafc;
	}

	.invitation-preview-frame iframe {
		width: 100%;
		height: 420px;
		border: none;
		background: #fff;
	}

	.share-link-box {
		margin-top: 14px;
		padding: 12px 14px;
		border-radius: 10px;
		background: #ecf4f8;
		border: 1px solid #c7dce6;
		font-size: 0.85rem;
		color: #1f3b57;
	}

	.share-link-row {
		display: flex;
		gap: 8px;
		margin-top: 8px;
	}

	.share-link-row input {
		flex: 1;
		font-size: 0.82rem;
		font-family: monospace;
	}

	.self-reg-fields {
		margin-top: 18px;
		padding-top: 16px;
		border-top: 1px dashed #d8e2eb;
	}

	.self-reg-fields-header {
		display: flex;
		align-items: center;
		justify-content: space-between;
		gap: 12px;
		flex-wrap: wrap;
	}

	.self-reg-fields-header h3 {
		margin: 0;
		font-size: 0.95rem;
		color: #1f3b57;
	}

	.checkbox.enable-all {
		font-weight: 700;
	}

	.self-reg-section {
		margin-top: 14px;
		padding: 12px 14px;
		border-radius: 10px;
		background: #f8fafc;
		border: 1px solid #e2e9f0;
	}

	.self-reg-section-header {
		display: flex;
		align-items: center;
		justify-content: space-between;
		gap: 12px;
		margin-bottom: 10px;
	}

	.self-reg-section-header strong {
		font-size: 0.85rem;
		color: #30475f;
	}

	.checkbox.small {
		font-size: 0.78rem;
		font-weight: 600;
	}

	.self-reg-field-grid {
		display: grid;
		grid-template-columns: repeat(auto-fill, minmax(220px, 1fr));
		gap: 8px 16px;
	}

	.checkbox.field-toggle {
		font-weight: 500;
		font-size: 0.82rem;
	}

	.checkbox.field-toggle.locked {
		color: #7a8ba0;
	}

	.checkbox {
		display: flex;
		align-items: center;
		gap: 8px;
		font-size: 0.9rem;
		font-weight: 600;
		color: #30475f;
	}

	.checkbox input {
		width: 16px;
		height: 16px;
		padding: 0;
	}

	.highlights-head {
		display: flex;
		justify-content: space-between;
		align-items: center;
		gap: 12px;
	}

	.secondary-btn,
	.danger-btn {
		padding: 8px 12px;
		border-radius: 10px;
		font-size: 0.82rem;
		font-weight: 700;
	}

	.secondary-btn {
		background: #ecf4f8;
		border: 1px solid #c7dce6;
		color: #1f3b57;
	}

	.danger-btn {
		align-self: end;
		background: #fff2f3;
		border: 1px solid #f2c7ce;
		color: #a11732;
		height: 40px;
	}

	.highlights-list {
		display: grid;
		gap: 12px;
	}

	.highlight-row {
		padding: 12px;
		border-radius: 14px;
		border: 1px solid #dde6ee;
		background: #fbfdff;
		display: grid;
		gap: 10px;
	}

	.grow-2 {
		grid-column: span 2;
	}

	.actions {
		display: flex;
		justify-content: flex-end;
	}

	.submit-btn {
		padding: 12px 18px;
		border: none;
		border-radius: 12px;
		background: linear-gradient(135deg, #f8ce1c, #e6b910);
		color: #263038;
		font-weight: 700;
		min-width: 160px;
	}

	@media (max-width: 1320px) {
		.field-grid.four-up {
			grid-template-columns: repeat(2, minmax(0, 1fr));
		}
	}

	@media (max-width: 760px) {
		.page-intro {
			flex-direction: column;
		}

		.field-grid.two-up,
		.field-grid.four-up {
			grid-template-columns: 1fr;
		}

		.grow-2 {
			grid-column: span 1;
		}
	}

	/* Enhanced premium styling with Dark Mode */
	:root {
		--font-primary: "Inter", sans-serif;
		--color-bg: #f0f4f8;
		--color-primary: #1c5c6d;
		--color-secondary: #2a7f96;
		--color-accent: #b11732;
		--glass-bg: rgba(255, 255, 255, 0.25);
		--glass-border: rgba(255, 255, 255, 0.3);
		--color-text: #10243e;
		--color-muted: #5a6d83;
	}

	.panel {
		background: var(--glass-bg);
		border: 1px solid var(--glass-border);
		backdrop-filter: blur(12px);
		box-shadow: 0 8px 20px rgba(0, 0, 0, 0.08);
	}

	button {
		transition:
			transform 0.2s ease,
			box-shadow 0.2s ease;
	}
	button:hover {
		transform: translateY(-2px);
		box-shadow: 0 6px 12px rgba(0, 0, 0, 0.12);
	}

	.animate-fade-in {
		animation: fadeIn 0.4s ease forwards;
	}
	@keyframes fadeIn {
		from {
			opacity: 0;
			transform: translateY(10px);
		}
		to {
			opacity: 1;
			transform: translateY(0);
		}
	}


	.image-upload-wrapper {
		display: grid;
		gap: 12px;
		width: 100%;
		border: 1px dashed #c9d6e2;
		padding: 16px;
		border-radius: 12px;
		background: rgba(248, 250, 252, 0.5);
	}

	.uploader-header {
		display: flex;
		justify-content: space-between;
		align-items: center;
		font-weight: 700;
		font-size: 0.82rem;
		color: #24384f;
	}

	.suggest-tags-btn {
		display: inline-flex;
		align-items: center;
		gap: 6px;
		margin-top: 6px;
		padding: 8px 14px;
		background: #f8ce1c;
		color: #263038;
		border: none;
		border-radius: 8px;
		cursor: pointer;
		font-weight: 600;
		font-size: 13px;
		width: fit-content;
		transition: background-color 0.2s;
	}

	.suggest-tags-btn:hover:not(:disabled) {
		background: #e6b910;
	}

	.suggest-tags-btn:disabled {
		opacity: 0.6;
		cursor: not-allowed;
	}

	.upload-trigger-btn {
		display: inline-flex;
		align-items: center;
		gap: 6px;
		padding: 8px 14px;
		background: #f8ce1c;
		color: #263038;
		border-radius: 8px;
		cursor: pointer;
		font-weight: 600;
		font-size: 13px;
		transition: background-color 0.2s;
	}

	.upload-trigger-btn:hover {
		background: #e6b910;
	}

	.upload-progress-info {
		font-size: 13px;
		color: #5a6d83;
		display: flex;
		align-items: center;
		gap: 6px;
	}

	.upload-error-message {
		font-size: 13px;
		color: #b11732;
	}

	.uploaded-image-preview-grid {
		display: flex;
		flex-wrap: wrap;
		gap: 10px;
		margin-top: 8px;
	}

	.uploaded-image-preview-card {
		position: relative;
		width: 80px;
		height: 80px;
		border-radius: 8px;
		overflow: hidden;
		border: 1px solid #cbd5e1;
	}

	.uploaded-image-preview-card img {
		width: 100%;
		height: 100%;
		object-fit: cover;
	}

	.remove-uploaded-image-btn {
		position: absolute;
		top: 2px;
		right: 2px;
		width: 18px;
		height: 18px;
		background: rgba(0, 0, 0, 0.6);
		color: #fff;
		border: none;
		border-radius: 50%;
		display: flex;
		align-items: center;
		justify-content: center;
		cursor: pointer;
		font-size: 12px;
		font-weight: bold;
		padding: 0;
	}

	.remove-uploaded-image-btn:hover {
		background: rgba(177, 23, 50, 0.9);
	}

	.organizer-image-wrapper {
		display: grid;
		gap: 12px;
		width: 100%;
		border: 1px dashed #c9d6e2;
		padding: 16px;
		border-radius: 12px;
		background: rgba(248, 250, 252, 0.5);
		margin-top: 12px;
	}

	.organizer-logo-preview {
		width: 96px;
		height: 96px;
		background: #fff;
	}

	.organizer-logo-preview img {
		object-fit: contain;
	}

	.map-field-wrapper {
		display: grid;
		gap: 8px;
		width: 100%;
		margin-top: 14px;
	}

	.map-field-wrapper > span {
		font-size: 0.82rem;
		font-weight: 700;
		color: #24384f;
	}

	.empty-map-placeholder {
		display: flex;
		flex-direction: column;
		align-items: center;
		justify-content: center;
		text-align: center;
		height: 200px;
		border: 1px solid #c9d6e2;
		border-radius: 12px;
		background: #f8fafc;
		color: #64748b;
		padding: 20px;
	}

	.empty-map-placeholder i {
		font-size: 24px;
		color: #94a3b8;
		margin-bottom: 8px;
	}

	.empty-map-placeholder p {
		margin: 0;
		font-size: 13px;
	}

	@media (prefers-color-scheme) {
		.image-upload-wrapper {
			border-color: #444;
			background: rgba(43, 43, 43, 0.5);
		}
		.uploader-header,
		.map-field-wrapper > span {
			color: #e0e0e0;
		}
		.uploaded-image-preview-card {
			border-color: #444;
		}
		.empty-map-placeholder {
			border-color: #444;
			background: #2b2b2b;
			color: #a0a0a0;
		}
		.empty-map-placeholder i {
			color: #555;
		}
	}

	/* Holiday toggle */
	.holiday-toggle-field {
		background: #fafafa;
		border: 1px solid #e2e8f0;
		border-radius: 12px;
		padding: 14px 16px;
		display: flex;
		flex-direction: column;
		gap: 10px;
	}

	.holiday-field-label {
		font-size: 13px;
		font-weight: 700;
		color: #1e293b;
		display: inline-flex;
		align-items: center;
		gap: 8px;
		text-transform: uppercase;
		letter-spacing: 0.5px;
	}

	.holiday-field-label i {
		color: #1c5c6d;
		font-size: 14px;
	}

	.holiday-toggle-switch {
		display: flex;
		align-items: center;
		gap: 12px;
		cursor: pointer;
		user-select: none;
	}

	.holiday-toggle-switch input[type="checkbox"] {
		position: absolute;
		opacity: 0;
		width: 0;
		height: 0;
	}

	.holiday-toggle-track {
		position: relative;
		width: 44px;
		height: 24px;
		background: #d1d5db;
		border-radius: 999px;
		flex-shrink: 0;
		transition: background 0.2s;
	}

	.holiday-toggle-switch input:checked ~ .holiday-toggle-track {
		background: #dc2626;
	}

	.holiday-toggle-thumb {
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

	.holiday-toggle-switch input:checked ~ .holiday-toggle-track .holiday-toggle-thumb {
		transform: translateX(20px);
	}

	.holiday-toggle-label {
		font-size: 13px;
		color: #475569;
		font-weight: 500;
	}
</style>
