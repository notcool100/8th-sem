export type FestivalStatus = "draft" | "published" | "archived";

export type CreateFestivalHighlightPayload = {
	icon?: string;
	title: string;
	description: string;
	tone?: "orange" | "blue" | "purple" | "green" | "red" | string;
};

export type CreateFestivalPayload = {
	slug?: string;
	title: string;
	summary: string;
	longDescription: string;
	category: string;
	status?: FestivalStatus;
	date_ad: string;
	end_date_ad?: string;
	date_bs?: string;
	end_date_bs?: string;
	color?: string;
	region: string;
	address?: string;
	latitude?: number;
	longitude?: number;
	dateRangeLabel?: string;
	durationLabel?: string;
	image: string[];
	organizer: string;
	organizerSubtitle?: string;
	organizerVerified?: boolean;
	organizerImageUrl?: string;
	highlights: CreateFestivalHighlightPayload[];
	featured: boolean;
	readTime?: string;
	isHoliday?: boolean;
};

export type FestivalHighlightDto = {
	icon: string;
	title: string;
	description: string;
	tone: string;
};

export type FestivalDto = {
	id: number;
	slug: string;
	title: string;
	date_ad: string;
	end_date_ad: string;
	date_bs: string;
	end_date_bs: string;
	color: string;
	region: string;
	address: string;
	latitude: number | null;
	longitude: number | null;
	category: string;
	status: FestivalStatus;
	summary: string;
	longDescription: string;
	dateRangeLabel: string;
	durationLabel: string;
	image: string[];
	organizer: string;
	organizerSubtitle: string;
	organizerVerified: boolean;
	organizerImageUrl?: string | null;
	highlights: FestivalHighlightDto[];
	featured: boolean;
	readTime: string;
	isHoliday: boolean;
};
