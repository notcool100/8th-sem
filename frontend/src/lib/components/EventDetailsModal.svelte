<script lang="ts">
  import type { PublicEvent } from "$lib/components/public/eventTypes";
  import { NTB_LOGO_URL } from "$lib/constants/branding";
  import LocationPickerMap from "$lib/components/LocationPickerMap.svelte";

  function portal(node: HTMLElement, enabled: boolean) {
    if (!enabled) return {};
    document.body.appendChild(node);
    return {
      destroy() {
        node.remove();
      },
      update(newEnabled: boolean) {
        if (!newEnabled && node.parentNode === document.body) {
          node.remove();
        }
      }
    };
  }

  let {
    open = false,
    event = null,
    categoryColorMap = {},
    onClose = () => {},
    inlinePreview = false,
  } = $props<{
    open: boolean;
    event: PublicEvent | null;
    categoryColorMap?: Record<string, string>;
    onClose: () => void;
    inlinePreview?: boolean;
  }>();

  function categoryColor(name: string): string {
    return categoryColorMap[name.trim().toLowerCase()] ?? event?.color ?? "#f97316";
  }

  let expandedAbout = $state(false);

  let showShareModal = $state(false);
  let shareUrl = $state("");
  let copyStatus = $state<"idle" | "copied" | "error">("idle");
  let copyResetTimeout: ReturnType<typeof setTimeout> | null = null;

  function openShareModal() {
    if (!event || typeof window === "undefined") return;
    shareUrl = `${window.location.origin}/?event=${encodeURIComponent(event.slug)}`;
    copyStatus = "idle";
    showShareModal = true;
  }

  function closeShareModal() {
    showShareModal = false;
  }

  function closeShareOnBackdrop(e: MouseEvent) {
    if (e.currentTarget === e.target) closeShareModal();
  }

  async function copyShareLink() {
    try {
      if (navigator.clipboard && window.isSecureContext) {
        await navigator.clipboard.writeText(shareUrl);
      } else {
        const textarea = document.createElement("textarea");
        textarea.value = shareUrl;
        textarea.style.position = "fixed";
        textarea.style.opacity = "0";
        document.body.appendChild(textarea);
        textarea.focus();
        textarea.select();
        document.execCommand("copy");
        textarea.remove();
      }
      copyStatus = "copied";
    } catch {
      copyStatus = "error";
    }
    if (copyResetTimeout) clearTimeout(copyResetTimeout);
    copyResetTimeout = setTimeout(() => (copyStatus = "idle"), 2000);
  }

  // Carousel states
  let currentImageIndex = $state(0);
  let carouselInterval: any = null;

  // Derive images list
  const carouselImages = $derived.by(() => {
    if (!event) return [];
    if (event.images && Array.isArray(event.images) && event.images.length > 0) {
      return event.images;
    }
    if (Array.isArray(event.image)) {
      return event.image.filter(Boolean);
    }
    // Check if the single image has commas, parse it as list
    if (event.image && typeof event.image === "string") {
      if (event.image.includes(",")) {
        return event.image.split(",").map((s: string) => s.trim()).filter(Boolean);
      }
      return [event.image];
    }
    return [];
  });

  // Start auto-scroll on change
  $effect(() => {
    if (!open || !event || carouselImages.length <= 1) {
      currentImageIndex = 0;
      if (carouselInterval) clearInterval(carouselInterval);
      return;
    }

    currentImageIndex = 0;
    carouselInterval = setInterval(() => {
      currentImageIndex = (currentImageIndex + 1) % carouselImages.length;
    }, 4000); // 4 seconds auto-scroll interval

    return () => {
      if (carouselInterval) clearInterval(carouselInterval);
    };
  });

  function nextImage(e: MouseEvent) {
    e.stopPropagation();
    currentImageIndex = (currentImageIndex + 1) % carouselImages.length;
  }

  function prevImage(e: MouseEvent) {
    e.stopPropagation();
    currentImageIndex = (currentImageIndex - 1 + carouselImages.length) % carouselImages.length;
  }

  function selectImage(idx: number, e: MouseEvent) {
    e.stopPropagation();
    currentImageIndex = idx;
  }

  const paragraphs = $derived(
    event?.longDescription
      ?.split("\n\n")
      .map((paragraph: string) => paragraph.trim())
      .filter(Boolean) ?? [],
  );

  const visibleParagraphs = $derived(
    expandedAbout ? paragraphs : paragraphs.slice(0, 2),
  );

  const isRichTextDescription = $derived(
    event?.longDescription?.includes("<") ?? false
  );

  const isRichTextAttendance = $derived(
    event?.attendanceNote?.includes("<") ?? false
  );

  const heroImage = $derived(
    event?.image?.length ? event.image[0] : "",
  );

  const mapImage = $derived(
    event?.mapImage ?? "",
  );

  const isMultiDayEvent = $derived.by(() => {
    if (!event?.end_date_ad) return false;
    const start = new Date(event.date_ad);
    const end = new Date(event.end_date_ad);
    return (
      start.getFullYear() !== end.getFullYear() ||
      start.getMonth() !== end.getMonth() ||
      start.getDate() !== end.getDate()
    );
  });

  $effect(() => {
    if (!open || inlinePreview) {
      expandedAbout = false;
      showShareModal = false;
      return;
    }

    if (typeof window === "undefined") return;

    console.log("[EventDetailsModal] opened with event data:", event);

    const previousOverflow = document.body.style.overflow;
    document.body.style.overflow = "hidden";

    const onKeyDown = (event: KeyboardEvent) => {
      if (event.key === "Escape") {
        if (showShareModal) {
          closeShareModal();
        } else {
          onClose();
        }
      }
    };

    window.addEventListener("keydown", onKeyDown);

    return () => {
      document.body.style.overflow = previousOverflow;
      window.removeEventListener("keydown", onKeyDown);
    };
  });

  function closeOnBackdrop(event: MouseEvent) {
    if (event.currentTarget === event.target) {
      onClose();
    }
  }

  function getToneClass(tone: string | undefined): string {
    switch (tone) {
      case "blue":
        return "tone-blue";
      case "purple":
        return "tone-purple";
      case "green":
        return "tone-green";
      case "red":
        return "tone-red";
      default:
        return "tone-orange";
    }
  }
</script>

{#if open && event}
  <div use:portal={!inlinePreview} class="event-modal-backdrop" role="presentation" onclick={closeOnBackdrop}>
    <div class="event-modal" role="dialog" aria-modal="true" aria-label={`${event.title} details`}>
      <div 
        class="hero"
        role="presentation"
        onmouseenter={() => { if (carouselInterval) clearInterval(carouselInterval); }}
        onmouseleave={() => {
          if (carouselImages.length > 1) {
            carouselInterval = setInterval(() => {
              currentImageIndex = (currentImageIndex + 1) % carouselImages.length;
            }, 4000);
          }
        }}
      >
        <!-- Background carousel slides -->
        {#each carouselImages as img, idx}
          <div 
            class="hero-slide" 
            class:active={idx === currentImageIndex}
            style={`background-image: url('${img}');`}
          ></div>
        {/each}

        <!-- Custom Navigation Overlays -->
        {#if carouselImages.length > 1}
          <button type="button" class="carousel-nav-btn prev" onclick={prevImage} aria-label="Previous image">
            <i class="fi fi-rr-angle-small-left"></i>
          </button>
          <button type="button" class="carousel-nav-btn next" onclick={nextImage} aria-label="Next image">
            <i class="fi fi-rr-angle-small-right"></i>
          </button>
          
          <div class="carousel-indicators">
            {#each carouselImages as _, idx}
              <button 
                type="button" 
                class="indicator-dot" 
                class:active={idx === currentImageIndex} 
                onclick={(e) => selectImage(idx, e)}
                aria-label={`Go to slide ${idx + 1}`}
              ></button>
            {/each}
          </div>
        {/if}

        <div class="hero-top">
          <div class="badge-row">
            {#each event.category.split(',') as cat}
              <span class="pill cat-pill" style="background:{categoryColor(cat)}">{cat.trim().toUpperCase()}</span>
            {/each}
            {#if event.featured}
              <span class="pill neutral-pill"
                ><i class="fi fi-rr-star"></i> Featured</span
              >
            {/if}
          </div>

          <div class="badge-row">
            {#if event.source !== "festival" && event.type !== "festival" && event.showEntryType !== false}
              <span class="pill entry-pill">{event.entryType.toUpperCase()}</span>
            {/if}
            {#if event.requiresInvitation}
              <span class="pill invite-pill"><i class="fi fi-rr-lock"></i> Invitation Only</span>
            {:else if event.requiresRegistration}
              <span class="pill register-pill"><i class="fi fi-rr-user-add"></i> Registration Open</span>
            {/if}
            {#if !inlinePreview}
              <button
                type="button"
                class="close-btn"
                aria-label="Close details"
                onclick={onClose}
              >
                <i class="fi fi-rr-cross"></i>
              </button>
            {/if}
          </div>
        </div>

        <div class="hero-bottom">
          <h2>{event.title}</h2>
          <div class="meta-row">
            <span><i class="fi fi-rr-marker"></i> {event.location}</span>
            <span
              ><i class="fi fi-rr-calendar-day"></i>
              {event.dateRangeLabel}</span
            >
          </div>
        </div>
      </div>

      <div class="content">
        <div class="fact-grid">
          {#if isMultiDayEvent}
            <article class="fact-card">
              <div class="fact-icon tone-red">
                <i class="fi fi-rr-calendar"></i>
              </div>
              <div>
                <p class="fact-label">Date & Duration</p>
                <h4>{event.dateRangeLabel}</h4>
                <p>{event.durationLabel}</p>
              </div>
            </article>
          {/if}

          <article class="fact-card">
            <div class="fact-icon tone-green">
              <i class="fi fi-rr-marker"></i>
            </div>
            <div>
              <p class="fact-label">Location</p>
              <h4>{event.region}</h4>
              <p>{event.location}</p>
            </div>
          </article>

          {#if event.type !== "festival" && event.source !== "festival"}
            <article class="fact-card">
              <div class="fact-icon tone-orange">
                <i class="fi fi-rr-users-alt"></i>
              </div>
              <div>
                <p class="fact-label">Attendance</p>
                <h4>{event.attendanceLabel}</h4>
                {#if isRichTextAttendance}
                  <div class="rich-attendance-note">{@html event.attendanceNote}</div>
                {:else}
                  <p>{event.attendanceNote}</p>
                {/if}
              </div>
            </article>
          {/if}
        </div>

        <section class="section-block">
          <h3><i class="fi fi-rr-document"></i> {event.type === "festival" || event.source === "festival" ? "About the Festival" : "About the Event"}</h3>

          <div class="about-copy">
            {#if isRichTextDescription}
              {@html event.longDescription}
            {:else}
              {#each visibleParagraphs as paragraph}
                <p>{paragraph}</p>
              {/each}
            {/if}
          </div>

          {#if !isRichTextDescription && paragraphs.length > 2}
            <button
              type="button"
              class="read-more"
              onclick={() => (expandedAbout = !expandedAbout)}
            >
              {expandedAbout ? "Show less" : "Read more"}
            </button>
          {/if}

          <div class="tag-strip">
            {#each event.tags as tag}
              <span>{tag}</span>
            {/each}
          </div>
        </section>

        <!--
        <section class="section-block">
          <h3><i class="fi fi-rr-star"></i> Festival Highlights</h3>
          <div class="highlight-grid">
            {#each event.highlights as highlight}
              <article class="highlight-item">
                <div class={`highlight-icon ${getToneClass(highlight.tone)}`}>
                  <i class={highlight.icon}></i>
                </div>
                <div>
                  <h4>{highlight.title}</h4>
                  <p>{highlight.description}</p>
                </div>
              </article>
            {/each}
          </div>
        </section>
        -->

        {#if event.type !== "festival" && event.source !== "festival"}
        <section class="section-block">
          <h3><i class="fi fi-rr-map-marker-home"></i> Event Location</h3>
          {#if event.latitude != null && event.longitude != null}
            <div class="interactive-map-wrapper" style="margin-top: 0.9rem;">
              <LocationPickerMap
                latitude={event.latitude}
                longitude={event.longitude}
                interactive={false}
                height="240px"
              />
            </div>
          {:else if mapImage && (mapImage.includes('google.com/maps') || mapImage.includes('maps.google.com'))}
            <div class="interactive-map-wrapper" style="margin-top: 0.9rem;">
              <iframe
                title="Event Location Map"
                width="100%"
                height="240"
                src={mapImage}
                frameborder="0"
                scrolling="no"
                marginheight="0"
                marginwidth="0"
                style="border: 1px solid #cbd5e1; border-radius: 16px;"
              ></iframe>
            </div>
          {:else}
            <div
              class="location-card"
              style={mapImage ? `background-image:url('${mapImage}');` : undefined}
            >
              <div class="location-badge">
                <i class="fi fi-rr-marker"></i>
                <div>
                  <strong>{event.region}</strong>
                  <p>{event.location}</p>
                </div>
              </div>
              <button type="button" class="maps-btn">
                <i class="fi fi-rr-map"></i> Open in Maps
              </button>
            </div>
          {/if}
          <p class="address"><i class="fi fi-rr-marker"></i> {event.address}</p>
        </section>

        <section class="section-block organizer">
          <div class="org-logo">
            <img src={event.organizerImageUrl || NTB_LOGO_URL} alt={event.organizer} />
          </div>
          <div>
            <p class="fact-label">Organized by</p>
            <h4>{event.organizer}</h4>
            <p>{event.organizerSubtitle}</p>
          </div>
          {#if event.organizerVerified}
            <span class="verified"
              ><i class="fi fi-rr-badge-check"></i> Verified</span
            >
          {/if}
        </section>
        {/if}

        <footer class="actions">
          {#if event.requiresRegistration}
            <a href={event.slug ? `/register/${event.slug}` : '#'} class="action action-primary">
              <i class="fi fi-rr-user-add"></i> Register to this Event
            </a>
          <!-- {:else}
            <button type="button" class="action action-primary">
              <i class="fi fi-rr-calendar"></i> Add to Calendar
            </button> -->
          {/if}
          <button type="button" class="action action-outline" onclick={openShareModal}
            ><i class="fi fi-rr-share"></i> Share</button
          >
          <!-- <button
            type="button"
            class="action action-icon"
            aria-label="Save event"
          >
            <i class="fi fi-rr-bookmark"></i>
          </button> -->
          <!-- <button
            type="button"
            class="action action-icon"
            aria-label="More actions"
          >
            <i class="fi fi-rr-menu-dots"></i>
          </button> -->
        </footer>
      </div>
    </div>

    {#if showShareModal}
      <div
        class="share-modal-backdrop"
        role="presentation"
        onclick={closeShareOnBackdrop}
      >
        <div class="share-modal" role="dialog" aria-modal="true" aria-label="Share event">
          <div class="share-modal-header">
            <h3><i class="fi fi-rr-share"></i> Share this event</h3>
            <button
              type="button"
              class="share-close-btn"
              aria-label="Close"
              onclick={closeShareModal}
            >
              <i class="fi fi-rr-cross-small"></i>
            </button>
          </div>
          <p class="share-modal-desc">
            Copy the link below to share &ldquo;{event.title}&rdquo; with others. Opening it will take them straight to this event.
          </p>
          <div class="share-link-row">
            <input
              type="text"
              readonly
              value={shareUrl}
              class="share-link-input"
              onclick={(e) => (e.currentTarget as HTMLInputElement).select()}
              aria-label="Shareable event link"
            />
            <button type="button" class="share-copy-btn" onclick={copyShareLink}>
              <i class={copyStatus === "copied" ? "fi fi-rr-check" : "fi fi-rr-copy"}></i>
              {copyStatus === "copied" ? "Copied" : "Copy"}
            </button>
          </div>
          {#if copyStatus === "error"}
            <p class="share-error">Couldn&apos;t copy automatically. Please copy the link manually.</p>
          {/if}
        </div>
      </div>
    {/if}
  </div>
{/if}

<style>
  .event-modal-inline-wrap {
    position: relative;
  }

  .event-modal-backdrop {
    position: fixed;
    inset: 0;
    z-index: 1500;
    background: rgba(3, 15, 28, 0.72);
    display: flex;
    align-items: center;
    justify-content: center;
    padding: 1rem;
    backdrop-filter: blur(4px);
  }

  .event-modal {
    width: min(980px, 100%);
    max-height: calc(100vh - 2rem);
    overflow: auto;
    border-radius: 24px;
    background: #f8fafc;
    border: 1px solid rgba(148, 163, 184, 0.25);
    box-shadow: 0 32px 60px rgba(2, 6, 23, 0.48);
  }

  .event-modal-inline {
    width: 100%;
    max-height: none;
    box-shadow: 0 20px 36px rgba(2, 6, 23, 0.2);
  }

  .hero {
    min-height: 340px;
    border-radius: 24px 24px 0 0;
    position: relative;
    color: white;
    padding: 1.2rem;
    overflow: hidden;
    display: flex;
    flex-direction: column;
    justify-content: space-between;
  }

  .hero-slide {
    position: absolute;
    inset: 0;
    background-size: cover;
    background-position: center;
    opacity: 0;
    z-index: 0;
    transition: opacity 0.8s ease-in-out;
  }

  .hero-slide.active {
    opacity: 1;
    z-index: 1;
  }

  .hero::before {
    content: "";
    position: absolute;
    inset: 0;
    border-radius: 24px 24px 0 0;
    background: linear-gradient(180deg, rgba(15, 23, 42, 0.12) 0%, rgba(15, 23, 42, 0.82) 100%);
    z-index: 2;
    pointer-events: none;
  }

  .carousel-nav-btn {
    position: absolute;
    top: 50%;
    transform: translateY(-50%);
    width: 40px;
    height: 40px;
    border-radius: 50%;
    border: 1px solid rgba(255, 255, 255, 0.35);
    background: rgba(15, 23, 42, 0.45);
    color: white;
    display: flex;
    align-items: center;
    justify-content: center;
    cursor: pointer;
    z-index: 10;
    font-size: 1.2rem;
    transition: all 0.2s;
  }

  .carousel-nav-btn:hover {
    background: rgba(200, 16, 46, 0.85);
    border-color: #c8102e;
    transform: translateY(-50%) scale(1.08);
  }

  .carousel-nav-btn.prev {
    left: 1.2rem;
  }

  .carousel-nav-btn.next {
    right: 1.2rem;
  }

  .carousel-indicators {
    position: absolute;
    bottom: 1.2rem;
    right: 1.2rem;
    display: flex;
    gap: 0.4rem;
    z-index: 10;
  }

  .indicator-dot {
    width: 8px;
    height: 8px;
    border-radius: 50%;
    border: none;
    background: rgba(255, 255, 255, 0.4);
    cursor: pointer;
    transition: all 0.2s;
    padding: 0;
  }

  .indicator-dot.active {
    background: #c8102e;
    width: 20px;
    border-radius: 4px;
  }

  .hero-top,
  .hero-bottom {
    position: relative;
    z-index: 1;
  }

  .hero-top {
    display: flex;
    justify-content: space-between;
    align-items: flex-start;
  }

  .badge-row {
    display: inline-flex;
    align-items: center;
    gap: 0.6rem;
  }

  .pill {
    border-radius: 999px;
    padding: 0.42rem 0.9rem;
    font-size: 0.8rem;
    font-weight: 800;
    letter-spacing: 0.06em;
    display: inline-flex;
    align-items: center;
    gap: 0.35rem;
    text-transform: uppercase;
  }

  .festival-pill {
    background: #f97316;
  }

  .cat-pill {
    color: #fff;
  }

  .neutral-pill {
    border: 1px solid rgba(255, 255, 255, 0.55);
    background: rgba(255, 255, 255, 0.16);
    text-transform: none;
  }

  .entry-pill {
    background: #0f766e;
  }

  .invite-pill {
    background: #7c3aed;
  }

  .register-pill {
    background: #16a34a;
  }

  .close-btn {
    width: 44px;
    height: 44px;
    border-radius: 50%;
    border: 1px solid rgba(255, 255, 255, 0.45);
    background: rgba(255, 255, 255, 0.2);
    color: white;
    display: inline-flex;
    align-items: center;
    justify-content: center;
  }

  .hero-bottom {
    margin-top: 7.2rem;
  }

  .hero-bottom h2 {
    margin: 0;
    font-size: clamp(1.9rem, 3.4vw, 2.8rem);
    color: #fff;
  }

  .meta-row {
    margin-top: 0.5rem;
    display: flex;
    flex-wrap: wrap;
    gap: 1rem;
    color: rgba(248, 250, 252, 0.96);
    font-size: 1rem;
  }

  .meta-row span {
    display: inline-flex;
    align-items: center;
    gap: 0.35rem;
  }

  .content {
    padding: 1.5rem;
    display: flex;
    flex-direction: column;
    gap: 1.2rem;
  }

  .fact-grid {
    display: grid;
    grid-template-columns: repeat(3, minmax(0, 1fr));
    gap: 0.9rem;
  }

  .fact-card {
    background: #fff;
    border: 1px solid #e2e8f0;
    border-radius: 16px;
    padding: 1rem;
    display: flex;
    align-items: center;
    gap: 0.85rem;
  }

  .fact-label {
    margin: 0;
    color: #64748b;
    text-transform: uppercase;
    letter-spacing: 0.07em;
    font-size: 0.72rem;
    font-weight: 800;
  }

  .fact-card h4 {
    margin: 0.25rem 0 0;
    color: #1e293b;
    font-size: 1.55rem;
  }

  .fact-card p {
    margin: 0.2rem 0 0;
    color: #64748b;
    font-size: 0.9rem;
  }

  .fact-icon,
  .highlight-icon {
    width: 46px;
    height: 46px;
    border-radius: 12px;
    display: inline-flex;
    align-items: center;
    justify-content: center;
    font-size: 1.05rem;
    flex-shrink: 0;
  }

  .tone-orange {
    color: #c2410c;
    background: #ffedd5;
  }

  .tone-blue {
    color: #0369a1;
    background: #e0f2fe;
  }

  .tone-purple {
    color: #7e22ce;
    background: #f3e8ff;
  }

  .tone-green {
    color: #166534;
    background: #dcfce7;
  }

  .tone-red {
    color: #be123c;
    background: #ffe4e6;
  }

  .section-block {
    background: #fff;
    border: 1px solid #e2e8f0;
    border-radius: 16px;
    padding: 1.1rem;
  }

  .section-block h3 {
    margin: 0;
    color: #1f2937;
    font-size: 2.05rem;
    display: inline-flex;
    align-items: center;
    gap: 0.5rem;
  }

  .section-block h3 i {
    color: #c8102e;
    font-size: 1rem;
  }

  .about-copy {
    margin-top: 0.9rem;
    display: flex;
    flex-direction: column;
    gap: 0.8rem;
  }

  .about-copy p {
    margin: 0;
    color: #475569;
    line-height: 1.7;
    font-size: 1rem;
  }

  .read-more {
    margin-top: 0.85rem;
    color: #c8102e;
    font-weight: 700;
  }

  .tag-strip {
    margin-top: 1rem;
    display: flex;
    flex-wrap: wrap;
    gap: 0.55rem;
  }

  .tag-strip span {
    background: #f1f5f9;
    border-radius: 999px;
    color: #64748b;
    font-size: 0.82rem;
    padding: 0.33rem 0.7rem;
    font-weight: 600;
  }

  .highlight-grid {
    margin-top: 0.9rem;
    display: grid;
    grid-template-columns: repeat(2, minmax(0, 1fr));
    gap: 0.75rem;
  }

  .highlight-item {
    border: 1px solid #e2e8f0;
    border-radius: 12px;
    padding: 0.8rem;
    display: flex;
    align-items: center;
    gap: 0.7rem;
    background: #f8fafc;
  }

  .highlight-item h4 {
    margin: 0;
    font-size: 1.2rem;
  }

  .highlight-item p {
    margin: 0.2rem 0 0;
    color: #64748b;
    font-size: 0.88rem;
  }

  .location-card {
    margin-top: 0.9rem;
    min-height: 220px;
    border-radius: 16px;
    background-size: cover;
    background-position: center;
    position: relative;
    overflow: hidden;
    border: 1px solid #cbd5e1;
  }

  .location-card::before {
    content: "";
    position: absolute;
    inset: 0;
    background: linear-gradient(
      180deg,
      rgba(15, 23, 42, 0.08),
      rgba(15, 23, 42, 0.45)
    );
  }

  .location-badge,
  .maps-btn {
    position: absolute;
    z-index: 1;
  }

  .location-badge {
    left: 1rem;
    top: 1rem;
    background: #fff;
    border-radius: 12px;
    padding: 0.6rem 0.72rem;
    display: inline-flex;
    align-items: center;
    gap: 0.5rem;
    color: #1f2937;
  }

  .location-badge i {
    color: #c8102e;
  }

  .location-badge strong {
    display: block;
    font-size: 0.95rem;
  }

  .location-badge p {
    margin: 0;
    color: #64748b;
    font-size: 0.8rem;
  }

  .maps-btn {
    right: 1rem;
    bottom: 1rem;
    background: #c8102e;
    color: white;
    border-radius: 10px;
    min-height: 38px;
    padding: 0 0.85rem;
    display: inline-flex;
    align-items: center;
    gap: 0.35rem;
    font-weight: 700;
  }

  .address {
    margin-top: 0.9rem;
    color: #475569;
    display: inline-flex;
    align-items: center;
    gap: 0.4rem;
  }

  .organizer {
    display: flex;
    align-items: center;
    gap: 0.8rem;
    position: relative;
  }

  .org-logo {
    width: 110px;
    height: 56px;
    border-radius: 10px;
    background: #fff;
    border: 1px solid #e2e8f0;
    display: inline-flex;
    align-items: center;
    justify-content: center;
    padding: 0.35rem;
    flex-shrink: 0;
  }

  .org-logo img {
    width: 100%;
    height: 100%;
    object-fit: contain;
    display: block;
  }

  .organizer h4 {
    margin: 0.2rem 0 0;
    font-size: 1.55rem;
  }

  .organizer p {
    margin: 0.1rem 0 0;
    color: #64748b;
  }

  .verified {
    margin-left: auto;
    border-radius: 999px;
    background: #dcfce7;
    color: #166534;
    font-size: 0.85rem;
    font-weight: 700;
    padding: 0.35rem 0.7rem;
    display: inline-flex;
    gap: 0.28rem;
    align-items: center;
  }

  .actions {
    display: grid;
    grid-template-columns: minmax(0, 1fr) auto auto auto;
    gap: 0.65rem;
    padding-top: 0.2rem;
  }

  .action {
    min-height: 52px;
    border-radius: 14px;
    font-weight: 700;
    display: inline-flex;
    align-items: center;
    justify-content: center;
    gap: 0.4rem;
  }

  .action-primary {
    background: #c8102e;
    color: white;
    text-decoration: none;
    cursor: pointer;
  }

  .action-outline {
    border: 1px solid #c8102e;
    color: #c8102e;
    background: #fff;
    padding: 0 1.2rem;
  }

  .action-icon {
    width: 52px;
    background: #f1f5f9;
    color: #475569;
  }

  .share-modal-backdrop {
    position: fixed;
    inset: 0;
    z-index: 1600;
    background: rgba(3, 15, 28, 0.72);
    display: flex;
    align-items: center;
    justify-content: center;
    padding: 1rem;
    backdrop-filter: blur(4px);
  }

  .share-modal {
    width: min(480px, 100%);
    background: #fff;
    border-radius: 20px;
    padding: 1.4rem;
    box-shadow: 0 32px 60px rgba(2, 6, 23, 0.48);
  }

  .share-modal-header {
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: 0.6rem;
  }

  .share-modal-header h3 {
    margin: 0;
    font-size: 1.15rem;
    color: #1f2937;
    display: inline-flex;
    align-items: center;
    gap: 0.5rem;
  }

  .share-modal-header h3 i {
    color: #c8102e;
  }

  .share-close-btn {
    width: 34px;
    height: 34px;
    border-radius: 50%;
    border: 1px solid #e2e8f0;
    background: #f8fafc;
    color: #475569;
    display: inline-flex;
    align-items: center;
    justify-content: center;
    flex-shrink: 0;
  }

  .share-modal-desc {
    margin: 0.75rem 0 0;
    color: #64748b;
    font-size: 0.92rem;
    line-height: 1.5;
  }

  .share-link-row {
    margin-top: 1rem;
    display: flex;
    gap: 0.6rem;
  }

  .share-link-input {
    flex: 1;
    min-width: 0;
    border-radius: 12px;
    border: 1px solid #e2e8f0;
    background: #f8fafc;
    color: #1f2937;
    padding: 0 0.9rem;
    height: 46px;
    font-size: 0.9rem;
  }

  .share-copy-btn {
    height: 46px;
    padding: 0 1.1rem;
    border-radius: 12px;
    border: none;
    background: #c8102e;
    color: #fff;
    font-weight: 700;
    display: inline-flex;
    align-items: center;
    gap: 0.4rem;
    flex-shrink: 0;
  }

  .share-error {
    margin: 0.6rem 0 0;
    color: #be123c;
    font-size: 0.85rem;
  }

  @media (max-width: 920px) {
    .fact-grid {
      grid-template-columns: 1fr;
    }

    .highlight-grid {
      grid-template-columns: 1fr;
    }

    .actions {
      grid-template-columns: 1fr;
    }

    .action-icon {
      width: 100%;
    }

    .organizer {
      flex-wrap: wrap;
    }

    .verified {
      margin-left: 0;
    }
  }
</style>
