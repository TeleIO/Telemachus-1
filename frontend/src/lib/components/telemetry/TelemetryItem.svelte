<script lang="ts">
	import { telemetry } from '$lib/stores/telemetry.svelte.js';
	import { formatValue } from '$lib/utils/formatters.js';

	interface Props {
		api: string;
		units?: string;
		onremove: () => void;
	}

	let { api, units, onremove }: Props = $props();

	let value = $derived(telemetry.get(api));
	let formatted = $derived(formatValue(value, units));
</script>

<div class="flex items-center gap-2 px-3 py-1 text-xs border-b border-border hover:bg-surface-bright group cursor-grab">
	<span class="text-text-dim w-48 shrink-0 truncate select-none">{api}</span>
	<span class="text-text flex-1 text-right font-bold tabular-nums">{formatted}</span>
	<button
		class="text-text-dim hover:text-danger opacity-0 group-hover:opacity-100 transition-opacity shrink-0"
		onclick={onremove}
	>
		&times;
	</button>
</div>
