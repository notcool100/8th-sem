<script>
    import { getContext } from "svelte";
    import { activeModal } from "./store.js";

    const modalId = getContext("modalId");

    function close() {
        activeModal.set(null);
    }
</script>

{#if $activeModal === modalId}
    <div>
        <div class="modal">
            <div class="content">
                <div class="close-button" on:click={close}>&times;</div>
                <slot />
            </div>
        </div>
    </div>
{/if}

<style>
    .modal {
        position: fixed;
        z-index: 9999;
        left: 0;
        top: 0;
        width: 100%;
        height: 100%;
        overflow: auto;
        background-color: rgb(0, 0, 0);
        background-color: rgba(0, 0, 0, 0.009);
    }
    .content {
        position: relative;
        background-color: #fefefe;
        margin: 15% auto;
        padding: 20px;
        border: 1px solid #888;
        width: 80%;
    }
    .close-button {
        position: absolute;
        top: 0;
        right: 0.5rem;
        color: #aaa;
        font-size: 28px;
        font-weight: bold;
        cursor: pointer;
    }
    .close-button:hover,
    .close-button:focus {
        color: black;
        text-decoration: none;
        cursor: pointer;
    }
    .hidden {
        display: none;
    }
</style>
