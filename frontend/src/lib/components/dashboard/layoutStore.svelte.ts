import type { DashboardLayout, WidgetInstance, WidgetConfig, GridCell } from './types.js';
import { WIDGET_CATALOG } from './types.js';
import { autoPlace, GRID_COLUMNS, DEFAULT_ROW_HEIGHT } from './gridUtils.js';
import { getPreset, getAllPresetNames, emptyLayout } from './presets.js';

const STORAGE_CURRENT = 'telemachus.dashboard.current';
const STORAGE_SAVED = 'telemachus.dashboard.saved';
const DEFAULT_PRESET = 'Flight Dynamics';

class DashboardStore {
	layout: DashboardLayout = $state(emptyLayout(''));
	savedLayouts: Record<string, DashboardLayout> = $state({});
	editingWidgetId: string | null = $state(null);
	editing: boolean = $state(false);

	private persistTimer: ReturnType<typeof setTimeout> | null = null;

	constructor() {
		if (typeof window !== 'undefined') {
			this.loadFromStorage();
		}
	}

	// Widget CRUD

	addWidget(config: WidgetConfig, title: string): void {
		const info = WIDGET_CATALOG.find((c) => c.type === config.type);
		const dw = info?.defaultW ?? 6;
		const dh = info?.defaultH ?? 4;
		const grid = autoPlace(this.layout.widgets, dw, dh, this.layout.columns);
		const widget: WidgetInstance = {
			id: crypto.randomUUID(),
			title,
			grid,
			config
		};
		this.layout.widgets = [...this.layout.widgets, widget];
		this.schedulePersist();
	}

	removeWidget(id: string): void {
		this.layout.widgets = this.layout.widgets.filter((w) => w.id !== id);
		if (this.editingWidgetId === id) this.editingWidgetId = null;
		this.schedulePersist();
	}

	updateWidgetGrid(id: string, grid: GridCell): void {
		this.layout.widgets = this.layout.widgets.map((w) =>
			w.id === id ? { ...w, grid } : w
		);
		this.schedulePersist();
	}

	updateWidgetConfig(id: string, config: WidgetConfig): void {
		this.layout.widgets = this.layout.widgets.map((w) =>
			w.id === id ? { ...w, config } : w
		);
		this.schedulePersist();
	}

	updateWidgetTitle(id: string, title: string): void {
		this.layout.widgets = this.layout.widgets.map((w) =>
			w.id === id ? { ...w, title } : w
		);
		this.schedulePersist();
	}

	// Layout management

	applyPreset(name: string): void {
		this.layout = getPreset(name);
		this.schedulePersist();
	}

	applyLayout(name: string): void {
		const saved = this.savedLayouts[name];
		if (saved) {
			this.layout = structuredClone(saved);
		} else {
			this.applyPreset(name);
		}
	}

	saveLayout(name: string): void {
		const toSave = structuredClone(this.layout);
		toSave.name = name;
		this.savedLayouts = { ...this.savedLayouts, [name]: toSave };
		this.layout.name = name;
		this.schedulePersist();
	}

	deleteLayout(name: string): void {
		const { [name]: _, ...rest } = this.savedLayouts;
		this.savedLayouts = rest;
		this.schedulePersist();
	}

	exportLayout(): string {
		return JSON.stringify(this.layout, null, 2);
	}

	importLayout(json: string): void {
		try {
			const parsed = JSON.parse(json) as DashboardLayout;
			if (!parsed.widgets || !Array.isArray(parsed.widgets)) return;
			parsed.columns ??= GRID_COLUMNS;
			parsed.rowHeight ??= DEFAULT_ROW_HEIGHT;
			// Regenerate IDs to avoid collisions
			parsed.widgets = parsed.widgets.map((w) => ({
				...w,
				id: crypto.randomUUID()
			}));
			this.layout = parsed;
			this.schedulePersist();
		} catch { /* ignore invalid JSON */ }
	}

	get presetNames(): string[] {
		return getAllPresetNames();
	}

	get savedNames(): string[] {
		return Object.keys(this.savedLayouts);
	}

	// Persistence

	private schedulePersist(): void {
		if (this.persistTimer) clearTimeout(this.persistTimer);
		this.persistTimer = setTimeout(() => this.persist(), 500);
	}

	private persist(): void {
		try {
			localStorage.setItem(STORAGE_CURRENT, JSON.stringify(this.layout));
			localStorage.setItem(STORAGE_SAVED, JSON.stringify(this.savedLayouts));
		} catch { /* storage full or unavailable */ }
	}

	private loadFromStorage(): void {
		try {
			const savedRaw = localStorage.getItem(STORAGE_SAVED);
			if (savedRaw) this.savedLayouts = JSON.parse(savedRaw);

			const currentRaw = localStorage.getItem(STORAGE_CURRENT);
			if (currentRaw) {
				const parsed = JSON.parse(currentRaw) as DashboardLayout;
				if (parsed.widgets && Array.isArray(parsed.widgets)) {
					this.layout = parsed;
					return;
				}
			}
		} catch { /* ignore parse errors */ }

		this.applyPreset(DEFAULT_PRESET);
	}
}

export const dashboard = new DashboardStore();
