// Kerbal Maps tile layers. Replaces the 560-line hand-written layer-defs.js
// (one object literal per body×style) with a generated table — same tiles,
// same TMS scheme, same CDN, a fraction of the code.
//
// CDN verified live: serves sat / biome / slope (NOT the retired "color"); the
// finitemonkeys mirror is down, so we point at the working CloudFront origin.

import L from "leaflet";

const BASE = "https://d3kmnwgldcmvsd.cloudfront.net";

// Solid bodies that have surface tiles (gas giants / the sun are excluded).
const BODIES = [
  "moho", "eve", "gilly", "kerbin", "mun", "minmus", "duna", "ike",
  "laythe", "vall", "tylo", "bop", "pol", "dres", "eeloo",
];

const STYLES: [style: string, label: string][] = [
  ["sat", "Satellite"],
  ["biome", "Biome"],
  ["slope", "Slope"],
];

const cap = (s: string) => s[0].toUpperCase() + s.slice(1);

function tileLayer(url: string, body: string, style: string): L.TileLayer {
  return L.tileLayer(`${url}/tiles/${body}/${style}/{z}/{x}/{y}.png`, {
    tms: true,
    maxZoom: 7,
    attribution: '&copy; <a href="https://kerbal-maps.finitemonkeys.org/">Kerbal Maps</a>',
  });
}

/** Named base layers for Leaflet's layer control (Stock + JNSQ variants). */
export function baseLayers(): Record<string, L.TileLayer> {
  const out: Record<string, L.TileLayer> = {};
  for (const body of BODIES) {
    for (const [style, label] of STYLES) {
      out[`${cap(body)} ${label} (Stock)`] = tileLayer(BASE, body, style);
      out[`${cap(body)} ${label} (JNSQ)`] = tileLayer(`${BASE}/jnsq`, body, style);
    }
  }
  return out;
}

/** Marker icon built from the bundled assets (avoids Leaflet's broken default
 *  icon path under bundlers). */
export function defaultIcon(): L.Icon {
  return L.icon({
    iconUrl: "img/marker-icon.png",
    iconRetinaUrl: "img/marker-icon@2x.png",
    shadowUrl: "img/marker-shadow.png",
    iconSize: [25, 41],
    iconAnchor: [12, 41],
    popupAnchor: [1, -34],
    shadowSize: [41, 41],
  });
}
