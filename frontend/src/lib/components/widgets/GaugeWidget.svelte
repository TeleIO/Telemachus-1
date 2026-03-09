<script lang="ts">
	import { untrack } from 'svelte';
	import type { GaugeWidgetConfig } from '../dashboard/types.js';
	import { telemetry } from '$lib/stores/telemetry.svelte.js';
	import { useSubscription } from '$lib/utils/useSubscription.svelte.js';
	import { formatValue, unitForApi } from '$lib/utils/formatters.js';

	interface Props {
		config: GaugeWidgetConfig;
	}

	let { config }: Props = $props();

	useSubscription(() => [config.api]);

	let value = $derived(telemetry.get(config.api));
	let unit = $derived(config.unit || unitForApi(config.api));
	let formatted = $derived(formatValue(value, unit));

	// Sparkline buffer
	const MAX_SPARK = 60;
	let sparkData: number[] = $state([]);

	$effect(() => {
		if (!config.sparkline) return;
		const v = value;
		if (typeof v !== 'number' || isNaN(v)) return;
		sparkData = [...untrack(() => sparkData).slice(-(MAX_SPARK - 1)), v];
	});

	let sparkPath = $derived.by(() => {
		if (sparkData.length < 2) return '';
		const min = Math.min(...sparkData);
		const max = Math.max(...sparkData);
		const range = max - min || 1;
		const w = 100;
		const h = 24;
		const step = w / (sparkData.length - 1);
		return sparkData
			.map((v, i) => `${i === 0 ? 'M' : 'L'}${(i * step).toFixed(1)},${(h - ((v - min) / range) * h).toFixed(1)}`)
			.join(' ');
	});

	// Threshold color
	let color = $derived.by(() => {
		if (!config.thresholds || typeof value !== 'number') return 'text-text';
		let c = 'text-text';
		for (const t of config.thresholds) {
			if (value >= t.value) c = t.color;
		}
		return c;
	});
</script>

<div class="flex flex-col items-center justify-center h-full px-3 py-2 gap-1">
	<span class="text-text-dim text-xs uppercase tracking-wider truncate w-full text-center">
		{config.label}
	</span>
	<span class="text-2xl font-bold tabular-nums {color} truncate w-full text-center">
		{formatted}
	</span>
	{#if config.sparkline && sparkData.length > 1}
		<svg viewBox="0 0 100 24" class="w-full h-6 opacity-60" preserveAspectRatio="none">
			<path d={sparkPath} fill="none" stroke="currentColor" stroke-width="1.5" class="text-accent" />
		</svg>
	{/if}
</div>
