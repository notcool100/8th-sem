<script>
    import { NTB_LOGO_URL } from "$lib/constants/branding";

    // Client/Partner Portal Dashboard
</script>

<div class="client-container">
    <header class="client-header">
        <div class="container header-content">
            <div class="logo">
                <img src={NTB_LOGO_URL} alt="Nepal Tourism Board" class="ntb-logo" />
            </div>
            <div class="client-actions">
                <span>Welcome, Trekking Agency Alpha</span>
                <button class="btn btn-primary btn-sm">Log Out</button>
            </div>
        </div>
    </header>

    <main class="container client-main">
        <div class="welcome-banner glass animate-fade-in">
            <h1>Partner Dashboard</h1>
            <p>
                Manage your listings and view upcoming events for the 2026
                tourism season.
            </p>
        </div>

        <div class="content-grid">
            <div class="card glass">
                <h3>Your Bookings</h3>
                <p>12 confirmed, 4 pending</p>
                <button class="btn btn-accent btn-sm">View Details</button>
            </div>
            <div class="card glass">
                <h3>Active Listings</h3>
                <p>8 Destinations, 3 Events</p>
                <button class="btn btn-accent btn-sm">Manage</button>
            </div>
            <div class="card glass">
                <h3>Rewards & Points</h3>
                <p>2,450 NTB Points</p>
                <button class="btn btn-accent btn-sm">Redeem</button>
            </div>
        </div>

        <section class="announcements glass">
            <h3>NTB Announcements</h3>
            <ul class="announcement-list">
                <li>New safety protocols for Everest Base Camp treks.</li>
                <li>
                    Subsidies available for sustainable tourism initiatives.
                </li>
                <li>Annual Tourism Meetup: Kathmandu, June 15th.</li>
            </ul>
        </section>
    </main>
</div>

<style>
    .client-container {
        background-color: #f8fafc;
        min-height: 100vh;
    }

    .client-header {
        background-color: white;
        border-bottom: 1px solid #e2e8f0;
        padding: 1rem 0;
    }

    .header-content {
        display: flex;
        justify-content: space-between;
        align-items: center;
    }

    .logo {
        display: inline-flex;
        align-items: center;
    }

    .ntb-logo {
        height: 44px;
        width: auto;
        display: block;
    }

    .client-actions {
        display: flex;
        align-items: center;
        gap: 1.5rem;
        font-size: 0.9rem;
        color: var(--color-text-muted);
    }

    .client-main {
        padding-top: 3rem;
        padding-bottom: 3rem;
        display: flex;
        flex-direction: column;
        gap: 2.5rem;
    }

    .welcome-banner {
        padding: 3rem;
        border-radius: 20px;
        background: linear-gradient(135deg, var(--color-primary), #2c5282);
        color: white;
    }

    .welcome-banner h1 {
        color: white;
        margin-bottom: 0.5rem;
    }

    .content-grid {
        display: grid;
        grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
        gap: 2rem;
    }

    .card {
        padding: 2rem;
        display: flex;
        flex-direction: column;
        gap: 1rem;
        align-items: flex-start;
    }

    .announcements {
        padding: 2rem;
    }

    .announcement-list {
        margin-top: 1.5rem;
        list-style: none;
        display: flex;
        flex-direction: column;
        gap: 1rem;
    }

    .announcement-list li {
        padding-left: 1.5rem;
        position: relative;
    }

    .announcement-list li::before {
        content: "→";
        position: absolute;
        left: 0;
        color: var(--color-accent);
    }

    .btn-sm {
        padding: 0.5rem 1rem;
        font-size: 0.85rem;
    }
</style>
