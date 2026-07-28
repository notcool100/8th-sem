<script lang="ts">
    import { onMount } from "svelte";
    import AppSidebar from "$lib/components/layout/AppSidebar.svelte";
    import AppHeader from "$lib/components/layout/AppHeader.svelte";

    let { children } = $props();

    let isSidebarOpen = $state(true);
    let isMobileView = $state(false);

    function toggleSidebar() {
        isSidebarOpen = !isSidebarOpen;
    }

    onMount(() => {
        let previousMobileView: boolean | null = null;

        const syncViewport = () => {
            const mobile = window.innerWidth <= 1024;
            isMobileView = mobile;

            if (previousMobileView === null || previousMobileView !== mobile) {
                isSidebarOpen = !mobile;
            }

            previousMobileView = mobile;
        };

        syncViewport();
        window.addEventListener("resize", syncViewport);

        return () => {
            window.removeEventListener("resize", syncViewport);
        };
    });
</script>

<div class="client-wrapper" class:sidebar-open={isSidebarOpen}>
    <AppSidebar
        role="client"
        isOpen={isSidebarOpen}
        isMobileView={isMobileView}
        onToggle={toggleSidebar}
    />

    <div class="client-main-area">
        <AppHeader
            role="client"
            sidebarOpen={isSidebarOpen}
            onToggleSidebar={toggleSidebar}
        />

        <main class="client-content">
            {@render children()}
        </main>

        {#if isSidebarOpen && isMobileView}
            <button
                class="mobile-overlay"
                onclick={toggleSidebar}
                aria-label="Close sidebar"
            ></button>
        {/if}
    </div>
</div>

<style>
    @import url("https://fonts.googleapis.com/css2?family=Inter:wght@300;400;500;600;700&display=swap");

    :global(body) {
        margin: 0;
        padding: 0;
        font-family: "Inter", sans-serif;
        background: #f0f2f5;
    }

    .client-wrapper {
        display: flex;
        min-height: 100vh;
        width: 100%;
        position: relative;
    }

    .client-main-area {
        flex: 1;
        display: flex;
        flex-direction: column;
        min-height: 100vh;
        background: #f0f2f5;
        width: 100%;
        overflow-x: hidden;
        transition: margin-left 0.3s cubic-bezier(0.4, 0, 0.2, 1);
    }

    .client-content {
        flex: 1;
        padding: 28px;
        overflow-y: auto;
    }

    .mobile-overlay {
        display: none;
        position: fixed;
        inset: 0;
        background: rgba(0, 0, 0, 0.4);
        backdrop-filter: blur(4px);
        z-index: 998;
    }

    @media (min-width: 1025px) {
        .client-main-area {
            margin-left: 86px;
        }

        .client-wrapper.sidebar-open .client-main-area {
            margin-left: 270px;
        }
    }

    @media (max-width: 1024px) {
        .client-wrapper {
            grid-template-columns: 1fr;
        }

        .client-content {
            padding: 16px;
        }

        .mobile-overlay {
            display: block;
        }
    }
</style>
