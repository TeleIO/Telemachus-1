<script lang="ts">
	import { onMount } from 'svelte';
	import uPlot from 'uplot';
	import type { ChartDefinition } from './ChartConfig.js';
	import { CHART_COLORS } from './ChartConfig.js';
	import { telemetry } from '$lib/stores/telemetry.svelte.js';
	import { useSubscription } from '$lib/utils/useSubscription.svelte.js';
	import { formatValue, unitForApi } from '$lib/utils/formatters.js';

	interface Props {
		config: ChartDefinition;
	}

	let { config }: Props = $props();

	const WINDOW = 300;
	const MAX_POINTS = 1000;

	let container: HTMLDivElement;
	let chart: uPlot | null = null;
	let data: uPlot.AlignedData = [[]];
	let lastTime = 0;

	useSubscription(() => config.series);

	function initData() {
		data = [new Float64Array(0) as unknown as number[]];
		for (let i = 0; i < config.series.length; i++) {
			data.push(new Float64Array(0) as unknown as number[]);
		}
	}

	function fmtMET(seconds: number): string {
		const sign = seconds < 0 ? '-' : '';
		const s = Math.abs(seconds);
		const h = Math.floor(s / 3600);
		const m = Math.floor((s % 3600) / 60);
		const sec = Math.floor(s % 60);
		if (h > 0) return `${sign}${h}:${String(m).padStart(2, '0')}:${String(sec).padStart(2, '0')}`;
		return `${sign}${m}:${String(sec).padStart(2, '0')}`;
	}

	function latestValue(u: uPlot, _raw: number | undefined, si: number): string {
		const d = u.data[si];
		if (!d || d.length === 0) return '—';
		const v = d[d.length - 1];
		if (v == null || isNaN(v as number)) return '—';
		if (si === 0) return fmtMET(v as number);
		const api = config.series[si - 1];
		return formatValue(v, unitForApi(api));
	}

	function buildOpts(width: number, height: number): uPlot.Options {
		const series: uPlot.Series[] = [
			{ label: 'Time', value: latestValue as uPlot.Series.Value }
		];
		for (let i = 0; i < config.series.length; i++) {
			series.push({
				label: config.series[i],
				stroke: CHART_COLORS[i % CHART_COLORS.length],
				width: 1.5,
				value: latestValue as uPlot.Series.Value
			});
		}

		return {
			width,
			height,
			series,
			scales: {
				x: { time: false }
			},
			axes: [
				{
					stroke: '#6b7280',
					grid: { stroke: '#1f293766' },
					values: (_u: uPlot, vals: number[]) => vals.map((v) => fmtMET(v))
				},
				{
					label: config.yaxis?.label ?? '',
					stroke: '#6b7280',
					grid: { stroke: '#1f293766' },
					...(config.yaxis?.min != null ? { min: config.yaxis.min } : {}),
					...(config.yaxis?.max != null ? { max: config.yaxis.max } : {})
				} as uPlot.Axis
			],
			cursor: { show: false },
			legend: { show: true, live: true }
		};
	}

	const LEGEND_HEIGHT = 28;

	onMount(() => {
		initData();
		const rect = container.getBoundingClientRect();
		const chartH = Math.max(rect.height - LEGEND_HEIGHT, 40);
		chart = new uPlot(buildOpts(rect.width, chartH), data, container);

		const ro = new ResizeObserver((entries) => {
			const { width, height } = entries[0].contentRect;
			chart?.setSize({ width, height: Math.max(height - LEGEND_HEIGHT, 40) });
		});
		ro.observe(container);

		return () => {
			ro.disconnect();
			chart?.destroy();
		};
	});

	$effect(() => {
		const mt = telemetry.missionTime;
		if (!chart || mt === lastTime) return;
		lastTime = mt;

		const t = data[0] as number[];
		const newT = [...t, mt];

		const newData: uPlot.AlignedData = [newT];
		for (let i = 0; i < config.series.length; i++) {
			const prev = data[i + 1] as number[];
			const val = telemetry.get(config.series[i]);
			newData.push([...prev, typeof val === 'number' ? val : NaN]);
		}

		// Trim points older than the visible window
		const windowMin = mt - WINDOW;
		let trim = 0;
		while (trim < newT.length && newT[trim] < windowMin) trim++;
		if (trim > 0) {
			newData[0] = newT.slice(trim);
			for (let i = 1; i < newData.length; i++) {
				newData[i] = (newData[i] as number[]).slice(trim);
			}
		}

		data = newData;
		chart.setData(data);
		chart.setScale('x', { min: windowMin, max: mt });
	});
</script>

<div bind:this={container} class="w-full h-full min-h-0 overflow-hidden"></div>
