<script lang="ts">
	import { command } from '$lib/api/client.js';

	interface Props {
		label: string;
		apiString: string;
		class?: string;
	}

	let { label, apiString, class: className = '' }: Props = $props();
	let pending = $state(false);

	async function execute() {
		pending = true;
		try {
			await command(apiString);
		} catch { /* */ }
		pending = false;
	}
</script>

<button
	class="px-3 py-2 text-xs font-bold rounded border border-border bg-surface-bright
		hover:border-accent hover:text-accent-bright active:bg-accent/20
		disabled:opacity-50 transition-colors {className}"
	onclick={execute}
	disabled={pending}
>
	{pending ? '...' : label}
</button>
