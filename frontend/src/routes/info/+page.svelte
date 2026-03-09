<script lang="ts">
	import { getAPI, getVersion } from '$lib/api/client.js';
	import { API_CATEGORIES } from '$lib/api/constants.js';
	import type { APIEntry } from '$lib/api/types.js';

	let apiEntries = $state<APIEntry[]>([]);
	let version = $state('');
	let loading = $state(true);
	let error = $state('');
	let expandedCategory = $state<string | null>(null);

	$effect(() => {
		loadData();
	});

	async function loadData() {
		try {
			const [api, ver] = await Promise.all([getAPI(), getVersion()]);
			apiEntries = api;
			version = ver;
		} catch (e) {
			error = 'Could not connect to Telemachus server.';
		} finally {
			loading = false;
		}
	}

	function categorize(entries: APIEntry[]): Record<string, APIEntry[]> {
		const result: Record<string, APIEntry[]> = {};
		for (const [name, regex] of Object.entries(API_CATEGORIES)) {
			const matches = entries.filter((e) => regex.test(e.apistring));
			if (matches.length > 0) result[name] = matches;
		}
		const categorized = new Set(Object.values(result).flat());
		const uncategorized = entries.filter((e) => !categorized.has(e));
		if (uncategorized.length > 0) result['Other'] = uncategorized;
		return result;
	}

	let categorized = $derived(categorize(apiEntries));
</script>

<div class="p-6 max-w-4xl mx-auto">
	<h1 class="text-xl font-bold text-accent-bright mb-1">Telemachus</h1>
	{#if version}
		<p class="text-text-dim text-xs mb-6">Version {version}</p>
	{/if}

	{#if loading}
		<p class="text-text-dim">Loading API...</p>
	{:else if error}
		<p class="text-danger">{error}</p>
	{:else}
		<h2 class="text-sm font-bold text-text mb-3">API Reference ({apiEntries.length} endpoints)</h2>

		<div class="flex flex-col gap-1">
			{#each Object.entries(categorized) as [category, entries]}
				<div class="border border-border rounded">
					<button
						class="w-full text-left px-4 py-2 text-sm font-bold text-accent-bright hover:bg-surface-bright flex justify-between items-center"
						onclick={() => (expandedCategory = expandedCategory === category ? null : category)}
					>
						<span>{category}</span>
						<span class="text-text-dim text-xs">{entries.length}</span>
					</button>

					{#if expandedCategory === category}
						<div class="border-t border-border">
							{#each entries as entry}
								<div class="px-4 py-1.5 text-xs flex gap-4 border-b border-border last:border-b-0 hover:bg-surface-bright">
									<code class="text-accent shrink-0 w-64">{entry.apistring}</code>
									<span class="text-text flex-1">{entry.name}</span>
									{#if entry.units}
										<span class="text-text-dim shrink-0">{entry.units}</span>
									{/if}
								</div>
							{/each}
						</div>
					{/if}
				</div>
			{/each}
		</div>
	{/if}
</div>
