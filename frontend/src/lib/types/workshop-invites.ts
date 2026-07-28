export type WorkshopInviteStatus = "pending" | "sent" | "verified";

export type WorkshopInviteDto = {
	id: number;
	eventId: number;
	eventTitle: string;
	eventDateAd?: string | null;
	eventLocation: string;
	fullName: string;
	email: string;
	phone: string;
	organization: string;
	status: WorkshopInviteStatus | string;
	sentAtUtc?: string | null;
	createdAtUtc: string;
	token: string;
	inviteUrl: string;
	qrDataUri: string;
	verifiedAtUtc?: string | null;
};

export type SendWorkshopInvitePayload = {
	workshopDate: string;
};

export type ImportWorkshopInviteeRow = {
	fullName: string;
	email: string;
	phone?: string;
	organization?: string;
};

export type WorkshopInviteScanResultDto = {
	result: "previewed" | "alreadyused" | "invalid" | string;
	message: string;
	canVerify: boolean;
	invite?: WorkshopInviteDto | null;
};
