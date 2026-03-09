import { standardLayouts, standardCharts } from '../charts/ChartConfig.js';
import type { DashboardLayout, WidgetInstance } from './types.js';
import { GRID_COLUMNS, DEFAULT_ROW_HEIGHT } from './gridUtils.js';

let idCounter = 0;
function stableId(): string {
	return `preset-${idCounter++}`;
}

export function migrateLayout(name: string): DashboardLayout {
	const def = standardLayouts[name];
	if (!def) return emptyLayout(name);

	idCounter = 0;
	const widgets: WidgetInstance[] = [];
	const chartH = 6;
	const chartW = 16;
	const sideW = GRID_COLUMNS - chartW;

	def.charts.forEach((chartName, i) => {
		const chartDef = standardCharts[chartName];
		widgets.push({
			id: stableId(),
			title: chartName,
			grid: { x: 0, y: i * chartH, w: chartW, h: chartH },
			config: chartDef?.type === 'map'
				? { type: 'map' }
				: { type: 'timeseries', chartName }
		});
	});

	if (def.telemetry.length > 0) {
		widgets.push({
			id: stableId(),
			title: 'Telemetry',
			grid: { x: chartW, y: 0, w: sideW, h: Math.max(def.charts.length * chartH, 6) },
			config: { type: 'telemetry-list', apis: def.telemetry }
		});
	}

	return { name, columns: GRID_COLUMNS, rowHeight: DEFAULT_ROW_HEIGHT, widgets };
}

export function emptyLayout(name: string): DashboardLayout {
	return { name, columns: GRID_COLUMNS, rowHeight: DEFAULT_ROW_HEIGHT, widgets: [] };
}

export function getAllPresetNames(): string[] {
	return Object.keys(standardLayouts);
}

export function getPreset(name: string): DashboardLayout {
	return migrateLayout(name);
}
