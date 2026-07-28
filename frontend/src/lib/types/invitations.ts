export type InvitationStatus = "pending" | "sent" | "verified" | "expired" | "cancelled";

export type InviteGuestPayload = {
	fullName: string;
	email: string;
	phone?: string;
	organization?: string;
	expiresAtUtc?: string;
};

export type InvitationDto = {
	id: number;
	eventId: number;
	eventTitle: string;
	eventDateAd?: string | null;
	eventLocation: string;
	guestId: number;
	guestName: string;
	guestEmail: string;
	guestPhone: string;
	guestOrganization: string;
	token: string;
	status: InvitationStatus | string;
	inviteUrl: string;
	qrDataUri: string;
	sentAtUtc?: string | null;
	expiresAtUtc?: string | null;
	verifiedAtUtc?: string | null;
	createdAtUtc: string;
};

export type ScanResultDto = {
	result: "verified" | "previewed" | "alreadyused" | "expired" | "cancelled" | "invalid" | string;
	message: string;
	canVerify: boolean;
	invitation?: InvitationDto | null;
};
