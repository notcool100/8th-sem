<script>
    import { getContext } from "svelte";
    import { activeModal } from "./store.js";

    const modalId = getContext("modalId");

    function open() {
        activeModal.set(modalId);
    }
</script>

<span on:click={open}>
    <slot />
</span>

<style>
    span:hover {
        cursor: pointer;
    }
</style>
