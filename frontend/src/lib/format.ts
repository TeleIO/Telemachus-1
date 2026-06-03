// Canonical value formatters — consolidates the two legacy formatter sets
// (jKSPWAPICore.js `formatters` + console.js `siUnit/dateString/...`) into one
// module keyed by the schema's UnitType. Output strings match the legacy console.

export function orderOfMagnitude(v: number): number {
  if (+v === 0) return 0;
  return 1 + Math.floor(1e-12 + Math.log(Math.abs(+v)) / Math.LN10);
}

/** SI-prefixed value, e.g. 1234 -> "1.23400 km". Matches console.js `siUnit`. */
export function siUnit(v: number, unit = ""): string {
  if (v === 0) return `0 ${unit}`;
  const prefixes = ["μ", "m", "", "k", "M", "G", "T"];
  let scale = Math.ceil(orderOfMagnitude(v) / 3);
  if (scale <= 0 && ++scale < 0) scale = 0;
  else if (scale === 1) scale = 2;
  else if (scale >= prefixes.length) scale = prefixes.length - 1;
  return `${(v / Math.pow(1000, scale - 2)).toPrecision(6)} ${prefixes[scale]}${unit}`;
}

export function hourMinSec(t = 0): string {
  const pad = (n: number) => (n < 10 ? "0" + n : "" + n);
  const hour = (t / 3600) | 0;
  t %= 3600;
  const min = (t / 60) | 0;
  const sec = ((t % 60) | 0);
  return `${pad(hour)}:${pad(min)}:${pad(sec)}`;
}

export function dateString(t = 0): string {
  const year = ((t / (365 * 24 * 3600)) | 0) + 1;
  t %= 365 * 24 * 3600;
  const day = ((t / (24 * 3600)) | 0) + 1;
  t %= 24 * 3600;
  return `Year ${year}, Day ${day}, ${hourMinSec(t)} UT`;
}

export function missionTimeString(t = 0): string {
  let result = "T+";
  if (t >= 365 * 24 * 3600) {
    result += ((t / (365 * 24 * 3600)) | 0) + ":";
    t %= 365 * 24 * 3600;
    if (t < 24 * 3600) result += "0:";
  }
  if (t >= 24 * 3600) result += ((t / (24 * 3600)) | 0) + ":";
  t %= 24 * 3600;
  return result + hourMinSec(t) + " MET";
}

export function durationString(t = 0): string {
  let result = t < 0 ? "-" : "";
  t = Math.abs(t);
  if (t >= 365 * 24 * 3600) {
    result += ((t / (365 * 24 * 3600)) | 0) + " years ";
    t %= 365 * 24 * 3600;
    if (t < 24 * 3600) result += "0 days ";
  }
  if (t >= 24 * 3600) result += ((t / (24 * 3600)) | 0) + " days ";
  t %= 24 * 3600;
  return result + hourMinSec(t);
}

type Formatter = (v: number) => string;

/** Unit-keyed formatters, matching console.js `Telemachus.formatters`. */
const byUnit: Record<string, Formatter> = {
  unitless: (v) => (typeof v === "number" ? v.toFixed(2) : `${v}`),
  velocity: (v) => siUnit(v, "m/s"),
  deg: (v) => v.toFixed(2) + "°",
  latlon: (v) => v.toFixed(6) + "°",
  distance: (v) => siUnit(v, "m"),
  time: (v) => durationString(v),
  temp: (v) => v.toFixed(2) + " K",
  pres: (v) => v.toFixed(4) + " kPa",
  grav: (v) => v.toFixed(2) + " m/s²",
  acc: (v) => v.toFixed(3) + " G",
  date: (v) => dateString(v),
};

/**
 * Format a telemetry value given its schema UnitType (e.g. "DISTANCE").
 * Handles the array-wrapped form `[t,[value]]` and null exactly as the
 * legacy console did ("No Data").
 */
export function formatValue(value: unknown, units: string): string {
  if (value == null) return "No Data";
  if (Array.isArray(value)) return formatValue((value as [number, number[]])[1][0], units);
  const fn = byUnit[units.toLowerCase()];
  return fn ? fn(value as number) : String(value);
}
