<script lang="ts">
	import { telemetry } from '$lib/stores/telemetry.svelte.js';
	import { useSubscription } from '$lib/utils/useSubscription.svelte.js';
	import { formatValue } from '$lib/utils/formatters.js';

	const TOTAL_APIS = ['dv.totalDVVac', 'dv.totalDVASL', 'dv.totalDVActual', 'dv.totalBurnTime', 'dv.stageCount'];
	const STAGE_APIS = ['dv.stageDVVac', 'dv.stageDVASL', 'dv.stageDVActual', 'dv.stageBurnTime', 'dv.stageTWRActual'];

	useSubscription(() => [...TOTAL_APIS, ...STAGE_APIS]);

	let stageCount = $derived((telemetry.get('dv.stageCount') as number) || 0);

	function fmt(v: unknown): string {
		if (v == null) return '--';
		if (typeof v === 'number') return v.toFixed(1);
		return String(v);
	}

	function fmtTime(v: unknown): string {
		if (v == null || typeof v !== 'number') return '--';
		if (v < 60) return `${v.toFixed(1)}s`;
		const m = Math.floor(v / 60);
		const s = (v % 60).toFixed(0);
		return `${m}m${s}s`;
	}
</script>

<div class="h-full overflow-y-auto text-xs">
	<table class="w-full">
		<thead>
			<tr class="text-text-dim border-b border-border">
				<th class="text-left px-2 py-1 font-normal">Stage</th>
				<th class="text-right px-2 py-1 font-normal">Vac</th>
				<th class="text-right px-2 py-1 font-normal">ASL</th>
				<th class="text-right px-2 py-1 font-normal">Burn</th>
				<th class="text-right px-2 py-1 font-normal">TWR</th>
			</tr>
		</thead>
		<tbody>
			<!-- Totals row -->
			<tr class="text-accent-bright font-bold border-b border-border">
				<td class="px-2 py-1">Total</td>
				<td class="text-right px-2 py-1 tabular-nums">{fmt(telemetry.get('dv.totalDVVac'))} m/s</td>
				<td class="text-right px-2 py-1 tabular-nums">{fmt(telemetry.get('dv.totalDVASL'))} m/s</td>
				<td class="text-right px-2 py-1 tabular-nums">{fmtTime(telemetry.get('dv.totalBurnTime'))}</td>
				<td class="text-right px-2 py-1">--</td>
			</tr>
			<!-- Per-stage note: the dv.stage* APIs return arrays or use indexing -->
			{#if stageCount === 0}
				<tr>
					<td colspan="5" class="px-2 py-3 text-center text-text-dim">
						No delta-V data available
					</td>
				</tr>
			{/if}
		</tbody>
	</table>
</div>
