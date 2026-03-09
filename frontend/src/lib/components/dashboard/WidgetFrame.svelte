<script lang="ts">
	import type { WidgetInstance } from './types.js';
	import { dashboard } from './layoutStore.svelte.js';
	import WidgetRenderer from './WidgetRenderer.svelte';
	import type { Snippet } from 'svelte';

	interface Props {
		widget: WidgetInstance;
	}

	let { widget }: Props = $props();
</script>

<div class="flex flex-col border border-border rounded bg-surface h-full w-full overflow-hidden">
	<!-- Title bar / drag handle -->
	<div
		class="flex items-center gap-1.5 px-2 py-1 border-b border-border shrink-0 select-none drag-handle"
	>
		<span class="cursor-grab text-text-dim text-xs mr-1" title="Drag to move">&#x2630;</span>
		<span class="text-xs font-bold text-text truncate flex-1">{widget.title}</span>
		{#if dashboard.editing}
			<button
				class="text-text-dim hover:text-accent text-xs px-1"
				title="Edit"
				onclick={() => (dashboard.editingWidgetId = widget.id)}
			>
				&#x2699;
			</button>
			<button
				class="text-text-dim hover:text-danger text-xs px-1"
				title="Remove"
				onclick={() => dashboard.removeWidget(widget.id)}
			>
				&times;
			</button>
		{/if}
	</div>

	<!-- Widget content -->
	<div class="flex-1 min-h-0 min-w-0 relative">
		<WidgetRenderer config={widget.config} />
	</div>

	<!-- Resize handle -->
	{#if dashboard.editing}
		<div
			class="resize-handle absolute bottom-0 right-0 w-3 h-3 cursor-nwse-resize"
			style="background: linear-gradient(135deg, transparent 50%, var(--color-text-dim) 50%);"
		></div>
	{/if}
</div>
