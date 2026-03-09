<script lang="ts">
	import { telemetry } from '$lib/stores/telemetry.svelte.js';
	import { useSubscription } from '$lib/utils/useSubscription.svelte.js';
	import { durationString } from '$lib/utils/time.js';

	interface Props {
		maxItems?: number;
	}

	let { maxItems = 10 }: Props = $props();

	const APIS = ['alarm.count', 'alarm.timeToNext'];
	useSubscription(() => APIS);

	let count = $derived(telemetry.get('alarm.count') as number ?? 0);
	let timeToNext = $derived(telemetry.get('alarm.timeToNext') as number | undefined);
</script>

<div class="flex flex-col h-full p-3 gap-2">
	{#if count === 0}
		<div class="flex-1 flex items-center justify-center text-text-dim text-xs">
			No active alarms
		</div>
	{:else}
		<div class="flex items-center justify-between text-xs">
			<span class="text-text-dim">{count} alarm{count !== 1 ? 's' : ''}</span>
		</div>
		{#if timeToNext != null}
			<div class="flex flex-col items-center gap-1 flex-1 justify-center">
				<span class="text-text-dim text-xs uppercase tracking-wider">Next Alarm</span>
				<span class="text-xl font-bold tabular-nums text-warning">
					{durationString(timeToNext)}
				</span>
			</div>
		{/if}
	{/if}
</div>
