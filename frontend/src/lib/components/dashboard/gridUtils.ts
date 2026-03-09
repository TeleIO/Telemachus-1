import type { GridCell, WidgetInstance, WidgetType } from './types.js';
import { WIDGET_CATALOG } from './types.js';

export const GRID_COLUMNS = 24;
export const DEFAULT_ROW_HEIGHT = 40;

export function rectsOverlap(a: GridCell, b: GridCell): boolean {
	return a.x < b.x + b.w && a.x + a.w > b.x && a.y < b.y + b.h && a.y + a.h > b.y;
}

export function findOverlaps(target: GridCell, widgets: WidgetInstance[], excludeId?: string): string[] {
	const ids: string[] = [];
	for (const w of widgets) {
		if (w.id === excludeId) continue;
		if (rectsOverlap(target, w.grid)) ids.push(w.id);
	}
	return ids;
}

export function autoPlace(existing: WidgetInstance[], newW: number, newH: number, cols: number): GridCell {
	const maxRow = existing.reduce((m, w) => Math.max(m, w.grid.y + w.grid.h), 0);

	for (let y = 0; y <= maxRow + 10; y++) {
		for (let x = 0; x <= cols - newW; x++) {
			const candidate: GridCell = { x, y, w: newW, h: newH };
			if (findOverlaps(candidate, existing).length === 0) {
				return candidate;
			}
		}
	}
	return { x: 0, y: maxRow, w: newW, h: newH };
}

export function snapToGrid(px: number, cellSize: number): number {
	return Math.round(px / cellSize);
}

export function clampToGrid(cell: GridCell, cols: number, widgetType?: WidgetType): GridCell {
	const info = widgetType ? WIDGET_CATALOG.find((c) => c.type === widgetType) : undefined;
	const minW = info?.minW ?? 2;
	const minH = info?.minH ?? 2;

	const w = Math.max(cell.w, minW);
	const h = Math.max(cell.h, minH);
	const x = Math.max(0, Math.min(cell.x, cols - w));
	const y = Math.max(0, cell.y);

	return { x, y, w, h };
}

export function gridMaxRow(widgets: WidgetInstance[]): number {
	return widgets.reduce((m, w) => Math.max(m, w.grid.y + w.grid.h), 0);
}
