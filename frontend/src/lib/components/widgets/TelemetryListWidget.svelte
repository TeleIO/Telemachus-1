<script lang="ts">
	import TelemetryItem from '../telemetry/TelemetryItem.svelte';
	import { useSubscription } from '$lib/utils/useSubscription.svelte.js';
	import { unitForApi } from '$lib/utils/formatters.js';

	interface Props {
		apis: string[];
	}

	let { apis }: Props = $props();

	useSubscription(() => apis);
</script>

<div class="h-full overflow-y-auto">
	{#each apis as api (api)}
		<TelemetryItem {api} units={unitForApi(api)} onremove={() => {}} />
	{/each}
	{#if apis.length === 0}
		<p class="text-text-dim text-xs p-4 text-center">No telemetry selected</p>
	{/if}
</div>
