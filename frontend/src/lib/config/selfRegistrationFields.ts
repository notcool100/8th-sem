/**
 * Registry of every field the public self-registration form can collect for an event.
 * Shared by the admin create/edit event form (field picker) and the public
 * /register/[slug] form (dynamic rendering) so labels/options never drift apart.
 *
 * `fullName` and `email` are locked: Guest requires them for identification,
 * so they're always collected regardless of what the admin toggles.
 */

export type SelfRegFieldType =
	| "text"
	| "email"
	| "tel"
	| "url"
	| "number"
	| "textarea"
	| "select"
	| "radio"
	| "checkboxGroup"
	| "checkbox";

export interface SelfRegFieldDef {
	key: string;
	label: string;
	type: SelfRegFieldType;
	/** Mirrors ntb_register's `req` — whether the field is required when enabled. */
	required?: boolean;
	/** Always collected; can't be disabled by the admin. */
	locked?: boolean;
	placeholder?: string;
	/** Options for select/radio/checkboxGroup. */
	options?: string[];
	/** Only show this field when another field currently has a given value. */
	conditional?: { field: string; equals: string };
}

export interface SelfRegFieldSection {
	id: string;
	title: string;
	fields: SelfRegFieldDef[];
}

export const SELF_REGISTRATION_FIELD_SECTIONS: SelfRegFieldSection[] = [
	{
		id: "personal",
		title: "Personal Info",
		fields: [
			{ key: "fullName", label: "Full Name", type: "text", required: true, locked: true, placeholder: "Your full name" },
			{ key: "gender", label: "Gender", type: "radio", required: true, options: ["Male", "Female", "Other", "Prefer not to say"] },
			{ key: "age", label: "Age", type: "number", placeholder: "e.g. 28" },
			{ key: "nationality", label: "Nationality", type: "text", required: true, placeholder: "e.g. Nepali" },
			{ key: "mobile", label: "Mobile Number", type: "tel", required: true, placeholder: "+977 98X-XXXXXXX" },
			{ key: "email", label: "Email Address", type: "email", required: true, locked: true, placeholder: "you@example.com" },
			{ key: "cityCountry", label: "City / Country", type: "text", required: true, placeholder: "e.g. Kathmandu, Nepal" },
			{ key: "tiktok", label: "TikTok Handle", type: "text", placeholder: "TikTok username" },
			{ key: "instagram", label: "Instagram Handle", type: "text", placeholder: "Instagram handle" },
			{ key: "youtube", label: "YouTube Channel", type: "text", placeholder: "YouTube channel" },
			{ key: "facebook", label: "Facebook Profile/Page", type: "text", placeholder: "Facebook profile/page" }
		]
	},
	{
		id: "professional",
		title: "Professional Info",
		fields: [
			{ key: "orgName", label: "Organization / Company", type: "text", placeholder: "Company or agency name" },
			{ key: "designation", label: "Designation / Role", type: "text", placeholder: "Your position" },
			{
				key: "sector",
				label: "Tourism Sector",
				type: "radio",
				required: true,
				options: ["Tour Operator", "Trekking Agency", "Hotel / Resort", "Adventure Operator", "Travel Media", "Freelancer", "Content Creator", "Other"]
			},
			{ key: "sectorOther", label: "Please specify your sector", type: "text", placeholder: "Describe your sector", conditional: { field: "sector", equals: "Other" } },
			{ key: "experience", label: "Years of Experience", type: "select", options: ["Less than 1 year", "1–3 years", "3–5 years", "5–10 years", "10+ years"] },
			{ key: "website", label: "Website", type: "url", placeholder: "https://yourwebsite.com" }
		]
	},
	{
		id: "content",
		title: "Content Background",
		fields: [
			{ key: "creatingContent", label: "Are you currently creating content?", type: "radio", required: true, options: ["Yes", "No"] },
			{ key: "platforms", label: "Platforms you use", type: "checkboxGroup", options: ["TikTok", "Instagram", "YouTube", "Facebook", "Website/Blog", "Other"] },
			{ key: "followerCount", label: "Approximate Follower / Subscriber Count", type: "select", options: ["Under 1,000", "1,000–10,000", "10,000–50,000", "50,000–100,000", "100,000–500,000", "500,000+"] },
			{ key: "contentType", label: "Content types you produce", type: "checkboxGroup", options: ["Reels", "TikTok Videos", "Photography", "Vlogs", "Blogs", "Drone Videos", "Documentary", "Other"] },
			{ key: "workedWithBrands", label: "Have you worked with brands or tourism organisations?", type: "radio", required: true, options: ["Yes", "No"] },
			{ key: "contentLink1", label: "Sample content link 1 (optional)", type: "url", placeholder: "Link 1 — https://..." },
			{ key: "contentLink2", label: "Sample content link 2 (optional)", type: "url", placeholder: "Link 2 — https://..." },
			{ key: "contentLink3", label: "Sample content link 3 (optional)", type: "url", placeholder: "Link 3 — https://..." }
		]
	},
	{
		id: "goals",
		title: "Workshop Goals",
		fields: [
			{ key: "whyJoin", label: "Why are you interested in joining this workshop?", type: "textarea", required: true, placeholder: "Share your motivation and what excites you..." },
			{ key: "skillsToLearn", label: "What skills do you want to learn?", type: "textarea", placeholder: "e.g. video editing, TikTok algorithm strategy, destination storytelling..." },
			{ key: "trainingUsage", label: "How do you plan to use this training for tourism promotion?", type: "textarea", placeholder: "Describe how you'll apply the skills..." },
			{ key: "attendedBefore", label: "Have you attended any previous NTB or tourism-related workshops?", type: "radio", required: true, options: ["Yes", "No"] },
			{ key: "prevWorkshopDetails", label: "Please provide details", type: "textarea", placeholder: "Workshop name, year, organizer...", conditional: { field: "attendedBefore", equals: "Yes" } }
		]
	},
	{
		id: "declaration",
		title: "Declaration",
		fields: [
			{
				key: "declaration",
				label: "I confirm that all information provided is accurate and complete.",
				type: "checkbox",
				required: true
			}
		]
	}
];

export const ALL_SELF_REGISTRATION_FIELDS: SelfRegFieldDef[] = SELF_REGISTRATION_FIELD_SECTIONS.flatMap((s) => s.fields);

/** Non-locked field keys — the ones an admin can actually toggle on/off. */
export const ALL_TOGGLEABLE_FIELD_KEYS: string[] = ALL_SELF_REGISTRATION_FIELDS.filter((f) => !f.locked).map((f) => f.key);

export function getFieldDef(key: string): SelfRegFieldDef | undefined {
	return ALL_SELF_REGISTRATION_FIELDS.find((f) => f.key === key);
}

export function getFieldLabel(key: string): string {
	return getFieldDef(key)?.label ?? key;
}
