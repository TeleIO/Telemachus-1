<script lang="ts">
	import { command } from '$lib/api/client.js';
	import { telemetry } from '$lib/stores/telemetry.svelte.js';
	import { useSubscription } from '$lib/utils/useSubscription.svelte.js';

	interface Props {
		label: string;
		toggleApi: string;
		valueApi: string;
	}

	let { label, toggleApi, valueApi }: Props = $props();
	let pending = $state(false);

	useSubscription(() => [valueApi]);

	let active = $derived(!!telemetry.get(valueApi));

	async function toggle() {
		pending = true;
		try {
			await command(toggleApi);
		} catch { /* */ }
		pending = false;
	}
</script>

<button
	class="px-3 py-2 text-xs font-bold rounded border transition-colors
		{active
			? 'border-success text-success bg-success/10'
			: 'border-border text-text-dim bg-surface-bright hover:border-accent hover:text-accent-bright'}"
	onclick={toggle}
	disabled={pending}
>
	{label}
</button>
