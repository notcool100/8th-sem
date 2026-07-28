<script lang="ts">
    import { page } from "$app/state";
    import { goto } from "$app/navigation";
    import { NTB_LOGO_URL } from "$lib/constants/branding";
    import type { SessionUser } from "$lib/types/auth";

    async function handleLogout() {
        await fetch("/logout", { method: "POST" });
        await goto("/login");
    }

    // Types for the Sidebar
    type NavLink = {
        type: "link";
        href: string;
        icon: string;
        label: string;
    };

    type NavDropdown = {
        type: "dropdown";
        key: string;
        icon: string;
        label: string;
        children: { href: string; label: string }[];
    };

    type NavSection = {
        section: string;
        items: (NavLink | NavDropdown)[];
    };

    // Props
    let { role = "admin", isOpen = false, isMobileView = false, onToggle } = $props<{ 
        role?: "admin" | "client", 
        isOpen?: boolean,
        isMobileView?: boolean,
        onToggle?: () => void
    }>();

    const adminNav: NavSection[] = [
        {
            section: "Main",
            items: [
                {
                    type: "link",
                    href: "/admin/dashboard",
                    icon: "fi fi-rr-apps",
                    label: "Dashboard",
                },
            ],
        },
        {
            section: "Content",
            items: [
                {
                    type: "dropdown",
                    key: "events",
                    icon: "fi fi-rr-calendar",
                    label: "Events Management",
                    children: [
                        { href: "/admin/events", label: "All Events" },
                        { href: "/admin/events/create", label: "Create Event" },
                        {
                            href: "/admin/events/pending-approval",
                            label: "Pending Approval",
                        },
                    ],
                },
                {
                    type: "dropdown",
                    key: "festivals",
                    icon: "fi fi-rr-confetti",
                    label: "Festivals Management",
                    children: [
                        { href: "/admin/festivals", label: "All Festivals" },
                        { href: "/admin/festivals/create", label: "Create Festival" },
                    ],
                },
                {
                    type: "dropdown",
                    key: "calendar",
                    icon: "fi fi-rr-calendar-lines",
                    label: "Calendar Management",
                    children: [
                        {
                            href: "/admin/calender/calender_setup",
                            label: "Calendar Display (AD/BS)",
                        },
                        {
                            href: "/admin/calender/colors",
                            label: "Event Color Setup",
                        },
                    ],
                },
                // {
                //     type: "link",
                //     href: "/admin/destinations",
                //     icon: "fi fi-rr-map-marker",
                //     label: "Destinations",
                // },
                {
                    type: "link",
                    href: "/admin/email-templates",
                    icon: "fi fi-rr-envelope",
                    label: "Email Templates",
                },
            ],
        },
        {
            section: "Management",
            items: [
                {
                    type: "link",
                    href: "/admin/users",
                    icon: "fi fi-rr-users",
                    label: "Users Management",
                },
                {
                    type: "link",
                    href: "/admin/departments",
                    icon: "fi fi-rr-building",
                    label: "Department Setup",
                },
                {
                    type: "link",
                    href: "/admin/attendees",
                    icon: "fi fi-rr-ticket",
                    label: "Attendees / Registrations",
                },
                {
                    type: "link",
                    href: "/admin/check-in",
                    icon: "fi fi-rr-qr-scan",
                    label: "Event Check-in",
                },
                {
                    type: "link",
                    href: "/admin/categories-setup",
                    icon: "fi fi-rr-label",
                    label: "Categories Setup",
                },
            ],
        },
        {
            section: "Analytics",
            items: [
                {
                    type: "link",
                    href: "/admin/reports",
                    icon: "fi fi-rr-stats",
                    label: "Reports & Analytics",
                },
                {
                    type: "link",
                    href: "/admin/notifications",
                    icon: "fi fi-rr-bell",
                    label: "Notifications",
                },
            ],
        },
        {
            section: "System",
            items: [
                {
                    type: "link",
                    href: "/admin/settings",
                    icon: "fi fi-rr-settings",
                    label: "Settings",
                },
            ],
        },
    ];

    const clientNav: NavSection[] = [
        {
            section: "Main",
            items: [
                {
                    type: "link",
                    href: "/client/dashboard",
                    icon: "fi fi-rr-apps",
                    label: "Dashboard",
                },
            ],
        },
        {
            section: "Events Management",
            items: [
                {
                    type: "link",
                    href: "/client/events",
                    icon: "fi fi-rr-calendar",
                    label: "My Events",
                },
                {
                    type: "link",
                    href: "/client/events/create",
                    icon: "fi fi-rr-add",
                    label: "Create Event",
                },
                {
                    type: "link",
                    href: "/client/calendar",
                    icon: "fi fi-rr-calendar-lines",
                    label: "Calendar View",
                },
            ],
        },
        {
            section: "Engagement",
            items: [
                {
                    type: "link",
                    href: "/client/attendees",
                    icon: "fi fi-rr-ticket",
                    label: "Attendees / RSVP",
                },
                {
                    type: "link",
                    href: "/client/invitations",
                    icon: "fi fi-rr-envelope",
                    label: "Invitations",
                },
            ],
        },
        {
            section: "System",
            items: [
                {
                    type: "link",
                    href: "/client/settings",
                    icon: "fi fi-rr-settings",
                    label: "Account Settings",
                },
            ],
        },
    ];

    // Dynamic selection based on role
    const currentUser = $derived((page.data.session?.user ?? null) as SessionUser | null);
    const isSuperAdmin = $derived(currentUser?.role === "superadmin");
    const userInitial = $derived(currentUser?.fullName?.charAt(0).toUpperCase() ?? (role === "admin" ? "A" : "C"));
    const userRoleLabel = $derived(
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

    const userPermissions = $derived(currentUser?.permissions ?? []);

    function hasViewPermission(navKey: string): boolean {
        if (isSuperAdmin) return true;
        return userPermissions.some(p => p.navItemKey === navKey && p.canView);
    }

    // Map nav item hrefs to nav_items.key values
    const navKeyMap: Record<string, string> = {
        "/admin/dashboard": "dashboard",
        "/admin/events": "events",
        "/admin/events/create": "events",
        "/admin/events/pending-approval": "events",
        "/admin/festivals": "festivals",
        "/admin/festivals/create": "festivals",
        "/admin/calender/calender_setup": "calendar",
        "/admin/calender/colors": "calendar",
        "/admin/destinations": "destinations",
        "/admin/email-templates": "email_templates",
        "/admin/users": "users",
        "/admin/departments": "departments",
        "/admin/attendees": "attendees",
        "/admin/check-in": "check_in",
        "/admin/categories-setup": "categories",
        "/admin/reports": "reports",
        "/admin/notifications": "notifications",
        "/admin/settings": "settings",
    };

    function itemNavKey(item: NavLink | NavDropdown): string {
        if (item.type === "link") return navKeyMap[item.href] ?? item.href;
        if (item.type === "dropdown") {
            return navKeyMap[item.children[0]?.href ?? ""] ?? item.key;
        }
        return "";
    }

    const navConfig = $derived(
        (() => {
            const baseConfig = role === "admin" ? adminNav : clientNav;
            if (role !== "admin") return baseConfig;
            if (isSuperAdmin) return baseConfig;

            return baseConfig
                .map((section) => ({
                    ...section,
                    items: section.items.filter((item) => hasViewPermission(itemNavKey(item)))
                }))
                .filter((section) => section.items.length > 0);
        })()
    );

    let activeDropdown = $state<string | null>(null);

    function toggle(key: string) {
        activeDropdown = activeDropdown === key ? null : key;
    }

    function isActive(href: string): boolean {
        return page.url.pathname === href;
    }

    function isParentActive(prefix: string): boolean {
        return page.url.pathname.startsWith(prefix);
    }

    function handleDropdownClick(item: NavDropdown) {
        if (!isOpen && !isMobileView) {
            const fallback = item.children[0]?.href ?? `/${role}/${item.key}`;
            void goto(fallback);
            return;
        }

        toggle(item.key);
    }
</script>

<aside
    class="sidebar"
    class:drawer-open={isOpen}
    class:collapsed={!isOpen && !isMobileView}
>
    <div class="sidebar-header-mobile">
        <button class="close-btn" onclick={onToggle} aria-label="Close sidebar">
            <i class="fi fi-rr-cross"></i>
        </button>
    </div>
    <div class="sidebar-brand">
        <a href={role === "admin" ? "/admin/dashboard" : "/client/dashboard"} class="brand-logo">
            <div class="logo-container">
                <img src={NTB_LOGO_URL} alt="Nepal Tourism Board" class="ntb-logo" />
            </div>
        </a>
        <div class="brand-divider"></div>
    </div>

    <nav class="sidebar-nav" aria-label="Main navigation">
        {#each navConfig as section}
            <div class="nav-section-label">{section.section}</div>

            {#each section.items as item}
                {#if item.type === "link"}
                    <a
                        href={item.href}
                        class="nav-item"
                        class:active={isActive(item.href)}
                        title={!isOpen && !isMobileView ? item.label : undefined}
                        aria-current={isActive(item.href) ? "page" : undefined}
                    >
                        <i class="nav-icon {item.icon}" aria-hidden="true"></i>
                        <span class="nav-label">{item.label}</span>
                    </a>
                {:else if item.type === "dropdown"}
                    <div class="nav-group">
                        <button
                            class="nav-item nav-dropdown-trigger"
                            class:active={isParentActive(
                                `/${role}/${item.key}`,
                            )}
                            onclick={() => handleDropdownClick(item)}
                            title={!isOpen && !isMobileView ? item.label : undefined}
                            aria-expanded={isOpen || isMobileView ? activeDropdown === item.key : undefined}
                        >
                            <i class="nav-icon {item.icon}" aria-hidden="true"
                            ></i>
                            <span class="nav-label">{item.label}</span>
                            <span
                                class="nav-chevron"
                                class:rotated={activeDropdown === item.key}
                                aria-hidden="true">›</span
                            >
                        </button>

                        {#if activeDropdown === item.key && (isOpen || isMobileView)}
                            <div class="nav-submenu">
                                {#each item.children as child}
                                    <a
                                        href={child.href}
                                        class="nav-subitem"
                                        class:active={isActive(child.href)}
                                        aria-current={isActive(child.href)
                                            ? "page"
                                            : undefined}
                                    >
                                        {child.label}
                                    </a>
                                {/each}
                            </div>
                        {/if}
                    </div>
                {/if}
            {/each}
        {/each}
    </nav>

    <!-- Footer -->
    <div class="sidebar-footer">
        <div class="sidebar-user">
            <div class="user-avatar" aria-hidden="true">
                {userInitial}
            </div>
            <div class="user-info">
                <span class="user-name">{currentUser?.fullName ?? (role === "admin" ? "Administrator" : "Client User")}</span>
                <span class="user-role">{userRoleLabel}</span>
            </div>
            <button
                class="user-logout"
                onclick={handleLogout}
                title="Log out"
                aria-label="Log out"
            >
                <i class="fi fi-rr-exit"></i>
            </button>
        </div>
    </div>
</aside>

<style>
    @import url("https://fonts.googleapis.com/css2?family=Inter:wght@300;400;500;600;700&display=swap");

    .sidebar {
        width: 270px;
        height: 100vh;
        background: #1e293b;
        color: #fff;
        display: flex;
        flex-direction: column;
        position: fixed;
        left: 0;
        top: 0;
        bottom: 0;
        z-index: 1000;
        transform: translateX(0);
        overflow-y: auto;
        overflow-x: hidden;
        border-right: 1px solid rgba(255, 255, 255, 0.1);
        box-shadow: 0 0 0 rgba(0, 0, 0, 0);
        transition:
            width 0.25s ease,
            transform 0.3s cubic-bezier(0.4, 0, 0.2, 1),
            box-shadow 0.3s ease;
    }

    .sidebar.drawer-open {
        box-shadow: 20px 0 50px rgba(0, 0, 0, 0.35);
    }

    .sidebar.collapsed {
        width: 86px;
    }

    .sidebar.collapsed .sidebar-brand {
        padding-top: 6px;
    }

    .sidebar.collapsed .brand-logo {
        padding: 10px 8px 12px;
    }

    .sidebar.collapsed .logo-container {
        max-width: 56px;
        padding: 4px;
    }

    .sidebar.collapsed .ntb-logo {
        max-height: 44px;
    }

    .sidebar.collapsed .brand-divider {
        margin: 0 10px;
    }

    .sidebar.collapsed .sidebar-nav {
        padding: 10px 8px;
    }

    .sidebar.collapsed .nav-section-label {
        display: none;
    }

    .sidebar.collapsed .nav-item {
        justify-content: center;
        padding: 10px 8px;
        gap: 0;
    }

    .sidebar.collapsed .nav-label,
    .sidebar.collapsed .nav-chevron {
        display: none;
    }

    .sidebar.collapsed .nav-submenu {
        display: none;
    }

    .sidebar.collapsed .sidebar-footer {
        padding: 10px 8px 12px;
    }

    .sidebar.collapsed .sidebar-user {
        justify-content: center;
    }

    .sidebar.collapsed .user-avatar,
    .sidebar.collapsed .user-info {
        display: none;
    }

    .sidebar-header-mobile {
        display: none;
        padding: 16px;
        justify-content: flex-end;
    }

    .close-btn {
        background: rgba(255, 255, 255, 0.1);
        border: 1px solid rgba(255, 255, 255, 0.2);
        color: white;
        width: 36px;
        height: 36px;
        border-radius: 8px;
        display: flex;
        align-items: center;
        justify-content: center;
        cursor: pointer;
    }

    @media (max-width: 1024px) {
        .sidebar {
            width: min(86vw, 320px);
            transform: translateX(-100%);
        }

        .sidebar.drawer-open {
            transform: translateX(0);
        }

        .sidebar.collapsed {
            width: min(86vw, 320px);
        }

        .sidebar.collapsed .sidebar-brand {
            padding-top: 10px;
        }

        .sidebar.collapsed .brand-logo {
            padding: 16px;
        }

        .sidebar.collapsed .logo-container {
            max-width: 180px;
            padding: 10px;
        }

        .sidebar.collapsed .ntb-logo {
            max-height: 70px;
        }

        .sidebar.collapsed .brand-divider {
            margin: 0 16px;
        }

        .sidebar.collapsed .sidebar-nav {
            padding: 12px 12px 8px;
        }

        .sidebar.collapsed .nav-section-label {
            display: block;
        }

        .sidebar.collapsed .nav-item {
            justify-content: flex-start;
            padding: 10px 14px;
            gap: 12px;
        }

        .sidebar.collapsed .nav-label,
        .sidebar.collapsed .nav-chevron {
            display: initial;
        }

        .sidebar.collapsed .sidebar-footer {
            padding: 12px 16px 16px;
        }

        .sidebar.collapsed .sidebar-user {
            justify-content: flex-start;
        }

        .sidebar.collapsed .user-avatar,
        .sidebar.collapsed .user-info {
            display: flex;
        }

        .sidebar-header-mobile {
            display: flex;
        }
    }

    .sidebar-brand {
        flex-shrink: 0;
        padding-top: 10px;
    }
    
    .brand-logo {
        display: flex;
        flex-direction: column;
        align-items: center;
        gap: 8px;
        text-decoration: none;
        padding: 16px;
    }

    .logo-container {
        width: 100%;
        max-width: 180px;
        padding: 10px;
        display: flex;
        align-items: center;
        justify-content: center;
    }

    .ntb-logo {
        width: 100%;
        max-height: 70px;
        height: auto;
        object-fit: contain;
    }

    .brand-divider {
        height: 1px;
        background: linear-gradient(
            90deg,
            transparent,
            rgba(248, 206, 28, 0.4),
            rgba(28, 92, 109, 0.3),
            transparent
        );
        margin: 0 16px;
    }

    .sidebar-nav {
        flex: 1;
        padding: 12px 12px 8px;
        display: flex;
        flex-direction: column;
        gap: 2px;
    }

    .nav-section-label {
        font-size: 10px;
        font-weight: 600;
        letter-spacing: 1.5px;
        text-transform: uppercase;
        color: var(--color-primary);
        opacity: 0.7;
        padding: 12px 10px 6px;
        margin-top: 4px;
    }

    .nav-item {
        display: flex;
        align-items: center;
        gap: 12px;
        padding: 10px 14px;
        border-radius: 8px;
        color: rgba(255, 255, 255, 0.65);
        text-decoration: none;
        font-size: 13.5px;
        font-weight: 450;
        cursor: pointer;
        border: none;
        background: transparent;
        width: 100%;
        text-align: left;
        transition: all 0.18s ease;
        position: relative;
        letter-spacing: 0.1px;
    }

    .nav-item:hover {
        background: rgba(255, 255, 255, 0.07);
        color: #fff;
    }
    .nav-item.active {
        background: linear-gradient(
            135deg,
            rgba(248, 206, 28, 0.15),
            rgba(248, 206, 28, 0.05)
        );
        color: var(--color-primary);
        font-weight: 600;
        border-left: 3px solid var(--color-primary);
        box-shadow: 0 2px 12px rgba(248, 206, 28, 0.1);
    }

    .nav-item.active .nav-icon {
        color: var(--color-primary);
        opacity: 1;
    }

    .nav-icon {
        font-size: 16px;
        width: 20px;
        text-align: center;
        flex-shrink: 0;
        opacity: 0.7;
        transition: all 0.2s;
    }

    .nav-label {
        flex: 1;
        white-space: nowrap;
        overflow: hidden;
        text-overflow: ellipsis;
    }
    .nav-chevron {
        font-size: 16px;
        color: rgba(255, 255, 255, 0.35);
        transition: transform 0.22s ease;
        margin-left: auto;
    }
    .nav-chevron.rotated {
        transform: rotate(90deg);
        color: #c8102e;
    }

    .nav-submenu {
        display: flex;
        flex-direction: column;
        padding: 4px 0 4px 44px;
        gap: 1px;
        animation: slideDown 0.18s ease;
    }
    @keyframes slideDown {
        from {
            opacity: 0;
            transform: translateY(-6px);
        }
        to {
            opacity: 1;
            transform: translateY(0);
        }
    }

    .nav-subitem {
        display: block;
        padding: 7px 12px;
        border-radius: 6px;
        color: rgba(255, 255, 255, 0.5);
        text-decoration: none;
        font-size: 12.5px;
        font-weight: 400;
        transition: all 0.15s ease;
        border-left: 2px solid transparent;
    }

    .nav-subitem:hover {
        color: rgba(255, 255, 255, 0.85);
        background: rgba(255, 255, 255, 0.06);
        border-left-color: rgba(200, 16, 46, 0.5);
    }
    .nav-subitem.active {
        color: #fff;
        font-weight: 600;
        background: rgba(200, 16, 46, 0.15);
        border-left-color: #c8102e;
    }

    .sidebar-footer {
        flex-shrink: 0;
        padding: 12px 16px 16px;
        border-top: 1px solid rgba(255, 255, 255, 0.07);
        background: rgba(0, 0, 0, 0.2);
    }
    .sidebar-user {
        display: flex;
        align-items: center;
        gap: 10px;
    }
    .user-avatar {
        width: 34px;
        height: 34px;
        border-radius: 50%;
        background: linear-gradient(135deg, #c8102e, #8b0000);
        color: white;
        font-size: 14px;
        font-weight: 700;
        display: flex;
        align-items: center;
        justify-content: center;
        border: 2px solid rgba(212, 175, 55, 0.4);
    }
    .user-info {
        display: flex;
        flex-direction: column;
        gap: 1px;
        flex: 1;
        overflow: hidden;
    }
    .user-name {
        font-size: 13px;
        font-weight: 600;
        color: #fff;
        white-space: nowrap;
        overflow: hidden;
        text-overflow: ellipsis;
    }
    .user-role {
        font-size: 10.5px;
        color: rgba(212, 175, 55, 0.7);
    }
    .user-logout {
        width: 28px;
        height: 28px;
        border-radius: 6px;
        border: 1px solid rgba(255, 255, 255, 0.1);
        background: transparent;
        color: rgba(255, 255, 255, 0.35);
        cursor: pointer;
        display: flex;
        align-items: center;
        justify-content: center;
        transition: all 0.15s ease;
    }
    .user-logout:hover {
        background: rgba(200, 16, 46, 0.2);
        border-color: rgba(200, 16, 46, 0.4);
        color: #c8102e;
    }
</style>
