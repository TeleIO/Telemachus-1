export const CELESTIAL_BODIES = [
	'Kerbol', 'Kerbin', 'Mun', 'Minmus', 'Moho', 'Eve', 'Duna', 'Ike',
	'Jool', 'Laythe', 'Vall', 'Bop', 'Tylo', 'Gilly', 'Pol', 'Dres', 'Eeloo'
] as const;

export const RESOURCES = [
	'ElectricCharge', 'SolidFuel', 'LiquidFuel', 'Oxidizer',
	'MonoPropellant', 'IntakeAir', 'XenonGas'
] as const;

export const API_CATEGORIES: Record<string, RegExp> = {
	Vessel: /^v\./,
	Orbit: /^o\./,
	NavBall: /^n\./,
	Sensors: /^s\./,
	Target: /^tar\./,
	Docking: /^dock\./,
	Resources: /^r\./,
	Bodies: /^b\./,
	Flight: /^f\./,
	MechJeb: /^mj\./,
	Time: /^t\./,
	'Delta-V': /^dv\./,
	Alarms: /^alarm\./,
	Game: /^p\./,
	Admin: /^a\./
};

export const PAUSE_MESSAGES: Record<number, string> = {
	0: 'Signal found.',
	1: 'Game paused.',
	2: 'Potential power loss on antenna.',
	3: 'Antenna is deactivated.',
	4: 'Unable to reach antenna.'
};

export const DEFAULT_PORT = 8085;
