export type RegistrationStatus = "pending" | "approved" | "rejected" | "cancelled";

export type RegisterGuestPayload = {
	fullName: string;
	email: string;
	phone?: string;
	organization?: string;
	/** Submitted values for the event's enabled optional self-registration fields, keyed by field key. */
	additionalFields?: Record<string, string>;
};

export type EventRegistrationDto = {
	id: number;
	eventId: number;
	eventTitle: string;
	guestId: number;
	guestName: string;
	guestEmail: string;
	guestPhone: string;
	guestOrganization: string;
	status: RegistrationStatus | string;
	requestedAtUtc: string;
	reviewedAtUtc?: string | null;
	createdAtUtc: string;
	additionalFields?: Record<string, string>;
};
