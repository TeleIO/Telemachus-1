import L from 'leaflet';

const BASE_URL = 'https://d3kmnwgldcmvsd.cloudfront.net';

export type MapStyle = 'sat' | 'biome' | 'slope';

export interface BodyLayer {
	name: string;
	body: string;
	style: MapStyle;
	pack: 'Stock' | 'JNSQ';
	layer: L.TileLayer;
}

function makeLayer(url: string, body: string, style: string): L.TileLayer {
	return L.tileLayer(`${url}/tiles/${body}/${style}/{z}/{x}/{y}.png`, {
		attribution: '&copy; <a href="https://kerbal-maps.finitemonkeys.org/">Kerbal Maps</a>',
		tms: true
	});
}

const STOCK_BODIES = [
	'moho', 'eve', 'gilly', 'kerbin', 'mun', 'minmus', 'duna', 'ike',
	'dres', 'laythe', 'vall', 'tylo', 'bop', 'pol', 'eeloo'
];

const JNSQ_BODIES = [
	'moho', 'eve', 'gilly', 'kerbin', 'mun', 'minmus', 'duna', 'ike',
	'edna', 'dak', 'dres', 'laythe', 'vall', 'tylo', 'bop', 'pol',
	'krel', 'aden', 'huygen', 'riga', 'talos', 'eeloo', 'celes', 'tam',
	'hamek', 'nara', 'amos', 'enon', 'prax'
];

const STYLES: MapStyle[] = ['sat', 'biome', 'slope'];

function capitalize(s: string): string {
	return s.charAt(0).toUpperCase() + s.slice(1);
}

export function getAllLayers(): BodyLayer[] {
	const layers: BodyLayer[] = [];

	for (const body of STOCK_BODIES) {
		for (const style of STYLES) {
			layers.push({
				name: `${capitalize(body)} ${capitalize(style)} (Stock)`,
				body,
				style,
				pack: 'Stock',
				layer: makeLayer(BASE_URL, body, style)
			});
		}
	}

	for (const body of JNSQ_BODIES) {
		for (const style of STYLES) {
			layers.push({
				name: `${capitalize(body)} ${capitalize(style)} (JNSQ)`,
				body,
				style,
				pack: 'JNSQ',
				layer: makeLayer(`${BASE_URL}/jnsq`, body, style)
			});
		}
	}

	return layers;
}

export function getDefaultLayer(): BodyLayer {
	return {
		name: 'Kerbin Satellite (Stock)',
		body: 'kerbin',
		style: 'sat',
		pack: 'Stock',
		layer: makeLayer(BASE_URL, 'kerbin', 'sat')
	};
}
