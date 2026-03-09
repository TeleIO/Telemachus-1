<script lang="ts">
	import { dashboard } from './layoutStore.svelte.js';
	import { standardCharts } from '../charts/ChartConfig.js';
	import { RESOURCES } from '$lib/api/constants.js';
	import type { WidgetConfig, GaugeWidgetConfig, ResourceBarWidgetConfig } from './types.js';

	let widget = $derived(
		dashboard.editingWidgetId
			? dashboard.layout.widgets.find((w) => w.id === dashboard.editingWidgetId)
			: null
	);

	function close() {
		dashboard.editingWidgetId = null;
	}

	function updateConfig(config: WidgetConfig) {
		if (!dashboard.editingWidgetId) return;
		dashboard.updateWidgetConfig(dashboard.editingWidgetId, config);
	}

	function updateTitle(title: string) {
		if (!dashboard.editingWidgetId) return;
		dashboard.updateWidgetTitle(dashboard.editingWidgetId, title);
	}

	let newApi = $state('');
	function addApi() {
		if (!widget || widget.config.type !== 'telemetry-list' || !newApi.trim()) return;
		const apis = [...widget.config.apis, newApi.trim()];
		updateConfig({ ...widget.config, apis });
		newApi = '';
	}
	function removeApi(index: number) {
		if (!widget || widget.config.type !== 'telemetry-list') return;
		const apis = widget.config.apis.filter((_: string, i: number) => i !== index);
		updateConfig({ ...widget.config, apis });
	}

	function toggleResource(res: string, checked: boolean) {
		if (!widget || widget.config.type !== 'resource-bar') return;
		const resources = checked
			? [...widget.config.resources, res]
			: widget.config.resources.filter((r: string) => r !== res);
		updateConfig({ type: 'resource-bar', resources });
	}
</script>

{#if widget}
	<!-- svelte-ignore a11y_no_static_element_interactions -->
	<!-- svelte-ignore a11y_click_events_have_key_events -->
	<div class="fixed inset-0 bg-black/50 z-50 flex items-center justify-center" onclick={close}>
		<div
			class="bg-surface border border-border rounded-lg shadow-xl w-96 max-h-[80vh] overflow-y-auto"
			onclick={(e) => e.stopPropagation()}
		>
			<div class="flex items-center justify-between px-4 py-3 border-b border-border">
				<span class="text-sm font-bold text-text">Configure Widget</span>
				<button class="text-text-dim hover:text-text text-lg" onclick={close}>&times;</button>
			</div>

			<div class="p-4 flex flex-col gap-3">
				<label class="flex flex-col gap-1">
					<span class="text-xs text-text-dim">Title</span>
					<input
						class="bg-surface-bright border border-border rounded px-2 py-1 text-xs text-text"
						value={widget.title}
						oninput={(e) => updateTitle((e.target as HTMLInputElement).value)}
					/>
				</label>

				{#if widget.config.type === 'timeseries'}
					<label class="flex flex-col gap-1">
						<span class="text-xs text-text-dim">Chart</span>
						<select
							class="bg-surface-bright border border-border rounded px-2 py-1 text-xs text-text"
							value={widget.config.chartName}
							onchange={(e) => updateConfig({ type: 'timeseries', chartName: (e.target as HTMLSelectElement).value })}
						>
							{#each Object.keys(standardCharts) as name}
								<option value={name}>{name}</option>
							{/each}
						</select>
					</label>

				{:else if widget.config.type === 'gauge'}
					<label class="flex flex-col gap-1">
						<span class="text-xs text-text-dim">API</span>
						<input
							class="bg-surface-bright border border-border rounded px-2 py-1 text-xs text-text"
							value={widget.config.api}
							oninput={(e) => updateConfig({ ...widget.config, api: (e.target as HTMLInputElement).value } as GaugeWidgetConfig)}
						/>
					</label>
					<label class="flex flex-col gap-1">
						<span class="text-xs text-text-dim">Label</span>
						<input
							class="bg-surface-bright border border-border rounded px-2 py-1 text-xs text-text"
							value={widget.config.label}
							oninput={(e) => updateConfig({ ...widget.config, label: (e.target as HTMLInputElement).value } as GaugeWidgetConfig)}
						/>
					</label>
					<label class="flex items-center gap-2">
						<input
							type="checkbox"
							checked={widget.config.sparkline ?? false}
							onchange={(e) => updateConfig({ ...widget.config, sparkline: (e.target as HTMLInputElement).checked } as GaugeWidgetConfig)}
						/>
						<span class="text-xs text-text-dim">Show sparkline</span>
					</label>

				{:else if widget.config.type === 'telemetry-list'}
					<div class="flex flex-col gap-1">
						<span class="text-xs text-text-dim">APIs</span>
						<div class="max-h-40 overflow-y-auto border border-border rounded">
							{#each widget.config.apis as api, i}
								<div class="flex items-center justify-between px-2 py-1 text-xs border-b border-border last:border-b-0">
									<span class="text-text">{api}</span>
									<button class="text-danger/50 hover:text-danger" onclick={() => removeApi(i)}>&times;</button>
								</div>
							{/each}
						</div>
						<div class="flex gap-1">
							<input
								class="bg-surface-bright border border-border rounded px-2 py-1 text-xs text-text flex-1"
								placeholder="e.g. v.altitude"
								bind:value={newApi}
								onkeydown={(e) => e.key === 'Enter' && addApi()}
							/>
							<button
								class="px-2 py-1 text-xs rounded border border-accent text-accent hover:bg-accent/10"
								onclick={addApi}
							>Add</button>
						</div>
					</div>

				{:else if widget.config.type === 'resource-bar'}
					<div class="flex flex-col gap-1">
						<span class="text-xs text-text-dim">Resources</span>
						{#each RESOURCES as res}
							<label class="flex items-center gap-2">
								<input
									type="checkbox"
									checked={widget.config.resources.includes(res)}
									onchange={(e) => toggleResource(res, (e.target as HTMLInputElement).checked)}
								/>
								<span class="text-xs text-text">{res}</span>
							</label>
						{/each}
					</div>

				{:else}
					<p class="text-xs text-text-dim">No additional configuration for this widget type.</p>
				{/if}
			</div>
		</div>
	</div>
{/if}
