<script lang="ts">
	import type { PageData } from "./$types";
	import type { InvitationDto } from "$lib/types/invitations";
	import { NTB_LOGO_URL } from "$lib/constants/branding";

	let { data } = $props<{ data: PageData }>();
	const invitation = data.invitation as InvitationDto | null;

	const statusLabel: Record<string, string> = {
		pending: "Invitation Ready",
		sent: "Invitation Active",
		verified: "Checked In",
		expired: "Invitation Expired",
		cancelled: "Invitation Cancelled"
	};

	function formatDate(value?: string | null): string {
		if (!value) return "";
		try {
			return new Date(value).toLocaleDateString("en-US", {
				weekday: "long",
				year: "numeric",
				month: "long",
				day: "numeric"
			});
		} catch {
			return "";
		}
	}

	const eventDate = invitation ? formatDate(invitation.eventDateAd) : "";
</script>

<svelte:head>
	<title>{invitation ? `Invitation · ${invitation.eventTitle}` : "Invitation"} — Nepal Tourism Board</title>
</svelte:head>

<div class="invite-wrap">
	{#if data.notFound || !invitation}
		<div class="card not-found">
			<img src={NTB_LOGO_URL} alt="Nepal Tourism Board" class="logo" />
			<h1>Invitation Not Found</h1>
			<p>This invitation link is invalid or has been removed. Please contact the event organizer.</p>
		</div>
	{:else}
		<div class="card">
			<div class="ribbon"></div>

			<div class="header">
				<img src="/Nepal-Tourism-Board_Logo-compact.jpg" alt="Nepal Tourism Board" class="logo-compact" />
				<img src={NTB_LOGO_URL} alt="Nepal Tourism Board" class="logo" />
				<p class="eyebrow">Official Invitation</p>
				<div class="divider"><span></span><i class="divider-dot"></i><span></span></div>
			</div>

			<div class="body">
				<h1>{invitation.eventTitle}</h1>
				<p class="greeting">
					You are cordially invited,
					<br />
					<strong>{invitation.guestName}</strong>
				</p>

				<div class="status-badge {invitation.status}">
					{#if invitation.status === "verified"}<i class="fi fi-rr-check"></i>{:else if invitation.status === "expired"}<i class="fi fi-rr-time-past"></i>{:else if invitation.status === "cancelled"}<i class="fi fi-rr-cross-circle"></i>{:else}<i class="fi fi-rr-sparkles"></i>{/if}
					{statusLabel[invitation.status] ?? invitation.status}
				</div>

				{#if eventDate || invitation.eventLocation}
					<div class="meta-row">
						{#if eventDate}
							<div class="meta-item">
								<i class="fi fi-rr-calendar-day"></i>
								<span>{eventDate}</span>
							</div>
						{/if}
						{#if invitation.eventLocation}
							<div class="meta-item">
								<i class="fi fi-rr-marker"></i>
								<span>{invitation.eventLocation}</span>
							</div>
						{/if}
					</div>
				{/if}

				{#if invitation.status === "verified"}
					<div class="qr-box used">
						<i class="fi fi-rr-check-circle"></i>
						<p>This invitation has already been used for check-in.</p>
						{#if invitation.verifiedAtUtc}<p class="used-at">Checked in on {formatDate(invitation.verifiedAtUtc)}</p>{/if}
					</div>
				{:else if invitation.status === "expired"}
					<div class="qr-box expired">
						<i class="fi fi-rr-time-past"></i>
						<p>This invitation has expired.</p>
					</div>
				{:else if invitation.status === "cancelled"}
					<div class="qr-box cancelled">
						<i class="fi fi-rr-cross-circle"></i>
						<p>This invitation is no longer valid.</p>
					</div>
				{:else}
					<div class="qr-box">
						<p class="qr-label">Present this QR code at the entrance</p>
						{#if invitation.qrDataUri}
							<div class="qr-frame">
								<img src={invitation.qrDataUri} alt="Your invitation QR code" />
							</div>
						{/if}
						{#if invitation.expiresAtUtc}
							<p class="expiry">Valid until {formatDate(invitation.expiresAtUtc)}</p>
						{/if}
					</div>
				{/if}

				<div class="details">
					<div class="row"><span>Guest</span><strong>{invitation.guestName}</strong></div>
					<div class="row"><span>Email</span><strong>{invitation.guestEmail}</strong></div>
					<div class="row"><span>Confirmation Code</span><strong class="mono">{invitation.token}</strong></div>
				</div>
			</div>

			<div class="footer">
				<p>This invitation is unique to you. Please do not share it.</p>
				<p class="brand-line">Nepal Tourism Board · in partnership with TikTok</p>
			</div>
		</div>
	{/if}
</div>

<style>
	.invite-wrap {
		min-height: 100vh;
		display: flex;
		align-items: center;
		justify-content: center;
		padding: 32px 20px;
		background: radial-gradient(circle at top, #2a7f96 0%, #1c5c6d 45%, #16414c 100%);
		font-family: "Inter", sans-serif;
	}

	.card {
		position: relative;
		background: #fffdf8;
		border-radius: 24px;
		max-width: 480px;
		width: 100%;
		padding: 0 0 32px;
		box-shadow: 0 30px 70px rgba(0, 0, 0, 0.35);
		overflow: hidden;
	}

	.card.not-found {
		text-align: center;
		padding: 44px 32px;
	}

	.ribbon {
		height: 10px;
		background: linear-gradient(90deg, #f8ce1c, #bd242b 50%, #1c5c6d);
	}

	.header {
		position: relative;
		text-align: center;
		padding: 30px 32px 0;
	}

	.logo {
		height: 48px;
		margin: 0 auto 14px;
		display: block;
	}

	.logo-compact {
		position: absolute;
		left: 32px;
		top: 30px;
		height: 40px;
	}

	.eyebrow {
		margin: 0;
		font-size: 11px;
		letter-spacing: 2.5px;
		text-transform: uppercase;
		color: #bd242b;
		font-weight: 700;
	}

	.divider {
		display: flex;
		align-items: center;
		justify-content: center;
		gap: 10px;
		margin: 16px 0 4px;
	}
	.divider span { height: 1px; width: 60px; background: rgba(248, 206, 28, 0.6); }
	.divider-dot { width: 8px; height: 8px; border-radius: 50%; background: #f8ce1c; display: inline-block; }

	.body { padding: 8px 32px 0; text-align: center; }

	h1 {
		margin: 6px 0 8px;
		font-size: 25px;
		color: #1c5c6d;
		line-height: 1.3;
	}

	.greeting { margin: 0 0 18px; color: #3f515b; font-size: 15px; }

	.status-badge {
		display: inline-flex;
		align-items: center;
		gap: 6px;
		padding: 7px 16px;
		border-radius: 999px;
		font-size: 12.5px;
		font-weight: 700;
		margin-bottom: 22px;
		background: rgba(28, 92, 109, 0.1);
		color: #1c5c6d;
		text-transform: uppercase;
		letter-spacing: 0.5px;
	}
	.status-badge.verified { background: rgba(34, 197, 94, 0.14); color: #16a34a; }
	.status-badge.expired { background: rgba(245, 158, 11, 0.14); color: #b28600; }
	.status-badge.cancelled { background: rgba(239, 68, 68, 0.12); color: #dc2626; }

	.meta-row {
		display: flex;
		justify-content: center;
		gap: 18px;
		flex-wrap: wrap;
		margin-bottom: 22px;
		font-size: 13.5px;
		color: #3f515b;
	}
	.meta-item { display: flex; align-items: center; gap: 6px; }
	.meta-item i { color: #bd242b; }

	.qr-box {
		background: linear-gradient(135deg, #fdf8e7, #fffdf8);
		border: 1.5px dashed rgba(248, 206, 28, 0.6);
		border-radius: 16px;
		padding: 24px;
		margin-bottom: 22px;
	}
	.qr-box.used, .qr-box.expired, .qr-box.cancelled {
		text-align: center;
	}
	.qr-box.used { background: rgba(34, 197, 94, 0.06); border: 1.5px solid rgba(34, 197, 94, 0.25); }
	.qr-box.used i { color: #16a34a; }
	.qr-box.expired { background: rgba(245, 158, 11, 0.06); border: 1.5px solid rgba(245, 158, 11, 0.25); }
	.qr-box.expired i { color: #b28600; }
	.qr-box.cancelled { background: rgba(239, 68, 68, 0.06); border: 1.5px solid rgba(239, 68, 68, 0.25); }
	.qr-box.cancelled i { color: #dc2626; }
	.qr-box.used i, .qr-box.expired i, .qr-box.cancelled i { font-size: 32px; margin-bottom: 8px; display: block; }
	.qr-box.used p, .qr-box.expired p, .qr-box.cancelled p { margin: 0; font-size: 14px; color: #3f515b; }
	.qr-box.used .used-at { font-size: 12.5px; color: #64748b; margin-top: 4px; }

	.qr-label { margin: 0 0 14px; font-size: 12.5px; font-weight: 700; color: #3f515b; text-transform: uppercase; letter-spacing: 0.5px; }
	.qr-frame {
		display: inline-block;
		background: #ffffff;
		padding: 14px;
		border-radius: 14px;
		border: 1px solid rgba(28, 92, 109, 0.12);
		box-shadow: 0 8px 20px rgba(28, 92, 109, 0.1);
	}
	.qr-frame img { width: 220px; height: 220px; display: block; }
	.expiry { margin: 14px 0 0; font-size: 12.5px; color: #64748b; text-align: center; }

	.details {
		text-align: left;
		border-top: 1px dashed rgba(63, 81, 91, 0.18);
		padding-top: 16px;
		display: flex;
		flex-direction: column;
		gap: 9px;
	}
	.row { display: flex; justify-content: space-between; gap: 12px; font-size: 13px; }
	.row span { color: #94a3b8; }
	.row strong { color: #1c5c6d; text-align: right; word-break: break-all; }
	.mono { font-family: monospace; font-size: 11.5px; }

	.footer {
		margin-top: 22px;
		padding: 16px 32px 0;
		text-align: center;
		border-top: 1px solid rgba(63, 81, 91, 0.08);
	}
	.footer p { margin: 0; font-size: 11.5px; color: #94a3b8; }
	.footer .brand-line { margin-top: 4px; font-weight: 600; color: #1c5c6d; }

	.not-found h1 { color: #bd242b; font-size: 22px; margin: 16px 0 8px; }
	.not-found p { color: #3f515b; font-size: 14px; }
</style>
