<script lang="ts">
	import { enhance } from "$app/forms";
	import type { PageData, ActionData } from "./$types";
	import { SELF_REGISTRATION_FIELD_SECTIONS, ALL_SELF_REGISTRATION_FIELDS, type SelfRegFieldDef } from "$lib/config/selfRegistrationFields";

	let { data, form } = $props<{ data: PageData; form?: ActionData }>();
	const event = data.event;

	let submitting = $state(false);

	function fmt(value?: string | null): string {
		if (!value) return "";
		try {
			return new Date(value).toLocaleDateString("en-US", { dateStyle: "long" });
		} catch {
			return value ?? "";
		}
	}

	type FieldValues = Record<string, string | string[] | boolean>;

	function buildInitialFieldValues(): FieldValues {
		const initial: FieldValues = {
			fullName: (form?.values?.fullName as string) ?? "",
			email: (form?.values?.email as string) ?? ""
		};
		for (const field of ALL_SELF_REGISTRATION_FIELDS) {
			if (field.locked) continue;
			const prior = form?.values?.[field.key];
			if (field.type === "checkboxGroup") {
				initial[field.key] = Array.isArray(prior) ? prior : [];
			} else if (field.type === "checkbox") {
				initial[field.key] = Boolean(prior);
			} else {
				initial[field.key] = typeof prior === "string" ? prior : "";
			}
		}
		return initial;
	}

	let fieldValues = $state<FieldValues>(buildInitialFieldValues());

	const enabledKeys = $derived(new Set<string>(event?.selfRegistrationFields ?? []));

	function isFieldVisible(field: SelfRegFieldDef): boolean {
		if (!field.locked && !enabledKeys.has(field.key)) return false;
		if (field.conditional) {
			return fieldValues[field.conditional.field] === field.conditional.equals;
		}
		return true;
	}
</script>

<div class="register-wrap">
	<div class="register-card">
		<div class="brand">Nepal Tourism Board · Registration</div>

		{#if data.notFound || !event}
			<h1>Event not found</h1>
			<p>This registration link is invalid or the event no longer exists.</p>
		{:else}
			<h1>{event.title}</h1>
			<p class="meta">
				<i class="fi fi-rr-calendar-day"></i> {fmt(event.date_ad)}
				{#if event.location}&nbsp;·&nbsp;<i class="fi fi-rr-marker"></i> {event.location}{/if}
			</p>

			{#if event.requiresInvitation}
				<div class="notice">
					<i class="fi fi-rr-lock"></i>
					<p>This event is invitation-only. Please contact the organizer to request an invite.</p>
				</div>
			{:else if !event.requiresRegistration}
				<div class="notice">
					<i class="fi fi-rr-info"></i>
					<p>This event does not require registration. Just show up — everyone is welcome.</p>
				</div>
			{:else if form?.success}
				<div class="notice ok">
					<i class="fi fi-rr-check-circle"></i>
					<p>{form.message}</p>
				</div>
			{:else}
				<p class="summary">{event.summary}</p>

				{#if form?.message}
					<div class="alert">{form.message}</div>
				{/if}

				<form
					method="POST"
					action="?/register"
					use:enhance={() => {
						submitting = true;
						return async ({ update }) => {
							submitting = false;
							await update();
						};
					}}
				>
					<div class="field">
						<label for="fullName">Full name *</label>
						<input id="fullName" name="fullName" type="text" required placeholder="e.g. Ramesh Sharma" bind:value={fieldValues.fullName} />
					</div>
					<div class="field">
						<label for="email">Email *</label>
						<input id="email" name="email" type="email" required placeholder="you@example.com" bind:value={fieldValues.email} />
					</div>

					{#each SELF_REGISTRATION_FIELD_SECTIONS as section (section.id)}
						{@const sectionFields = section.fields.filter((f) => !f.locked && isFieldVisible(f))}
						{#if sectionFields.length}
							<div class="section-heading">{section.title}</div>
							{#each sectionFields as field (field.key)}
								<div class="field">
									{#if field.type === "checkbox"}
										<label class="pill single">
											<input
												type="checkbox"
												id={field.key}
												name={field.key}
												required={field.required}
												checked={Boolean(fieldValues[field.key])}
												onchange={(e) => (fieldValues[field.key] = e.currentTarget.checked)}
											/>
											<span>{field.label}</span>
										</label>
									{:else}
										<label for={field.key}>{field.label}{field.required ? " *" : ""}</label>
										{#if field.type === "textarea"}
											<textarea id={field.key} name={field.key} required={field.required} placeholder={field.placeholder} bind:value={fieldValues[field.key]}></textarea>
										{:else if field.type === "select"}
											<select id={field.key} name={field.key} required={field.required} bind:value={fieldValues[field.key]}>
												<option value="" disabled>Select an option</option>
												{#each field.options ?? [] as opt (opt)}
													<option value={opt}>{opt}</option>
												{/each}
											</select>
										{:else if field.type === "radio"}
											<div class="pills">
												{#each field.options ?? [] as opt (opt)}
													<label class="pill">
														<input type="radio" name={field.key} value={opt} required={field.required} bind:group={fieldValues[field.key]} />
														<span>{opt}</span>
													</label>
												{/each}
											</div>
										{:else if field.type === "checkboxGroup"}
											<div class="pills">
												{#each field.options ?? [] as opt (opt)}
													<label class="pill">
														<input type="checkbox" name={field.key} value={opt} bind:group={fieldValues[field.key]} />
														<span>{opt}</span>
													</label>
												{/each}
											</div>
										{:else}
											<input id={field.key} name={field.key} type={field.type} required={field.required} placeholder={field.placeholder} bind:value={fieldValues[field.key]} />
										{/if}
									{/if}
								</div>
							{/each}
						{/if}
					{/each}

					<button type="submit" class="primary-btn" disabled={submitting}>
						{#if submitting}<i class="fi fi-rr-spinner"></i> Submitting…{:else}<i class="fi fi-rr-paper-plane"></i> Submit registration{/if}
					</button>
					<p class="hint">An admin will review your registration before it's confirmed.</p>
				</form>
			{/if}
		{/if}
	</div>
</div>

<style>
	.register-wrap { min-height: 100vh; display: flex; align-items: center; justify-content: center; padding: 24px; background: linear-gradient(135deg, #0f172a, #1c5c6d); font-family: "Inter", sans-serif; }
	.register-card { background: #fff; border-radius: 20px; max-width: 460px; width: 100%; padding: 32px; box-shadow: 0 25px 60px rgba(0,0,0,.3); }
	.brand { font-size: 11px; letter-spacing: 1.5px; text-transform: uppercase; color: #1c5c6d; font-weight: 700; text-align: center; }
	h1 { margin: 12px 0 6px; font-size: 24px; color: #0f172a; text-align: center; }
	.meta { margin: 0 0 18px; color: #475569; font-size: 13.5px; text-align: center; }
	.summary { margin: 0 0 18px; color: #475569; font-size: 14px; text-align: center; }

	.notice { display: flex; flex-direction: column; align-items: center; gap: 10px; text-align: center; background: #f8fafc; border: 1px solid #e2e8f0; border-radius: 14px; padding: 24px; color: #475569; font-size: 14px; }
	.notice i { font-size: 28px; color: #1c5c6d; }
	.notice.ok { background: rgba(34,197,94,.08); border-color: rgba(34,197,94,.25); }
	.notice.ok i { color: #16a34a; }
	.notice p { margin: 0; }

	.alert { background: rgba(239,68,68,.1); color: #dc2626; border: 1px solid rgba(239,68,68,.25); padding: 10px 14px; border-radius: 10px; font-size: 13.5px; margin-bottom: 14px; }

	.section-heading { margin: 22px 0 10px; padding-top: 14px; border-top: 1px solid #eef2f6; font-size: 12px; font-weight: 700; letter-spacing: .4px; text-transform: uppercase; color: #1c5c6d; }
	.section-heading:first-of-type { margin-top: 18px; }

	.field { display: flex; flex-direction: column; gap: 6px; margin-bottom: 14px; }
	.field label { font-size: 13px; font-weight: 600; color: #0f172a; }
	.field input, .field select, .field textarea { padding: 11px 12px; border: 1px solid #d7dee7; border-radius: 10px; font-size: 14px; outline: none; font-family: inherit; }
	.field textarea { min-height: 80px; resize: vertical; }
	.field input:focus, .field select:focus, .field textarea:focus { border-color: #1c5c6d; box-shadow: 0 0 0 3px rgba(28,92,109,.12); }

	.pills { display: flex; flex-wrap: wrap; gap: 8px; }
	.pill { display: flex; align-items: center; gap: 6px; padding: 8px 12px; border: 1px solid #d7dee7; border-radius: 999px; font-size: 13px; color: #334155; cursor: pointer; }
	.pill input { width: 14px; height: 14px; margin: 0; }
	.pill.single { border-radius: 10px; align-items: flex-start; }

	.primary-btn { width: 100%; background: linear-gradient(135deg, #f8ce1c, #e6b910); color: #263038; border: none; padding: 13px 20px; border-radius: 10px; font-weight: 600; font-size: 14px; cursor: pointer; display: inline-flex; align-items: center; justify-content: center; gap: 8px; margin-top: 6px; }
	.primary-btn:disabled { opacity: .6; cursor: not-allowed; }

	.hint { margin: 10px 0 0; font-size: 11.5px; color: #94a3b8; text-align: center; }
</style>
