export type UserRole = "superadmin" | "admin" | "client";

export type UserPermission = {
	navItemKey: string;
	navItemLabel: string;
	canView: boolean;
	canCreate: boolean;
	canUpdate: boolean;
	canDelete: boolean;
	needsApproval: boolean;
};

export type ApprovalRequest = {
	id: number;
	eventId: number;
	eventTitle: string;
	eventSlug: string;
	requestedByName: string;
	action: "create" | "update";
	status: "pending" | "approved" | "rejected";
	rejectionReason: string | null;
	reviewedByName: string | null;
	requestedAtUtc: string;
	reviewedAtUtc: string | null;
};

export type NavItem = {
	id: number;
	key: string;
	label: string;
	section: string;
	sortOrder: number;
};

export type SessionUser = {
	id: number;
	fullName: string;
	email: string;
	department: string;
	role: UserRole;
	permissions: UserPermission[];
};

export type AuthSessionPayload = {
	accessToken: string;
	accessTokenExpiresAtUtc: string;
	refreshToken: string;
	refreshTokenExpiresAtUtc: string;
	user: SessionUser;
};

export type AppSession = {
	user: SessionUser;
	accessToken: string;
	accessTokenExpiresAtUtc: string;
};

export type ApiResponse<T> = {
	isSuccess: boolean;
	message?: string;
	data?: T;
	errors?: string[];
	requiresApproval?: boolean;
};

export type ManagedUser = {
	id: number;
	fullName: string;
	email: string;
	department: string;
	role: UserRole;
	isActive: boolean;
	lastLoginAtUtc: string | null;
	createdAtUtc: string;
	permissions: UserPermission[];
};
