<script lang="ts">
	import { WIDGET_CATALOG } from './types.js';
	import { dashboard } from './layoutStore.svelte.js';

	interface Props {
		onclose: () => void;
	}

	let { onclose }: Props = $props();

	const categories = [...new Set(WIDGET_CATALOG.map((w) => w.category))];

	function add(index: number) {
		const info = WIDGET_CATALOG[index];
		dashboard.addWidget(info.defaultConfig(), info.label);
		onclose();
	}
</script>

<div class="bg-surface border border-border rounded shadow-lg w-56 py-1 max-h-80 overflow-y-auto">
	{#each categories as cat}
		<div class="px-3 py-1 text-xs text-text-dim font-bold uppercase tracking-wider">{cat}</div>
		{#each WIDGET_CATALOG as info, i}
			{#if info.category === cat}
				<button
					class="w-full text-left px-3 py-1.5 text-xs text-text hover:bg-surface-bright"
					onclick={() => add(i)}
				>
					{info.label}
				</button>
			{/if}
		{/each}
	{/each}
</div>
