import { env } from "$env/dynamic/private";
import type { ApprovalRequest, AuthSessionPayload, ManagedUser, NavItem, SessionUser, UserRole, ApiResponse } from "$lib/types/auth";
import type { CreateCategoryPayload, CategoryResponse } from "$lib/types/categories";
import type { EmailTemplateDto, EmailTemplateType, UpdateEmailTemplatePayload } from "$lib/types/email-templates";
import type { CreateEventPayload, EventDto } from "$lib/types/events";
import type { CreateFestivalPayload, FestivalDto } from "$lib/types/festivals";
import type { InvitationDto, InviteGuestPayload, ScanResultDto } from "$lib/types/invitations";
import type { EventRegistrationDto, RegisterGuestPayload } from "$lib/types/registrations";
import type { ReportsSummaryDto } from "$lib/types/reports";
import type {
	ImportWorkshopInviteeRow,
	SendWorkshopInvitePayload,
	WorkshopInviteDto,
	WorkshopInviteScanResultDto
} from "$lib/types/workshop-invites";

type ClientMetadata = {
	ipAddress: string;
	userAgent: string;
};

type PermissionEntry = {
	navItemId: number;
	canView: boolean;
	canCreate: boolean;
	canUpdate: boolean;
	canDelete: boolean;
	needsApproval: boolean;
};

type CreateUserPayload = {
	fullName: string;
	email: string;
	department: string;
	password: string;
	permissions: PermissionEntry[];
};

const DEFAULT_API_BASE_URL = "http://localhost:5232";

export class BackendApiError extends Error {
	status: number;
	errors: string[];

	constructor(message: string, status: number, errors: string[] = []) {
		super(message);
		this.name = "BackendApiError";
		this.status = status;
		this.errors = errors;
	}
}


export async function loginUser(
	email: string,
	password: string,
	metadata: ClientMetadata
): Promise<AuthSessionPayload> {
	return requestJson<AuthSessionPayload>("/api/auth/login", {
		method: "POST",
		headers: buildHeaders(metadata),
		body: JSON.stringify({ email, password })
	});
}

export async function refreshUserSession(
	refreshToken: string,
	metadata: ClientMetadata
): Promise<AuthSessionPayload> {
	return requestJson<AuthSessionPayload>("/api/auth/refresh", {
		method: "POST",
		headers: buildHeaders(metadata),
		body: JSON.stringify({ refreshToken })
	});
}

export async function logoutUser(refreshToken: string, metadata: ClientMetadata): Promise<void> {
	await requestSuccess("/api/auth/logout", {
		method: "POST",
		headers: buildHeaders(metadata),
		body: JSON.stringify({ refreshToken })
	});
}

export async function fetchCurrentUser(accessToken: string): Promise<SessionUser> {
	return requestJson<SessionUser>("/api/auth/me", {
		headers: buildHeaders(undefined, accessToken)
	});
}

export async function listUsers(accessToken: string): Promise<ManagedUser[]> {
	return requestJson<ManagedUser[]>("/api/users", {
		headers: buildHeaders(undefined, accessToken)
	});
}

export async function fetchCategories(accessToken?: string, type?: "event" | "festival"): Promise<CategoryResponse[]> {
  // TODO: Adjust the DTO mapping if backend changes
  const query = type ? `?type=${encodeURIComponent(type)}` : "";
  return requestJson<CategoryResponse[]>(`/api/categories${query}`, {
    method: "GET",
    headers: buildHeaders(undefined, accessToken)
  });
}

export async function createCategory(
	accessToken: string,
	payload: { name: string; description: string; color: string; icon: string; tag: string[]; isHoliday?: boolean; type: "event" | "festival" }
): Promise<CategoryResponse> {
	return requestJson<CategoryResponse>("/api/categories", {
		method: "POST",
		headers: buildHeaders(undefined, accessToken),
		body: JSON.stringify(payload)
	});
}

export async function updateCategory(
	accessToken: string,
	id: number,
	payload: { id: number; name: string; description: string; color: string; icon: string; tag: string[]; isHoliday?: boolean; type: "event" | "festival" }
): Promise<CategoryResponse> {
	return requestJson<CategoryResponse>(`/api/categories/${id}`, {
		method: "PUT",
		headers: buildHeaders(undefined, accessToken),
		body: JSON.stringify(payload)
	});
}

export async function deleteCategory(accessToken: string, id: number): Promise<string> {
	return requestJson<string>(`/api/categories/${id}`, {
		method: "DELETE",
		headers: buildHeaders(undefined, accessToken)
	});
}

export async function fetchTags(): Promise<string[]> {
	return requestJson<string[]>("/api/tags", {
		method: "GET",
		headers: buildHeaders()
	});
}

export async function createUser(accessToken: string, payload: CreateUserPayload): Promise<ManagedUser> {
	return requestJson<ManagedUser>("/api/users", {
		method: "POST",
		headers: buildHeaders(undefined, accessToken),
		body: JSON.stringify(payload)
	});
}

export async function fetchNavItems(accessToken: string): Promise<NavItem[]> {
	return requestJson<NavItem[]>("/api/users/nav-items", {
		headers: buildHeaders(undefined, accessToken)
	});
}

export async function updateUserPermissions(
	accessToken: string,
	userId: number,
	permissions: PermissionEntry[]
): Promise<void> {
	await requestSuccess(`/api/users/${userId}/permissions`, {
		method: "PUT",
		headers: buildHeaders(undefined, accessToken),
		body: JSON.stringify({ permissions })
	});
}

export async function updateUser(
	accessToken: string,
	userId: number,
	payload: { fullName: string; department: string; isActive: boolean }
): Promise<ManagedUser> {
	return requestJson<ManagedUser>(`/api/users/${userId}`, {
		method: "PUT",
		headers: buildHeaders(undefined, accessToken),
		body: JSON.stringify(payload)
	});
}

export async function deleteUser(accessToken: string, userId: number): Promise<void> {
	await requestSuccess(`/api/users/${userId}`, {
		method: "DELETE",
		headers: buildHeaders(undefined, accessToken)
	});
}

export async function getEvents(accessToken: string): Promise<EventDto[]> {
	return requestJson<EventDto[]>("/api/events", {
		method: "GET",
		headers: buildHeaders(undefined, accessToken)
	});
}

export async function getPublicEvents(): Promise<EventDto[]> {
	return requestJson<EventDto[]>("/api/events/public", {
		method: "GET",
		headers: buildHeaders()
	});
}

export async function getEventById(accessToken: string, id: number): Promise<EventDto> {
	return requestJson<EventDto>(`/api/events/${id}`, {
		method: "GET",
		headers: buildHeaders(undefined, accessToken)
	});
}

export async function getPublicEventBySlug(slug: string): Promise<EventDto> {
	return requestJson<EventDto>(`/api/events/public/by-slug/${encodeURIComponent(slug)}`, {
		method: "GET",
		headers: buildHeaders()
	});
}

export async function createEvent(accessToken: string, payload: CreateEventPayload): Promise<{ event: EventDto; requiresApproval: boolean }> {
	const res = await requestPayload<EventDto>("/api/events", {
		method: "POST",
		headers: buildHeaders(undefined, accessToken),
		body: JSON.stringify(payload)
	});
	return { event: res.data!, requiresApproval: res.requiresApproval ?? false };
}

export async function updateEvent(accessToken: string, id: number, payload: CreateEventPayload): Promise<{ event: EventDto; requiresApproval: boolean }> {
	const res = await requestPayload<EventDto>(`/api/events/${id}`, {
		method: "PUT",
		headers: buildHeaders(undefined, accessToken),
		body: JSON.stringify(payload)
	});
	return { event: res.data!, requiresApproval: res.requiresApproval ?? false };
}

export async function deleteEvent(accessToken: string, id: number): Promise<void> {
	await requestSuccess(`/api/events/${id}`, {
		method: "DELETE",
		headers: buildHeaders(undefined, accessToken)
	});
}

// ─── Festivals ────────────────────────────────────────────────────────────────

export async function getFestivals(accessToken: string): Promise<FestivalDto[]> {
	return requestJson<FestivalDto[]>("/api/festivals", {
		method: "GET",
		headers: buildHeaders(undefined, accessToken)
	});
}

export async function getPublicFestivals(): Promise<FestivalDto[]> {
	return requestJson<FestivalDto[]>("/api/festivals/public", {
		method: "GET",
		headers: buildHeaders()
	});
}

export async function getFestivalById(accessToken: string, id: number): Promise<FestivalDto> {
	return requestJson<FestivalDto>(`/api/festivals/${id}`, {
		method: "GET",
		headers: buildHeaders(undefined, accessToken)
	});
}

export async function createFestival(accessToken: string, payload: CreateFestivalPayload): Promise<FestivalDto> {
	return requestJson<FestivalDto>("/api/festivals", {
		method: "POST",
		headers: buildHeaders(undefined, accessToken),
		body: JSON.stringify(payload)
	});
}

export async function updateFestival(accessToken: string, id: number, payload: CreateFestivalPayload): Promise<FestivalDto> {
	return requestJson<FestivalDto>(`/api/festivals/${id}`, {
		method: "PUT",
		headers: buildHeaders(undefined, accessToken),
		body: JSON.stringify(payload)
	});
}

export async function deleteFestival(accessToken: string, id: number): Promise<void> {
	await requestSuccess(`/api/festivals/${id}`, {
		method: "DELETE",
		headers: buildHeaders(undefined, accessToken)
	});
}

// ─── Approvals ────────────────────────────────────────────────────────────────

export async function listApprovals(accessToken: string): Promise<ApprovalRequest[]> {
	return requestJson<ApprovalRequest[]>("/api/approvals", {
		headers: buildHeaders(undefined, accessToken)
	});
}

export async function approveEvent(accessToken: string, approvalId: number): Promise<void> {
	await requestSuccess(`/api/approvals/${approvalId}/approve`, {
		method: "POST",
		headers: buildHeaders(undefined, accessToken)
	});
}

export async function rejectEvent(accessToken: string, approvalId: number, reason: string): Promise<void> {
	await requestSuccess(`/api/approvals/${approvalId}/reject`, {
		method: "POST",
		headers: buildHeaders(undefined, accessToken),
		body: JSON.stringify({ rejectionReason: reason })
	});
}

// ─── Calendar filter settings ─────────────────────────────────────────────────

export interface CalendarFilterSettingsPayload {
	showCategory: boolean;
	showDateRange: boolean;
	showLocation: boolean;
	showPrice: boolean;
	showTags: boolean;
	isSundayHoliday: boolean;
}

export async function fetchCalendarFilterSettings(): Promise<CalendarFilterSettingsPayload> {
	return requestJson<CalendarFilterSettingsPayload>("/api/calendar-filter-settings", {
		method: "GET",
		headers: buildHeaders()
	});
}

export async function updateCalendarFilterSettings(
	accessToken: string,
	payload: CalendarFilterSettingsPayload
): Promise<CalendarFilterSettingsPayload> {
	return requestJson<CalendarFilterSettingsPayload>("/api/calendar-filter-settings", {
		method: "PUT",
		headers: buildHeaders(undefined, accessToken),
		body: JSON.stringify(payload)
	});
}

// ─── Invitations ──────────────────────────────────────────────────────────────

export async function createInvitation(
	accessToken: string,
	eventId: number,
	payload: InviteGuestPayload
): Promise<InvitationDto> {
	return requestJson<InvitationDto>(`/api/events/${eventId}/invitations`, {
		method: "POST",
		headers: buildHeaders(undefined, accessToken),
		body: JSON.stringify(payload)
	});
}

export async function listInvitations(accessToken: string, eventId: number): Promise<InvitationDto[]> {
	return requestJson<InvitationDto[]>(`/api/events/${eventId}/invitations`, {
		method: "GET",
		headers: buildHeaders(undefined, accessToken)
	});
}

export async function listAllInvitations(accessToken: string): Promise<InvitationDto[]> {
	return requestJson<InvitationDto[]>("/api/invitations", {
		method: "GET",
		headers: buildHeaders(undefined, accessToken)
	});
}

export async function getInvitation(accessToken: string, id: number): Promise<InvitationDto> {
	return requestJson<InvitationDto>(`/api/invitations/${id}`, {
		method: "GET",
		headers: buildHeaders(undefined, accessToken)
	});
}

export async function resendInvitation(accessToken: string, id: number): Promise<InvitationDto> {
	return requestJson<InvitationDto>(`/api/invitations/${id}/resend`, {
		method: "POST",
		headers: buildHeaders(undefined, accessToken)
	});
}

export async function cancelInvitation(accessToken: string, id: number): Promise<void> {
	await requestSuccess(`/api/invitations/${id}`, {
		method: "DELETE",
		headers: buildHeaders(undefined, accessToken)
	});
}

export async function scanInvitation(accessToken: string, token: string): Promise<ScanResultDto> {
	return requestJson<ScanResultDto>("/api/invitations/scan", {
		method: "POST",
		headers: buildHeaders(undefined, accessToken),
		body: JSON.stringify({ token })
	});
}

export async function verifyInvitation(accessToken: string, id: number): Promise<ScanResultDto> {
	return requestJson<ScanResultDto>(`/api/invitations/${id}/verify`, {
		method: "POST",
		headers: buildHeaders(undefined, accessToken)
	});
}

export async function getInvitationByToken(token: string): Promise<InvitationDto> {
	return requestJson<InvitationDto>(`/api/invitations/by-token/${encodeURIComponent(token)}`, {
		method: "GET",
		headers: buildHeaders()
	});
}

// ─── Workshop invites (bulk, plain-text — no QR) ───────────────────────────────

export async function importWorkshopInvites(
	accessToken: string,
	eventId: number,
	rows: ImportWorkshopInviteeRow[]
): Promise<number> {
	return requestJson<number>(`/api/events/${eventId}/workshop-invites/import`, {
		method: "POST",
		headers: buildHeaders(undefined, accessToken),
		body: JSON.stringify({ rows })
	});
}

export async function listWorkshopInvites(accessToken: string, eventId: number): Promise<WorkshopInviteDto[]> {
	return requestJson<WorkshopInviteDto[]>(`/api/events/${eventId}/workshop-invites`, {
		method: "GET",
		headers: buildHeaders(undefined, accessToken)
	});
}

export async function listAllWorkshopInvites(accessToken: string): Promise<WorkshopInviteDto[]> {
	return requestJson<WorkshopInviteDto[]>("/api/workshop-invites", {
		method: "GET",
		headers: buildHeaders(undefined, accessToken)
	});
}

export async function sendWorkshopInvite(
	accessToken: string,
	id: number,
	payload: SendWorkshopInvitePayload
): Promise<WorkshopInviteDto> {
	return requestJson<WorkshopInviteDto>(`/api/workshop-invites/${id}/send`, {
		method: "POST",
		headers: buildHeaders(undefined, accessToken),
		body: JSON.stringify(payload)
	});
}

export async function getWorkshopInviteByToken(token: string): Promise<WorkshopInviteDto> {
	return requestJson<WorkshopInviteDto>(`/api/workshop-invites/by-token/${encodeURIComponent(token)}`, {
		method: "GET",
		headers: buildHeaders()
	});
}

export async function scanWorkshopInvite(accessToken: string, token: string): Promise<WorkshopInviteScanResultDto> {
	return requestJson<WorkshopInviteScanResultDto>("/api/workshop-invites/scan", {
		method: "POST",
		headers: buildHeaders(undefined, accessToken),
		body: JSON.stringify({ token })
	});
}

export async function verifyWorkshopInvite(accessToken: string, id: number): Promise<WorkshopInviteScanResultDto> {
	return requestJson<WorkshopInviteScanResultDto>(`/api/workshop-invites/${id}/verify`, {
		method: "POST",
		headers: buildHeaders(undefined, accessToken)
	});
}

// ─── Email templates ────────────────────────────────────────────────────────

export async function getEmailTemplate(accessToken: string, type: EmailTemplateType): Promise<EmailTemplateDto> {
	return requestJson<EmailTemplateDto>(`/api/email-templates/${type}`, {
		method: "GET",
		headers: buildHeaders(undefined, accessToken)
	});
}

export async function updateEmailTemplate(
	accessToken: string,
	type: EmailTemplateType,
	payload: UpdateEmailTemplatePayload
): Promise<EmailTemplateDto> {
	return requestJson<EmailTemplateDto>(`/api/email-templates/${type}`, {
		method: "PUT",
		headers: buildHeaders(undefined, accessToken),
		body: JSON.stringify(payload)
	});
}

// ─── Event registrations ───────────────────────────────────────────────────────

export async function registerForEvent(eventId: number, payload: RegisterGuestPayload): Promise<EventRegistrationDto> {
	return requestJson<EventRegistrationDto>(`/api/events/${eventId}/registrations`, {
		method: "POST",
		headers: buildHeaders(),
		body: JSON.stringify(payload)
	});
}

export async function listEventRegistrations(accessToken: string, eventId: number): Promise<EventRegistrationDto[]> {
	return requestJson<EventRegistrationDto[]>(`/api/events/${eventId}/registrations`, {
		method: "GET",
		headers: buildHeaders(undefined, accessToken)
	});
}

export async function listAllRegistrations(accessToken: string): Promise<EventRegistrationDto[]> {
	return requestJson<EventRegistrationDto[]>("/api/registrations", {
		method: "GET",
		headers: buildHeaders(undefined, accessToken)
	});
}

export async function approveRegistration(accessToken: string, id: number): Promise<EventRegistrationDto> {
	return requestJson<EventRegistrationDto>(`/api/registrations/${id}/approve`, {
		method: "POST",
		headers: buildHeaders(undefined, accessToken)
	});
}

export async function rejectRegistration(accessToken: string, id: number): Promise<EventRegistrationDto> {
	return requestJson<EventRegistrationDto>(`/api/registrations/${id}/reject`, {
		method: "POST",
		headers: buildHeaders(undefined, accessToken)
	});
}

async function requestJson<T>(path: string, init: RequestInit): Promise<T> {
	const payload = await requestPayload<T>(path, init);
	if (payload.data === undefined) {
		throw new BackendApiError("Backend response did not include data.", 500);
	}

	return payload.data;
}

async function requestSuccess(path: string, init: RequestInit): Promise<void> {
	await requestPayload(path, init);
}

async function requestPayload<T>(path: string, init: RequestInit): Promise<ApiResponse<T>> {
	const response = await fetch(`${getApiBaseUrl()}${path}`, {
		...init,
		cache: "no-store"
	});

	const payload = await parseJson<T>(response);
	if (!response.ok || !payload?.isSuccess) {
		throw new BackendApiError(
			payload?.message ?? `Backend request failed with status ${response.status}.`,
			response.status,
			payload?.errors ?? []
		);
	}

	return payload;
}

async function parseJson<T>(response: Response): Promise<ApiResponse<T> | null> {
	try {
		return (await response.json()) as ApiResponse<T>;
	} catch {
		return null;
	}
}

function buildHeaders(metadata?: ClientMetadata, accessToken?: string): Headers {
	const headers = new Headers({
		Accept: "application/json",
		"Content-Type": "application/json"
	});

	if (metadata?.ipAddress) {
		headers.set("X-Forwarded-For", metadata.ipAddress);
	}

	if (metadata?.userAgent) {
		headers.set("X-Client-User-Agent", metadata.userAgent);
	}

	if (accessToken) {
		headers.set("Authorization", `Bearer ${accessToken}`);
	}

	return headers;
}

// ─── Reports ──────────────────────────────────────────────────────────────────

export async function getReportsSummary(
	accessToken: string,
	from?: string,
	to?: string
): Promise<ReportsSummaryDto> {
	const params = new URLSearchParams();
	if (from) params.set("from", from);
	if (to) params.set("to", to);
	const qs = params.toString() ? `?${params}` : "";
	return requestJson<ReportsSummaryDto>(`/api/reports/summary${qs}`, {
		method: "GET",
		headers: buildHeaders(undefined, accessToken)
	});
}

function getApiBaseUrl(): string {
	const configuredBaseUrl = env.API_BASE_URL?.trim() || DEFAULT_API_BASE_URL;
	return configuredBaseUrl.endsWith("/") ? configuredBaseUrl.slice(0, -1) : configuredBaseUrl;
}
