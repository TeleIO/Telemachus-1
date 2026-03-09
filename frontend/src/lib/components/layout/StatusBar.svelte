<script lang="ts">
	import { connection } from '$lib/stores/connection.svelte.js';
	import { telemetry } from '$lib/stores/telemetry.svelte.js';
	import { missionTimeString, dateString } from '$lib/utils/time.js';

	const stateLabels: Record<string, string> = {
		open: 'CONNECTED',
		connecting: 'CONNECTING',
		closed: 'DISCONNECTED',
		error: 'ERROR'
	};

	const stateColors: Record<string, string> = {
		open: 'text-success',
		connecting: 'text-warning',
		closed: 'text-text-dim',
		error: 'text-danger'
	};
</script>

<div class="flex items-center gap-6 px-4 py-1.5 bg-surface text-xs border-t border-border">
	<span class="{stateColors[connection.state]} font-bold">
		{stateLabels[connection.state]}
	</span>

	{#if connection.state === 'open'}
		<span class="text-text-dim">
			MET {missionTimeString(telemetry.missionTime)}
		</span>
		<span class="text-text-dim">
			{dateString(telemetry.universalTime)}
		</span>
		{#if telemetry.paused}
			<span class="text-warning font-bold">PAUSED</span>
		{/if}
	{/if}
</div>
