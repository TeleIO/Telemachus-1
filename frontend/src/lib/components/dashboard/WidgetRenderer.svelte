<script lang="ts">
	import type { WidgetConfig } from './types.js';
	import { standardCharts } from '../charts/ChartConfig.js';
	import TimeSeriesChart from '../charts/TimeSeriesChart.svelte';
	import KspMap from '../map/KspMap.svelte';
	import TelemetryListWidget from '../widgets/TelemetryListWidget.svelte';
	import GaugeWidget from '../widgets/GaugeWidget.svelte';
	import DeltaVWidget from '../widgets/DeltaVWidget.svelte';
	import ResourceBarWidget from '../widgets/ResourceBarWidget.svelte';
	import NavBallInfoWidget from '../widgets/NavBallInfoWidget.svelte';
	import AlarmListWidget from '../widgets/AlarmListWidget.svelte';
	import DockingHUDWidget from '../widgets/DockingHUDWidget.svelte';

	interface Props {
		config: WidgetConfig;
	}

	let { config }: Props = $props();
</script>

<div class="w-full h-full min-h-0 min-w-0 overflow-hidden">
	{#if config.type === 'timeseries'}
		{@const chartDef = standardCharts[config.chartName]}
		{#if chartDef}
			{#key config.chartName}
				<TimeSeriesChart config={chartDef} />
			{/key}
		{:else}
			<div class="flex items-center justify-center h-full text-text-dim text-xs">
				Unknown chart: {config.chartName}
			</div>
		{/if}
	{:else if config.type === 'map'}
		<KspMap />
	{:else if config.type === 'telemetry-list'}
		<TelemetryListWidget apis={config.apis} />
	{:else if config.type === 'gauge'}
		<GaugeWidget {config} />
	{:else if config.type === 'deltav'}
		<DeltaVWidget />
	{:else if config.type === 'resource-bar'}
		<ResourceBarWidget resources={config.resources} />
	{:else if config.type === 'navball-info'}
		<NavBallInfoWidget />
	{:else if config.type === 'alarm-list'}
		<AlarmListWidget maxItems={config.maxItems} />
	{:else if config.type === 'docking-hud'}
		<DockingHUDWidget />
	{/if}
</div>
