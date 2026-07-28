import { browser } from "$app/environment";
import { HubConnectionBuilder, HubConnectionState, type HubConnection } from "@microsoft/signalr";
import { env } from "$env/dynamic/public";

const API_BASE = env.PUBLIC_BACKEND_URL || "http://localhost:5232";

export type NotificationItem = {
	id: number;
	type: string;
	title: string;
	message: string;
	link: string | null;
	isRead: boolean;
	createdAtUtc: string;
};

class NotificationStore {
	items = $state<NotificationItem[]>([]);
	private _token = "";

	get unreadCount() {
		return this.items.filter((n) => !n.isRead).length;
	}

	setItems(items: NotificationItem[]) {
		this.items = items;
	}

	prepend(item: NotificationItem) {
		this.items = [item, ...this.items];
	}

	async markRead(id: number) {
		this.items = this.items.map((n) => (n.id === id ? { ...n, isRead: true } : n));
		if (this._token) {
			fetch(`${API_BASE}/api/notifications/${id}/read`, {
				method: "POST",
				headers: { Authorization: `Bearer ${this._token}` }
			}).catch(() => {});
		}
	}

	async markAllRead() {
		this.items = this.items.map((n) => ({ ...n, isRead: true }));
		if (this._token) {
			fetch(`${API_BASE}/api/notifications/read-all`, {
				method: "POST",
				headers: { Authorization: `Bearer ${this._token}` }
			}).catch(() => {});
		}
	}

	async remove(id: number) {
		this.items = this.items.filter((n) => n.id !== id);
		if (this._token) {
			fetch(`${API_BASE}/api/notifications/${id}`, {
				method: "DELETE",
				headers: { Authorization: `Bearer ${this._token}` }
			}).catch(() => {});
		}
	}

	setToken(token: string) {
		this._token = token;
	}
}

export const notifStore = new NotificationStore();

let _connection: HubConnection | null = null;

export async function connectNotifications(accessToken: string) {
	if (!browser) return;

	notifStore.setToken(accessToken);

	// Fetch existing notifications
	try {
		const res = await fetch(`${API_BASE}/api/notifications`, {
			headers: { Authorization: `Bearer ${accessToken}` }
		});
		if (res.ok) {
			const json = await res.json();
			if (json.isSuccess && Array.isArray(json.data)) {
				notifStore.setItems(json.data);
			}
		}
	} catch {
		// non-fatal
	}

	if (_connection?.state === HubConnectionState.Connected) return;

	_connection = new HubConnectionBuilder()
		.withUrl(`${API_BASE}/hubs/notifications`, {
			accessTokenFactory: () => accessToken
		})
		.withAutomaticReconnect()
		.build();

	_connection.on("notification", (item: NotificationItem) => {
		notifStore.prepend(item);
	});

	try {
		await _connection.start();
	} catch (e) {
		console.error("[SignalR] connection failed:", e);
	}
}

export async function disconnectNotifications() {
	if (_connection) {
		_connection.off("notification");
		await _connection.stop();
		_connection = null;
	}
}
