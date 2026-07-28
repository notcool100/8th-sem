import { redirect, type Handle, type RequestEvent } from "@sveltejs/kit";
import { BackendApiError, fetchCurrentUser, refreshUserSession } from "$lib/server/auth/api";
import {
	ACCESS_TOKEN_COOKIE,
	REFRESH_TOKEN_COOKIE,
	canAccessAdmin,
	canAccessClient,
	clearAuthCookies,
	getClientMetadata,
	getDashboardPathForRole,
	persistAuthCookies,
	toAppSession
} from "$lib/server/auth/session";
import {
	checkRateLimit,
	LOGIN_IP_LIMIT,
	LOGIN_EMAIL_LIMIT,
	PUBLIC_PAGE_LIMIT,
	PUBLIC_API_LIMIT,
	UPLOAD_LIMIT
} from "$lib/server/rateLimit";
import type { AppSession } from "$lib/types/auth";

// ─── Rate limit helpers ───────────────────────────────────────────────────────

function rateLimitedResponse(retryAfter: number, message: string, asJson = false): Response {
	const headers = {
		"Retry-After": String(retryAfter),
		"X-RateLimit-Reset": String(Math.floor(Date.now() / 1000) + retryAfter),
	};

	if (asJson) {
		return new Response(
			JSON.stringify({ message }),
			{ status: 429, headers: { ...headers, "Content-Type": "application/json" } }
		);
	}

	// Plain-text fallback (used for page loads & form actions)
	return new Response(message, { status: 429, headers });
}

function getClientIp(event: RequestEvent): string {
	// Standard proxy headers → fallback to SvelteKit's getClientAddress()
	return (
		event.request.headers.get("x-forwarded-for")?.split(",")[0].trim() ||
		event.request.headers.get("x-real-ip") ||
		event.getClientAddress()
	);
}

// ─── Main handler ─────────────────────────────────────────────────────────────

export const handle: Handle = async ({ event, resolve }) => {
	const { pathname } = event.url;
	const method = event.request.method.toUpperCase();
	const ip = getClientIp(event);

	// ── Login rate limiting ────────────────────────────────────────────────────
	if (pathname === "/login" && method === "POST") {
		// 1. Per-IP check
		const ipResult = checkRateLimit(`login:ip:${ip}`, LOGIN_IP_LIMIT);
		if (!ipResult.allowed) {
			return rateLimitedResponse(
				ipResult.retryAfter,
				`Too many login attempts. Please wait ${ipResult.retryAfter} seconds before trying again.`
			);
		}

		// 2. Per-email check (read the form body non-destructively)
		const cloned = event.request.clone();
		const formData = await cloned.formData().catch(() => null);
		const email = formData?.get("email")?.toString().trim().toLowerCase() ?? "";

		if (email) {
			const emailResult = checkRateLimit(`login:email:${email}`, LOGIN_EMAIL_LIMIT);
			if (!emailResult.allowed) {
				return rateLimitedResponse(
					emailResult.retryAfter,
					`Too many login attempts for this account. Please wait ${emailResult.retryAfter} seconds.`
				);
			}
		}
	}

	// ── Upload rate limiting ───────────────────────────────────────────────────
	if (pathname.startsWith("/api/upload") && method === "POST") {
		const result = checkRateLimit(`upload:ip:${ip}`, UPLOAD_LIMIT);
		if (!result.allowed) {
			return rateLimitedResponse(
				result.retryAfter,
				`Upload limit reached. Please wait ${result.retryAfter} seconds.`,
				true
			);
		}
	}

	// ── Public JSON API rate limiting (/api/tags, /api/categories, …) ─────────
	if (pathname.startsWith("/api/") && !pathname.startsWith("/api/upload") && method === "GET") {
		const result = checkRateLimit(`api:ip:${ip}`, PUBLIC_API_LIMIT);
		if (!result.allowed) {
			return rateLimitedResponse(
				result.retryAfter,
				`API rate limit exceeded. Please wait ${result.retryAfter} seconds.`,
				true
			);
		}
	}

	// ── Public page rate limiting (SSR page loads) ────────────────────────────
	// Only the public-facing routes — not admin/client/api which require auth
	const isPublicPage =
		method === "GET" &&
		!pathname.startsWith("/admin") &&
		!pathname.startsWith("/client") &&
		!pathname.startsWith("/api/") &&
		!pathname.startsWith("/_app/") &&     // SvelteKit internals
		!pathname.startsWith("/__data") &&
		!pathname.includes(".");               // static assets (.js, .css, .png, …)

	if (isPublicPage) {
		const result = checkRateLimit(`page:ip:${ip}`, PUBLIC_PAGE_LIMIT);
		if (!result.allowed) {
			return rateLimitedResponse(
				result.retryAfter,
				`Too many requests. Please wait ${result.retryAfter} seconds.`
			);
		}
	}

	// ── Session resolution & auth guards ──────────────────────────────────────
	event.locals.session = await resolveSession(event);

	if (pathname === "/login" && event.locals.session) {
		throw redirect(303, getDashboardPathForRole(event.locals.session.user.role));
	}

	if (pathname.startsWith("/admin")) {
		if (!event.locals.session) {
			throw redirect(303, "/login");
		}
		if (!canAccessAdmin(event.locals.session.user.role)) {
			throw redirect(303, getDashboardPathForRole(event.locals.session.user.role));
		}
	}

	if (pathname.startsWith("/client")) {
		if (!event.locals.session) {
			throw redirect(303, "/login");
		}
		if (!canAccessClient(event.locals.session.user.role)) {
			throw redirect(303, getDashboardPathForRole(event.locals.session.user.role));
		}
	}

	return resolve(event);
};

// ─── Session resolution ───────────────────────────────────────────────────────

async function resolveSession(event: RequestEvent): Promise<AppSession | null> {
	const accessToken = event.cookies.get(ACCESS_TOKEN_COOKIE);
	const refreshToken = event.cookies.get(REFRESH_TOKEN_COOKIE);

	if (!accessToken && !refreshToken) {
		return null;
	}

	if (accessToken) {
		try {
			const user = await fetchCurrentUser(accessToken);
			return {
				user,
				accessToken,
				accessTokenExpiresAtUtc: ""
			};
		} catch (error) {
			if (!(error instanceof BackendApiError) || error.status !== 401) {
				clearAuthCookies(event.cookies);
				return null;
			}
		}
	}

	if (!refreshToken) {
		clearAuthCookies(event.cookies);
		return null;
	}

	try {
		const refreshedSession = await refreshUserSession(refreshToken, getClientMetadata(event));
		persistAuthCookies(event.cookies, refreshedSession);
		return toAppSession(refreshedSession);
	} catch {
		clearAuthCookies(event.cookies);
		return null;
	}
}
