<script lang="ts">
	import { dashboard } from './layoutStore.svelte.js';
	import { clampToGrid, findOverlaps, snapToGrid, gridMaxRow } from './gridUtils.js';
	import type { GridCell } from './types.js';
	import WidgetFrame from './WidgetFrame.svelte';

	let gridEl: HTMLDivElement;

	// Drag/resize state
	let activeId: string | null = $state(null);
	let mode: 'drag' | 'resize' | null = $state(null);
	let ghost: GridCell | null = $state(null);
	let startPx = { x: 0, y: 0 };
	let startGrid: GridCell = { x: 0, y: 0, w: 0, h: 0 };

	function cellSize(): { cw: number; rh: number } {
		if (!gridEl) return { cw: 1, rh: 1 };
		return {
			cw: gridEl.clientWidth / dashboard.layout.columns,
			rh: dashboard.layout.rowHeight
		};
	}

	function onPointerDown(e: PointerEvent, widgetId: string, action: 'drag' | 'resize') {
		if (!dashboard.editing) return;
		const widget = dashboard.layout.widgets.find((w) => w.id === widgetId);
		if (!widget) return;

		e.preventDefault();
		(e.target as HTMLElement).setPointerCapture(e.pointerId);

		activeId = widgetId;
		mode = action;
		startPx = { x: e.clientX, y: e.clientY };
		startGrid = { ...widget.grid };
		ghost = { ...widget.grid };
	}

	function onPointerMove(e: PointerEvent) {
		if (!activeId || !mode || !ghost) return;
		const { cw, rh } = cellSize();
		const dx = snapToGrid(e.clientX - startPx.x, cw);
		const dy = snapToGrid(e.clientY - startPx.y, rh);

		const widget = dashboard.layout.widgets.find((w) => w.id === activeId);
		if (!widget) return;

		let candidate: GridCell;
		if (mode === 'drag') {
			candidate = { x: startGrid.x + dx, y: startGrid.y + dy, w: startGrid.w, h: startGrid.h };
		} else {
			candidate = { x: startGrid.x, y: startGrid.y, w: startGrid.w + dx, h: startGrid.h + dy };
		}
		ghost = clampToGrid(candidate, dashboard.layout.columns, widget.config.type);
	}

	function onPointerUp() {
		if (!activeId || !ghost) {
			reset();
			return;
		}

		const overlaps = findOverlaps(ghost, dashboard.layout.widgets, activeId);
		if (overlaps.length === 0) {
			dashboard.updateWidgetGrid(activeId, ghost);
		}
		reset();
	}

	function reset() {
		activeId = null;
		mode = null;
		ghost = null;
	}

	let minRows = $derived(Math.max(gridMaxRow(dashboard.layout.widgets) + 2, 15));
</script>

<!-- svelte-ignore a11y_no_static_element_interactions -->
<div
	bind:this={gridEl}
	class="dashboard-grid relative"
	style="
		display: grid;
		grid-template-columns: repeat({dashboard.layout.columns}, 1fr);
		grid-auto-rows: {dashboard.layout.rowHeight}px;
		gap: 4px;
		padding: 4px;
		min-height: {minRows * dashboard.layout.rowHeight}px;
	"
	onpointermove={onPointerMove}
	onpointerup={onPointerUp}
	onpointercancel={onPointerUp}
>
	{#each dashboard.layout.widgets as widget (widget.id)}
		{@const isActive = activeId === widget.id}
		<div
			class="relative"
			style="
				grid-column: {widget.grid.x + 1} / span {widget.grid.w};
				grid-row: {widget.grid.y + 1} / span {widget.grid.h};
				{isActive ? 'opacity: 0.4; pointer-events: none;' : ''}
			"
		>
			<!-- Drag handle intercept -->
			<!-- svelte-ignore a11y_no_static_element_interactions -->
			<div
				class="absolute inset-0 z-10"
				style="pointer-events: none;"
			>
				<!-- Drag handle overlay - only captures events on the drag handle area -->
			</div>
			<div
				class="h-full w-full"
				onpointerdown={(e) => {
					const target = e.target as HTMLElement;
					if (target.closest('.drag-handle')) {
						onPointerDown(e, widget.id, 'drag');
					} else if (target.closest('.resize-handle')) {
						onPointerDown(e, widget.id, 'resize');
					}
				}}
			>
				<WidgetFrame {widget} />
			</div>
		</div>
	{/each}

	<!-- Ghost preview during drag/resize -->
	{#if ghost && activeId}
		<div
			class="border-2 border-accent/50 bg-accent/10 rounded pointer-events-none"
			style="
				grid-column: {ghost.x + 1} / span {ghost.w};
				grid-row: {ghost.y + 1} / span {ghost.h};
			"
		></div>
	{/if}
</div>
