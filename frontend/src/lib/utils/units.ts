export interface ScaledValue {
	value: number;
	unit: string;
}

export function formatScale(v: number, scale: number, units: string[]): ScaledValue {
	let i = 1;
	const isNeg = v < 0;
	v = Math.abs(v);

	while (v > scale && i < units.length - 1) {
		v /= scale;
		i++;
	}

	if (i >= units.length) i = 0;
	if (isNeg) v = -v;

	return { value: v, unit: units[i] };
}

export function orderOfMagnitude(v: number): number {
	if (v === 0) return 0;
	return Math.floor(Math.log10(Math.abs(v)));
}

export function siUnit(v: number, baseUnit: string): ScaledValue {
	const prefixes = [
		{ threshold: 1e12, unit: `T${baseUnit}`, divisor: 1e12 },
		{ threshold: 1e9, unit: `G${baseUnit}`, divisor: 1e9 },
		{ threshold: 1e6, unit: `M${baseUnit}`, divisor: 1e6 },
		{ threshold: 1e3, unit: `k${baseUnit}`, divisor: 1e3 },
		{ threshold: 1, unit: baseUnit, divisor: 1 },
		{ threshold: 1e-3, unit: `m${baseUnit}`, divisor: 1e-3 },
		{ threshold: 1e-6, unit: `\u00B5${baseUnit}`, divisor: 1e-6 }
	];

	const abs = Math.abs(v);
	for (const p of prefixes) {
		if (abs >= p.threshold) {
			return { value: v / p.divisor, unit: p.unit };
		}
	}

	return { value: v, unit: baseUnit };
}
