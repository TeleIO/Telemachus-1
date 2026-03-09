<script lang="ts">
	import { telemetry } from '$lib/stores/telemetry.svelte.js';
	import { useSubscription } from '$lib/utils/useSubscription.svelte.js';

	interface Props {
		resources: string[];
	}

	let { resources }: Props = $props();

	let apis = $derived(
		resources.flatMap((r) => [`r.resource[${r}]`, `r.resourceMax[${r}]`])
	);

	useSubscription(() => apis);

	function getPercent(name: string): number {
		const current = telemetry.get(`r.resource[${name}]`) as number | undefined;
		const max = telemetry.get(`r.resourceMax[${name}]`) as number | undefined;
		if (current == null || max == null || max === 0) return 0;
		return Math.min(100, Math.max(0, (current / max) * 100));
	}

	function getColor(pct: number): string {
		if (pct > 50) return 'bg-success';
		if (pct > 20) return 'bg-warning';
		return 'bg-danger';
	}

	function fmt(v: unknown): string {
		if (v == null) return '--';
		if (typeof v === 'number') return v.toFixed(1);
		return String(v);
	}
</script>

<div class="flex flex-col gap-2 h-full overflow-y-auto p-3">
	{#each resources as res}
		{@const pct = getPercent(res)}
		{@const current = telemetry.get(`r.resource[${res}]`)}
		{@const max = telemetry.get(`r.resourceMax[${res}]`)}
		<div class="flex flex-col gap-0.5">
			<div class="flex justify-between text-xs">
				<span class="text-text-dim">{res}</span>
				<span class="text-text tabular-nums">{fmt(current)} / {fmt(max)}</span>
			</div>
			<div class="h-3 bg-surface-bright rounded-sm overflow-hidden">
				<div
					class="h-full rounded-sm transition-all duration-300 {getColor(pct)}"
					style="width: {pct}%"
				></div>
			</div>
		</div>
	{/each}
	{#if resources.length === 0}
		<p class="text-text-dim text-xs text-center py-4">No resources configured</p>
	{/if}
</div>
