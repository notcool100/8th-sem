<script>
  import PublicNav from "$lib/components/public/PublicNav.svelte";
  let { children } = $props();
</script>

<PublicNav />

<main>
  {@render children()}
</main>

<style>
  main {
    flex: 1;
    display: flex;
    flex-direction: column;
    padding-top: 86px;
  }

  @media (max-width: 920px) {
    main {
      padding-top: 76px;
    }
  }
</style>
