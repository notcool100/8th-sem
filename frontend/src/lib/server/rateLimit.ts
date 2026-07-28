/**
 * In-memory sliding-window rate limiter.
 *
 * Each entry stores an array of request timestamps within the current window.
 * A periodic cleanup job prevents unbounded memory growth.
 */

type RateLimitConfig = {
	/** Time window in milliseconds */
	windowMs: number;
	/** Maximum requests allowed within the window */
	max: number;
};

type RateLimitResult =
	| { allowed: true }
	| { allowed: false; retryAfter: number };

// key → sorted list of request timestamps (oldest first)
const store = new Map<string, number[]>();

// Purge entries whose entire timestamp list has fallen outside any reasonable
// window (1 hour is safely beyond our longest window of 15 minutes).
const CLEANUP_INTERVAL_MS = 5 * 60 * 1000; // every 5 minutes
const MAX_WINDOW_MS = 60 * 60 * 1000; // 1 hour

function cleanup() {
	const cutoff = Date.now() - MAX_WINDOW_MS;
	for (const [key, timestamps] of store) {
		const fresh = timestamps.filter((t) => t > cutoff);
		if (fresh.length === 0) {
			store.delete(key);
		} else {
			store.set(key, fresh);
		}
	}
}

// Only run the cleanup interval in real server environments
if (typeof setInterval !== "undefined") {
	setInterval(cleanup, CLEANUP_INTERVAL_MS).unref?.();
}

/**
 * Check and record a request against the given rate-limit config.
 *
 * @param key   Unique identifier for the bucket (e.g. "login:ip:1.2.3.4")
 * @param config Window size and max request count
 */
export function checkRateLimit(key: string, config: RateLimitConfig): RateLimitResult {
	const now = Date.now();
	const windowStart = now - config.windowMs;

	let timestamps = store.get(key) ?? [];

	// Drop timestamps outside the current window
	timestamps = timestamps.filter((t) => t > windowStart);

	if (timestamps.length >= config.max) {
		// Earliest timestamp in window — request is allowed again after it expires
		const retryAfter = Math.ceil((timestamps[0] + config.windowMs - now) / 1000);
		store.set(key, timestamps);
		return { allowed: false, retryAfter };
	}

	timestamps.push(now);
	store.set(key, timestamps);
	return { allowed: true };
}

// ─── Preset configs ──────────────────────────────────────────────────────────

/**
 * Login by IP: 5 attempts per 15 minutes.
 * Strict enough to block credential-stuffing while not locking out
 * a genuine user who mistyped their password a couple of times.
 */
export const LOGIN_IP_LIMIT: RateLimitConfig = {
	windowMs: 15 * 60 * 1000,
	max: 5,
};

/**
 * Login by email: 3 attempts per 15 minutes across ALL IPs.
 * Prevents distributed attacks that rotate source IPs.
 */
export const LOGIN_EMAIL_LIMIT: RateLimitConfig = {
	windowMs: 15 * 60 * 1000,
	max: 3,
};

/**
 * Public-facing page loads (SSR): 120 requests per minute per IP.
 * Generous enough for normal browsing; stops scrapers & DoS.
 */
export const PUBLIC_PAGE_LIMIT: RateLimitConfig = {
	windowMs: 60 * 1000,
	max: 120,
};

/**
 * Public JSON API endpoints (/api/tags, /api/categories, etc.):
 * 60 requests per minute per IP.
 */
export const PUBLIC_API_LIMIT: RateLimitConfig = {
	windowMs: 60 * 1000,
	max: 60,
};

/**
 * File upload: 10 uploads per minute per IP.
 */
export const UPLOAD_LIMIT: RateLimitConfig = {
	windowMs: 60 * 1000,
	max: 10,
};
