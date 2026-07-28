<script lang="ts">
	import { page } from "$app/state";
</script>

<div class="error-shell">
	<div class="error-card">
		<div class="error-icon">
			{#if page.status === 403}
				<i class="fi fi-rr-lock"></i>
			{:else if page.status === 404}
				<i class="fi fi-rr-search"></i>
			{:else}
				<i class="fi fi-rr-exclamation"></i>
			{/if}
		</div>

		<div class="error-code">{page.status}</div>

		<h1 class="error-title">
			{#if page.status === 403}
				Permission Denied
			{:else if page.status === 404}
				Page Not Found
			{:else}
				Something went wrong
			{/if}
		</h1>

		<p class="error-message">
			{#if page.status === 403}
				You don't have permission to access this page. Contact your administrator to request access.
			{:else}
				{page.error?.message ?? "An unexpected error occurred."}
			{/if}
		</p>

		<a href="/admin/dashboard" class="back-btn">
			<i class="fi fi-rr-arrow-left"></i> Back to Dashboard
		</a>
	</div>
</div>

<style>
	.error-shell {
		display: flex;
		align-items: center;
		justify-content: center;
		min-height: 60vh;
		padding: 40px 20px;
	}

	.error-card {
		background: white;
		border: 1px solid #d8e2eb;
		border-radius: 24px;
		box-shadow: 0 16px 48px rgba(15, 23, 42, 0.08);
		padding: 48px 40px;
		text-align: center;
		max-width: 480px;
		width: 100%;
	}

	.error-icon {
		width: 72px;
		height: 72px;
		border-radius: 20px;
		background: #fef2f2;
		color: #c8102e;
		font-size: 32px;
		display: flex;
		align-items: center;
		justify-content: center;
		margin: 0 auto 20px;
	}

	.error-code {
		font-size: 3.5rem;
		font-weight: 800;
		color: #c8102e;
		line-height: 1;
		margin-bottom: 12px;
	}

	.error-title {
		margin: 0 0 12px;
		font-size: 1.5rem;
		color: #10243e;
	}

	.error-message {
		margin: 0 0 28px;
		color: #5a6d83;
		font-size: 0.95rem;
		line-height: 1.6;
	}

	.back-btn {
		display: inline-flex;
		align-items: center;
		gap: 8px;
		padding: 12px 24px;
		border-radius: 12px;
		background: linear-gradient(135deg, #c8102e, #8f1027);
		color: white;
		text-decoration: none;
		font-weight: 700;
		font-size: 0.92rem;
		transition: opacity 0.15s;
	}

	.back-btn:hover { opacity: 0.88; }
</style>
