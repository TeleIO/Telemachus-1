<script lang="ts">
	import { dashboard } from './layoutStore.svelte.js';
	import AddWidgetMenu from './AddWidgetMenu.svelte';

	let showSave = $state(false);
	let saveName = $state('');
	let showAdd = $state(false);

	function save() {
		if (!saveName.trim()) return;
		dashboard.saveLayout(saveName.trim());
		showSave = false;
		saveName = '';
	}

	function exportLayout() {
		const json = dashboard.exportLayout();
		const blob = new Blob([json], { type: 'application/json' });
		const url = URL.createObjectURL(blob);
		const a = document.createElement('a');
		a.href = url;
		a.download = `${dashboard.layout.name || 'layout'}.json`;
		a.click();
		URL.revokeObjectURL(url);
	}

	function importLayout() {
		const input = document.createElement('input');
		input.type = 'file';
		input.accept = '.json';
		input.onchange = async () => {
			const file = input.files?.[0];
			if (!file) return;
			const text = await file.text();
			dashboard.importLayout(text);
		};
		input.click();
	}
</script>

<div class="flex items-center gap-2 px-4 py-2 border-b border-border bg-surface shrink-0 overflow-x-auto text-xs">
	<span class="text-text-dim shrink-0">Layout:</span>

	<!-- Presets -->
	{#each dashboard.presetNames as name}
		<button
			class="px-2 py-0.5 rounded border transition-colors shrink-0
				{dashboard.layout.name === name
					? 'border-accent text-accent-bright bg-accent/10'
					: 'border-border text-text-dim hover:text-text hover:border-accent'}"
			onclick={() => dashboard.applyPreset(name)}
		>
			{name}
		</button>
	{/each}

	<!-- Saved layouts -->
	{#each dashboard.savedNames as name}
		<div class="flex items-center shrink-0">
			<button
				class="px-2 py-0.5 rounded-l border transition-colors
					{dashboard.layout.name === name
						? 'border-accent text-accent-bright bg-accent/10'
						: 'border-border text-text-dim hover:text-text hover:border-accent'}"
				onclick={() => dashboard.applyLayout(name)}
			>
				{name}
			</button>
			<button
				class="px-1 py-0.5 rounded-r border-y border-r border-border text-danger/50 hover:text-danger"
				onclick={() => dashboard.deleteLayout(name)}
			>&times;</button>
		</div>
	{/each}

	<span class="border-l border-border h-4 mx-1"></span>

	<!-- Save -->
	{#if showSave}
		<input
			class="bg-surface-bright border border-border rounded px-2 py-0.5 text-text w-28"
			placeholder="Layout name"
			bind:value={saveName}
			onkeydown={(e) => e.key === 'Enter' && save()}
		/>
		<button class="px-2 py-0.5 rounded bg-accent text-white hover:bg-accent-bright" onclick={save}>
			Save
		</button>
		<button class="text-text-dim hover:text-text" onclick={() => (showSave = false)}>Cancel</button>
	{:else}
		<button
			class="px-2 py-0.5 rounded border border-border text-text-dim hover:text-text hover:border-accent shrink-0"
			onclick={() => (showSave = true)}
		>
			Save...
		</button>
	{/if}

	<!-- Import/Export -->
	<button
		class="px-2 py-0.5 rounded border border-border text-text-dim hover:text-text hover:border-accent shrink-0"
		onclick={exportLayout}
		title="Export layout as JSON"
	>Export</button>
	<button
		class="px-2 py-0.5 rounded border border-border text-text-dim hover:text-text hover:border-accent shrink-0"
		onclick={importLayout}
		title="Import layout from JSON"
	>Import</button>

	<span class="border-l border-border h-4 mx-1"></span>

	<!-- Edit mode toggle -->
	<button
		class="px-2 py-0.5 rounded border transition-colors shrink-0
			{dashboard.editing
				? 'border-warning text-warning bg-warning/10'
				: 'border-border text-text-dim hover:text-text hover:border-accent'}"
		onclick={() => (dashboard.editing = !dashboard.editing)}
	>
		{dashboard.editing ? 'Done' : 'Edit'}
	</button>

	<!-- Add widget -->
	{#if dashboard.editing}
		<div class="relative shrink-0">
			<button
				class="px-2 py-0.5 rounded border border-accent text-accent hover:bg-accent/10"
				onclick={() => (showAdd = !showAdd)}
			>
				+ Add Widget
			</button>
			{#if showAdd}
				<div class="absolute top-full left-0 mt-1 z-50">
					<AddWidgetMenu onclose={() => (showAdd = false)} />
				</div>
			{/if}
		</div>
	{/if}
</div>
