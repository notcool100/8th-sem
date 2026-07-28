<script lang="ts">
	import RichTextEditor from "$lib/components/RichTextEditor.svelte";
	import { EMAIL_TEMPLATE_PRESETS, type EmailTemplatePreset } from "$lib/data/email-template-presets";
	import { shellTitleTemplate, wrapEmailShell } from "$lib/utils/email-shell";
	import type { EmailTemplateType } from "$lib/types/email-templates";
	import type { ActionData, PageData } from "./$types";

	let { data, form } = $props<{ data: PageData; form?: ActionData }>();

	const TABS: { type: EmailTemplateType; label: string }[] = [
		{ type: "WorkshopInvite", label: "Workshop Invite" },
		{ type: "EventInvitation", label: "Event Invitation" }
	];

	const SAMPLE_VALUES: Record<EmailTemplateType, Record<string, string>> = {
		WorkshopInvite: {
			FullName: "Jane Doe",
			WorkshopDate: "12 July",
			InviteUrlLine:
				'<p style="text-align:center;font-size:12px;color:#94a3b8;margin:14px 0 0;">Or open: https://example.com/workshop-invite/abc123</p>',
			QrCodeImage:
				'<div style="width:220px;height:220px;margin:0 auto;border-radius:8px;background:#eef2f7;display:flex;align-items:center;justify-content:center;color:#94a3b8;font-size:12px;font-family:sans-serif;">QR CODE</div>'
		},
		EventInvitation: {
			FullName: "Jane Doe",
			EventTitle: "Kathmandu Heritage Walk",
			EventSummary: "Join us for a guided walk through the historic heritage sites of Kathmandu.",
			EventDate: "Saturday, July 12, 2026",
			EventLocation: "Kathmandu Durbar Square",
			InviteUrl: "https://example.com/invite/abc123",
			ExpiryLine:
				'<p style="color:#64748b;font-size:13px;margin:4px 0 0;">Valid until Friday, July 18, 2026.</p>',
			QrCodeImage:
				'<div style="width:220px;height:220px;margin:0 auto;border-radius:8px;background:#eef2f7;display:flex;align-items:center;justify-content:center;color:#94a3b8;font-size:12px;font-family:sans-serif;">QR CODE</div>'
		}
	};

	let activeType = $state<EmailTemplateType>(form?.type ?? "WorkshopInvite");

	let values = $state<Record<EmailTemplateType, { subject: string; bodyHtml: string }>>({
		WorkshopInvite: {
			subject: data.templates.WorkshopInvite.subject,
			bodyHtml: data.templates.WorkshopInvite.bodyHtml
		},
		EventInvitation: {
			subject: data.templates.EventInvitation.subject,
			bodyHtml: data.templates.EventInvitation.bodyHtml
		}
	});

	if (form?.type && (form.subject !== undefined || form.bodyHtml !== undefined)) {
		values[form.type as EmailTemplateType] = {
			subject: form.subject ?? values[form.type as EmailTemplateType].subject,
			bodyHtml: form.bodyHtml ?? values[form.type as EmailTemplateType].bodyHtml
		};
	}

	const activeValues = $derived(values[activeType]);
	const activePlaceholders = $derived(data.templates[activeType].availablePlaceholders as string[]);
	const activePresets = $derived(EMAIL_TEMPLATE_PRESETS[activeType]);

	function usePreset(preset: EmailTemplatePreset) {
		if (!confirm(`Replace the current "${TABS.find((t) => t.type === activeType)?.label}" subject and body with the "${preset.name}" template? Unsaved changes will be lost.`)) {
			return;
		}
		values[activeType] = { subject: preset.subject, bodyHtml: preset.bodyHtml };
	}

	function substitute(html: string, type: EmailTemplateType): string {
		let result = html;
		for (const [key, value] of Object.entries(SAMPLE_VALUES[type])) {
			result = result.split(`{{${key}}}`).join(value);
		}
		return result.replace(/{{\s*[A-Za-z]+\s*}}/g, "");
	}

	const previewHtml = $derived(
		wrapEmailShell(
			activeType,
			substitute(shellTitleTemplate(activeType), activeType),
			substitute(activeValues.bodyHtml, activeType)
		)
	);

	let copiedToken = $state("");

	function copyPlaceholder(token: string) {
		const text = `{{${token}}}`;
		navigator.clipboard.writeText(text).then(() => {
			copiedToken = token;
			setTimeout(() => (copiedToken = ""), 1500);
		});
	}
</script>

<section class="templates-shell animate-fade-in">
	<div class="page-intro">
		<div>
			<p class="eyebrow">Content</p>
			<h1>Email Templates</h1>
			<p class="lede">
				Edit the wording of invitation emails sent to guests. Changes apply to every
				future send — existing recipients aren't re-notified.
			</p>
		</div>
	</div>

	{#if form?.message}
		<div class="notice" class:notice-success={form?.success}>
			{form.message}
		</div>
	{/if}

	<div class="tabs">
		{#each TABS as tab}
			<button
				type="button"
				class="tab"
				class:active={activeType === tab.type}
				onclick={() => (activeType = tab.type)}
			>
				{tab.label}
			</button>
		{/each}
	</div>

	<div class="content-grid">
		<form method="POST" action="?/save" class="template-form">
			<input type="hidden" name="type" value={activeType} />

			{#if data.canUpdate}
				<section class="panel">
					<h2>Starter templates</h2>
					<p class="panel-hint">
						Pick a predefined design to load into the editor below, then customize the
						wording and placeholders to fit. This won't be saved until you click
						"Save template".
					</p>
					<div class="preset-grid">
						{#each activePresets as preset}
							<div class="preset-card">
								<div class="preset-thumb-wrapper">
									<iframe
										title={preset.name}
										class="preset-thumb"
										srcdoc={wrapEmailShell(
											activeType,
											substitute(shellTitleTemplate(activeType), activeType),
											substitute(preset.bodyHtml, activeType)
										)}
										tabindex="-1"
									></iframe>
								</div>
								<div class="preset-info">
									<h3>{preset.name}</h3>
									<p>{preset.description}</p>
								</div>
								<button type="button" class="preset-btn" onclick={() => usePreset(preset)}>
									Use this template
								</button>
							</div>
						{/each}
					</div>
				</section>
			{/if}

			<section class="panel">
				<h2>Subject</h2>
				<input
					name="subject"
					bind:value={activeValues.subject}
					placeholder="Email subject line"
					required
					disabled={!data.canUpdate}
				/>
			</section>

			<section class="panel">
				<h2>Body</h2>
				<input type="hidden" name="bodyHtml" bind:value={activeValues.bodyHtml} />
				<RichTextEditor
					bind:value={activeValues.bodyHtml}
					placeholder="Write the email content..."
				/>
			</section>

			<section class="panel">
				<h2>Placeholders</h2>
				<p class="panel-hint">
					Click to copy a token, then paste it into the subject or body where the
					real value should appear.
				</p>
				<div class="placeholder-chips">
					{#each activePlaceholders as token}
						<button
							type="button"
							class="chip"
							onclick={() => copyPlaceholder(token)}
						>
							{`{{${token}}}`}
							{#if copiedToken === token}<span class="copied">Copied!</span>{/if}
						</button>
					{/each}
				</div>
			</section>

			{#if data.canUpdate}
				<div class="actions">
					<button class="submit-btn" type="submit">Save template</button>
				</div>
			{/if}
		</form>

		<section class="panel preview-panel">
			<h2>Live preview</h2>
			<p class="panel-hint">Rendered with sample data — the QR code area shown here is a placeholder; the real email embeds the actual QR image.</p>
			<div class="preview-frame-wrapper">
				<iframe title="Email preview" srcdoc={previewHtml}></iframe>
			</div>
		</section>
	</div>
</section>

<style>
	.templates-shell {
		display: grid;
		gap: 20px;
	}

	.page-intro {
		display: flex;
		justify-content: space-between;
		gap: 16px;
		align-items: flex-start;
	}

	.eyebrow {
		margin: 0 0 8px;
		font-size: 12px;
		letter-spacing: 0.12em;
		text-transform: uppercase;
		font-weight: 700;
		color: #c8102e;
	}

	h1 {
		margin: 0;
		font-size: 2rem;
		color: #10243e;
	}

	.lede {
		margin: 10px 0 0;
		max-width: 760px;
		color: #5a6d83;
	}

	.notice {
		padding: 12px 14px;
		border-radius: 14px;
		border: 1px solid rgba(200, 16, 46, 0.18);
		background: #fff2f3;
		color: #9f1733;
		font-size: 0.92rem;
	}

	.notice.notice-success {
		background: #d1fae5;
		color: #065f46;
		border-color: rgba(6, 95, 70, 0.2);
	}

	.tabs {
		display: inline-flex;
		gap: 6px;
		padding: 6px;
		background: #f1f5f9;
		border-radius: 12px;
		width: fit-content;
	}

	.tab {
		padding: 8px 16px;
		border-radius: 8px;
		font-weight: 600;
		font-size: 0.88rem;
		color: #5a6d83;
		background: transparent;
	}

	.tab.active {
		background: #fff;
		color: #1c5c6d;
		box-shadow: 0 2px 6px rgba(15, 23, 42, 0.08);
	}

	.content-grid {
		display: grid;
		grid-template-columns: 1.3fr 1fr;
		gap: 20px;
		align-items: start;
	}

	.template-form {
		display: grid;
		gap: 16px;
	}

	.panel {
		background: #fff;
		border: 1px solid #d8e2eb;
		border-radius: 18px;
		box-shadow: 0 10px 26px rgba(15, 23, 42, 0.05);
		padding: 18px;
		display: grid;
		gap: 10px;
	}

	.panel h2 {
		margin: 0;
		font-size: 1.05rem;
		color: #10243e;
	}

	.panel-hint {
		margin: -4px 0 4px;
		font-size: 0.82rem;
		color: #64748b;
	}

	input {
		width: 100%;
		padding: 10px 12px;
		border-radius: 10px;
		border: 1px solid #c9d6e2;
		background: #f8fafc;
		color: #10243e;
		font: inherit;
	}

	input:focus {
		outline: none;
		border-color: #1c5c6d;
		box-shadow: 0 0 0 3px rgba(28, 92, 109, 0.12);
		background: #fff;
	}

	.preset-grid {
		display: grid;
		grid-template-columns: repeat(auto-fill, minmax(160px, 1fr));
		gap: 14px;
	}

	.preset-card {
		display: grid;
		gap: 8px;
		border: 1px solid #e2e8f0;
		border-radius: 14px;
		padding: 10px;
		background: #f8fafc;
	}

	.preset-thumb-wrapper {
		border-radius: 10px;
		overflow: hidden;
		border: 1px solid #e2e8f0;
		background: #fff;
		height: 160px;
	}

	.preset-thumb {
		width: 400%;
		height: 400%;
		border: none;
		transform: scale(0.25);
		transform-origin: top left;
		pointer-events: none;
	}

	.preset-info h3 {
		margin: 0;
		font-size: 0.92rem;
		color: #10243e;
	}

	.preset-info p {
		margin: 4px 0 0;
		font-size: 0.78rem;
		color: #64748b;
		line-height: 1.4;
	}

	.preset-btn {
		padding: 8px 10px;
		border-radius: 8px;
		border: 1px solid #1c5c6d;
		background: #fff;
		color: #1c5c6d;
		font-weight: 600;
		font-size: 0.82rem;
	}

	.preset-btn:hover {
		background: #ecf4f8;
	}

	.placeholder-chips {
		display: flex;
		flex-wrap: wrap;
		gap: 8px;
	}

	.chip {
		padding: 6px 10px;
		border-radius: 999px;
		background: #ecf4f8;
		border: 1px solid #c7dce6;
		color: #1f3b57;
		font-family: monospace;
		font-size: 0.8rem;
		display: inline-flex;
		align-items: center;
		gap: 6px;
	}

	.copied {
		font-family: inherit;
		font-weight: 700;
		color: #16a34a;
		font-size: 0.72rem;
	}

	.actions {
		display: flex;
		justify-content: flex-end;
	}

	.submit-btn {
		padding: 12px 18px;
		border: none;
		border-radius: 12px;
		background: linear-gradient(135deg, #f8ce1c, #e6b910);
		color: #263038;
		font-weight: 700;
		min-width: 160px;
	}

	.preview-panel {
		position: sticky;
		top: 16px;
	}

	.preview-frame-wrapper {
		border: 1px solid #d8e2eb;
		border-radius: 12px;
		overflow: hidden;
		background: #f8fafc;
	}

	.preview-frame-wrapper iframe {
		width: 100%;
		height: 600px;
		border: none;
		background: #fff;
	}

	.animate-fade-in {
		animation: fadeIn 0.4s ease forwards;
	}

	@keyframes fadeIn {
		from {
			opacity: 0;
			transform: translateY(10px);
		}
		to {
			opacity: 1;
			transform: translateY(0);
		}
	}

	@media (max-width: 960px) {
		.content-grid {
			grid-template-columns: 1fr;
		}

		.preview-panel {
			position: static;
		}
	}
</style>
