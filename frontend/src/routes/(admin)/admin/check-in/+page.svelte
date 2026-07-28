<script lang="ts">
	import { onMount, onDestroy } from "svelte";
	import type { ScanResultDto } from "$lib/types/invitations";
	import type { WorkshopInviteScanResultDto } from "$lib/types/workshop-invites";

	type UnifiedScanResult =
		| (ScanResultDto & { kind: "invitation" })
		| (WorkshopInviteScanResultDto & { kind: "workshop" });

	let scannerReady = $state(false);
	let cameraRunning = $state(false);
	let cameraError = $state("");
	let loadingLib = $state(true);

	let manualToken = $state("");
	let busy = $state(false);

	let result = $state<UnifiedScanResult | null>(null);
	let resultError = $state("");
	let verifying = $state(false);

	// Normalizes either ticket type into one shape for the popup.
	let guest = $derived.by(() => {
		if (!result) return null;
		if (result.kind === "workshop") {
			const invite = result.invite;
			if (!invite) return null;
			return {
				name: invite.fullName,
				email: invite.email,
				phone: invite.phone,
				organization: invite.organization,
				eventTitle: invite.eventTitle,
				status: invite.status,
				verifiedAtUtc: invite.verifiedAtUtc,
				id: invite.id
			};
		}
		const invitation = result.invitation;
		if (!invitation) return null;
		return {
			name: invitation.guestName,
			email: invitation.guestEmail,
			phone: invitation.guestPhone,
			organization: invitation.guestOrganization,
			eventTitle: invitation.eventTitle,
			status: invitation.status,
			verifiedAtUtc: invitation.verifiedAtUtc,
			id: invitation.id
		};
	});

	// Prevent the camera from firing the same code repeatedly.
	let lastHandled = "";
	let lastHandledAt = 0;
	let paused = false; // true while a result modal is open

	// eslint-disable-next-line @typescript-eslint/no-explicit-any
	let html5Qrcode: any = null;
	const READER_ID = "qr-reader";

	function loadScript(src: string): Promise<void> {
		return new Promise((resolve, reject) => {
			if (document.querySelector(`script[src="${src}"]`)) {
				resolve();
				return;
			}
			const s = document.createElement("script");
			s.src = src;
			s.onload = () => resolve();
			s.onerror = () => reject(new Error("Failed to load scanner library."));
			document.head.appendChild(s);
		});
	}

	onMount(async () => {
		try {
			await loadScript("https://cdn.jsdelivr.net/npm/html5-qrcode@2.3.8/html5-qrcode.min.js");
			scannerReady = true;
		} catch (e) {
			cameraError = "Could not load the camera scanner. You can still verify guests by entering the code manually.";
		} finally {
			loadingLib = false;
		}
	});

	onDestroy(() => {
		void stopCamera();
	});

	async function startCamera() {
		cameraError = "";
		// eslint-disable-next-line @typescript-eslint/no-explicit-any
		const Html5Qrcode = (window as any).Html5Qrcode;
		if (!Html5Qrcode) {
			cameraError = "Scanner library is not available.";
			return;
		}

		try {
			html5Qrcode = new Html5Qrcode(READER_ID);
			await html5Qrcode.start(
				{ facingMode: "environment" },
				{ fps: 10, qrbox: { width: 240, height: 240 } },
				(decodedText: string) => onDecoded(decodedText),
				() => {}
			);
			cameraRunning = true;
		} catch (e) {
			cameraError = "Unable to access the camera. Check permissions or use manual entry.";
			cameraRunning = false;
		}
	}

	async function stopCamera() {
		try {
			if (html5Qrcode && cameraRunning) {
				await html5Qrcode.stop();
				html5Qrcode.clear();
			}
		} catch {
			/* ignore */
		} finally {
			cameraRunning = false;
		}
	}

	function onDecoded(token: string) {
		if (paused) return;
		const now = Date.now();
		if (token === lastHandled && now - lastHandledAt < 3000) return;
		lastHandled = token;
		lastHandledAt = now;
		void handleToken(token);
	}

	async function handleToken(token: string) {
		const value = token.trim();
		if (!value || busy) return;
		busy = true;
		paused = true;
		resultError = "";
		try {
			const res = await fetch("/api/check-in/scan", {
				method: "POST",
				headers: { "Content-Type": "application/json" },
				body: JSON.stringify({ token: value })
			});
			const data = await res.json();
			if (!res.ok) {
				resultError = data?.message ?? "Scan failed.";
				result = null;
			} else {
				result = data as UnifiedScanResult;
			}
		} catch {
			resultError = "Network error while scanning.";
			result = null;
		} finally {
			busy = false;
		}
	}

	function submitManual(e: Event) {
		e.preventDefault();
		if (manualToken.trim()) {
			void handleToken(manualToken.trim());
		}
	}

	async function confirmVerify() {
		if (!result || !guest) return;
		verifying = true;
		try {
			const res = await fetch("/api/check-in/verify", {
				method: "POST",
				headers: { "Content-Type": "application/json" },
				body: JSON.stringify({ id: guest.id, kind: result.kind })
			});
			const data = await res.json();
			if (!res.ok) {
				resultError = data?.message ?? "Verification failed.";
			} else {
				result = data as UnifiedScanResult;
			}
		} catch {
			resultError = "Network error while verifying.";
		} finally {
			verifying = false;
		}
	}

	function dismiss() {
		result = null;
		resultError = "";
		manualToken = "";
		paused = false;
		lastHandled = "";
	}

	function fmt(value?: string | null): string {
		if (!value) return "—";
		try {
			return new Date(value).toLocaleString();
		} catch {
			return value;
		}
	}

	const popupTone = $derived(
		result?.result === "verified"
			? "ok"
			: result && result.canVerify
				? "info"
				: "err"
	);
</script>

<div class="checkin-page">
	<div class="head">
		<h1><i class="fi fi-rr-qr-scan"></i> Event Check-in</h1>
		<p class="subtitle">Scan a guest's invitation QR or enter their code to verify entry.</p>
	</div>

	<div class="grid">
		<div class="card scanner-card">
			<h2>Camera scanner</h2>
			<div id={READER_ID} class="reader" class:active={cameraRunning}>
				{#if !cameraRunning}
					<div class="reader-placeholder">
						<i class="fi fi-rr-camera"></i>
						<span>{loadingLib ? "Loading scanner…" : cameraRunning ? "" : "Camera is off"}</span>
					</div>
				{/if}
			</div>

			{#if cameraError}
				<div class="inline-err">{cameraError}</div>
			{/if}

			<div class="scanner-actions">
				{#if !cameraRunning}
					<button class="primary-btn" onclick={startCamera} disabled={!scannerReady}>
						<i class="fi fi-rr-play"></i> Start camera
					</button>
				{:else}
					<button class="ghost-btn" onclick={stopCamera}>
						<i class="fi fi-rr-pause"></i> Stop camera
					</button>
				{/if}
			</div>
		</div>

		<div class="card manual-card">
			<h2>Manual entry</h2>
			<p class="card-hint">If the camera isn't available, paste the invitation code or link.</p>
			<form onsubmit={submitManual}>
				<input
					type="text"
					placeholder="Paste code or invite link"
					bind:value={manualToken}
					autocomplete="off"
				/>
				<button class="primary-btn" type="submit" disabled={busy || !manualToken.trim()}>
					<i class="fi fi-rr-search"></i> Look up
				</button>
			</form>
			<div class="tip">
				<i class="fi fi-rr-info"></i>
				Scanning shows the guest's details first. Press <strong>Confirm check-in</strong> to verify — the QR then expires and can't be reused.
			</div>
		</div>
	</div>
</div>

{#if result || resultError}
	<div class="overlay" role="presentation" onclick={dismiss}>
		<div class="result-modal {popupTone}" role="dialog" aria-label="Scan result" tabindex="-1" onclick={(e) => e.stopPropagation()}>
			{#if resultError}
				<div class="result-icon err"><i class="fi fi-rr-cross-circle"></i></div>
				<h3>Could not verify</h3>
				<p class="result-msg">{resultError}</p>
				<button class="ghost-btn wide" onclick={dismiss}>Close</button>
			{:else if result}
				<div class="result-icon {popupTone}">
					{#if result.result === "verified"}
						<i class="fi fi-rr-check"></i>
					{:else if result.canVerify}
						<i class="fi fi-rr-user"></i>
					{:else}
						<i class="fi fi-rr-ban"></i>
					{/if}
				</div>

				{#if guest}
					<h3>{guest.name}</h3>
					<div class="guest-card">
						<div class="row"><span>Email</span><strong>{guest.email}</strong></div>
						{#if guest.phone}<div class="row"><span>Phone</span><strong>{guest.phone}</strong></div>{/if}
						{#if guest.organization}<div class="row"><span>Organization</span><strong>{guest.organization}</strong></div>{/if}
						<div class="row"><span>Event</span><strong>{guest.eventTitle}</strong></div>
						<div class="row"><span>Ticket type</span><strong class="cap">{result.kind === "workshop" ? "Workshop invite" : "Event invitation"}</strong></div>
						<div class="row"><span>Status</span><strong class="cap">{guest.status}</strong></div>
						{#if guest.verifiedAtUtc}<div class="row"><span>Checked in</span><strong>{fmt(guest.verifiedAtUtc)}</strong></div>{/if}
					</div>
				{:else}
					<h3>Invalid code</h3>
				{/if}

				<p class="result-msg">{result.message}</p>

				<div class="modal-actions">
					{#if result.canVerify}
						<button class="ghost-btn" onclick={dismiss}>Cancel</button>
						<button class="primary-btn" onclick={confirmVerify} disabled={verifying}>
							{#if verifying}Verifying…{:else}<i class="fi fi-rr-check"></i> Confirm check-in{/if}
						</button>
					{:else}
						<button class="primary-btn wide" onclick={dismiss}>Done</button>
					{/if}
				</div>
			{/if}
		</div>
	</div>
{/if}

<style>
	.checkin-page { max-width: 1000px; margin: 0 auto; display: flex; flex-direction: column; gap: 22px; }
	.head h1 { margin: 0; font-size: 28px; color: var(--color-dark); display: flex; align-items: center; gap: 10px; }
	.subtitle { margin: 4px 0 0; color: var(--color-text-muted); }

	.grid { display: grid; grid-template-columns: 1fr 1fr; gap: 22px; }
	@media (max-width: 860px) { .grid { grid-template-columns: 1fr; } }

	.card { background: #fff; border: 1px solid rgba(28,92,109,.1); border-radius: 16px; padding: 22px; }
	.card h2 { margin: 0 0 14px; font-size: 17px; color: var(--color-dark); }
	.card-hint { margin: -8px 0 14px; font-size: 13px; color: var(--color-text-muted); }

	.reader { width: 100%; min-height: 280px; border-radius: 12px; overflow: hidden; background: #0f172a; display: flex; align-items: center; justify-content: center; }
	.reader-placeholder { color: rgba(255,255,255,.6); display: flex; flex-direction: column; align-items: center; gap: 10px; font-size: 14px; }
	.reader-placeholder i { font-size: 40px; }

	.scanner-actions { margin-top: 14px; }
	.inline-err { margin-top: 12px; font-size: 13px; color: #dc2626; background: rgba(239,68,68,.08); border: 1px solid rgba(239,68,68,.2); padding: 10px 12px; border-radius: 10px; }

	.primary-btn { background: linear-gradient(135deg, var(--color-primary), #e6b910); color: var(--color-dark); border: none; padding: 12px 20px; border-radius: 10px; font-weight: 600; font-size: 14px; cursor: pointer; display: inline-flex; align-items: center; gap: 8px; }
	.primary-btn:disabled { opacity: .55; cursor: not-allowed; }
	.ghost-btn { background: #f1f5f9; color: #475569; border: 1px solid #e2e8f0; padding: 12px 20px; border-radius: 10px; font-weight: 600; font-size: 14px; cursor: pointer; }
	.wide { width: 100%; }

	.manual-card form { display: flex; gap: 10px; }
	.manual-card input { flex: 1; padding: 12px; border: 1px solid rgba(0,0,0,.12); border-radius: 10px; font-size: 14px; outline: none; }
	.manual-card input:focus { border-color: var(--color-secondary); box-shadow: 0 0 0 3px rgba(28,92,109,.12); }
	.tip { margin-top: 16px; font-size: 12.5px; color: var(--color-text-muted); background: #f8fafc; border: 1px solid #eef2f7; border-radius: 10px; padding: 12px; display: flex; gap: 8px; }

	.overlay { position: fixed; inset: 0; background: rgba(15,23,42,.6); backdrop-filter: blur(6px); display: flex; align-items: center; justify-content: center; z-index: 1100; padding: 20px; }
	.result-modal { background: #fff; border-radius: 18px; padding: 26px; max-width: 420px; width: 100%; text-align: center; border-top: 5px solid #94a3b8; }
	.result-modal.ok { border-top-color: #16a34a; }
	.result-modal.info { border-top-color: var(--color-secondary); }
	.result-modal.err { border-top-color: #dc2626; }
	.result-modal h3 { margin: 8px 0 4px; font-size: 20px; }

	.result-icon { width: 60px; height: 60px; border-radius: 50%; display: flex; align-items: center; justify-content: center; margin: 0 auto; font-size: 26px; }
	.result-icon.ok { background: rgba(34,197,94,.15); color: #16a34a; }
	.result-icon.info { background: rgba(28,92,109,.12); color: var(--color-secondary); }
	.result-icon.err { background: rgba(239,68,68,.12); color: #dc2626; }

	.guest-card { text-align: left; background: #f8fafc; border: 1px solid #eef2f7; border-radius: 12px; padding: 14px; margin: 14px 0; display: flex; flex-direction: column; gap: 8px; }
	.guest-card .row { display: flex; justify-content: space-between; gap: 12px; font-size: 13.5px; }
	.guest-card .row span { color: var(--color-text-muted); }
	.guest-card .row strong { color: var(--color-dark); text-align: right; }
	.cap { text-transform: capitalize; }
	.result-msg { font-size: 14px; color: var(--color-text); margin: 6px 0 18px; }
	.modal-actions { display: flex; gap: 10px; justify-content: center; }
	.modal-actions .primary-btn, .modal-actions .ghost-btn { flex: 1; justify-content: center; }
</style>
