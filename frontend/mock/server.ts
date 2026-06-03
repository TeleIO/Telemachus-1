// Local mock harness for eyeballing the bundled UI without KSP.
//
//   deno task mock        # build the UI, then serve it with fake telemetry
//   -> open http://localhost:8085/telemachus/index.html
//
// Emulates the plugin's surface: static files under /telemachus/, the HTTP
// `datalink?alias=api&...` endpoint, the `/datalink` WebSocket push channel
// (+/-/run/rate), and the a.api / a.version / a.ip meta calls. Telemetry is a
// small coherent flight sim so maps move, charts scroll and tables update.

const PORT = Number(Deno.env.get("PORT") ?? 8085);
const ROOT = new URL("..", import.meta.url).pathname; // frontend/
const DIST = `${ROOT}dist`;
const SCHEMA_PATH = `${ROOT}../docs/api-schema.json`;

interface SchemaEntry {
  key: string;
  description?: string;
  units?: string;
  plotable?: boolean;
  isAction?: boolean;
}
let schema: SchemaEntry[] = [];
const byKey = new Map<string, SchemaEntry>();
try {
  schema = JSON.parse(await Deno.readTextFile(SCHEMA_PATH));
  for (const e of schema) byKey.set(e.key, e);
} catch {
  console.warn(`mock: could not read ${SCHEMA_PATH}; a.api will be sparse`);
}

const START = Date.now();
const now = () => (Date.now() - START) / 1000;

// Stable per-key offset so generic values differ but don't jitter randomly.
function hash(s: string): number {
  let h = 0;
  for (let i = 0; i < s.length; i++) h = (h * 31 + s.charCodeAt(i)) | 0;
  return Math.abs(h % 1000) / 1000;
}

const stripArgs = (k: string) => k.replace(/\[.*$/, "");

// ---- the flight sim ----
function evaluate(rawKey: string, t: number): unknown {
  const key = rawKey.trim();
  const base = stripArgs(key);

  // meta
  if (base === "a.api") {
    return schema.map((e) => ({ apistring: e.key, name: e.description ?? e.key, units: e.units ?? "UNITLESS", plotable: !!e.plotable }));
  }
  if (base === "a.version") return "1.12.0-mock";
  if (base === "a.ip") return ["127.0.0.1", "192.168.1.42"];
  if (base === "p.paused") return 0;

  // actions return 0 (success)
  if (byKey.get(base)?.isAction || /^(f|mj)\.|^v\.set/.test(base)) return 0;

  const alt = 75000 + 20000 * Math.sin(t / 30);
  switch (base) {
    case "v.missionTime": return t;
    case "t.universalTime": return 1_000_000 + t;
    case "v.name": return "Mock Vessel";
    case "v.body": return "Kerbin";
    case "v.altitude": return alt;
    case "v.heightFromTerrain": return Math.max(0, alt - 2200);
    case "v.lat": return 12 * Math.sin(t / 20);
    case "v.long": return ((t * 2) % 360) - 180;
    case "v.surfaceVelocity":
    case "v.surfaceSpeed": return 2200 + 120 * Math.sin(t / 10);
    case "v.orbitalVelocity": return 2300 + 80 * Math.sin(t / 13);
    case "v.verticalSpeed": return 60 * Math.cos(t / 15);
    case "v.angularVelocity": return Math.abs(5 * Math.sin(t / 5));
    case "v.atmosphericDensity": return Math.max(0, 1.2 * Math.exp(-alt / 5600));
    case "v.dynamicPressure": return Math.max(0, 4000 * Math.exp(-alt / 5600));
    case "o.ApA": return 82000 + 5000 * Math.sin(t / 40);
    case "o.PeA": return 71000 + 3000 * Math.cos(t / 40);
    case "o.sma": return 700000 + alt;
    case "o.eccentricity": return 0.05 + 0.01 * Math.sin(t / 50);
    case "o.inclination": return 28.5;
    case "o.lan": return 120;
    case "o.argumentOfPeriapsis": return 45;
    case "o.timeOfPeriapsisPassage": return 600 - (t % 600);
    case "o.trueAnomaly": return (t * 6) % 360;
    case "n.heading": return (t * 3) % 360;
    case "n.pitch": return 30 * Math.sin(t / 8);
    case "n.roll": return 45 * Math.sin(t / 12);
    case "s.sensor.acc": return Math.abs(2 * Math.sin(t / 6));
    case "s.sensor.grav": return 9.81 - alt / 1e6;
    case "s.sensor.pres": return Math.max(0, 101.3 * Math.exp(-alt / 5600));
    case "s.sensor.temp": return 290 - alt / 1000 + 5 * Math.sin(t / 9);
    case "tar.name": return "Target Station";
    case "tar.distance": return 1500 + 800 * Math.sin(t / 18);
    case "tar.o.relativeVelocity": return 50 + 30 * Math.sin(t / 11);
  }

  // resources: r.resource[X] drains, r.resourceMax[X] is constant
  if (base === "r.resource") {
    const max = 1000;
    return max * (0.5 + 0.5 * Math.cos(t / 60));
  }
  if (base === "r.resourceMax") return 1000;

  // generic fallback by unit type
  const units = byKey.get(base)?.units ?? "UNITLESS";
  const off = hash(key);
  switch (units) {
    case "STRING": return "—";
    case "DATE": return 1_000_000 + t;
    case "TIME": return t % 3600;
    case "DEG":
    case "LATLON": return ((t * 10 + off * 360) % 360) - 180;
    case "DISTANCE": return 1000 + 500 * Math.sin(t / 10 + off * 6);
    case "VELOCITY": return 100 + 60 * Math.sin(t / 8 + off * 6);
    default: return 50 + 25 * Math.sin(t / 7 + off * 6);
  }
}

// ---- HTTP datalink: ?alias=api&alias2=api2 -> {alias: value} ----
function handleDatalink(url: URL): Response {
  const t = now();
  const out: Record<string, unknown> = {};
  for (const [alias, api] of url.searchParams) out[alias] = evaluate(api, t);
  return json(out);
}

// ---- WebSocket datalink: +/-/run/rate, frames keyed by api string ----
function handleSocket(req: Request): Response {
  const { socket, response } = Deno.upgradeWebSocket(req);
  const subs = new Set<string>();
  let oneShots: string[] = [];
  let rate = 500;
  let timer: number | undefined;

  const pump = () => {
    if (socket.readyState !== WebSocket.OPEN) return;
    const t = now();
    const frame: Record<string, unknown> = { "p.paused": 0 };
    for (const k of subs) frame[k] = evaluate(k, t);
    for (const k of oneShots) frame[k] = evaluate(k, t);
    oneShots = [];
    socket.send(JSON.stringify(frame));
  };
  const restart = () => {
    if (timer) clearInterval(timer);
    timer = setInterval(pump, rate);
  };

  socket.onopen = restart;
  socket.onmessage = (ev) => {
    let msg: Record<string, unknown>;
    try { msg = JSON.parse(ev.data); } catch { return; }
    const list = (v: unknown) => (Array.isArray(v) ? v.map(String) : typeof v === "string" ? [v] : []);
    if ("+" in msg) for (const k of list(msg["+"])) subs.add(k);
    if ("-" in msg) for (const k of list(msg["-"])) subs.delete(k);
    if ("run" in msg) oneShots.push(...list(msg["run"]));
    if ("rate" in msg) { rate = Math.max(50, Number(msg["rate"]) || 500); restart(); }
  };
  socket.onclose = () => timer && clearInterval(timer);
  return response;
}

// ---- static files from dist/ ----
const MIME: Record<string, string> = {
  ".html": "text/html", ".js": "text/javascript", ".css": "text/css",
  ".json": "application/json", ".png": "image/png", ".jpg": "image/jpeg",
  ".svg": "image/svg+xml", ".woff": "font/woff", ".woff2": "font/woff2",
  ".xml": "application/xml",
};

async function serveStatic(url: URL): Promise<Response> {
  let path = url.pathname;
  if (path === "/" || path === "/telemachus" || path === "/telemachus/") {
    return Response.redirect(`${url.origin}/telemachus/index.html`, 302);
  }
  path = path.replace(/^\/telemachus\//, "/").replace(/\.\./g, "");
  try {
    const file = await Deno.readFile(`${DIST}${path}`);
    const ext = path.slice(path.lastIndexOf("."));
    return new Response(file, { headers: { "content-type": MIME[ext] ?? "application/octet-stream" } });
  } catch {
    return new Response("Not found", { status: 404 });
  }
}

function json(data: unknown): Response {
  return new Response(JSON.stringify(data), { headers: { "content-type": "application/json" } });
}

Deno.serve({ port: PORT, onListen: () => {
  console.log(`\n  Telemachus mock UI  →  http://localhost:${PORT}/telemachus/index.html\n`);
} }, (req) => {
  const url = new URL(req.url);
  if (url.pathname === "/datalink" && req.headers.get("upgrade")?.toLowerCase() === "websocket") {
    return handleSocket(req);
  }
  if (url.pathname.endsWith("/datalink")) return handleDatalink(url);
  return serveStatic(url);
});
