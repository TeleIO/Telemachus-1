<script lang="ts">
	import TimeSeriesChart from './TimeSeriesChart.svelte';
	import KspMap from '../map/KspMap.svelte';
	import { standardCharts, type ChartDefinition } from './ChartConfig.js';

	interface Props {
		chartName: string;
		onchange: (name: string) => void;
	}

	let { chartName, onchange }: Props = $props();

	let config = $derived(standardCharts[chartName]);
	let chartNames = $derived(Object.keys(standardCharts));
</script>

<div class="flex flex-col border border-border rounded bg-surface h-full">
	<div class="flex items-center justify-between px-3 py-1.5 border-b border-border shrink-0">
		<select
			class="bg-surface-bright text-text text-xs border border-border rounded px-2 py-1 cursor-pointer"
			value={chartName}
			onchange={(e) => onchange((e.target as HTMLSelectElement).value)}
		>
			{#each chartNames as name}
				<option value={name}>{name}</option>
			{/each}
		</select>
	</div>
	<div class="flex-1 min-h-0">
		{#if config}
			{#if config.type === 'map'}
				<KspMap />
			{:else}
				<div class="h-full p-1">
					{#key chartName}
						<TimeSeriesChart {config} />
					{/key}
				</div>
			{/if}
		{/if}
	</div>
</div>
