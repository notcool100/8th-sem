<script lang="ts">
  import { locale, setLocale, t } from "$lib/i18n";

  const LANGUAGES = [
    { code: 'en' as const, label: 'EN', full: 'English' },
    { code: 'ne' as const, label: 'ने', full: 'नेपाली' },
  ];

  let isMobileMenuOpen = $state(false);
  let showLangMenu = $state(false);
  let showSearch = $state(false);

  function toggleMobileMenu() {
    isMobileMenuOpen = !isMobileMenuOpen;
    if (isMobileMenuOpen) { showSearch = false; showLangMenu = false; }
  }

  function toggleLangMenu() {
    showLangMenu = !showLangMenu;
    if (showLangMenu) showSearch = false;
  }

  function toggleSearch() {
    showSearch = !showSearch;
    if (showSearch) showLangMenu = false;
  }

  function changeLocale(code: (typeof LANGUAGES)[number]['code']) {
    setLocale(code);
    showLangMenu = false;
  }

  const NAV_LINKS = [
    { label: 'Places to go',    href: '#' },
    { label: 'Things to do',    href: '#' },
    { label: 'Festivals & Events', href: '/', active: true },
    { label: 'Plan Your Trip',  href: '#' },
    { label: 'Travel Updates',  href: '#' },
    { label: 'About Us',        href: '#' },
  ];
</script>

<nav class="public-nav">
  <div class="nav-inner">

    <!-- Logo -->
    <a href="/" class="brand" aria-label="Nepal Tourism Board">
      <img
        src="https://bucket-hnjdmr.s3.ap-south-1.amazonaws.com/public/media/1920/ntb_logo-1663927863_resized1920.jpg"
        alt="NTB"
        class="logo-emblem"
      />
      <span class="logo-divider"></span>
      <img
        src="https://bucket-hnjdmr.s3.ap-south-1.amazonaws.com/public/website/logo.svg"
        alt="Nepal"
        class="logo-text"
      />
    </a>

    <!-- Center nav links (desktop) -->
    <ul class="nav-links desktop-only" role="list">
      {#each NAV_LINKS as link}
        <li>
          <a
            href={link.href}
            class="nav-link"
            class:active={link.active}
          >{link.label}</a>
        </li>
      {/each}
    </ul>

    <!-- Right icons (desktop) -->
    <div class="nav-actions desktop-only">
      <!-- Search -->
      <div class="action-wrap">
        <button type="button" class="icon-btn" aria-label="Search" onclick={toggleSearch}>
          <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
            <circle cx="11" cy="11" r="8"/><line x1="21" y1="21" x2="16.65" y2="16.65"/>
          </svg>
        </button>
        {#if showSearch}
          <div class="search-dropdown">
            <label class="search-box" aria-label="Search">
              <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                <circle cx="11" cy="11" r="8"/><line x1="21" y1="21" x2="16.65" y2="16.65"/>
              </svg>
              <input type="text" placeholder={$t('searchPlaceholder')} autofocus />
            </label>
          </div>
        {/if}
      </div>

      <!-- Language -->
      <div class="action-wrap">
        <button type="button" class="icon-btn" aria-label={$t('language')} onclick={toggleLangMenu}>
          <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
            <circle cx="12" cy="12" r="10"/>
            <line x1="2" y1="12" x2="22" y2="12"/>
            <path d="M12 2a15.3 15.3 0 0 1 4 10 15.3 15.3 0 0 1-4 10 15.3 15.3 0 0 1-4-10 15.3 15.3 0 0 1 4-10z"/>
          </svg>
        </button>
        {#if showLangMenu}
          <div class="lang-dropdown">
            {#each LANGUAGES as lang}
              <button
                type="button"
                class="lang-opt"
                class:active={lang.code === $locale}
                onclick={() => changeLocale(lang.code)}
              >
                <span class="lang-code">{lang.label}</span>
                <span class="lang-full">{lang.full}</span>
              </button>
            {/each}
          </div>
        {/if}
      </div>
    </div>

    <!-- Mobile hamburger -->
    <button
      class="mobile-toggle"
      type="button"
      onclick={toggleMobileMenu}
      aria-label="Toggle menu"
      aria-expanded={isMobileMenuOpen}
    >
      {#if isMobileMenuOpen}
        <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.2" stroke-linecap="round">
          <line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/>
        </svg>
      {:else}
        <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.2" stroke-linecap="round">
          <line x1="3" y1="6" x2="21" y2="6"/><line x1="3" y1="12" x2="21" y2="12"/><line x1="3" y1="18" x2="21" y2="18"/>
        </svg>
      {/if}
    </button>

  </div>

  <!-- Mobile drawer -->
  {#if isMobileMenuOpen}
    <div class="mobile-drawer">
      <ul role="list">
        {#each NAV_LINKS as link}
          <li>
            <a
              href={link.href}
              class="mobile-link"
              class:active={link.active}
              onclick={toggleMobileMenu}
            >{link.label}</a>
          </li>
        {/each}
      </ul>
      <div class="mobile-bottom">
        <label class="search-box mobile-search">
          <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
            <circle cx="11" cy="11" r="8"/><line x1="21" y1="21" x2="16.65" y2="16.65"/>
          </svg>
          <input type="text" placeholder={$t('searchPlaceholder')} />
        </label>
        <div class="mobile-langs">
          {#each LANGUAGES as lang}
            <button
              type="button"
              class="lang-opt"
              class:active={lang.code === $locale}
              onclick={() => { changeLocale(lang.code); isMobileMenuOpen = false; }}
            >
              <span class="lang-code">{lang.label}</span>
              <span class="lang-full">{lang.full}</span>
            </button>
          {/each}
        </div>
      </div>
    </div>
  {/if}
</nav>

<!-- Click-outside overlay to close dropdowns -->
{#if showSearch || showLangMenu}
  <div
    class="dropdown-backdrop"
    role="presentation"
    onclick={() => { showSearch = false; showLangMenu = false; }}
  ></div>
{/if}

<style>
  /* ── Shell ───────────────────────────────────────────────────────────────── */
  .public-nav {
    position: fixed;
    top: 0;
    left: 0;
    right: 0;
    z-index: 120;
    background: #fff;
    box-shadow: 0 1px 0 rgba(0, 0, 0, 0.08);
    font-family: Arial, sans-serif;
  }

  .nav-inner {
    max-width: 1540px;
    margin: 0 auto;
    padding-inline: clamp(1rem, 2.5vw, 2.5rem);
    height: 74px;
    display: flex;
    align-items: center;
    gap: 2rem;
  }

  /* ── Logo ─────────────────────────────────────────────────────────────────── */
  .brand {
    display: inline-flex;
    align-items: center;
    gap: 0;
    text-decoration: none;
    flex-shrink: 0;
  }

  .logo-emblem {
    height: 52px;
    width: auto;
    display: block;
    object-fit: contain;
  }

  .logo-divider {
    display: inline-block;
    width: 1px;
    height: 38px;
    background: #d1d5db;
    margin: 0 14px;
    flex-shrink: 0;
  }

  .logo-text {
    height: 42px;
    width: auto;
    display: block;
    object-fit: contain;
  }

  /* ── Center nav links ─────────────────────────────────────────────────────── */
  .nav-links {
    display: flex;
    align-items: center;
    list-style: none;
    margin: 0;
    padding: 0;
    gap: 0;
    flex: 1;
    justify-content: center;
  }

  .nav-link {
    display: inline-block;
    padding: 0.3rem 0.9rem;
    color: #1a1a1a;
    font-size: 0.96rem;
    font-weight: 400;
    text-decoration: none;
    white-space: nowrap;
    transition: color 0.15s;
    border-bottom: 2px solid transparent;
    line-height: 1;
    padding-bottom: calc(0.3rem + 2px);
  }

  .nav-link:hover {
    color: #c8102e;
  }

  .nav-link.active {
    color: #c8102e;
    font-weight: 600;
    border-bottom-color: #f8ce1c;
  }

  /* ── Right icons ─────────────────────────────────────────────────────────── */
  .nav-actions {
    display: flex;
    align-items: center;
    gap: 0.25rem;
    flex-shrink: 0;
  }

  .action-wrap {
    position: relative;
  }

  .icon-btn {
    width: 40px;
    height: 40px;
    border-radius: 50%;
    border: none;
    background: transparent;
    color: #1a1a1a;
    display: inline-flex;
    align-items: center;
    justify-content: center;
    cursor: pointer;
    transition: background 0.15s, color 0.15s;
  }

  .icon-btn:hover {
    background: #f3f4f6;
    color: #c8102e;
  }

  /* ── Search dropdown ─────────────────────────────────────────────────────── */
  .search-dropdown {
    position: absolute;
    right: 0;
    top: calc(100% + 10px);
    background: #fff;
    border: 1px solid #e5e7eb;
    border-radius: 12px;
    box-shadow: 0 8px 24px rgba(0, 0, 0, 0.12);
    padding: 0.65rem;
    z-index: 200;
    min-width: 280px;
  }

  .search-box {
    display: flex;
    align-items: center;
    gap: 0.5rem;
    background: #f9fafb;
    border: 1px solid #e5e7eb;
    border-radius: 8px;
    padding: 0.5rem 0.75rem;
    cursor: text;
  }

  .search-box svg {
    color: #9ca3af;
    flex-shrink: 0;
  }

  .search-box input {
    border: none;
    outline: none;
    background: transparent;
    color: #111;
    font-size: 0.95rem;
    width: 100%;
  }

  .search-box input::placeholder {
    color: #9ca3af;
  }

  /* ── Language dropdown ───────────────────────────────────────────────────── */
  .lang-dropdown {
    position: absolute;
    right: 0;
    top: calc(100% + 10px);
    background: #fff;
    border: 1px solid #e5e7eb;
    border-radius: 12px;
    box-shadow: 0 8px 24px rgba(0, 0, 0, 0.12);
    overflow: hidden;
    z-index: 200;
    min-width: 150px;
  }

  .lang-opt {
    width: 100%;
    display: flex;
    align-items: center;
    gap: 0.6rem;
    padding: 0.7rem 1rem;
    border: none;
    background: transparent;
    cursor: pointer;
    text-align: left;
    transition: background 0.13s;
  }

  .lang-opt:hover,
  .lang-opt.active {
    background: #f9fafb;
  }

  .lang-opt.active .lang-code {
    color: #c8102e;
    font-weight: 700;
  }

  .lang-code {
    font-weight: 600;
    color: #111;
    font-size: 0.9rem;
    min-width: 24px;
  }

  .lang-full {
    color: #6b7280;
    font-size: 0.88rem;
  }

  /* ── Mobile toggle ───────────────────────────────────────────────────────── */
  .mobile-toggle {
    display: none;
    width: 40px;
    height: 40px;
    border-radius: 8px;
    border: 1px solid #e5e7eb;
    background: #fff;
    color: #374151;
    align-items: center;
    justify-content: center;
    cursor: pointer;
    margin-left: auto;
    flex-shrink: 0;
  }

  .mobile-toggle:hover {
    background: #f9fafb;
    color: #c8102e;
  }

  /* ── Mobile drawer ───────────────────────────────────────────────────────── */
  .mobile-drawer {
    background: #fff;
    border-top: 1px solid #f3f4f6;
    padding: 1rem 1.25rem 1.5rem;
    animation: slideDown 0.18s ease both;
  }

  @keyframes slideDown {
    from { opacity: 0; transform: translateY(-6px); }
    to { opacity: 1; transform: translateY(0); }
  }

  .mobile-drawer ul {
    list-style: none;
    margin: 0;
    padding: 0;
    display: flex;
    flex-direction: column;
    gap: 0;
  }

  .mobile-link {
    display: block;
    padding: 0.8rem 0;
    color: #1a1a1a;
    font-size: 1rem;
    font-weight: 500;
    text-decoration: none;
    border-bottom: 1px solid #f3f4f6;
    transition: color 0.15s;
  }

  .mobile-link:hover,
  .mobile-link.active {
    color: #c8102e;
  }

  .mobile-bottom {
    margin-top: 1rem;
    display: flex;
    flex-direction: column;
    gap: 0.75rem;
  }

  .mobile-search {
    width: 100%;
  }

  .mobile-langs {
    display: flex;
    gap: 0.5rem;
  }

  .mobile-langs .lang-opt {
    flex: 1;
    border: 1px solid #e5e7eb;
    border-radius: 8px;
    justify-content: center;
  }

  /* ── Dropdown backdrop ───────────────────────────────────────────────────── */
  .dropdown-backdrop {
    position: fixed;
    inset: 0;
    z-index: 119;
  }

  /* ── Desktop-only / Mobile-only helpers ──────────────────────────────────── */
  .desktop-only {
    display: flex;
  }

  @media (max-width: 1080px) {
    .desktop-only {
      display: none !important;
    }

    .mobile-toggle {
      display: inline-flex;
    }
  }

  @media (max-width: 600px) {
    .nav-inner {
      height: 64px;
    }

    .logo-emblem {
      height: 44px;
    }

    .logo-text {
      height: 34px;
    }

    .logo-divider {
      height: 30px;
      margin: 0 10px;
    }
  }
</style>
