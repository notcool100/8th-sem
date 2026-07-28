<script>
    import { setContext } from "svelte";

    let { modalId } = $props();

    setContext("modalId", modalId);
</script>

<div>
    <slot />
</div>
