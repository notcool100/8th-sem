<script lang="ts">
    let {
        view = "month",
        onViewChange
    } = $props<{
        view: "month" | "year";
        onViewChange: (view: "month" | "year") => void;
    }>();
</script>

<div class="view-toggle">
    <button
        class:active={view === "month"}
        onclick={() => onViewChange("month")}
    >
        Month
    </button>
    <button
        class:active={view === "year"}
        onclick={() => onViewChange("year")}
    >
        Year
    </button>
</div>

<style>
    .view-toggle {
        display: inline-flex;
        background: #f1f5f9;
        padding: 4px;
        border-radius: 10px;
        border: 1px solid #e2e8f0;
    }

    button {
        padding: 6px 16px;
        border-radius: 7px;
        font-size: 0.85rem;
        font-weight: 600;
        color: #64748b;
        cursor: pointer;
        transition: all 0.2s;
        border: none;
        background: transparent;
    }

    button.active {
        background: white;
        color: var(--color-dark);
        box-shadow: 0 2px 4px rgba(0,0,0,0.05);
    }

    button:hover:not(.active) {
        color: var(--color-secondary);
    }

    @media (max-width: 700px) {
        button {
            padding: 5px 10px;
            font-size: 0.78rem;
        }
    }
</style>
