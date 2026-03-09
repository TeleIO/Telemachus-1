<script lang="ts">
	import { telemetry } from '$lib/stores/telemetry.svelte.js';
	import { useSubscription } from '$lib/utils/useSubscription.svelte.js';
	import { formatValue } from '$lib/utils/formatters.js';

	const APIS = ['dock.ax', 'dock.ay', 'dock.az', 'dock.x', 'dock.y', 'tar.distance', 'tar.o.relativeVelocity'];
	useSubscription(() => APIS);

	let ax = $derived(telemetry.get('dock.ax') as number ?? 0);
	let ay = $derived(telemetry.get('dock.ay') as number ?? 0);
	let dx = $derived(telemetry.get('dock.x') as number ?? 0);
	let dy = $derived(telemetry.get('dock.y') as number ?? 0);
	let dist = $derived(telemetry.get('tar.distance'));
	let relV = $derived(telemetry.get('tar.o.relativeVelocity'));

	// Map dock offset to crosshair position (-1 to 1 range, clamped)
	function clamp(v: number, range: number): number {
		return Math.max(-1, Math.min(1, v / range));
	}

	let cx = $derived(50 + clamp(dx, 10) * 40);
	let cy = $derived(50 - clamp(dy, 10) * 40);
</script>

<div class="flex flex-col h-full p-2">
	<!-- Crosshair display -->
	<div class="flex-1 flex items-center justify-center">
		<svg viewBox="0 0 100 100" class="w-full h-full max-w-[200px] max-h-[200px]">
			<!-- Grid rings -->
			<circle cx="50" cy="50" r="40" fill="none" stroke="var(--color-border)" stroke-width="0.5" />
			<circle cx="50" cy="50" r="20" fill="none" stroke="var(--color-border)" stroke-width="0.5" />
			<!-- Crosshairs -->
			<line x1="10" y1="50" x2="90" y2="50" stroke="var(--color-border)" stroke-width="0.5" />
			<line x1="50" y1="10" x2="50" y2="90" stroke="var(--color-border)" stroke-width="0.5" />
			<!-- Vessel marker -->
			<circle cx={cx} cy={cy} r="3" fill="var(--color-accent)" />
			<circle cx={cx} cy={cy} r="5" fill="none" stroke="var(--color-accent)" stroke-width="0.5" />
		</svg>
	</div>

	<!-- Data readouts -->
	<div class="grid grid-cols-2 gap-x-4 gap-y-1 text-xs px-2">
		<div class="flex justify-between">
			<span class="text-text-dim">Dist</span>
			<span class="text-text tabular-nums">{formatValue(dist, 'distance')}</span>
		</div>
		<div class="flex justify-between">
			<span class="text-text-dim">Rel V</span>
			<span class="text-text tabular-nums">{formatValue(relV, 'velocity')}</span>
		</div>
		<div class="flex justify-between">
			<span class="text-text-dim">Ang X</span>
			<span class="text-text tabular-nums">{formatValue(ax, 'deg')}</span>
		</div>
		<div class="flex justify-between">
			<span class="text-text-dim">Ang Y</span>
			<span class="text-text tabular-nums">{formatValue(ay, 'deg')}</span>
		</div>
	</div>
</div>
