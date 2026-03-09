<script lang="ts">
	import { API_CATEGORIES } from '$lib/api/constants.js';
	import { getAPI } from '$lib/api/client.js';
	import type { APIEntry } from '$lib/api/types.js';

	interface Props {
		onadd: (api: string, units: string) => void;
	}

	let { onadd }: Props = $props();

	let allApis = $state<APIEntry[]>([]);
	let category = $state('Vessel');
	let loaded = $state(false);

	let filtered = $derived(() => {
		const regex = API_CATEGORIES[category];
		if (!regex) return allApis;
		return allApis.filter((a) => regex.test(a.apistring));
	});

	async function load() {
		if (loaded) return;
		try {
			allApis = await getAPI();
			loaded = true;
		} catch { /* */ }
	}

	function handleAdd(entry: APIEntry) {
		onadd(entry.apistring, entry.units);
	}
</script>

<div class="flex flex-col gap-2">
	<div class="flex gap-2">
		<select
			class="bg-surface-bright text-text text-xs border border-border rounded px-2 py-1 flex-1"
			bind:value={category}
			onfocus={load}
		>
			{#each Object.keys(API_CATEGORIES) as cat}
				<option value={cat}>{cat}</option>
			{/each}
		</select>
	</div>
	{#if loaded}
		<div class="max-h-48 overflow-y-auto border border-border rounded">
			{#each filtered() as entry}
				<button
					class="w-full text-left px-3 py-1 text-xs hover:bg-surface-bright text-text-dim hover:text-text border-b border-border last:border-b-0"
					onclick={() => handleAdd(entry)}
				>
					{entry.apistring}
					<span class="text-text-dim/50 ml-2">{entry.name}</span>
				</button>
			{/each}
		</div>
	{:else}
		<p class="text-text-dim text-xs px-2">Click to load API list</p>
	{/if}
</div>
