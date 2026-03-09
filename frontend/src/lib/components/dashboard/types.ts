// ---- Widget Types ----

export type WidgetType =
	| 'timeseries'
	| 'map'
	| 'telemetry-list'
	| 'gauge'
	| 'deltav'
	| 'resource-bar'
	| 'navball-info'
	| 'alarm-list'
	| 'docking-hud';

// ---- Widget Config (discriminated union) ----

export interface TimeSeriesWidgetConfig {
	type: 'timeseries';
	chartName: string;
}

export interface MapWidgetConfig {
	type: 'map';
}

export interface TelemetryListWidgetConfig {
	type: 'telemetry-list';
	apis: string[];
}

export interface GaugeWidgetConfig {
	type: 'gauge';
	api: string;
	label: string;
	unit?: string;
	sparkline?: boolean;
	thresholds?: { value: number; color: string }[];
}

export interface DeltaVWidgetConfig {
	type: 'deltav';
}

export interface ResourceBarWidgetConfig {
	type: 'resource-bar';
	resources: string[];
}

export interface NavBallInfoWidgetConfig {
	type: 'navball-info';
}

export interface AlarmListWidgetConfig {
	type: 'alarm-list';
	maxItems?: number;
}

export interface DockingHUDWidgetConfig {
	type: 'docking-hud';
}

export type WidgetConfig =
	| TimeSeriesWidgetConfig
	| MapWidgetConfig
	| TelemetryListWidgetConfig
	| GaugeWidgetConfig
	| DeltaVWidgetConfig
	| ResourceBarWidgetConfig
	| NavBallInfoWidgetConfig
	| AlarmListWidgetConfig
	| DockingHUDWidgetConfig;

// ---- Grid Placement ----

export interface GridCell {
	x: number;
	y: number;
	w: number;
	h: number;
}

// ---- Widget Instance ----

export interface WidgetInstance {
	id: string;
	title: string;
	grid: GridCell;
	config: WidgetConfig;
}

// ---- Dashboard Layout ----

export interface DashboardLayout {
	name: string;
	columns: number;
	rowHeight: number;
	widgets: WidgetInstance[];
}

// ---- Widget Metadata ----

export interface WidgetTypeInfo {
	type: WidgetType;
	label: string;
	category: string;
	minW: number;
	minH: number;
	defaultW: number;
	defaultH: number;
	defaultConfig: () => WidgetConfig;
}

export const WIDGET_CATALOG: WidgetTypeInfo[] = [
	{
		type: 'timeseries',
		label: 'Time Series Chart',
		category: 'Charts',
		minW: 6, minH: 5, defaultW: 16, defaultH: 6,
		defaultConfig: () => ({ type: 'timeseries', chartName: 'Altitude' })
	},
	{
		type: 'map',
		label: 'Map',
		category: 'Charts',
		minW: 6, minH: 4, defaultW: 12, defaultH: 8,
		defaultConfig: () => ({ type: 'map' })
	},
	{
		type: 'telemetry-list',
		label: 'Telemetry List',
		category: 'Data',
		minW: 4, minH: 3, defaultW: 8, defaultH: 8,
		defaultConfig: () => ({ type: 'telemetry-list', apis: ['v.altitude', 'v.orbitalVelocity'] })
	},
	{
		type: 'gauge',
		label: 'Gauge',
		category: 'Data',
		minW: 3, minH: 2, defaultW: 4, defaultH: 3,
		defaultConfig: () => ({ type: 'gauge', api: 'v.altitude', label: 'Altitude', sparkline: true })
	},
	{
		type: 'deltav',
		label: 'Delta-V Readout',
		category: 'Flight',
		minW: 6, minH: 4, defaultW: 8, defaultH: 6,
		defaultConfig: () => ({ type: 'deltav' })
	},
	{
		type: 'resource-bar',
		label: 'Resource Bars',
		category: 'Flight',
		minW: 4, minH: 2, defaultW: 6, defaultH: 4,
		defaultConfig: () => ({ type: 'resource-bar', resources: ['LiquidFuel', 'Oxidizer', 'ElectricCharge'] })
	},
	{
		type: 'navball-info',
		label: 'NavBall Info',
		category: 'Flight',
		minW: 4, minH: 2, defaultW: 6, defaultH: 3,
		defaultConfig: () => ({ type: 'navball-info' })
	},
	{
		type: 'alarm-list',
		label: 'Alarm List',
		category: 'Operations',
		minW: 4, minH: 3, defaultW: 6, defaultH: 4,
		defaultConfig: () => ({ type: 'alarm-list' })
	},
	{
		type: 'docking-hud',
		label: 'Docking HUD',
		category: 'Operations',
		minW: 5, minH: 5, defaultW: 6, defaultH: 6,
		defaultConfig: () => ({ type: 'docking-hud' })
	}
];
