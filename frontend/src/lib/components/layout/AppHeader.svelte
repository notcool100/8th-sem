<script lang="ts">
    import { page } from "$app/state";
    import { goto } from "$app/navigation";
    import type { SessionUser } from "$lib/types/auth";
    import { notifStore } from "$lib/notifications.svelte";

    let { role = "admin", sidebarOpen = false, onToggleSidebar } = $props<{ role?: "admin" | "client", sidebarOpen?: boolean, onToggleSidebar?: () => void }>();

    let showNotifications = $state(false);
    let showProfile = $state(false);

    const segmentLabels: Record<string, string> = {
        admin: "Dashboard",
        client: "Dashboard",
        events: "Events Management",
        "pending-approval": "Pending Approval",
        calendar: "Calendar",
        setup: "Calendar Setup",
        colors: "Color Setup",
        // destinations: "Destinations",
        users: "Users Management",
        settings: "Settings",
        reports: "Reports & Analytics",
        departments: "Department Setup",
        attendees: "Attendees / Registrations",
        notifications: "Notifications",
        "categories-setup": "Categories Setup",
        create: "Create Event",
        invitations: "Invitations",
    };

    const breadcrumbs = $derived(
        (() => {
            const parts = page.url.pathname.split("/").filter(Boolean);
            return parts.map((seg, i) => ({
                label:
                    segmentLabels[seg] ??
                    seg
                        .replace(/-/g, " ")
                        .replace(/\b\w/g, (c) => c.toUpperCase()),
                href: "/" + parts.slice(0, i + 1).join("/"),
                isLast: i === parts.length - 1,
            }));
        })(),
    );

    const pageTitle = $derived(breadcrumbs[breadcrumbs.length - 1]?.label || "Dashboard");
    const currentUser = $derived((page.data.session?.user ?? null) as SessionUser | null);
    const profileInitial = $derived(currentUser?.fullName?.charAt(0).toUpperCase() ?? (role === "admin" ? "A" : "C"));
    const profileRoleLabel = $derived(
        currentUser?.role === "superadmin"
            ? "Super Admin"
            : currentUser?.role === "admin"
                ? "Administrator"
                : currentUser?.role === "client"
                    ? "Client User"
                    : role === "admin"
                        ? "Administrator"
                        : "Client User"
    );

    function toggleNotifications() {
        showNotifications = !showNotifications;
        showProfile = false;
    }

    function toggleProfile() {
        showProfile = !showProfile;
        showNotifications = false;
    }

    function formatTime(iso: string) {
        const diff = Date.now() - new Date(iso).getTime();
        const mins = Math.floor(diff / 60000);
        if (mins < 1) return "just now";
        if (mins < 60) return `${mins}m ago`;
        const hrs = Math.floor(mins / 60);
        if (hrs < 24) return `${hrs}h ago`;
        return `${Math.floor(hrs / 24)}d ago`;
    }

    function getTypeIcon(type: string) {
        if (type === "approval_request") return "fi-rr-time-check";
        if (type === "approved") return "fi-rr-check-circle";
        if (type === "rejected") return "fi-rr-cross-circle";
        return "fi-rr-bell";
    }

    async function handleMarkAllRead() {
        await notifStore.markAllRead();
    }

    async function handleNotificationClick(id: number, link: string | null) {
        await notifStore.markRead(id);
        showNotifications = false;
        if (link) goto(link);
    }

    async function handleLogout() {
        await fetch("/logout", { method: "POST" });
        await goto("/login");
    }
</script>

<header class="app-header">
    <div class="header-left">
        <button class="sidebar-toggle" onclick={onToggleSidebar} aria-label={sidebarOpen ? "Close Sidebar" : "Open Sidebar"}>
            <i class={sidebarOpen ? "fi fi-rr-cross" : "fi fi-rr-menu-burger"}></i>
        </button>
        <div class="page-context">
            <h1 class="page-title">{pageTitle}</h1>
            <div class="breadcrumb">
                <a href="/{role}" class="bc-root">NTB {role === 'admin' ? 'Admin' : 'Organizer'}</a>
                {#each breadcrumbs.slice(1) as crumb}
                    <span class="bc-sep">›</span>
                    {#if crumb.isLast}
                        <span class="bc-current">{crumb.label}</span>
                    {:else}
                        <a href={crumb.href} class="bc-link">{crumb.label}</a>
                    {/if}
                {/each}
            </div>
        </div>
    </div>

    <div class="header-right">
        <!-- Notification Center -->
        <div class="header-action-wrapper">
            <button
                class="header-btn"
                class:active={showNotifications}
                onclick={toggleNotifications}
                title="Notifications"
            >
                <i class="fi fi-rr-bell"></i>
                {#if notifStore.unreadCount > 0}
                    <span class="badge">{notifStore.unreadCount > 99 ? "99+" : notifStore.unreadCount}</span>
                {/if}
            </button>

            {#if showNotifications}
                <div class="dropdown notification-dropdown">
                    <div class="dropdown-header">
                        <h3>Notifications {#if notifStore.unreadCount > 0}<span class="unread-chip">{notifStore.unreadCount}</span>{/if}</h3>
                        {#if notifStore.items.some(n => !n.isRead)}
                            <button onclick={handleMarkAllRead} class="mark-all-btn">Mark all read</button>
                        {/if}
                    </div>
                    <div class="dropdown-content">
                        {#if notifStore.items.length === 0}
                            <div class="note-empty">
                                <i class="fi fi-rr-bell-slash"></i>
                                <p>No notifications yet</p>
                            </div>
                        {:else}
                            {#each notifStore.items.slice(0, 8) as note (note.id)}
                                <button
                                    type="button"
                                    class="notification-item"
                                    class:unread={!note.isRead}
                                    onclick={() => handleNotificationClick(note.id, note.link)}
                                >
                                    <div class="note-icon {note.type}">
                                        <i class="fi {getTypeIcon(note.type)}"></i>
                                    </div>
                                    <div class="note-body">
                                        <p class="note-title">{note.title}</p>
                                        <p class="note-msg">{note.message}</p>
                                        <span class="note-time">{formatTime(note.createdAtUtc)}</span>
                                    </div>
                                    {#if !note.isRead}
                                        <span class="unread-dot"></span>
                                    {/if}
                                </button>
                            {/each}
                        {/if}
                    </div>
                    <div class="dropdown-footer">
                        <a href="/{role}/notifications" onclick={() => showNotifications = false}>View all notifications</a>
                    </div>
                </div>
            {/if}
        </div>

        <!-- User Profile Center -->
        <div class="header-action-wrapper">
            <button 
                class="header-btn profile-trigger" 
                class:active={showProfile}
                onclick={toggleProfile}
            >
                <div class="header-avatar">{profileInitial}</div>
                <i class="fi fi-rr-angle-small-down"></i>
            </button>

            {#if showProfile}
                <div class="dropdown profile-dropdown">
                    <div class="profile-header">
                        <div class="large-avatar">{profileInitial}</div>
                        <div class="profile-info">
                            <h4>{currentUser?.fullName ?? (role === 'admin' ? 'Administrator' : 'Client User')}</h4>
                            <span>{currentUser?.email ?? (role === 'admin' ? 'admin@ntb.gov.np' : 'client@ntb.gov.np')}</span>
                            <span class="profile-role-chip">{profileRoleLabel}</span>
                        </div>
                    </div>
                    <div class="dropdown-content">
                        <a href="/{role}/settings" class="dropdown-link">
                            <i class="fi fi-rr-user"></i> My Profile
                        </a>
                        <a href="/{role}/settings" class="dropdown-link">
                            <i class="fi fi-rr-settings-sliders"></i> Account Settings
                        </a>
                        <div class="dropdown-divider"></div>
                        <button class="dropdown-link logout-link" onclick={handleLogout}>
                            <i class="fi fi-rr-exit"></i> Sign Out
                        </button>
                    </div>
                </div>
            {/if}
        </div>
    </div>
</header>

{#if showNotifications || showProfile}
    <!-- svelte-ignore a11y_click_events_have_key_events -->
    <!-- svelte-ignore a11y_no_static_element_interactions -->
    <div class="overlay" onclick={() => { showNotifications = false; showProfile = false; }}></div>
{/if}

<style>
    @import url("https://fonts.googleapis.com/css2?family=Inter:wght@300;400;500;600;700&display=swap");

    .app-header {
        display: flex;
        align-items: center;
        justify-content: space-between;
        padding: 0 32px;
        height: 72px;
        background: #fff;
        border-bottom: 1px solid #e2e8f0;
        box-shadow: 0 1px 8px rgba(0, 0, 0, 0.04);
        position: sticky;
        top: 0;
        z-index: 100;
        flex-shrink: 0;
        font-family: "Inter", sans-serif;
    }

    .header-left {
        display: flex;
        align-items: center;
        gap: 12px;
    }

    .sidebar-toggle {
        display: flex;
        width: 40px;
        height: 40px;
        align-items: center;
        justify-content: center;
        background: white;
        border: 1px solid #e2e8f0;
        border-radius: 8px;
        color: #1e293b;
        font-size: 1.25rem;
        cursor: pointer;
        transition: all 0.2s;
    }

    .sidebar-toggle:hover {
        background: #f8fafc;
        border-color: var(--color-primary);
        color: var(--color-primary);
    }

    .page-context {
        display: flex;
        flex-direction: column;
    }

    .page-title {
        font-size: 1.2rem;
        font-weight: 700;
        color: #1e293b;
        margin: 0;
        line-height: 1.2;
    }

    @media screen and (max-width: 1024px) {
        .breadcrumb {
            display: none !important;
        }

        .page-title {
            font-size: 1.1rem;
        }
    }

    .breadcrumb { display: flex; align-items: center; gap: 6px; font-size: 12px; }
    .bc-root { color: #94a3b8; font-weight: 500; text-decoration: none; transition: color 0.15s; }
    .bc-root:hover { color: #c8102e; }
    .bc-sep { color: #cbd5e1; font-size: 14px; }
    .bc-link { color: #94a3b8; font-weight: 500; text-decoration: none; }
    .bc-link:hover { color: #c8102e; }
    .bc-current { color: #64748b; font-weight: 500; }

    .header-right { display: flex; align-items: center; gap: 16px; }

    .header-action-wrapper { position: relative; }

    .header-btn {
        width: 40px;
        height: 40px;
        border-radius: 10px;
        border: 1px solid #e2e8f0;
        background: #f8fafc;
        cursor: pointer;
        display: flex;
        align-items: center;
        justify-content: center;
        transition: all 0.2s ease;
        color: #64748b;
        position: relative;
    }

    .header-btn:hover, .header-btn.active {
        background: #fff;
        border-color: #c8102e;
        color: #c8102e;
        box-shadow: 0 4px 12px rgba(200, 16, 46, 0.1);
    }

    .header-btn .badge {
        position: absolute;
        top: -5px;
        right: -5px;
        background: #c8102e;
        color: white;
        font-size: 10px;
        font-weight: 700;
        min-width: 18px;
        height: 18px;
        border-radius: 9px;
        display: flex;
        align-items: center;
        justify-content: center;
        border: 2px solid #fff;
    }

    .profile-trigger {
        width: auto;
        padding: 0 8px 0 6px;
        gap: 6px;
    }

    .header-avatar {
        width: 28px;
        height: 28px;
        border-radius: 8px;
        background: linear-gradient(135deg, #c8102e, #8b0000);
        color: white;
        font-size: 12px;
        font-weight: 700;
        display: flex;
        align-items: center;
        justify-content: center;
    }

    /* ── Dropdowns ────────────────────────────────── */
    .dropdown {
        position: absolute;
        top: calc(100% + 12px);
        right: 0;
        width: 320px;
        background: white;
        border-radius: 12px;
        border: 1px solid #e2e8f0;
        box-shadow: 0 10px 25px rgba(0, 0, 0, 0.1);
        overflow: hidden;
        animation: dropIn 0.2s ease;
    }

    @keyframes dropIn {
        from { opacity: 0; transform: translateY(10px); }
        to { opacity: 1; transform: translateY(0); }
    }

    .dropdown-header {
        padding: 16px 20px;
        border-bottom: 1px solid #f1f5f9;
        display: flex;
        justify-content: space-between;
        align-items: center;
    }

    .dropdown-header h3 { font-size: 15px; font-weight: 700; color: #1e293b; }
    .dropdown-header button { font-size: 12px; color: #c8102e; font-weight: 600; cursor: pointer; }

    .dropdown-content { max-height: 400px; overflow-y: auto; }

    .notification-item {
        padding: 14px 20px;
        display: flex;
        align-items: flex-start;
        gap: 12px;
        border-bottom: 1px solid #f1f5f9;
        transition: background 0.2s;
        cursor: pointer;
        width: 100%;
        background: none;
        border-left: none;
        border-right: none;
        border-top: none;
        text-align: left;
    }

    .notification-item:hover { background: #f8fafc; }
    .notification-item.unread { background: rgba(200, 16, 46, 0.025); }

    .note-icon {
        width: 36px;
        height: 36px;
        border-radius: 10px;
        display: flex;
        align-items: center;
        justify-content: center;
        flex-shrink: 0;
    }

    .note-icon.approval_request { background: rgba(245, 158, 11, 0.12); color: #d97706; }
    .note-icon.approved { background: rgba(34, 197, 94, 0.12); color: #16a34a; }
    .note-icon.rejected { background: rgba(239, 68, 68, 0.12); color: #dc2626; }
    .note-icon.registration { background: rgba(59, 130, 246, 0.12); color: #3b82f6; }

    .note-body { flex: 1; min-width: 0; }
    .note-title { font-size: 12px; font-weight: 700; color: #1e293b; margin: 0 0 2px; }
    .note-msg { font-size: 12px; color: #475569; line-height: 1.4; margin: 0 0 4px; white-space: normal; overflow: hidden; display: -webkit-box; -webkit-line-clamp: 2; -webkit-box-orient: vertical; }
    .note-time { font-size: 11px; color: #94a3b8; }

    .unread-dot {
        width: 8px; height: 8px; border-radius: 50%;
        background: #c8102e; flex-shrink: 0; margin-left: 4px;
    }

    .unread-chip {
        display: inline-flex; align-items: center; justify-content: center;
        background: #c8102e; color: white; font-size: 10px; font-weight: 700;
        min-width: 18px; height: 18px; border-radius: 9px; padding: 0 4px;
        margin-left: 6px; vertical-align: middle;
    }

    .mark-all-btn { font-size: 12px; color: #c8102e; font-weight: 600; cursor: pointer; background: none; border: none; }

    .note-empty {
        padding: 32px 20px; text-align: center; color: #94a3b8;
        display: flex; flex-direction: column; align-items: center; gap: 8px;
    }
    .note-empty i { font-size: 1.6rem; }
    .note-empty p { margin: 0; font-size: 13px; }

    .dropdown-footer { padding: 12px; text-align: center; border-top: 1px solid #f1f5f9; }
    .dropdown-footer a { font-size: 13px; font-weight: 600; color: #c8102e; text-decoration: none; }

    /* ── Profile Dropdown ─────────────────────────── */
    .profile-dropdown { width: 260px; }
    
    .profile-header {
        padding: 24px 20px;
        background: #f8fafc;
        display: flex;
        flex-direction: column;
        align-items: center;
        gap: 12px;
        border-bottom: 1px solid #f1f5f9;
    }

    .large-avatar {
        width: 60px;
        height: 60px;
        border-radius: 18px;
        background: linear-gradient(135deg, #c8102e, #8b0000);
        color: white;
        font-size: 24px;
        font-weight: 700;
        display: flex;
        align-items: center;
        justify-content: center;
        box-shadow: 0 8px 16px rgba(200, 16, 46, 0.2);
    }

    .profile-info { text-align: center; }
    .profile-info h4 { font-size: 16px; font-weight: 700; color: #1e293b; margin-bottom: 2px; }
    .profile-info span { font-size: 12px; color: #64748b; }
    .profile-role-chip {
        display: inline-flex;
        margin-top: 8px;
        padding: 5px 10px;
        border-radius: 999px;
        background: rgba(200, 16, 46, 0.08);
        color: #9f1239;
        font-weight: 700;
    }

    .dropdown-link {
        display: flex;
        align-items: center;
        gap: 12px;
        padding: 12px 20px;
        color: #475569;
        text-decoration: none;
        font-size: 14px;
        font-weight: 500;
        transition: all 0.2s;
        width: 100%;
        text-align: left;
    }

    .dropdown-link:hover { background: #f8fafc; color: #c8102e; }
    .dropdown-link i { font-size: 16px; width: 20px; }

    .dropdown-divider { height: 1px; background: #f1f5f9; margin: 4px 0; }
    
    .logout-link { color: #ef4444; }
    .logout-link:hover { background: #fff1f2; color: #dc2626; }

    .overlay {
        position: fixed;
        top: 0; left: 0; right: 0; bottom: 0;
        z-index: 90;
    }
</style>
