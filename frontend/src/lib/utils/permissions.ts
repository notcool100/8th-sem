import type { SessionUser } from "$lib/types/auth";

export type NavKey =
	| "dashboard" | "events" | "festivals" | "calendar" | "destinations" | "users"
	| "departments" | "attendees" | "check_in" | "categories" | "email_templates"
	| "reports" | "notifications" | "settings";

export type CrudOp = "canView" | "canCreate" | "canUpdate" | "canDelete";

export function can(user: SessionUser | null | undefined, navKey: NavKey, op: CrudOp): boolean {
	if (!user) return false;
	if (user.role === "superadmin") return true;
	return user.permissions.some(p => p.navItemKey === navKey && p[op]);
}
