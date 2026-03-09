import { formatScale } from './units.js';
import { durationString, dateString } from './time.js';

function fix(v: unknown): string {
	if (v === undefined || v === null) return '0';
	const n = Number(v);
	if (isNaN(n)) return String(v);
	return n.toPrecision(6).replace(/((\.\d*?[1-9])|\.)0+($|e)/, '$2$3');
}

function sigFigs(n: number, sig: number): number {
	if (n === 0) return 0;
	const abs = Math.abs(n);
	const mult = Math.pow(10, sig - Math.floor(Math.log10(abs)) - 1);
	return Math.round(n * mult) / mult;
}

const DISTANCE_UNITS = ['Too Large', 'm', 'km', 'Mm', 'Gm', 'Tm'];
const VELOCITY_UNITS = ['Too Large', 'm/s', 'km/s', 'Mm/s', 'Gm/s', 'Tm/s'];

export const formatters: Record<string, (v: number) => string> = {
	velocity(v: number): string {
		const f = formatScale(v, 1000, VELOCITY_UNITS);
		return `${fix(f.value)} ${f.unit}`;
	},

	distance(v: number): string {
		const f = formatScale(v, 1000, DISTANCE_UNITS);
		return `${fix(f.value)} ${f.unit}`;
	},

	time(v: number): string {
		return durationString(v);
	},

	date(v: number): string {
		return dateString(v);
	},

	deg(v: number): string {
		return `${fix(v)}\u00B0`;
	},

	latlon(v: number): string {
		return `${fix(v)}\u00B0`;
	},

	string(v: number): string {
		return String(v);
	},

	g(v: number): string {
		return `${fix(v)} G`;
	},

	acc(v: number): string {
		return `${fix(v)} m/s\u00B2`;
	},

	pres(v: number): string {
		return `${fix(v)} kPa`;
	},

	temp(v: number): string {
		return `${fix(v)} K`;
	},

	density(v: number): string {
		return `${fix(v)} kg/m\u00B3`;
	},

	dynamicpressure(v: number): string {
		return `${fix(v)} Pa`;
	}
};

// Map server enum names to formatter keys
const UNIT_ALIASES: Record<string, string> = {
	dynamicpressure: 'dynamicpressure',
	grav: 'acc',
};

function resolveUnit(raw: string): string {
	const lower = raw.toLowerCase();
	return UNIT_ALIASES[lower] ?? lower;
}

export function formatValue(value: unknown, units?: string): string {
	if (value === null || value === undefined) return 'No Data';
	if (typeof value === 'boolean') return value ? 'True' : 'False';
	if (typeof value === 'string') return value;

	const key = resolveUnit(units ?? '');
	const formatter = formatters[key];
	if (formatter && typeof value === 'number') {
		return formatter(value);
	}

	if (typeof value === 'number') return fix(value);
	return String(value);
}

/** Infer the unit type for a Telemachus API string. */
const EXACT_UNITS: Record<string, string> = {
	// Vessel
	'v.altitude': 'distance', 'v.heightFromTerrain': 'distance', 'v.heightFromSurface': 'distance',
	'v.terrainHeight': 'distance', 'v.pqsAltitude': 'distance', 'v.distanceToSun': 'distance',
	'v.long': 'latlon', 'v.lat': 'latlon',
	'v.surfaceVelocity': 'velocity', 'v.orbitalVelocity': 'velocity',
	'v.surfaceSpeed': 'velocity', 'v.verticalSpeed': 'velocity',
	'v.speed': 'velocity', 'v.srfSpeed': 'velocity', 'v.obtSpeed': 'velocity',
	'v.angularVelocity': 'velocity', 'v.speedOfSound': 'velocity', 'v.indicatedAirSpeed': 'velocity',
	'v.geeForce': 'g', 'v.geeForceImmediate': 'g',
	'v.acceleration': 'acc', 'v.specificAcceleration': 'acc',
	'v.atmosphericDensity': 'density',
	'v.dynamicPressurekPa': 'dynamicpressure', 'v.dynamicPressure': 'dynamicpressure',
	'v.staticPressurekPa': 'pres', 'v.staticPressure': 'pres',
	'v.atmosphericPressurePa': 'pres', 'v.atmosphericPressure': 'pres',
	'v.atmosphericTemperature': 'temp', 'v.externalTemperature': 'temp',
	'v.missionTime': 'time', 'v.launchTime': 'date',
	'v.angleToPrograde': 'deg',
	// Orbit
	'o.PeA': 'distance', 'o.ApA': 'distance', 'o.PeR': 'distance', 'o.ApR': 'distance',
	'o.sma': 'distance', 'o.semiMinorAxis': 'distance', 'o.semiLatusRectum': 'distance', 'o.radius': 'distance',
	'o.timeToAp': 'time', 'o.timeToPe': 'time', 'o.period': 'time',
	'o.timeToTransition1': 'time', 'o.timeToTransition2': 'time',
	'o.inclination': 'deg', 'o.lan': 'deg', 'o.argumentOfPeriapsis': 'deg',
	'o.trueAnomaly': 'deg', 'o.meanAnomaly': 'deg', 'o.eccentricAnomaly': 'deg',
	'o.timeOfPeriapsisPassage': 'date', 'o.StartUT': 'date', 'o.EndUT': 'date', 'o.UTsoi': 'date',
	'o.relativeVelocity': 'velocity', 'o.orbitalSpeed': 'velocity',
	// NavBall
	'n.heading': 'deg', 'n.pitch': 'deg', 'n.roll': 'deg',
	'n.heading2': 'deg', 'n.pitch2': 'deg', 'n.roll2': 'deg',
	'n.rawheading': 'deg', 'n.rawpitch': 'deg', 'n.rawroll': 'deg',
	'n.rawheading2': 'deg', 'n.rawpitch2': 'deg', 'n.rawroll2': 'deg',
	// Sensors
	's.sensor.temp': 'temp', 's.sensor.pres': 'pres',
	's.sensor.grav': 'acc', 's.sensor.acc': 'g',
	// Target
	'tar.distance': 'distance',
	'tar.o.relativeVelocity': 'velocity', 'tar.o.velocity': 'velocity',
	'tar.o.PeA': 'distance', 'tar.o.ApA': 'distance', 'tar.o.sma': 'distance',
	'tar.o.timeToAp': 'time', 'tar.o.timeToPe': 'time', 'tar.o.period': 'time',
	'tar.o.inclination': 'deg', 'tar.o.lan': 'deg', 'tar.o.argumentOfPeriapsis': 'deg',
	'tar.o.trueAnomaly': 'deg', 'tar.o.timeOfPeriapsisPassage': 'date',
	// Docking
	'dock.ax': 'deg', 'dock.ay': 'deg', 'dock.az': 'deg',
	'dock.x': 'distance', 'dock.y': 'distance',
	// Body
	'b.radius': 'distance', 'b.soi': 'distance', 'b.hillSphere': 'distance',
	'b.maxAtmosphere': 'distance', 'b.geeASL': 'g',
	'b.rotationPeriod': 'time', 'b.rotationAngle': 'deg',
	// Time
	't.universalTime': 'date',
	// Delta-V
	'dv.totalDVVac': 'velocity', 'dv.totalDVASL': 'velocity', 'dv.totalDVActual': 'velocity',
	'dv.totalBurnTime': 'time', 'dv.stageBurnTime': 'time',
	'dv.stageDVVac': 'velocity', 'dv.stageDVASL': 'velocity', 'dv.stageDVActual': 'velocity',
	// Alarms
	'alarm.timeToNext': 'time',
};

export function unitForApi(api: string): string {
	return EXACT_UNITS[api] ?? '';
}

export { fix, sigFigs };
