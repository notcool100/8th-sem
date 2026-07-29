import type { PageServerLoad } from "./$types";
import { BackendApiError, getPublicEvents, getPublicFestivals, fetchCalendarFilterSettings, fetchCategories, searchEvents } from "$lib/server/auth/api";

export const load: PageServerLoad = async ({ url }) => {
	const search = url.searchParams.get("search")?.trim();

	const [eventsResult, festivalsResult, filterSettingsResult, categoriesResult] = await Promise.allSettled([
		search ? searchEvents(search) : getPublicEvents(),
		getPublicFestivals(),
		fetchCalendarFilterSettings(),
		fetchCategories()
	]);

	const events = eventsResult.status === "fulfilled" ? eventsResult.value : [];
	const festivals = festivalsResult.status === "fulfilled" ? festivalsResult.value : [];
	const categories = categoriesResult.status === "fulfilled" ? categoriesResult.value : [];
	const eventsError =
		eventsResult.status === "rejected" && eventsResult.reason instanceof BackendApiError
			? { message: eventsResult.reason.message, status: eventsResult.reason.status }
			: null;

	// Filter settings fall back gracefully — all visible if the request fails
	const filterSettings =
		filterSettingsResult.status === "fulfilled"
			? filterSettingsResult.value
			: { showCategory: true, showDateRange: true, showLocation: true, showPrice: true, showTags: true, isSundayHoliday: true };

	return {
		events,
		festivals,
		categories,
		filterSettings,
		search: search ?? "",
		...(eventsError ? { error: eventsError } : {})
	};
};
