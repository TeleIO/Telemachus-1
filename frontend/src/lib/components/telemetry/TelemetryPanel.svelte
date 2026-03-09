<script lang="ts">
	import TelemetryItem from './TelemetryItem.svelte';
	import ApiSelector from './ApiSelector.svelte';
	import { useSubscription } from '$lib/utils/useSubscription.svelte.js';

	interface TelemetryEntry {
		api: string;
		units: string;
	}

	interface Props {
		entries: TelemetryEntry[];
		onchange: (entries: TelemetryEntry[]) => void;
	}

	let { entries, onchange }: Props = $props();

	let showSelector = $state(false);

	let apis = $derived(entries.map((e) => e.api));

	useSubscription(() => apis);

	function addEntry(api: string, units: string) {
		if (!entries.some((e) => e.api === api)) {
			onchange([...entries, { api, units }]);
		}
		showSelector = false;
	}

	function removeEntry(index: number) {
		onchange(entries.filter((_, i) => i !== index));
	}
</script>

<div class="flex flex-col border border-border rounded bg-surface h-full">
	<div class="flex items-center justify-between px-3 py-1.5 border-b border-border">
		<span class="text-xs font-bold text-accent-bright">Telemetry</span>
		<button
			class="text-xs text-text-dim hover:text-text px-2 py-0.5 border border-border rounded hover:border-accent"
			onclick={() => (showSelector = !showSelector)}
		>
			{showSelector ? 'Close' : '+ Add'}
		</button>
	</div>

	{#if showSelector}
		<div class="p-2 border-b border-border">
			<ApiSelector onadd={addEntry} />
		</div>
	{/if}

	<div class="flex-1 overflow-y-auto">
		{#each entries as entry, i (entry.api)}
			<TelemetryItem api={entry.api} units={entry.units} onremove={() => removeEntry(i)} />
		{/each}
		{#if entries.length === 0}
			<p class="text-text-dim text-xs p-4 text-center">No telemetry selected</p>
		{/if}
	</div>
</div>
