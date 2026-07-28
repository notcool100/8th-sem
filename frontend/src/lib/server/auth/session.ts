import { dev } from "$app/environment";
import type { Cookies, RequestEvent } from "@sveltejs/kit";
import type { AppSession, AuthSessionPayload, UserRole } from "$lib/types/auth";

export const ACCESS_TOKEN_COOKIE = "ntb_access_token";
export const REFRESH_TOKEN_COOKIE = "ntb_refresh_token";

export function getDashboardPathForRole(role: UserRole): string {
	return role === "client" ? "/client/dashboard" : "/admin/dashboard";
}

export function canAccessAdmin(role: UserRole): boolean {
	return role === "superadmin" || role === "admin";
}

export function canAccessClient(role: UserRole): boolean {
	return role === "client";
}

export function toAppSession(payload: AuthSessionPayload): AppSession {
	return {
		user: payload.user,
		accessToken: payload.accessToken,
		accessTokenExpiresAtUtc: payload.accessTokenExpiresAtUtc
	};
}

export function persistAuthCookies(cookies: Cookies, payload: AuthSessionPayload): void {
	setCookie(cookies, ACCESS_TOKEN_COOKIE, payload.accessToken, payload.accessTokenExpiresAtUtc);
	setCookie(cookies, REFRESH_TOKEN_COOKIE, payload.refreshToken, payload.refreshTokenExpiresAtUtc);
}

export function clearAuthCookies(cookies: Cookies): void {
	for (const cookieName of [ACCESS_TOKEN_COOKIE, REFRESH_TOKEN_COOKIE]) {
		cookies.delete(cookieName, {
			path: "/",
			httpOnly: true,
			sameSite: "lax",
			secure: !dev
		});
	}
}

export function getClientMetadata(event: RequestEvent): { ipAddress: string; userAgent: string } {
	const forwardedFor = event.request.headers.get("x-forwarded-for");
	const ipAddress =
		forwardedFor?.split(",")[0]?.trim() ||
		safeClientAddress(event) ||
		"unknown";

	const userAgent = (event.request.headers.get("user-agent") ?? "unknown").trim() || "unknown";

	return {
		ipAddress,
		userAgent
	};
}

function setCookie(cookies: Cookies, name: string, value: string, expiresAtUtc: string): void {
	const expiresAt = new Date(expiresAtUtc);
	const maxAgeSeconds = Math.max(0, Math.floor((expiresAt.getTime() - Date.now()) / 1000));

	cookies.set(name, value, {
		path: "/",
		httpOnly: true,
		sameSite: "lax",
		secure: !dev,
		expires: expiresAt,
		maxAge: maxAgeSeconds
	});
}

function safeClientAddress(event: RequestEvent): string | null {
	try {
		return event.getClientAddress();
	} catch {
		return null;
	}
}
