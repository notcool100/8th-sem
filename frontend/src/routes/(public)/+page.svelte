<script>
  // import Hero from "$lib/components/public/Hero.svelte";
  // import UpcommingEvents from "$lib/components/public/UpcommingEvents.svelte";
  import EventsLandingSection from "$lib/components/public/EventsLandingSection.svelte";

  let { data } = $props();
</script>

<!-- <Hero /> -->
<!-- <UpcommingEvents events={data.events} error={data.error} filterSettings={data.filterSettings}/> -->
<EventsLandingSection
  events={data.events}
  festivals={data.festivals}
  categories={data.categories}
  error={data.error}
  filterSettings={data.filterSettings}
/>
