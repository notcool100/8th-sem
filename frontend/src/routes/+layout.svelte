<script lang="ts">
	import { browser } from '$app/environment';
	import { onMount } from 'svelte';
	import "../lib/styles/app.css";
	import { initLocale, locale } from '$lib/i18n';
	let { children } = $props();
	let currentLocale = 'en';

	const unsubscribe = locale.subscribe((value) => {
		currentLocale = value;
	});

	if (browser) {
		$: document.documentElement.lang = currentLocale;
	}

	onMount(() => {
		initLocale();
		return unsubscribe;
	});
</script>

<svelte:head>
	<title>Nepal Tourism Board</title>

	<meta
		name="description"
		content="Official Portal of Nepal Tourism Board - Experience the Excellence of Nepal"
	/>
	<link
		rel="stylesheet"
		href="https://cdn-uicons.flaticon.com/uicons-regular-rounded/css/uicons-regular-rounded.css"
	/>
</svelte:head>

<div class="app-container">
	{@render children()}
</div>

<style>
	.app-container {
		min-height: 100vh;
		display: flex;
		flex-direction: column;
	}
</style>
