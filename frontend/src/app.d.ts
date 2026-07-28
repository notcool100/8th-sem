import type { AppSession } from "$lib/types/auth";

// See https://svelte.dev/docs/kit/types#app.d.ts
// for information about these interfaces
declare global {
	namespace App {
		// interface Error {}
		interface Locals {
			session: AppSession | null;
		}

		interface PageData {
			session: AppSession | null;
		}
		// interface PageState {}
		// interface Platform {}
	}

	namespace svelteHTML {
		interface HTMLAttributes<T> {
			'onemoji-click'?: (event: CustomEvent<{ unicode: string }>) => void;
		}
	}
}

export {};
