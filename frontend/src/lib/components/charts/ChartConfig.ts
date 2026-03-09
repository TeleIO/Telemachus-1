export interface ChartDefinition {
	series: string[];
	type?: 'map';
	yaxis?: {
		label: string;
		unit: string;
		min: number | null;
		max: number | null;
	};
}

export interface LayoutDefinition {
	charts: string[];
	telemetry: string[];
}

export const standardCharts: Record<string, ChartDefinition> = {
	'Altitude': {
		series: ['v.altitude', 'v.heightFromTerrain'],
		yaxis: { label: 'Altitude', unit: 'm', min: 0, max: null }
	},
	'Apoapsis and Periapsis': {
		series: ['o.ApA', 'o.PeA'],
		yaxis: { label: 'Altitude', unit: 'm', min: 0, max: null }
	},
	'Atmospheric Density': {
		series: ['v.atmosphericDensity'],
		yaxis: { label: 'Density', unit: 'kg/m\u00B3', min: 0, max: null }
	},
	'Dynamic Pressure': {
		series: ['v.dynamicPressure'],
		yaxis: { label: 'Dynamic Pressure', unit: 'Pa', min: 0, max: null }
	},
	'G-Force': {
		series: ['s.sensor.acc'],
		yaxis: { label: 'Acceleration', unit: 'Gs', min: null, max: null }
	},
	'Gravity': {
		series: ['s.sensor.grav'],
		yaxis: { label: 'Gravity', unit: 'm/s\u00B2', min: 0, max: null }
	},
	'Pressure': {
		series: ['s.sensor.pres'],
		yaxis: { label: 'Pressure', unit: 'kPa', min: 0, max: null }
	},
	'Temperature': {
		series: ['s.sensor.temp'],
		yaxis: { label: 'Temperature', unit: '\u00B0K', min: null, max: null }
	},
	'Orbital Velocity': {
		series: ['v.orbitalVelocity'],
		yaxis: { label: 'Velocity', unit: 'm/s', min: 0, max: null }
	},
	'Surface Velocity': {
		series: ['v.surfaceSpeed', 'v.verticalSpeed'],
		yaxis: { label: 'Velocity', unit: 'm/s', min: null, max: null }
	},
	'Angular Velocity': {
		series: ['v.angularVelocity'],
		yaxis: { label: 'Angular Velocity', unit: '\u00B0/s', min: 0, max: null }
	},
	'Liquid Fuel and Oxidizer': {
		series: ['r.resource[LiquidFuel]', 'r.resource[Oxidizer]'],
		yaxis: { label: 'Volume', unit: 'L', min: 0, max: null }
	},
	'Electric Charge': {
		series: ['r.resource[ElectricCharge]'],
		yaxis: { label: 'Electric Charge', unit: 'Wh', min: 0, max: null }
	},
	'Monopropellant': {
		series: ['r.resource[MonoPropellant]'],
		yaxis: { label: 'Volume', unit: 'L', min: 0, max: null }
	},
	'Heading': {
		series: ['n.heading'],
		yaxis: { label: 'Angle', unit: '\u00B0', min: 0, max: 360 }
	},
	'Pitch': {
		series: ['n.pitch'],
		yaxis: { label: 'Angle', unit: '\u00B0', min: -90, max: 90 }
	},
	'Roll': {
		series: ['n.roll'],
		yaxis: { label: 'Angle', unit: '\u00B0', min: -180, max: 180 }
	},
	'Target Distance': {
		series: ['tar.distance'],
		yaxis: { label: 'Distance', unit: 'm', min: 0, max: null }
	},
	'Relative Velocity': {
		series: ['tar.o.relativeVelocity'],
		yaxis: { label: 'Velocity', unit: 'm/s', min: 0, max: null }
	},
	'True Anomaly': {
		series: ['o.trueAnomaly'],
		yaxis: { label: 'Angle', unit: '\u00B0', min: null, max: null }
	},
	'Map': {
		series: ['v.long', 'v.lat', 'v.name', 'v.body'],
		type: 'map'
	}
};

export const standardLayouts: Record<string, LayoutDefinition> = {
	'Flight Dynamics': {
		charts: ['Altitude', 'Orbital Velocity', 'True Anomaly'],
		telemetry: ['o.sma', 'o.eccentricity', 'o.inclination', 'o.lan', 'o.argumentOfPeriapsis', 'o.timeOfPeriapsisPassage', 'o.trueAnomaly', 'v.altitude', 'v.orbitalVelocity']
	},
	'Retrofire': {
		charts: ['Map', 'Altitude', 'Surface Velocity'],
		telemetry: ['v.altitude', 'v.heightFromTerrain', 'v.surfaceSpeed', 'v.verticalSpeed', 'v.lat', 'v.long']
	},
	'Booster Systems': {
		charts: ['Liquid Fuel and Oxidizer', 'Dynamic Pressure', 'Atmospheric Density'],
		telemetry: ['r.resource[LiquidFuel]', 'r.resourceMax[LiquidFuel]', 'r.resource[Oxidizer]', 'r.resourceMax[Oxidizer]', 'v.dynamicPressure', 'v.atmosphericDensity']
	},
	'Instrumentation': {
		charts: ['G-Force', 'Pressure', 'Temperature'],
		telemetry: ['s.sensor.acc', 's.sensor.pres', 's.sensor.temp', 's.sensor.grav']
	},
	'Electrical, Environmental and Comm.': {
		charts: ['Electric Charge', 'Pressure', 'Temperature'],
		telemetry: ['r.resource[ElectricCharge]', 'r.resourceMax[ElectricCharge]', 's.sensor.pres', 's.sensor.temp']
	},
	'Guidance, Navigation and Control': {
		charts: ['Heading', 'Pitch', 'Roll'],
		telemetry: ['r.resource[MonoPropellant]', 'r.resourceMax[MonoPropellant]', 'n.heading', 'n.pitch', 'n.roll']
	},
	'Rendezvous and Docking': {
		charts: ['Target Distance', 'Relative Velocity'],
		telemetry: ['tar.name', 'tar.o.sma', 'tar.o.eccentricity', 'tar.o.inclination', 'tar.o.lan', 'tar.o.argumentOfPeriapsis', 'tar.o.timeOfPeriapsisPassage', 'tar.o.trueAnomaly', 'tar.distance', 'tar.o.relativeVelocity']
	}
};

export const defaultLayout = 'Flight Dynamics';

const STORAGE_KEY = 'telemachus.console.customLayouts';

export function loadCustomLayouts(): Record<string, LayoutDefinition> {
	try {
		const raw = localStorage.getItem(STORAGE_KEY);
		return raw ? JSON.parse(raw) : {};
	} catch {
		return {};
	}
}

export function saveCustomLayouts(layouts: Record<string, LayoutDefinition>): void {
	localStorage.setItem(STORAGE_KEY, JSON.stringify(layouts));
}

export const CHART_COLORS = [
	'#3b82f6', '#22c55e', '#eab308', '#ef4444', '#a855f7', '#06b6d4'
];
