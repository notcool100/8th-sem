<script lang="ts">
  import { onMount } from "svelte";
  import nepal_map from "$lib/assets/nepal_map.png";
  import hero_slider1 from "$lib/assets/hero_slider1.png";
  import hero_slider2 from "$lib/assets/hero_slider2.png";
  import hero_slider3 from "$lib/assets/hero_slider3.png";
  import hero_slider4 from "$lib/assets/hero_slider4.png";
  import hero_slider5 from "$lib/assets/hero_slider5.png";
  import hero_slider6 from "$lib/assets/hero_slider6.png";

  const clamp = (value: number, min: number, max: number): number =>
    Math.min(max, Math.max(min, value));

  const heroSlides = [
    hero_slider1,
    hero_slider2,
    hero_slider3,
    hero_slider4,
    hero_slider5,
    hero_slider6,
  ];

  let sectionElement: HTMLElement | null = null;
  let activeSlide = $state(0);
  let progress = $state(0);
  let slideTimer: ReturnType<typeof setInterval> | undefined;

  const overlayTranslate = $derived(-240 * progress);
  const overlayScale = $derived(1 + progress * 0.46);
  const titleTranslate = $derived(progress * 90);
  const titleOpacity = $derived(clamp(1 - progress * 1.18, 0, 1));
  const shadeOpacity = $derived(0.38 + progress * 0.33);

  function rotateSlide() {
    activeSlide = (activeSlide + 1) % heroSlides.length;
  }

  function syncProgress() {
    if (!sectionElement || typeof window === "undefined") return;

    const sectionTop = sectionElement.offsetTop;
    const availableScroll = Math.max(
      sectionElement.offsetHeight - window.innerHeight,
      1,
    );

    progress = clamp((window.scrollY - sectionTop) / availableScroll, 0, 1);
  }

  onMount(() => {
    if (typeof window === "undefined") return;

    slideTimer = setInterval(rotateSlide, 5800);

    const onScroll = () => syncProgress();
    const onResize = () => syncProgress();

    window.addEventListener("scroll", onScroll, { passive: true });
    window.addEventListener("resize", onResize);

    requestAnimationFrame(syncProgress);

    return () => {
      if (slideTimer) {
        clearInterval(slideTimer);
      }
      window.removeEventListener("scroll", onScroll);
      window.removeEventListener("resize", onResize);
    };
  });
</script>

<section class="hero-scroll-section" bind:this={sectionElement}>
  <div class="sticky-frame">
    <div class="hero-slideshow">
      {#each heroSlides as slide, index}
        <div
          class="slide"
          class:active={index === activeSlide}
          style={`background-image: url('${slide}');`}
        ></div>
      {/each}
    </div>

    <div class="scene-shade" style={`opacity: ${shadeOpacity};`}></div>

    <div
      class="overlay-layer"
      style={`transform: translateY(${overlayTranslate}px) scale(${overlayScale});`}
    >
      <img class="nepal-map" src={nepal_map} alt="" aria-hidden="true" />
    </div>

    <div
      class="hero-layer"
      style={`opacity: ${titleOpacity}; transform: translateY(-${titleTranslate}px);`}
    >
     
    </div>

    <div class="scroll-hint" style={`opacity: ${titleOpacity};`}>
      <span>Scroll</span>
      <i class="fi fi-rr-angle-small-down"></i>
    </div>
  </div>
</section>

<style>
  .hero-scroll-section {
    min-height: 200vh;
    position: relative;
    background: #041727;
    margin-top: 20px;
  }

  .sticky-frame {
    position: sticky;
    top: 0;
    height: 100vh;
    overflow: hidden;
    isolation: isolate;
  }

  .hero-slideshow {
    position: absolute;
    inset: 0;
    z-index: 1;
  }

  .slide {
    position: absolute;
    inset: 0;
    opacity: 0;
    transition: opacity 1.2s ease;
    background-position: center;
    background-size: cover;
    background-repeat: no-repeat;
    transform: scale(1);
  }

  .slide.active {
    opacity: 1;
    animation: kenBurns 7.2s linear forwards;
  }

  .scene-shade {
    position: absolute;
    inset: 0;
    z-index: 2;
    background: radial-gradient(
        circle at 48% 18%,
        rgba(56, 189, 248, 0.16),
        transparent 54%
      ),
      linear-gradient(
        180deg,
        rgba(2, 6, 23, 0.52) 0%,
        rgba(2, 6, 23, 0.72) 52%,
        rgba(2, 6, 23, 0.9) 100%
      );
  }

  .overlay-layer {
    position: absolute;
    inset: 0;
    z-index: 3;
    opacity: 1;
    transform-origin: center top;
    pointer-events: none;
    will-change: transform;
  }

  .overlay-layer img {
    width: 100%;
    height: 97%;
    object-fit: cover;
    display: block;
  }
  .hero-layer {
    position: absolute;
    inset: 0;
    z-index: 4;
    display: flex;
    align-items: center;
    justify-content: center;
    pointer-events: none;
    will-change: opacity, transform;
  }

  .hero-content {
    max-width: 1000px;
    text-align: center;
    margin-top: -2rem;
  }

  .hero-copy {
    max-width: 860px;
    margin: 0 auto;
    padding: 1.35rem 1.9rem 1.5rem;
    border-radius: 24px;
    border: 1px solid rgba(226, 232, 240, 0.3);
    background: linear-gradient(
      160deg,
      rgba(2, 6, 23, 0.64) 0%,
      rgba(2, 6, 23, 0.44) 100%
    );
    box-shadow: 0 28px 56px rgba(2, 6, 23, 0.34);
    backdrop-filter: blur(7px);
    -webkit-backdrop-filter: blur(7px);
  }

  .hero-badge {
    display: inline-flex;
    align-items: center;
    gap: 0.45rem;
    background: #bd242b;
    color: #fff;
    border-radius: 999px;
    padding: 0.58rem 1.35rem;
    font-size: 0.8rem;
    font-weight: 700;
    letter-spacing: 0.08em;
    text-transform: uppercase;
  }

  .hero-badge i {
    font-size: 0.78rem;
  }

  .hero-content h1 {
    margin: 1.2rem 0 0.95rem;
    color: #f8fafc;
    font-size: clamp(2.2rem, 5vw, 4.3rem);
    letter-spacing: -0.02em;
    text-shadow: 0 8px 28px rgba(2, 6, 23, 0.55);
  }

  .hero-content p {
    margin: 0 auto;
    max-width: 760px;
    color: rgba(248, 250, 252, 0.93);
    font-size: clamp(1rem, 1.5vw, 1.42rem);
    text-shadow: 0 4px 18px rgba(2, 6, 23, 0.5);
  }

  .search-strip {
    margin: 1.4rem auto 0;
    max-width: 940px;
    background: rgba(255, 255, 255, 0.95);
    border: 1px solid rgba(248, 250, 252, 0.54);
    box-shadow: 0 32px 60px rgba(2, 6, 23, 0.22);
    border-radius: 22px;
    overflow: hidden;
    display: grid;
    grid-template-columns: minmax(0, 1.45fr) minmax(0, 1fr) minmax(0, 1fr) auto;
    pointer-events: auto;
  }

  .search-field {
    min-height: 76px;
    display: flex;
    align-items: center;
    gap: 0.68rem;
    padding: 0 1.1rem;
    color: #64748b;
    font-size: 1rem;
    border-right: 1px solid #e2e8f0;
  }

  .search-field i {
    font-size: 0.98rem;
  }

  .search-button {
    min-height: 76px;
    min-width: 170px;
    background: #c8102e;
    color: #fff;
    font-weight: 700;
    font-size: 1.02rem;
    display: inline-flex;
    align-items: center;
    justify-content: center;
    gap: 0.55rem;
    transition: filter 0.2s ease;
  }

  .search-button:hover {
    filter: brightness(1.06);
  }

  .scroll-hint {
    position: absolute;
    left: 50%;
    bottom: 1.35rem;
    transform: translateX(-50%);
    color: rgba(241, 245, 249, 0.92);
    display: flex;
    align-items: center;
    gap: 0.45rem;
    font-size: 0.8rem;
    letter-spacing: 0.07em;
    text-transform: uppercase;
    z-index: 6;
  }

  .scroll-hint i {
    animation: hintBounce 1.6s ease-in-out infinite;
  }

  @keyframes kenBurns {
    0% {
      transform: scale(1);
    }
    100% {
      transform: scale(1.09);
    }
  }

  @keyframes hintBounce {
    0%,
    100% {
      transform: translateY(0);
    }
    50% {
      transform: translateY(5px);
    }
  }

  @media (max-width: 1200px) {
    .search-strip {
      grid-template-columns: repeat(2, minmax(0, 1fr));
      gap: 1px;
      background: rgba(148, 163, 184, 0.36);
    }

    .search-field {
      border-right: 0;
      min-height: 65px;
      background: rgba(255, 255, 255, 0.95);
    }

    .search-button {
      grid-column: span 2;
      min-height: 62px;
      width: 100%;
    }
  }

  @media (max-width: 920px) {
    .hero-scroll-section {
      min-height: 175vh;
    }

    .hero-content {
      margin-top: -1.3rem;
    }

    .hero-copy {
      padding: 1.2rem 1.25rem 1.35rem;
      border-radius: 20px;
    }

    .hero-content h1 {
      font-size: clamp(1.95rem, 9vw, 3rem);
    }

    .hero-content p {
      font-size: 0.95rem;
      max-width: 92%;
    }
  }

  @media (max-width: 640px) {
    .hero-content {
      text-align: left;
    }

    .hero-copy {
      padding: 1.05rem 1rem 1.15rem;
      border-radius: 16px;
    }

    .hero-badge {
      margin-left: 0;
      font-size: 0.72rem;
      padding: 0.5rem 1rem;
    }

    .hero-content h1 {
      margin-top: 0.95rem;
      font-size: clamp(1.75rem, 9.2vw, 2.35rem);
    }

    .hero-content p {
      max-width: 100%;
    }

    .search-strip {
      margin-top: 1rem;
      grid-template-columns: 1fr;
    }

    .search-button {
      grid-column: span 1;
    }

    .scroll-hint {
      font-size: 0.7rem;
      bottom: 0.95rem;
    }
  }
</style>
