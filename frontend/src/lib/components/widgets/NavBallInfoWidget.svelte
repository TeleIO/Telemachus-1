<script lang="ts">
	import { telemetry } from '$lib/stores/telemetry.svelte.js';
	import { useSubscription } from '$lib/utils/useSubscription.svelte.js';
	import { formatValue } from '$lib/utils/formatters.js';

	const APIS = ['n.heading', 'n.pitch', 'n.roll'];
	useSubscription(() => APIS);

	let heading = $derived(telemetry.get('n.heading'));
	let pitch = $derived(telemetry.get('n.pitch'));
	let roll = $derived(telemetry.get('n.roll'));

	function fmt(v: unknown): string {
		return formatValue(v, 'deg');
	}
</script>

<div class="flex items-center justify-around h-full px-2">
	<div class="flex flex-col items-center gap-1">
		<span class="text-text-dim text-xs uppercase tracking-wider">HDG</span>
		<span class="text-lg font-bold tabular-nums text-text">{fmt(heading)}</span>
	</div>
	<div class="flex flex-col items-center gap-1">
		<span class="text-text-dim text-xs uppercase tracking-wider">PIT</span>
		<span class="text-lg font-bold tabular-nums text-text">{fmt(pitch)}</span>
	</div>
	<div class="flex flex-col items-center gap-1">
		<span class="text-text-dim text-xs uppercase tracking-wider">ROL</span>
		<span class="text-lg font-bold tabular-nums text-text">{fmt(roll)}</span>
	</div>
</div>
